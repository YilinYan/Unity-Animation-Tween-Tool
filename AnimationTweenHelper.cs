// Unity Animation Tween Tool
// Copyright 2020, Yilin Yan, All rights reserved.
// Contact: yilinyan1030@gmail.com


using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;


public class AnimationTweenHelper
{
    static Type animWindowType
    {
        get
        {
            if (_animWindowType != null) return _animWindowType;
            _animWindowType = System.Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
            return _animWindowType;
        }
    }
    static Type _animWindowType;
    static UnityEngine.Object animWindowObject
    {
        get
        {
            if(_animWindowObject != null)
            {
                return _animWindowObject;
            }

            //Debug.Log("get animWindowObject");

            _animWindowObject = GetOpenAnimationWindow();
            return _animWindowObject;
        }
    }
    static UnityEngine.Object _animWindowObject;


    static FieldInfo _animEditorField;
    static Type _animEditorType;
    static System.Object animEditorObject
    {
        get
        {
            if(_animEditorObject != null)
            {
                return _animEditorObject;
            }

            //Debug.Log("get animEditorObject");

            if (animWindowObject != null)
            {
                _animEditorField = animWindowType.GetField("m_AnimEditor", BindingFlags.Instance | BindingFlags.NonPublic);
                _animEditorType = _animEditorField.FieldType;
                _animEditorObject = _animEditorField.GetValue(animWindowObject);
            }
            return _animEditorObject;
        }
    }
    static System.Object _animEditorObject;


    static FieldInfo _animWindowStateField;
    static Type _animwindowStateType;
    static System.Object animwindowStateObject
    {
        get
        {
            if (_animwindowStateObject != null)
            {
                return _animwindowStateObject;
            }

            //Debug.Log("get animwindowStateObject");

            if (animEditorObject != null)
            {
                _animWindowStateField = _animEditorType.GetField("m_State", BindingFlags.Instance | BindingFlags.NonPublic);
                _animwindowStateType = _animWindowStateField.FieldType;
                _animwindowStateObject = _animWindowStateField.GetValue(animEditorObject);
            }
            return _animwindowStateObject;
        }
    }
    static System.Object _animwindowStateObject;


    static FieldInfo _curveEditorField;
    static Type _curveEditorType;
    static System.Object curveEditorObject
    {
        get
        {
            if (_curveEditorObject != null)
            {
                return _curveEditorObject;
            }

            //Debug.Log("get curveEditorObject");

            if (animEditorObject != null)
            {
                _curveEditorField = _animEditorType.GetField("m_CurveEditor", BindingFlags.Instance | BindingFlags.NonPublic);
                _curveEditorType = _curveEditorField.FieldType;
                _curveEditorObject = _curveEditorField.GetValue(animEditorObject);
            }
            return _curveEditorObject;
        }
    }
    static System.Object _curveEditorObject;


    static Type curveWrapperType
    {
        get
        {
            if (_curveWrapperType != null)
            {
                return _curveWrapperType;
            }
            _curveWrapperType = System.Type.GetType("UnityEditor.CurveWrapper,UnityEditor");
            return _curveWrapperType;
        }
    }
    static Type _curveWrapperType;


    #region AnimationWindowInit
    
    public static void Init()
    {
        _animWindowType = System.Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
        _curveWrapperType = System.Type.GetType("UnityEditor.CurveWrapper,UnityEditor");
    }

    static UnityEngine.Object GetOpenAnimationWindow()
    {
        try
        {
            UnityEngine.Object[] openAnimationWindows = Resources.FindObjectsOfTypeAll(animWindowType);
            if (openAnimationWindows.Length > 0)
            {
                return openAnimationWindows[0];
            }
        }
        catch
        {
            return null;
        }
        return null;
    }

    #endregion

    #region InvokeCurveEditorMethods

    public static void CurveEditorAddKey(float ratio)
    {
        if (curveEditorObject != null)
        {
            //Debug.Log("CurveEditorAddKey()");

            // get CurrentTime
            float time = GetAnimationWindowCurrentTime();

            // get curveWrappers
            PropertyInfo propInfo = _curveEditorType.GetProperty("animationCurves");
            System.Object curveWrappers = propInfo.GetValue(curveEditorObject);

            // turn curveWrappers into IEnumerable
            IEnumerable<object> curveWrappersList = (curveWrappers as IEnumerable<object>);
            var iter = curveWrappersList.GetEnumerator();

            // add undo label
            string undoLabel = "Add Key";
            //SaveKeySelection(undoLabel);
            _curveEditorType.InvokeMember("SaveKeySelection", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null,
                curveEditorObject, new object[1] { undoLabel });

            FieldInfo field;
            object current;
            AnimationCurve curve;
            Keyframe keyL, keyR, insertKey;
            while (true)
            {
                // iteration
                try { iter.MoveNext(); current = iter.Current; } catch { break; }
                if (current == null) break;

                // check if hidden
                field = curveWrapperType.GetField("hidden");
                bool hidden = (bool)field.GetValue(current);

                if (hidden == false)
                {
                    // get curve
                    propInfo = curveWrapperType.GetProperty("curve");
                    curve = propInfo.GetValue(current) as AnimationCurve;
                    if (curve == null) continue;

                    // record left right key
                    keyL = new Keyframe(float.MinValue, 0f);
                    keyR = new Keyframe(float.MaxValue, 1f);
                    int removeIndex = -1;
                    for (int i = 0; i < curve.keys.Length; ++i)
                    {
                        Keyframe key = curve.keys[i];
                        if (key.time == time) removeIndex = i;
                        else if (key.time < time && key.time > keyL.time) keyL = key;
                        else if (key.time > time && key.time < keyR.time) keyR = key;
                    }

                    // insert new key
                    // add +-2f is to make upper or lower key
                    float valL = keyL.time > float.MinValue ? keyL.value : keyR.value - 2f;
                    float valR = keyR.time < float.MaxValue ? keyR.value : keyL.value + 2f;
                    insertKey = new Keyframe(time, (valR - valL) * ratio + valL, keyL.outTangent, keyL.inTangent);
                    if (removeIndex >= 0)
                    {
                        //Debug.Log("removeIndex >= 0");
                        curveWrapperType.InvokeMember("MoveKey", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null,
                            current, new object[2] { removeIndex, insertKey });
                    }
                    else
                    {
                        //Debug.Log("removeIndex < 0");
                        curveWrapperType.InvokeMember("AddKey", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null,
                            current, new object[1] { insertKey });
                    }

                    // changed
                    propInfo = curveWrapperType.GetProperty("changed", typeof(bool));
                    propInfo.SetValue(current, true);
                }
            }

            //field: UnityEditor.CurveEditor+CallbackFunction curvesUpdated
            field = _curveEditorType.GetField("curvesUpdated");
            object CallbackFunctionObj = field.GetValue(curveEditorObject);
            MethodInfo CallbackFunctionMethod = CallbackFunctionObj.GetType().GetMethod("Invoke");
            CallbackFunctionMethod.Invoke(CallbackFunctionObj, new object[0]);

            //SelectNone()
            _curveEditorType.InvokeMember("SelectNone", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null,
                curveEditorObject, null);
        }
    }

    #endregion

    #region InvokeAnimEditorMethods
    
    public static void AnimEditorRepaint()
    {
        if (animEditorObject != null)
        {
            //Debug.Log("AnimEditorRepaint()");
            _animEditorType.InvokeMember("Repaint", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null,
                animEditorObject, null);
        }
    }

    #endregion

    #region GetFromAnimationWindowState

    public static float GetAnimationWindowCurrentTime()
    {
        if (animwindowStateObject != null)
        {
            PropertyInfo propInfo = _animwindowStateType.GetProperty("currentTime");
            System.Object currentTime = propInfo.GetValue(animwindowStateObject);

            //Debug.Log("currentTime: " + currentTime.ToString());
            return (float)currentTime;
        }

        return -1f;
    }

    #endregion
}
