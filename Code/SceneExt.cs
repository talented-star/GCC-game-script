#if UNITY_EDITOR
using ModestTree;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SceneExt
{
    public static string[] GetScenes()
    {
        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(Path.GetFileNameWithoutExtension(scene.path));
        }
        return scenes.ToArray();
    }
}

public class ListToPopupAttribute : PropertyAttribute
{
    public Type myType;
    public string propertyName;
    public ListToPopupAttribute(Type _myType, string _propertyName)
    {
        myType = _myType;
        propertyName = _propertyName;
    }

}

[CustomPropertyDrawer(typeof(ListToPopupAttribute))]
public class ListToPopupDrawer : PropertyDrawer
{
    int selectedIndex = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ListToPopupAttribute atb = attribute as ListToPopupAttribute;
        string[] stringList = null;

        if (atb.myType.GetField(atb.propertyName) != null)
        {
            stringList = atb.myType.GetField(atb.propertyName).GetValue(atb.myType) as string[];
            selectedIndex = stringList.IndexOf(property.stringValue);
            if (stringList != null)
                selectedIndex = stringList.IndexOf(property.stringValue);
        }
        if (stringList != null && stringList.Length > 0 && selectedIndex < stringList.Length)
        {
            selectedIndex = EditorGUI.Popup(position, property.name, selectedIndex, stringList);
            if (selectedIndex < 0)
                selectedIndex = 0;
            property.stringValue = stringList[selectedIndex];
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }

    }
}
#endif