#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using JadePhoenix.Tools;

[CustomEditor(typeof(MultipleObjectPooler))]
public class MultipleObjectPoolerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Allow default Inspector to draw other properties
        DrawDefaultInspector();

        MultipleObjectPooler pooler = (MultipleObjectPooler)target;
        Dictionary<string, ObjectPool> dictionary = pooler.ObjectPoolsByObjectType;

        EditorGUILayout.LabelField("Object Pools by Type", EditorStyles.boldLabel);
        if (dictionary != null)
        {
            // Loop through the dictionary and display its content
            foreach (KeyValuePair<string, ObjectPool> pair in dictionary)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(pair.Key);
                EditorGUILayout.ObjectField(pair.Value, typeof(ObjectPool), false);
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
        }
        else
        {
            EditorGUILayout.LabelField("Dictionary is not initialized.");
        }
    }
}
#endif