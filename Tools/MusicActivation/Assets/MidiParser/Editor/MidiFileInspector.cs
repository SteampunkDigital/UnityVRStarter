using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MidiParser;

 [InitializeOnLoad]
 public class MidiFileGlobal
 {
     private static MidiFileWrapper wrapper = null;
     private static bool selectionChanged = false;
 
     static MidiFileGlobal()
     {
         Selection.selectionChanged += SelectionChanged;
         EditorApplication.update += Update;
     }
 
     private static void SelectionChanged()
     {
         selectionChanged = true;
         // can't do the wrapper stuff here. it does not work 
         // when you Selection.activeObject = wrapper
         // so do it in Update
     }
 
     private static void Update()
     {
         if (selectionChanged == false) return;
 
         selectionChanged = false;
         if (Selection.activeObject != wrapper)
         {
             string fn = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
             if (fn.ToLower().EndsWith(".mid")) 
             {
                 if (wrapper == null)
                 {
                     wrapper = ScriptableObject.CreateInstance<MidiFileWrapper>();
                     wrapper.hideFlags = HideFlags.DontSave;
                 }
 
                 wrapper.fileName = fn;
                 Selection.activeObject = wrapper;
 
                 Editor[] ed = Resources.FindObjectsOfTypeAll<MidiFileWrapperInspector>();
                 if (ed.Length > 0) ed[0].Repaint();
             }
         }
     }
 }
 // MidiFileWrapper.cs 
 public class MidiFileWrapper: ScriptableObject
 {
     [System.NonSerialized] public string fileName; // path is relative to Assets/
 }

[CustomEditor(typeof(MidiFileWrapper))]
public class MidiFileWrapperInspector : Editor {
    public override void OnInspectorGUI()
    {
        // Check if target is MidiFileWrapper
        if (target.GetType() != typeof(MidiFileWrapper))
        {
            return;
        }

        MidiFileWrapper Target = (MidiFileWrapper)target;
        var midiFile = new MidiFile(Target.fileName);
        GUILayout.Label("Editing: " + Target.fileName);

        foreach (var track in midiFile.Tracks)
        {
            GUILayout.Label("Track: " + track.Index);
            Dictionary<int,int> notes = new Dictionary<int,int>();
            Dictionary<int,int> controllers = new Dictionary<int,int>();
            foreach( var midiEvent in track.MidiEvents)
            {
                switch(midiEvent._MidiEventType)
                {
                    case MidiEventType.NoteOn:
                    case MidiEventType.NoteOff:
                    {
                        MidiNoteEvent noteEvent = (MidiNoteEvent)midiEvent;
                        if( notes.ContainsKey(noteEvent.Note) == false )
                        {  
                            notes.Add(noteEvent.Note, noteEvent.Note);
                        }
                        break;
                    }
                    case MidiEventType.ControlChange:
                    {
                        MidiControlChangeEvent controlEvent = (MidiControlChangeEvent)midiEvent;
                        if( controllers.ContainsKey(controlEvent.Controller) == false )
                        {  
                            controllers.Add(controlEvent.Controller, controlEvent.Controller);
                        }
                        break;
                    }
                    default:
                        break;
                }
            }

            GUILayout.Label(string.Format("    Note IDs: [{0}]", string.Join(", ", notes.Keys)));
            GUILayout.Label(string.Format("    Controller IDs: [{0}]", string.Join(", ", controllers.Keys)));
        }
    }
}