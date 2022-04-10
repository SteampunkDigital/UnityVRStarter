using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiParser;

public class ConvertMidiToAni : MonoBehaviour
{
    public string midiFilePath;
    public string aniOutputFilename;

    void Start()
    {
        // Get whole midiFilePath from StreamingAssets folder
        string fullMidiFilePath = Application.streamingAssetsPath + "/" + this.midiFilePath;
        var midiFile = new MidiFile(fullMidiFilePath);
        
        // Create Streaming Assets path with aniOutputFilename.
        string aniOutputPath = Application.streamingAssetsPath + "/" + aniOutputFilename;

        // Iterate over all the tracks in the midi file.
        foreach (var track in midiFile.Tracks)
        {
            Debug.Log("Track: " + track.Index);

            // Create a new AnimationClip.
            // var aniClip = new AnimationClip();

            // Iterate over all the notes in the track.
            foreach (var eventData in track.MidiEvents)
            {
                // Convert event to strings.
                var ticks = eventData.Ticks;
                var type = eventData.MidiEventType;
                var arg1 = eventData.Arg1;
                var arg2 = eventData.Arg2;
                var arg3 = eventData.Arg3;

                var typeName = type.ToString();
                switch(type) {
                    case MidiEventType.NoteOn:
                    case MidiEventType.NoteOff:
                        Debug.Log(string.Format("{0} {1} ch:{2} n:{3} vel:{4}", ticks, typeName, arg1, arg2, arg3));
                        break;
                    case MidiEventType.PitchBendChange:
                        Debug.Log(string.Format("{0} {1} ch:{2} {3}-{4}", ticks, typeName, arg1, arg2, arg3));
                        break;
                    case MidiEventType.KeyAfterTouch:
                        Debug.Log(string.Format("{0} {1} ch:{2} n:{3} amt:{4}", ticks, typeName, arg1, arg2, arg3));
                        break;
                    case MidiEventType.ControlChange: {
                        var controlChangeType = eventData.ControlChangeType;
                        var controlChangeName = controlChangeType.ToString();
                        Debug.Log(string.Format("{0} {1} ch:{2} controller:{3} val:{4}", ticks, typeName, arg1, controlChangeName, arg3));
                    }   break;
                    case MidiEventType.ProgramChange:
                        Debug.Log(string.Format("{0} {1} ch:{2} prog:{3}", ticks, typeName, arg1, arg2));
                        break;
                    case MidiEventType.ChannelAfterTouch:
                        Debug.Log(string.Format("{0} {1} ch:{2} amt:{3}", ticks, typeName, arg1, arg2));
                        break;
                    case MidiEventType.MetaEvent: {
                        var metaEventType = eventData.MetaEventType;
                        var metaEventName = metaEventType.ToString();
                        switch(metaEventType) {
                            case MetaEventType.Tempo:
                                Debug.Log(string.Format("{0} {1} {2} bpm:{3}", ticks, typeName, metaEventName, arg2));
                                break;
                            case MetaEventType.TimeSignature:
                                Debug.Log(string.Format("{0} {1} {2} {3}/{4}", ticks, typeName, metaEventName, arg2, arg3));
                                break;
                            case MetaEventType.KeySignature:
                                Debug.Log(string.Format("{0} {1} {2} {3} {4}", ticks, typeName, metaEventName, arg2, arg3));
                                break;
                        }
                    } break;
                }

                // Print the event info.
                // Debug.Log(string.Format("{0} {1} {2} {3} {4} {5}", time, typeName, arg1, arg2, arg3, eventData.MetaEventType));


                // Create a new keyframe.
                // var keyframe = new Keyframe(eventData.Time, eventData.Value);

                // // Add the keyframe to the animation clip.
                // aniClip.SetCurve("", typeof(Transform), "localPosition.y", keyframe);
            }

            // // Add the animation clip to the animation.
            // GetComponent<Animation>().AddClip(aniClip, track.Name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
