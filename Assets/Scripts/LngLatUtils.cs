using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LngLatUtils : MonoBehaviour
{
    public static Vector2 Vector3ToLngLat(Vector3 point)
    {
        float lng = -1 * Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
        float lat = -1 * Mathf.Atan2(-point.y, new Vector2(point.x, point.z).magnitude) * Mathf.Rad2Deg;

        return new Vector2(lng, lat);
    }

    public static Vector3 LngLatToVector3(Vector2 lngLat, float radius)
    {
        return new Vector3(
            radius * Mathf.Cos(lngLat[0] * Mathf.Deg2Rad) * Mathf.Cos(lngLat[1] * Mathf.Deg2Rad),
            radius * Mathf.Sin(lngLat[1] * Mathf.Deg2Rad),
            radius * Mathf.Sin(lngLat[0] * Mathf.Deg2Rad) * Mathf.Cos(lngLat[1] * Mathf.Deg2Rad)
        );
    }

    public static Vector2 Point2LngLat(Vector3 point, Vector3 lngPlaneNormal, Vector3 lngZero)
    {
        Vector3 lngProjectedVec = Vector3.ProjectOnPlane(point, lngPlaneNormal);
        float lng = -1 * Vector3.SignedAngle(lngZero, lngProjectedVec, lngPlaneNormal);
        float lat = 90f - Vector3.Angle(point, lngPlaneNormal);
        return new Vector2(lng, lat);
    }

    public static Vector2 LngLat2UVs(Vector2 lngLat)
    {
        float lng = (lngLat.x + 180f) / 360f;
        float lat = (lngLat.y + 90f) / 180f;
        return new Vector2(lng, lat);
    }
}
