using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
             if (fn.ToLower().EndsWith(".midi"))
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
        MidiFileWrapper Target = (MidiFileWrapper)target;

        GUILayout.Label("Editing: " + Target.fileName);
        GUILayout.Label(".... stuff");
    }
}