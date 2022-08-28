using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotationController : MonoBehaviour
{
    public Camera cam;
    public SphereCollider terrainCollider;
    public Transform universalTransform;
    public Transform lightBoxAngleCorrectionTransform;
    public Transform longitudeTransform;
    public Transform latitudeTransform;

    [Range(1, 10)]
    public float correctionFactor = 5;
    public bool zoomFollowCursor = true;

    private Vector3 targetPoint;
    private bool targetExists;

    public Vector2 boundCenter = new Vector2(-46, -15);
    public Vector2 boundSize = new Vector2(70, 50);
    private Bounds bounds;

    void Start()
    {
        bounds = new Bounds(
            new Vector3(boundCenter.x, boundCenter.y, 0),
            new Vector3(boundSize.x, boundSize.y, 0)
        );
    }

    public void RotateToCursor(Vector3 oldCameraPos, float factor)
    {
        if (!targetExists || !zoomFollowCursor) return;

        Vector3 newCameraPos = ((oldCameraPos - targetPoint) * Mathf.Pow(factor, correctionFactor)) + targetPoint;
        Vector3 interpolatedTarget = newCameraPos - universalTransform.position;

        Vector2 lngLat = ClampLngLat(Vector3ToLngLat(Quaternion.Inverse(lightBoxAngleCorrectionTransform.rotation) * interpolatedTarget));

        longitudeTransform.localRotation = Quaternion.Euler(0, 0, lngLat.x);
        latitudeTransform.localRotation = Quaternion.Euler(lngLat.y, 0, 0);
    }

    private Vector2 ClampLngLat(Vector2 lngLat)
    {
        return bounds.ClosestPoint(new Vector3(lngLat.x, lngLat.y, 0));
    }

    private Vector2 Vector3ToLngLat(Vector3 point)
    {
        float lng = -1 * Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
        float lat = -1 * Mathf.Atan2(-point.y, new Vector2(point.x, point.z).magnitude) * Mathf.Rad2Deg;

        return new Vector2(lng, lat);
    }

    public void OnMouseMovement(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 position= context.action.ReadValue<Vector2>();

            RaycastHit raycast;
            if(targetExists = terrainCollider.Raycast(cam.ScreenPointToRay(position), out raycast, 5000))
                targetPoint = raycast.point;
        }
    }
}
