// Unity Animation Tween Tool
// Copyright 2020, Yilin Yan, All rights reserved.
// Contact: yilinyan1030@gmail.com


using UnityEngine;
using UnityEditor;
 

public class AnimationTweenToolEditor : EditorWindow
{
    [MenuItem("Custom/Animation Tween Tool", false, 2000)]
    public static void DoWindow()
    {
        AnimationTweenHelper.Init();
        var window = GetWindowWithRect<AnimationTweenToolEditor>(new Rect(0, 0, 300, 80), false, "Animation Tween Tool");
        window.Show();
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Add KeyFrame");

        EditorGUILayout.BeginHorizontal();
        for (int i = -2; i < 0; ++i)
        {
            if (GUILayout.Button(""))
            {
                AnimationTweenHelper.CurveEditorAddKey(i / 10f);
                AnimationTweenHelper.AnimEditorRepaint();
            }
        }
        EditorGUILayout.Space();
        for (int i = 0; i < 11; ++i)
        {
            if (GUILayout.Button(""))
            {
                AnimationTweenHelper.CurveEditorAddKey(i / 10f);
                AnimationTweenHelper.AnimEditorRepaint();
            }
        }
        EditorGUILayout.Space();
        for (int i = 11; i < 13; ++i)
        {
            if (GUILayout.Button(""))
            {
                AnimationTweenHelper.CurveEditorAddKey(i / 10f);
                AnimationTweenHelper.AnimEditorRepaint();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

}