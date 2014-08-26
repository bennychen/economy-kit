﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VirtualCurrency))]
public class VirtualCurrencyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VirtualCurrency item = target as VirtualCurrency;
        EditorGUILayout.LabelField("ID", item.ID);
        EditorGUILayout.LabelField("Name", item.Name);
        EditorGUILayout.LabelField("Description", item.Description);
        if (item.Category != null)
        {
            EditorGUILayout.LabelField("Category", item.Category.ID);
        }
    }
}