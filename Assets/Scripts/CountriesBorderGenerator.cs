using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CountriesBorderGenerator : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform lightBoxAngleCorrectionTransform;
    public TextAsset geojson;
    public float borderDistanceThreshold = 0.1f;

    public void LoadCountriesBorders()
    {
        var data = JObject.Parse(geojson.text);

        Dictionary<string, List<Vector2>> borders = new Dictionary<string, List<Vector2>>();
        Dictionary<string, List<List<Vector2>>> multiBorders = new Dictionary<string, List<List<Vector2>>>();

        foreach (JObject country in data["features"])
            foreach (JArray c1 in country["geometry"]["coordinates"])
            {
                string countryName = (string)country["properties"]["NAME_PT"];
                switch ((string)country["geometry"]["type"])
                {
                    case "Polygon":
                        Vector2 lastLngLat1 = new Vector2((float)c1[0][0], (float)c1[0][1]);

                        foreach (JArray c2 in c1)
                        {
                            if (!borders.ContainsKey(countryName))
                                borders.Add(countryName, new List<Vector2>());

                            Vector2 newLngLat = new Vector2((float)c2[0], (float)c2[1]);
                            if (Vector2.Distance(newLngLat, lastLngLat1) > borderDistanceThreshold)
                            {
                                lastLngLat1 = newLngLat;
                                borders[countryName].Add(newLngLat);
                            }
                        }
                        break;
                    case "MultiPolygon":
                        foreach (JArray c2 in c1)
                        {
                            if (!multiBorders.ContainsKey(countryName))
                                multiBorders.Add(countryName, new List<List<Vector2>>());

                            List<Vector2> border = new List<Vector2>();
                            Vector2 lastLngLat2 = new Vector2((float)c2[0][0], (float)c2[0][1]);
                            foreach (JArray c3 in c2)
                            {
                                Vector2 newLngLat = new Vector2((float)c3[0], (float)c3[1]);
                                if (Vector2.Distance(newLngLat, lastLngLat2) > borderDistanceThreshold)
                                {
                                    lastLngLat2 = newLngLat;
                                    border.Add(newLngLat);
                                }
                            }
                            multiBorders[countryName].Add(border);
                        }
                        break;
                }
            }

        Vector3[] positions = new Vector3[0];
        Quaternion angleCorrection = lightBoxAngleCorrectionTransform.localRotation;

        foreach (List<Vector2> region in multiBorders["Brasil"])
        {
            positions = new Vector3[region.Count];

            for (int i = 0; i < region.Count; ++i)
                positions[i] = LngLatUtils.LngLatToVector3(region[i], 500f);

            break;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
}
