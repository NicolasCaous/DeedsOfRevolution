using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BaseMapLoader : MonoBehaviour
{
    public TextAsset geojson;
    public Material material;
    public ComputeShader computeShader;
    public int width = 1024;
    public int height = 1024;
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

        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelHandle, "Result", tex);
        computeShader.SetInt("_Width", width);
        computeShader.SetInt("_Height", height);
        computeShader.SetBuffer(kernelHandle, "_Lines", buffer);
        computeShader.Dispatch(kernelHandle, (lines.Count / 256) + 1, 1, 1);

        buffer.Release();
        material.SetTexture("_MainTex", tex);
    }
}
