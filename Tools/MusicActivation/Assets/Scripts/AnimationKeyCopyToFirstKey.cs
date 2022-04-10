using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 
public class AnimationKeyCopyToFirstKey : EditorWindow
{
    [MenuItem("Tools/Copy To First Key")]
    public static void Open()
    {
        var w = GetWindow<AnimationKeyCopyToFirstKey>();
        w.minSize = new Vector2(500, 200);
    }
 
    Animator animator;
    AnimationClip clip;
    Vector2 scroll;
 
    void OnGUI()
    {
        animator = EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true) as Animator;
        var animWindow = EditorWindow.GetWindow<AnimationWindow>();
        clip = animWindow.animationClip;
        var time = animWindow.time;

        // clip = EditorGUILayout.ObjectField("Clip", clip, typeof(AnimationClip), false) as AnimationClip;
        // GUILayout.Label(EditorWindow.focusedWindow.ToString());

        EditorGUILayout.Space();
        if (animator != null && clip != null)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Transfer Key at Time {time} to First Key");
            }
            EditorGUILayout.EndHorizontal();
 
            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                Transfert(clip, time, animator.transform);
            }
            EditorGUILayout.EndScrollView();
 
            GUI.enabled = true;
        }
    }

    void TransferFloatToFirstKey( AnimationClip clip, string basePath, string keyPath, float time ) {
        var binding = EditorCurveBinding.FloatCurve(basePath, typeof(Transform), keyPath);
        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
        Keyframe? srcKey = GetKeyframeAtTime( curve, time );
        if( srcKey.HasValue ) {

            Keyframe key;
            if( curve.keys.Length > 0 ) {
                key = curve.keys[0];
                if( key.time == 0 ) { curve.RemoveKey(0); }
            }

            key = new Keyframe(0, srcKey.Value.value);
            curve.AddKey(key);

            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
    }

    Keyframe? GetKeyframeAtTime( AnimationCurve curve, float time ) {
        if( curve == null ) return null;

        foreach( var key in curve.keys ) {
            if( key.time == time ) {
                return key;
            }
        }
        return null;
    }
 
    void Transfert(AnimationClip clip, float time, Transform transform)
    {
        EditorGUILayout.BeginHorizontal();
        {
            string path = AnimationUtility.CalculateTransformPath(transform, animator.transform);
            GUILayout.Label("/" + path);
            GUILayout.FlexibleSpace();
 
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip,
                EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"));

            Keyframe? key;
            key = GetKeyframeAtTime(curve, time);
            GUI.enabled = key != null;
            if (GUILayout.Button("Position", GUILayout.Width(60)))
            {
                Undo.RecordObject(clip, "Transform position from scene");
 
                var position = transform.localPosition;
                TransferFloatToFirstKey(clip, path, "m_LocalPosition.x", time);
                TransferFloatToFirstKey(clip, path, "m_LocalPosition.y", time);
                TransferFloatToFirstKey(clip, path, "m_LocalPosition.z", time);
            }

            var allCurves = AnimationUtility.GetAllCurves(clip, true);

            curve = AnimationUtility.GetEditorCurve(clip,
                EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"));
            key = GetKeyframeAtTime(curve, time);
            GUI.enabled = key != null;
            if (GUILayout.Button("Rotation", GUILayout.Width(60)))
            {
                Undo.RecordObject(clip, "Transform rosition from scene");

                var rotation = transform.localEulerAngles;
                TransferFloatToFirstKey(clip, path, "localEulerAnglesRaw.x", time);
                TransferFloatToFirstKey(clip, path, "localEulerAnglesRaw.y", time);
                TransferFloatToFirstKey(clip, path, "localEulerAnglesRaw.z", time);
            }
 
            curve = AnimationUtility.GetEditorCurve(clip,
                EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalScale.x"));
            key = GetKeyframeAtTime(curve, time);
            GUI.enabled = key != null;
            if (GUILayout.Button("Scale", GUILayout.Width(60)))
            {
                Undo.RecordObject(clip, "Transform scale from scene");

                TransferFloatToFirstKey(clip, path, "m_LocalScale.x", time);
                TransferFloatToFirstKey(clip, path, "m_LocalScale.y", time);
                TransferFloatToFirstKey(clip, path, "m_LocalScale.z", time);
            }
        }
        EditorGUILayout.EndHorizontal();
 
        for (int i = 0; i < transform.childCount; i++)
            Transfert(clip, time, transform.GetChild(i));
    }
}