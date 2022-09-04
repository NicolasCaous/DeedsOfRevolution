using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseMapLoader))]
public class BaseMapLoaderCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Generate Meshes"))
        {
            ((BaseMapLoader)target).StartDestroyAndInstantiateSphereSegments(false);
        }

        /*if (GUILayout.Button("Load Base Map"))
        {
            ((BaseMapLoader)target).LoadData();
        }*/
    }
}
