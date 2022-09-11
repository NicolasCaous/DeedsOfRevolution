using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CountriesBorderGenerator : MonoBehaviour
{
    public Transform bordersParent;
    public GameObject borderPrefab;

    public Transform lightBoxAngleCorrectionTransform;
    public TextAsset geojson;
    public float simplifyThreshold = 0.5f;
    public int skipIfFewer = 10;

    public List<Region> allowedRegions;

    public void OnValidate()
    {
        DisableOutOfBounds(GameObject.Find("Borders Container"));
        DisableOutOfBounds(GameObject.Find("Multi-borders Container"));
    }

    public void DisableOutOfBounds(GameObject parent)
    {
        parent.SetActive(true);

        RegionDataContainer regionDataContainer = parent.GetComponent<RegionDataContainer>();
        if(regionDataContainer != null)
        {
            foreach(Region region in allowedRegions)
                if (regionDataContainer.region == region.name)
                {
                    if (!region.valid)
                    {
                        parent.SetActive(false);
                        return;
                    }

                    foreach(Region.SubRegion subRegion in region.subRegions)
                        if (regionDataContainer.subRegion == subRegion.name)
                        {
                            if (!subRegion.valid)
                            {
                                parent.SetActive(false);
                                return;
                            }

                            foreach(Region.SubRegion.Country country in subRegion.countries)
                            {
                                if(regionDataContainer.countryName == country.name)
                                {
                                    if(!country.valid)
                                    {
                                        parent.SetActive(false);
                                        return;
                                    }

                                    if (regionDataContainer.multiBordersIndex > -1)
                                        if (!country.multiBorders[regionDataContainer.multiBordersIndex])
                                        {
                                            parent.SetActive(false);
                                            return;
                                        }

                                    break;
                                }
                            }
                            break;
                        }
                    break;
                }
        }

        for (int i = 0; i < parent.transform.childCount; ++i)
        {
            DisableOutOfBounds(parent.transform.GetChild(i).gameObject);
        }
    }

    public void StartDestroyAndLoadBorders()
    {
        StartCoroutine(DestroyAndLoadBorders());
    }

    public void LoadCountriesBorders()
    {
        var data = JObject.Parse(geojson.text);

        SortedDictionary<string, BorderData> borders = new SortedDictionary<string, BorderData>();
        SortedDictionary<string, List<BorderData>> multiBorders = new SortedDictionary<string, List<BorderData>>();

        foreach (JObject country in data["features"])
            foreach (JArray c1 in country["geometry"]["coordinates"])
            {
                string countryName = (string) country["properties"]["NAME_PT"];
                switch ((string)country["geometry"]["type"])
                {
                    case "Polygon":
                        foreach (JArray c2 in c1)
                        {
                            if (!borders.ContainsKey(countryName))
                                borders.Add(countryName, new BorderData
                                    {
                                        CountryName = countryName,
                                        Region = (string)country["properties"]["REGION_UN"],
                                        SubRegion = (string)country["properties"]["SUBREGION"],
                                        LngLat = new List<Vector2>(),
                                        MultiBordersIndex = -1
                                    }
                                );

                            borders[countryName].LngLat.Add(new Vector2((float)c2[0], (float)c2[1]));
                        }
                        break;
                    case "MultiPolygon":
                        foreach (JArray c2 in c1)
                        {
                            if (!multiBorders.ContainsKey(countryName))
                                multiBorders.Add(countryName, new List<BorderData>());

                            BorderData border = new BorderData
                            {
                                CountryName = countryName,
                                Region = (string)country["properties"]["REGION_UN"],
                                SubRegion = (string)country["properties"]["SUBREGION"],
                                LngLat = new List<Vector2>()
                            };
                            foreach (JArray c3 in c2)
                                border.LngLat.Add(new Vector2((float)c3[0], (float)c3[1]));

                            multiBorders[countryName].Add(border);
                        }
                        break;
                }
            }

        GameObject bordersContainer = new GameObject("Borders Container");
        bordersContainer.transform.SetParent(bordersParent);
        bordersContainer.transform.localPosition = Vector3.zero;
        bordersContainer.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
        bordersContainer.transform.localScale = Vector3.one;

        GameObject multiBordersContainer = new GameObject("Multi-borders Container");
        multiBordersContainer.transform.SetParent(bordersParent);
        multiBordersContainer.transform.localPosition = Vector3.zero;
        multiBordersContainer.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
        multiBordersContainer.transform.localScale = Vector3.one;

        SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, SortedSet<int>>>> newRegions
            = new SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, SortedSet<int>>>>();

        foreach (KeyValuePair<string, List<BorderData>> entry in multiBorders)
        {
            int i = 0;
            for (int j=0; j<entry.Value.Count; ++j)
            {
                BorderData borderData = entry.Value[j];
                borderData.MultiBordersIndex = i;

                if (!newRegions.ContainsKey(borderData.Region))
                    newRegions.Add(borderData.Region, new SortedDictionary<string, SortedDictionary<string, SortedSet<int>>>());

                if (!newRegions[borderData.Region].ContainsKey(borderData.SubRegion))
                    newRegions[borderData.Region].Add(borderData.SubRegion, new SortedDictionary<string, SortedSet<int>>());

                if (!newRegions[borderData.Region][borderData.SubRegion].ContainsKey(borderData.CountryName))
                    newRegions[borderData.Region][borderData.SubRegion].Add(borderData.CountryName, new SortedSet<int>());

                if(CreateBorder(borderData, multiBordersContainer)) 
                {
                    newRegions[borderData.Region][borderData.SubRegion][borderData.CountryName].Add(borderData.MultiBordersIndex);
                    ++i;
                };
            }
        }

        foreach (KeyValuePair<string, BorderData> entry in borders)
        {
            if (!newRegions.ContainsKey(entry.Value.Region))
                newRegions.Add(entry.Value.Region, new SortedDictionary<string, SortedDictionary<string, SortedSet<int>>>());

            if (!newRegions[entry.Value.Region].ContainsKey(entry.Value.SubRegion))
                newRegions[entry.Value.Region].Add(entry.Value.SubRegion, new SortedDictionary<string, SortedSet<int>>());

            if (!newRegions[entry.Value.Region][entry.Value.SubRegion].ContainsKey(entry.Value.CountryName))
                newRegions[entry.Value.Region][entry.Value.SubRegion].Add(entry.Value.CountryName, new SortedSet<int>());

            CreateBorder(entry.Value, bordersContainer);
        }

        List<Region> newAllowedRegions = new List<Region>();
        int regionIndex = 0;
        foreach (KeyValuePair<string, SortedDictionary<string, SortedDictionary<string, SortedSet<int>>>> entry in newRegions)
        {
            Region region = new Region();
            try
            {
                region.valid = allowedRegions[regionIndex].valid;
            } catch
            {
                region.valid = true;
            }
            region.name = entry.Key;
            region.subRegions = new List<Region.SubRegion>();

            int subRegionIndex = 0;
            foreach (KeyValuePair<string, SortedDictionary<string, SortedSet<int>>> subEntry in entry.Value)
            {
                Region.SubRegion subRegion = new Region.SubRegion();
                try
                {
                    subRegion.valid = allowedRegions[regionIndex].subRegions[subRegionIndex].valid;
                } catch
                {
                    subRegion.valid = true;
                }
                subRegion.name = subEntry.Key;
                subRegion.countries = new List<Region.SubRegion.Country>();

                int countryIndex = 0;
                foreach (KeyValuePair<string, SortedSet<int>> countryEntry in subEntry.Value)
                {
                    Region.SubRegion.Country country = new Region.SubRegion.Country();
                    try
                    {
                        country.valid = allowedRegions[regionIndex].subRegions[subRegionIndex].countries[countryIndex].valid;
                    } catch
                    {
                        country.valid = true;
                    }
                    country.name = countryEntry.Key;
                    country.multiBorders = new List<bool>();

                    foreach (int i in countryEntry.Value)
                        try
                        {
                            country.multiBorders.Add(allowedRegions[regionIndex].subRegions[subRegionIndex].countries[countryIndex].multiBorders[i]); 
                        } catch
                        {
                            country.multiBorders.Add(true);
                        }

                    subRegion.countries.Add(country);
                    ++countryIndex;
                }

                region.subRegions.Add(subRegion);
                ++subRegionIndex;
            }

            newAllowedRegions.Add(region);
            ++regionIndex;
        }

        allowedRegions = newAllowedRegions;
        DisableOutOfBounds(GameObject.Find("Borders Container"));
        DisableOutOfBounds(GameObject.Find("Multi-borders Container"));
    }

    private bool CreateBorder(BorderData borderData, GameObject parent)
    {
        GameObject borderSet = GameObject.Find(borderData.CountryName + " Border Set");
        if (borderSet == null)
        {
            borderSet = new GameObject(borderData.CountryName + " Border Set");
            borderSet.transform.SetParent(parent.transform);
            borderSet.transform.localPosition = Vector3.zero;
            borderSet.transform.localEulerAngles = Vector3.zero;
            borderSet.transform.localScale = Vector3.one;
        }

        Vector3[] positions = new Vector3[borderData.LngLat.Count];
        GameObject clone = Instantiate(borderPrefab);
        clone.name = borderData.CountryName + " Border";
        if (borderData.MultiBordersIndex > -1)
            clone.name += " " + borderData.MultiBordersIndex;
        clone.transform.SetParent(borderSet.transform);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localEulerAngles = Vector3.zero;
        clone.transform.localScale = Vector3.one;

        RegionDataContainer regionDataContainer = clone.AddComponent<RegionDataContainer>();
        regionDataContainer.countryName = borderData.CountryName;
        regionDataContainer.region = borderData.Region;
        regionDataContainer.subRegion = borderData.SubRegion;
        regionDataContainer.multiBordersIndex = borderData.MultiBordersIndex;

        LineRenderer lineRenderer = clone.GetComponent<LineRenderer>();

        for (int i = 0; i < borderData.LngLat.Count; ++i)
            positions[i] = LngLatUtils.LngLatToVector3(borderData.LngLat[i], 500f);

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
        lineRenderer.Simplify(simplifyThreshold);

        if (lineRenderer.positionCount < skipIfFewer)
        {
            DestroyImmediate(clone);
            return false;
        }
        return true;
    }

    IEnumerator DestroyAndLoadBorders()
    {
        yield return null;
        DestroyImmediate(GameObject.Find("Borders Container"));
        DestroyImmediate(GameObject.Find("Multi-borders Container"));
        LoadCountriesBorders();
    }

    [System.Serializable]
    public class Region
    {
        public bool valid;
        public string name;

        [System.Serializable]
        public class SubRegion
        {
            public bool valid;
            public string name;

            [System.Serializable]
            public class Country
            {
                public bool valid;
                public string name;

                public List<bool> multiBorders;
            }

            public List<Country> countries;
        }

        public List<SubRegion> subRegions;
    }

    public struct BorderData
    {
        public string CountryName;
        public string Region;
        public string SubRegion;
        public List<Vector2> LngLat;
        public int MultiBordersIndex;
    };
}