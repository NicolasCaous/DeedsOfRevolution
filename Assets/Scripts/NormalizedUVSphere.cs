using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class NormalizedUVSphere : MonoBehaviour
{
    public Texture2D heightMap;
    [Range(0f, 1f)]
    public float heightScaler = 0.1f;
    [Range(3, 400)]
    public int parallels = 100;
    [Range(2, 8)]
    public int minimumFactor = 5;
    [Range(3, 10)]
    public int maximumFactor = 10;
    public List<Vector2> restrictiveBounds;
    public List<Vector2> permissiveBounds;

    void Start()
    {

    }

    private void OnValidate()
    {
        CullMesh(GenerateMesh());
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> UVs = new List<Vector2>();

        int lastMeridians = 0;
        for (int i = 0; i < parallels; i++)
        {
            float pAngle = (180f * i / (parallels - 1)) - 90f;
            int meridians = CountMeridians(pAngle);
            int indexOfLastMeridian = vertices.Count - lastMeridians;

            for (int j = 0; j < meridians; j++)
            {
                float mAngle = (360f * j / (meridians - 1)) - 180f;
                Vector2 UV = LngLat2UVs(new Vector2(mAngle, pAngle) * -1);
                UVs.Add(UV);

                float scaler = 1 + heightMap.GetPixelBilinear(UV.x, UV.y).r * heightScaler;
                Vector3 vertex = Quaternion.Euler(0, mAngle, 0) * (Quaternion.Euler(pAngle, 0, 0) * (Vector3.forward * scaler));
                vertices.Add(vertex);

                if (lastMeridians == 0)
                    continue;

                if (lastMeridians > meridians)
                {
                    // Is a decrease
                    int rate = lastMeridians / meridians;

                    int indexStartOfChain = indexOfLastMeridian + j * rate;

                    // Left Triangles and Right Triangles
                    for (int k = 0; k < rate; ++k)
                    {
                        triangles.Add(indexStartOfChain + k + 1);
                        triangles.Add(indexStartOfChain + k);
                        triangles.Add(vertices.Count - 1);
                    }

                    // Middle Triangle
                    triangles.Add(vertices.Count - 2);
                    triangles.Add(vertices.Count - 1);
                    triangles.Add(indexStartOfChain);
                }
                else
                {
                    // Is an increase
                    int rate = meridians / lastMeridians;

                    if (j % rate != rate - 1) continue;

                    int indexStartOfChain = vertices.Count - rate;

                    // Left Triangles and Right Triangles
                    for (int k = 0; k < rate; ++k)
                    {
                        triangles.Add(indexStartOfChain + k);
                        triangles.Add(indexStartOfChain + k + 1);
                        triangles.Add(indexOfLastMeridian + (j / rate) + ((k >= rate / 2) ? 1 : 0));
                    }

                    // Middle Triangle
                    triangles.Add(indexStartOfChain + rate / 2);
                    triangles.Add(indexOfLastMeridian + (j / rate) + 1);
                    triangles.Add(indexOfLastMeridian + (j / rate));
                }
            }

            lastMeridians = meridians;
        }

        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, UVs);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }

    public void CullMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector2[] UVs = mesh.uv;

        bool[] vertexInUse = new bool[vertices.Length];
        List<int> culledTriangles = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            bool skipTriangle = false;
            foreach (Vector2 bound in restrictiveBounds)
            {
                Vector3 reference = Quaternion.Euler(bound) * Vector3.back;
                if (Vector3.Dot(reference, v1) > 0 && Vector3.Dot(reference, v2) > 0 && Vector3.Dot(reference, v3) > 0)
                    skipTriangle = true;
            }

            if (skipTriangle) continue;

            int boundCount = 0;
            foreach (Vector2 bound in permissiveBounds)
            {
                Vector3 reference = Quaternion.Euler(bound) * Vector3.back;
                if (Vector3.Dot(reference, v1) > 0 && Vector3.Dot(reference, v2) > 0 && Vector3.Dot(reference, v3) > 0)
                    boundCount += 1;
            }
            if (boundCount == permissiveBounds.Count && permissiveBounds.Count != 0) continue;

            culledTriangles.Add(triangles[i]);
            culledTriangles.Add(triangles[i + 1]);
            culledTriangles.Add(triangles[i + 2]);

            vertexInUse[triangles[i]] = true;
            vertexInUse[triangles[i + 1]] = true;
            vertexInUse[triangles[i + 2]] = true;
        }

        List<Vector3> culledVertices = new List<Vector3>();
        List<Vector2> culledUVs = new List<Vector2>();
        int[] offsets = new int[vertices.Length];
        int offset = 0;
        for (int i = 0; i < vertices.Length; ++i)
        {
            if (vertexInUse[i])
            {
                offsets[i] = offset;
                culledVertices.Add(vertices[i]);
                culledUVs.Add(UVs[i]);
            }
            else
            {
                offsets[i] = -1;
                offset++;
            }
        }

        for (int i = 0; i < culledTriangles.Count; ++i)
        {
            culledTriangles[i] -= offsets[culledTriangles[i]];
        }

        mesh.Clear();

        mesh.vertices = culledVertices.ToArray();
        mesh.triangles = culledTriangles.ToArray();
        mesh.SetUVs(0, culledUVs);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    private int CountMeridians(float angle)
    {
        int minimum = Mathf.RoundToInt(Mathf.Pow(2, minimumFactor));
        int maximum = Mathf.RoundToInt(Mathf.Pow(2, maximumFactor));

        float factor = Mathf.Lerp(minimum, maximum, Mathf.Pow(Mathf.Cos(angle * Mathf.Deg2Rad), 2));

        int result = minimum;
        while (result < factor)
        {
            result *= 2;
        }

        return result;
    }

    private Vector2 Point2LngLat(Vector3 point, Vector3 lngPlaneNormal, Vector3 lngZero)
    {
        Vector3 lngProjectedVec = Vector3.ProjectOnPlane(point, lngPlaneNormal);
        float lng = -1 * Vector3.SignedAngle(lngZero, lngProjectedVec, lngPlaneNormal);
        float lat = 90f - Vector3.Angle(point, lngPlaneNormal);
        return new Vector2(lng, lat);
    }

    private Vector2 LngLat2UVs(Vector2 lngLat)
    {
        float lng = (lngLat.x + 180f) / 360f;
        float lat = (lngLat.y + 90f) / 180f;
        return new Vector2(lng, lat);
    }
}
