using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FinalMeshGenerator))]
public class FinalMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Generate Meshes"))
        {
            ((FinalMeshGenerator)target).StartDestroyAndInstantiateSphereSegments(false);
        }
    }
}
