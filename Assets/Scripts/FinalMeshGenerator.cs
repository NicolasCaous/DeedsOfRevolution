using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinalMeshGenerator : MonoBehaviour
{
    public Texture2D baseTex;
    public Texture2D alphaTex;

    public GameObject landPrefab;
    public GameObject waterPrefab;
    public Transform prefabParent;
    [Range(1, 100)]
    public int sectionParallels = 10;
    [Range(1, 100)]
    public int sectionMeridians = 10;
    public Vector2[] defaultPermissiveBounds;

    public Texture2D heightMap;
    [Range(0f, 1f)]
    public float heightScaler = 0.1f;
    [Range(3, 400)]
    public int parallels = 100;
    [Range(2, 8)]
    public int minimumFactor = 5;
    [Range(3, 10)]
    public int maximumFactor = 10;

    public bool liveEdit = false;
    public bool canUpdateSpheres = true;

    void OnValidate()
    {
        if (liveEdit)
            StartDestroyAndInstantiateSphereSegments(true);
    }

    public void StartDestroyAndInstantiateSphereSegments(bool overrideValues)
    {
        if (canUpdateSpheres)
        {
            canUpdateSpheres = false;
            StartCoroutine(DestroyAndInstantiateSphereSegments(overrideValues));
        }
    }

    public void InstantiateSphereSegments()
    {
        Mesh mesh = NormalizedUVSphere.GenerateMesh(parallels, minimumFactor, maximumFactor, heightScaler, heightMap);
        Mesh roundMesh = NormalizedUVSphere.GenerateMesh(parallels, minimumFactor, maximumFactor, heightScaler, Texture2D.blackTexture);

        GameObject meridiansParent = new GameObject($"Meridians");
        meridiansParent.transform.SetParent(prefabParent);
        meridiansParent.transform.localPosition = Vector3.zero;
        meridiansParent.transform.localEulerAngles = Vector3.zero;
        meridiansParent.transform.localScale = Vector3.one;
        meridiansParent.isStatic = true;

        for (float meridian = -180f; meridian < 180f; meridian += 360f / sectionMeridians)
        {
            GameObject meridianParent = new GameObject($"Meridian {meridian:F2}");
            meridianParent.transform.SetParent(meridiansParent.transform);
            meridianParent.transform.localPosition = Vector3.zero;
            meridianParent.transform.localEulerAngles = Vector3.zero;
            meridianParent.transform.localScale = Vector3.one;
            meridianParent.isStatic = true;

            for (float parallel = -90f; parallel < 90f; parallel += 180f / sectionParallels)
            {
                List<Vector2> restrictiveBounds = new List<Vector2>();
                List<Vector2> permissiveBounds = defaultPermissiveBounds.ToList();

                restrictiveBounds.Add(new Vector2(0, meridian - 180f + (360f / sectionMeridians)));
                restrictiveBounds.Add(new Vector2(0, meridian));
                restrictiveBounds.Add(new Vector2(parallel - 90f + (180f / sectionParallels), meridian - 90f + (180f / sectionMeridians)));
                restrictiveBounds.Add(new Vector2(parallel + 90f, meridian - 90f + (180f / sectionMeridians)));

                // There is more water than land, so it is better to cull round mesh first
                Mesh newRoundMesh = CullMesh(roundMesh, restrictiveBounds, permissiveBounds);
                if (newRoundMesh == null) continue;

                bool hasWater = false;
                bool hasLand = false;
                for (int i = 0; i < newRoundMesh.uv.Length; i++)
                {
                    float u = newRoundMesh.uv[i].x;
                    float v = newRoundMesh.uv[i].y;
                    if (alphaTex.GetPixelBilinear(u, v).r > 0f)
                        hasWater = true;
                    else
                        hasLand = true;
                }

                if (hasWater)
                {
                    GameObject clone = Instantiate(waterPrefab);
                    clone.transform.SetParent(meridianParent.transform);
                    clone.transform.localPosition = Vector3.zero;
                    clone.transform.localEulerAngles = Vector3.zero;
                    clone.transform.localScale = Vector3.one;
                    clone.isStatic = true;

                    clone.GetComponent<MeshFilter>().mesh = newRoundMesh;
                }
                if (hasLand)
                {
                    GameObject clone = Instantiate(landPrefab);
                    clone.transform.SetParent(meridianParent.transform);
                    clone.transform.localPosition = Vector3.zero;
                    clone.transform.localEulerAngles = Vector3.zero;
                    clone.transform.localScale = Vector3.one;
                    clone.isStatic = true;

                    clone.GetComponent<MeshFilter>().mesh = CullMesh(mesh, restrictiveBounds, permissiveBounds);
                }
            }
        }
    }

    private Mesh CullMesh(Mesh mesh, List<Vector2> restrictiveBounds, List<Vector2> permissiveBounds)
    {
        return NormalizedUVSphere.CullMesh(
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

    IEnumerator DestroyAndInstantiateSphereSegments(bool overrideValues)
    {
        yield return null;
        DestroyImmediate(GameObject.Find("Meridians"));

        if (!overrideValues)
        {
            InstantiateSphereSegments();
        }
        else
        {
            int oldSectionParallels = sectionParallels;
            int oldSectionMeridians = sectionMeridians;
            int oldParallels = parallels;
            int oldMaximumFactor = maximumFactor;

            sectionParallels = 2;
            sectionMeridians = 2;
            parallels = 50;
            maximumFactor = 7;

            InstantiateSphereSegments();

            sectionParallels = oldSectionParallels;
            sectionMeridians = oldSectionMeridians;
            parallels = oldParallels;
            maximumFactor = oldMaximumFactor;
        }

        canUpdateSpheres = true;
    }
}

