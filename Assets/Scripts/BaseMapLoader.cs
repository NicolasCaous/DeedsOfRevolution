using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BaseMapLoader : MonoBehaviour
{
    public Texture2D baseTex;
    public TextAsset geojson;
    public Material material;
    public ComputeShader computeShader;
    public int width = 1024;
    public int height = 1024;

    public GameObject prefab;
    public Transform prefabParent;
    [Range(1, 100)]
    public int parallels = 10;
    [Range(1, 100)]
    public int meridians = 10;
    public Vector2[] defaultPermissiveBounds;
    // Start is called before the first frame update
    void Start()
    {
        LoadData();
    }

    public void LoadData()
    {
        var data = JObject.Parse(geojson.text);

        List<Vector2> lines = new List<Vector2>();
        foreach (JObject country in data["features"])
            foreach (JArray c1 in country["geometry"]["coordinates"])
                foreach (JArray c2 in c1)
                    switch ((string) country["geometry"]["type"])
                    {
                        case "Polygon":
                            if ((float)c2[0] + float.Epsilon >= 180f
                             || (float)c2[0] - float.Epsilon <= -180f
                             || (float)c2[1] + float.Epsilon >= 90f
                             || (float)c2[1] - float.Epsilon <= -90f)
                                continue;
                            lines.Add(new Vector2((float)c2[0], (float)c2[1]));
                            break;
                        case "MultiPolygon":
                            foreach (JArray c3 in c2)
                            {
                                if ((float)c3[0] + float.Epsilon >= 180f
                                 || (float)c3[0] - float.Epsilon <= -180f
                                 || (float)c3[1] + float.Epsilon >= 90f
                                 || (float)c3[1] - float.Epsilon <= -90f)
                                    continue;
                                lines.Add(new Vector2((float)c3[0], (float)c3[1]));
                            }
                            break;
                    }

        ComputeBuffer buffer = new ComputeBuffer(lines.Count, sizeof(float) * 2);
        buffer.SetData(lines.ToArray());

        RenderTexture tex = new RenderTexture(width, height, 32);
        tex.enableRandomWrite = true;
        tex.Create();

        computeShader.SetInt("_Width", width);
        computeShader.SetInt("_Height", height);

        int kernelHandleCopyTexture = computeShader.FindKernel("CopyBaseTexture");
        computeShader.SetTexture(kernelHandleCopyTexture, "BaseTexture", baseTex);
        computeShader.SetTexture(kernelHandleCopyTexture, "Result", tex);
        computeShader.Dispatch(kernelHandleCopyTexture, (width / 16) + 1, (height / 16) + 1, 1);

        int kernelHandleMain = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelHandleMain, "Result", tex);
        computeShader.SetBuffer(kernelHandleMain, "_Lines", buffer);
        computeShader.Dispatch(kernelHandleMain, (lines.Count / 256) + 1, 1, 1);

        buffer.Release();
        material.SetTexture("_BaseMap", tex);
    }

    public void InstantiateSphereSegments()
    {
        NormalizedUVSphere sphereGenerator = GetComponent<NormalizedUVSphere>();
        Mesh mesh = sphereGenerator.GenerateMesh();

        GameObject meridiansParent = new GameObject($"Meridians");
        meridiansParent.transform.SetParent(prefabParent);
        meridiansParent.transform.localPosition = Vector3.zero;
        meridiansParent.transform.localEulerAngles = Vector3.zero;
        meridiansParent.transform.localScale = Vector3.one;

        for (float meridian = -180f; meridian < 180f; meridian += 360f/meridians)
        {
            GameObject meridianParent = new GameObject($"Meridian {meridian:F2}");
            meridianParent.transform.SetParent(meridiansParent.transform);
            meridianParent.transform.localPosition = Vector3.zero;
            meridianParent.transform.localEulerAngles = Vector3.zero;
            meridianParent.transform.localScale = Vector3.one;

            for (float parallel = -90f; parallel < 90f; parallel += 180f/parallels)
            {
                GameObject clone = Instantiate(prefab);
                clone.transform.SetParent(meridianParent.transform);
                clone.transform.localPosition = Vector3.zero;
                clone.transform.localEulerAngles = Vector3.zero;
                clone.transform.localScale = Vector3.one;

                List<Vector2> restrictiveBounds = new List<Vector2>();
                List<Vector2> permissiveBounds = defaultPermissiveBounds.ToList();

                restrictiveBounds.Add(new Vector2(0, meridian - 180f + (360f / meridians)));
                restrictiveBounds.Add(new Vector2(0, meridian));
                restrictiveBounds.Add(new Vector2(parallel -90f + (180f / parallels), meridian - 90f + (180f / meridians)));
                restrictiveBounds.Add(new Vector2(parallel + 90f, meridian - 90f + (180f / meridians)));

                clone.GetComponent<MeshFilter>().mesh = sphereGenerator.CullMesh(
                    new Mesh()
                    {
                        vertices = mesh.vertices,
                        triangles = mesh.triangles,
                        normals = mesh.normals,
                        tangents = mesh.tangents,
                        bounds = mesh.bounds,
                        uv = mesh.uv
                    },
                    restrictiveBounds,
                    permissiveBounds
                );
            }
        }
    }

}

