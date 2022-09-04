using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CircleMeshGenerator : MonoBehaviour
{
    public int totalTriangles = 3;
    public float radius = 1f;

    void OnValidate()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> UVs = new List<Vector2>();

        float step = 360f / totalTriangles;

        vertices.Add(Vector3.zero);
        vertices.Add(Vector3.forward * radius);
        UVs.Add(Vector2.zero);
        UVs.Add(Vector2.up);

        for (int i=1; i<totalTriangles; ++i)
        {
            Vector3 t = Quaternion.Euler(0, i * step, 0) * Vector3.forward;

            vertices.Add(t * radius);
            UVs.Add(new Vector2(t.x, t.z));

            triangles.Add(0);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);
        }

        triangles.Add(0);
        triangles.Add(vertices.Count - 1);
        triangles.Add(1);

        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, UVs);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
