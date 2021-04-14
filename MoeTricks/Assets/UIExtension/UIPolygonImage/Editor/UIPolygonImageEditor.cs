using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(UIPolygonImage))]
public class UIPolygonImageEditor : Editor
{
    public UIPolygonImage curImage = null;
    public void OnEnable()
    {
        curImage = (UIPolygonImage)target;
    }
    public void OnDisable()
    {
        curImage = null;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("GeneratePolygonFromSprite"))
        {
            curImage.GeneratePolygonFromSprite();
        }
        if(GUILayout.Button("Set Navie Size"))
        {
            curImage.SetNativeSize();
        }
    }
}
