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
    public CameraRotationInterpolator rotationInterpolator;

    [Range(1, 10)]
    public float correctionFactor = 5;
    public bool zoomFollowCursor = true;

    public Vector2 boundCenter = new Vector2(-46, -15);
    public Vector2 boundSize = new Vector2(70, 50);
    private Bounds bounds;

    private Vector3 dragPoint;
    private bool isDragging = false;

    private Vector3 targetPoint;
    private bool targetExists;

    private Vector2 latestScreenPosition;
    private Quaternion cameraRotationFromCenter;

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

        rotationInterpolator.RotateTo(lngLat, 0);
        CalculateTargetRaycast();
    }

    private void CalculateTargetRaycast()
    {

        RaycastHit raycastHit;
        Ray lastestRay = cam.ScreenPointToRay(latestScreenPosition);

        if (targetExists = terrainCollider.Raycast(lastestRay, out raycastHit, 5000))
            targetPoint = raycastHit.point;

        if (isDragging) CalculateDrag();
    }

    private void CalculateDrag()
    {
        Quaternion rotationCorrection = Quaternion.Inverse(lightBoxAngleCorrectionTransform.rotation);
        Quaternion dragPointRotation = Quaternion.FromToRotation(targetPoint - universalTransform.position, dragPoint - universalTransform.position);

        Vector2 lngLat = ClampLngLat(Vector3ToLngLat(rotationCorrection * (dragPointRotation * (cameraRotationFromCenter * Vector3.forward)) * transform.position.magnitude));

        rotationInterpolator.RotateTo(lngLat, 0.1f);
    }

    public void OnMouseMovement(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            latestScreenPosition = context.action.ReadValue<Vector2>();
            CalculateTargetRaycast();
        }
    }

    public void OnMouseDrag(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (targetExists)
            {
                dragPoint = targetPoint;
                isDragging = true;
                cameraRotationFromCenter = Quaternion.FromToRotation(Vector3.forward, transform.position);
            }
        } if (context.canceled)
        {
            if (isDragging)
            {
                isDragging = false;
            }
        }
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
}
