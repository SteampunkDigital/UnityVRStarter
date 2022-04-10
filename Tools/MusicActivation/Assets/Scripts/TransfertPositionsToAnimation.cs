using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 
public class TransfertPositionsToAnimation : EditorWindow
{
    [MenuItem("Tools/Positions to Animation")]
    public static void Open()
    {
        var w = GetWindow<TransfertPositionsToAnimation>();
        w.minSize = new Vector2(500, 200);
    }
 
    Animator animator;
    AnimationClip clip;
    Vector2 scroll;
 
    void OnGUI()
    {
        animator = EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true) as Animator;
        clip = EditorGUILayout.ObjectField("Clip", clip, typeof(AnimationClip), false) as AnimationClip;
        EditorGUILayout.Space();
        if (animator != null && clip != null)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Transfert");
            }
            EditorGUILayout.EndHorizontal();
 
            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                Transfert(clip, animator.transform);
            }
            EditorGUILayout.EndScrollView();
 
            GUI.enabled = true;
        }
    }

    void TransferFloatToFirstKey( AnimationClip clip, string basePath, string keyPath, float value ) {
        var binding = EditorCurveBinding.FloatCurve(basePath, typeof(Transform), keyPath);
        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
        Keyframe key;
        if( curve.keys.Length > 0 ) {
            key = curve.keys[0];
            key.value = value;
        } else {
            key = new Keyframe(0, value);
            curve.AddKey(key);
        }

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }
 
    void Transfert(AnimationClip clip, Transform transform)
    {
        EditorGUILayout.BeginHorizontal();
        {
            string path = AnimationUtility.CalculateTransformPath(transform, animator.transform);
            GUILayout.Label("/" + path);
            GUILayout.FlexibleSpace();
 
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip,
                EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"));
            GUI.enabled = curve != null;
            if (GUILayout.Button("Position", GUILayout.Width(60)))
            {
                Undo.RecordObject(clip, "Transform position from scene");
 
                var position = transform.localPosition;
                TransferFloatToFirstKey(clip, path, "m_LocalPosition.x", position.x);
                TransferFloatToFirstKey(clip, path, "m_LocalPosition.y", position.y);
                TransferFloatToFirstKey(clip, path, "m_LocalPosition.z", position.z);
            }

            var allCurves = AnimationUtility.GetAllCurves(clip, true);

            curve = AnimationUtility.GetEditorCurve(clip,
                EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"));
            GUI.enabled = curve != null;
            if (GUILayout.Button("Rotation", GUILayout.Width(60)))
            {
                Undo.RecordObject(clip, "Transform rosition from scene");

                var rotation = transform.localEulerAngles;
                TransferFloatToFirstKey(clip, path, "localEulerAnglesRaw.x", rotation.x);
                TransferFloatToFirstKey(clip, path, "localEulerAnglesRaw.y", rotation.y);
                TransferFloatToFirstKey(clip, path, "localEulerAnglesRaw.z", rotation.z);
            }
 
            curve = AnimationUtility.GetEditorCurve(clip,
                EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalScale.x"));
            GUI.enabled = curve != null;
            if (GUILayout.Button("Scale", GUILayout.Width(60)))
            {
                Undo.RecordObject(clip, "Transform scale from scene");

                TransferFloatToFirstKey(clip, path, "m_LocalScale.x", transform.localScale.x);
                TransferFloatToFirstKey(clip, path, "m_LocalScale.y", transform.localScale.y);
                TransferFloatToFirstKey(clip, path, "m_LocalScale.z", transform.localScale.z);
            }
        }
        EditorGUILayout.EndHorizontal();
 
        for (int i = 0; i < transform.childCount; i++)
            Transfert(clip, transform.GetChild(i));
    }
}