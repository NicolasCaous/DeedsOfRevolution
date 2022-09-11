using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CountriesBorderGenerator))]
public class CountriesBorderGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Load Countries Borders"))
        {
            ((CountriesBorderGenerator) target).StartDestroyAndLoadBorders();
        }
    }
}