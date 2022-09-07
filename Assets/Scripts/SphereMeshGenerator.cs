using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SphereMeshGenerator : MonoBehaviour
{
    [Range(0f, 6f)]
    public int resolution = 0;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshFilter>().mesh = GenerateMesh();
    }

    private void OnValidate()
    {
        GetComponent<MeshFilter>().mesh = GenerateMesh();
    }

    private Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Quaternion.AngleAxis(-108f, Vector3.up) * Vector3.right;
        Vector3 v3 = (Quaternion.AngleAxis(-108f, Vector3.up) * (v2 * -1)) + v2;
        Vector3 v4 = (Quaternion.AngleAxis(108f, Vector3.up) * Vector3.left) + Vector3.right;
        Vector3 v5 = Vector3.right;
        float c = Mathf.Pow(Mathf.Sin(Mathf.Deg2Rad * 54f) / Mathf.Sin(Mathf.Deg2Rad * 72f), 2);
        Vector3 v6 = new Vector3(0.5f, Mathf.Sqrt(1 - c), Mathf.Sqrt(c - (1f / 4f)));

        Vector3 center = (v1 + v2 + v3 + v4 + v5) / 5;

        Vector3 v7 = (Quaternion.AngleAxis(36f, Vector3.up) * (v1 - center)) + center;
        Vector3 v8 = (Quaternion.AngleAxis(36f, Vector3.up) * (v2 - center)) + center;
        Vector3 v9 = (Quaternion.AngleAxis(36f, Vector3.up) * (v3 - center)) + center;
        Vector3 v10 = (Quaternion.AngleAxis(36f, Vector3.up) * (v4 - center)) + center;
        Vector3 v11 = (Quaternion.AngleAxis(36f, Vector3.up) * (v5 - center)) + center;
        Vector3 v12 = new Vector3(v6.x, -v6.y, v6.z);

        Vector3 shift = new Vector3(0f, Mathf.Sqrt((3f - Mathf.Pow(Mathf.Tan(18f * Mathf.Deg2Rad), 2f)) / 4f) / 2f, 0f);

        v1 = v1 + shift;
        v2 = v2 + shift;
        v3 = v3 + shift;
        v4 = v4 + shift;
        v5 = v5 + shift;
        v6 = v6 + shift;
        v7 = v7 - shift;
        v8 = v8 - shift;
        v9 = v9 - shift;
        v10 = v10 - shift;
        v11 = v11 - shift;
        v12 = v12 - shift;

        center = (v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10 + v11 + v12) / 12;

        v1 = (v1 - center).normalized;
        v2 = (v2 - center).normalized;
        v3 = (v3 - center).normalized;
        v4 = (v4 - center).normalized;
        v5 = (v5 - center).normalized;
        v6 = (v6 - center).normalized;
        v7 = (v7 - center).normalized;
        v8 = (v8 - center).normalized;
        v9 = (v9 - center).normalized;
        v10 = (v10 - center).normalized;
        v11 = (v11 - center).normalized;
        v12 = (v12 - center).normalized;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        vertices.Add(v5);
        vertices.Add(v6);
        vertices.Add(v7);
        vertices.Add(v8);
        vertices.Add(v9);
        vertices.Add(v10);
        vertices.Add(v11);
        vertices.Add(v12);

        triangles.Add(0);triangles.Add(1);triangles.Add(5);
        triangles.Add(1);triangles.Add(2);triangles.Add(5);
        triangles.Add(2);triangles.Add(3);triangles.Add(5);
        triangles.Add(3);triangles.Add(4);triangles.Add(5);
        triangles.Add(4);triangles.Add(0);triangles.Add(5);
        triangles.Add(10); triangles.Add(0); triangles.Add(4);
        triangles.Add(10); triangles.Add(6); triangles.Add(0);
        triangles.Add(6); triangles.Add(1); triangles.Add(0);
        triangles.Add(6); triangles.Add(7); triangles.Add(1);
        triangles.Add(7); triangles.Add(2); triangles.Add(1);
        triangles.Add(7); triangles.Add(8); triangles.Add(2);
        triangles.Add(8); triangles.Add(3); triangles.Add(2);
        triangles.Add(8); triangles.Add(9); triangles.Add(3);
        triangles.Add(9); triangles.Add(4); triangles.Add(3);
        triangles.Add(9); triangles.Add(10); triangles.Add(4);
        triangles.Add(7); triangles.Add(6); triangles.Add(11);
        triangles.Add(8); triangles.Add(7); triangles.Add(11);
        triangles.Add(9); triangles.Add(8); triangles.Add(11);
        triangles.Add(10); triangles.Add(9); triangles.Add(11);
        triangles.Add(6); triangles.Add(10); triangles.Add(11);

        Vector3[] verticesA = vertices.ToArray();
        int[] trianglesA = triangles.ToArray();

        for (int i = 0; i < resolution; i++)
        {
            Vector3[] newVerticesA = new Vector3[trianglesA.Length * 4];
            int[] newTrianglesA = new int[trianglesA.Length * 4];

            Dictionary<Vector3, int> dict = new Dictionary<Vector3, int>();
            int vindex = 0;

            for (int tindex = 0; tindex < trianglesA.Length; tindex += 3)
            {
                Vector3 vertice1 = verticesA[trianglesA[tindex]], vertice2 = verticesA[trianglesA[tindex + 1]], vertice3 = verticesA[trianglesA[tindex + 2]];

                Vector3 vertice4 = Vector3.Lerp(vertice1, vertice2, 0.5f).normalized;
                Vector3 vertice5 = Vector3.Lerp(vertice2, vertice3, 0.5f).normalized;
                Vector3 vertice6 = Vector3.Lerp(vertice3, vertice1, 0.5f).normalized;

                int[] verticesIndex = new int[6];

                if (dict.ContainsKey(vertice1))
                    verticesIndex[0] = dict.GetValueOrDefault(vertice1);
                else
                {
                    dict.Add(vertice1, vindex);
                    newVerticesA[vindex] = vertice1;
                    verticesIndex[0] = vindex;
                    vindex++;
                }

                if (dict.ContainsKey(vertice2))
                    verticesIndex[1] = dict.GetValueOrDefault(vertice2);
                else
                {
                    dict.Add(vertice2, vindex);
                    newVerticesA[vindex] = vertice2;
                    verticesIndex[1] = vindex;
                    vindex++;
                }

                if (dict.ContainsKey(vertice3))
                    verticesIndex[2] = dict.GetValueOrDefault(vertice3);
                else
                {
                    dict.Add(vertice3, vindex);
                    newVerticesA[vindex] = vertice3;
                    verticesIndex[2] = vindex;
                    vindex++;
                }

                if (dict.ContainsKey(vertice4))
                    verticesIndex[3] = dict.GetValueOrDefault(vertice4);
                else
                {
                    dict.Add(vertice4, vindex);
                    newVerticesA[vindex] = vertice4;
                    verticesIndex[3] = vindex;
                    vindex++;
                }

                if (dict.ContainsKey(vertice5))
                    verticesIndex[4] = dict.GetValueOrDefault(vertice5);
                else
                {
                    dict.Add(vertice5, vindex);
                    newVerticesA[vindex] = vertice5;
                    verticesIndex[4] = vindex;
                    vindex++;
                }

                if (dict.ContainsKey(vertice6))
                    verticesIndex[5] = dict.GetValueOrDefault(vertice6);
                else
                {
                    dict.Add(vertice6, vindex);
                    newVerticesA[vindex] = vertice6;
                    verticesIndex[5] = vindex;
                    vindex++;
                }

                newTrianglesA[tindex * 4 + 0] = verticesIndex[0]; newTrianglesA[tindex * 4 +  1] = verticesIndex[3]; newTrianglesA[tindex * 4 +  2] = verticesIndex[5];
                newTrianglesA[tindex * 4 + 3] = verticesIndex[3]; newTrianglesA[tindex * 4 +  4] = verticesIndex[1]; newTrianglesA[tindex * 4 +  5] = verticesIndex[4];
                newTrianglesA[tindex * 4 + 6] = verticesIndex[4]; newTrianglesA[tindex * 4 +  7] = verticesIndex[2]; newTrianglesA[tindex * 4 +  8] = verticesIndex[5];
                newTrianglesA[tindex * 4 + 9] = verticesIndex[3]; newTrianglesA[tindex * 4 + 10] = verticesIndex[4]; newTrianglesA[tindex * 4 + 11] = verticesIndex[5];
            }

            Array.Resize(ref newVerticesA, vindex);
            verticesA = newVerticesA;
            trianglesA = newTrianglesA;
        }

        List<Vector2> UVs = new List<Vector2>();
        for (int i = 0; i < verticesA.Length; ++i)
        {
            UVs.Add(LngLatUtils.LngLat2UVs(LngLatUtils.Point2LngLat(verticesA[i], Vector3.up, Vector3.forward)));
        }

        mesh.Clear();

        mesh.vertices = verticesA;
        mesh.triangles = trianglesA;
        mesh.SetUVs(0, UVs);

        mesh.RecalculateNormals();
        return mesh;
    }
}
