using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotationController : MonoBehaviour
{
    public Camera cam;
    public SphereCollider terrainCollider;
    public Transform lightBoxAngleCorrectionTransform;
    public Transform universalTransform;
    public Transform terrainContainerTransform;
    public CameraRotationInterpolator rotationInterpolator;

    [Range(1, 10)]
    public float correctionFactor = 5;
    public bool zoomFollowCursor = true;

    [Range(0, 1)]
    public float dragMomentumDecay = 0.98f;
    [Range(0, 100)]
    public float dragMomentumDecayThreshold = 0.01f;
    [Range(0, 1)]
    public float dragMomentumIgnoreLastSeconds = 0.01f;
    [Range(0, 1)]
    public float dragMomentumTimeDelta = 0.25f;
    [Range(0, 10)]
    public float dragMomentumTimeImunity = 0.2f;

    public Vector2 boundCenter = new Vector2(-46, -15);
    public Vector2 boundSize = new Vector2(70, 50);
    private Bounds bounds;

    private Vector2 latestScreenPosition;
    private Vector3 targetPoint;
    private bool targetExists;
    private Vector2 lastLngLat;

    private Vector2 dragLngLat;
    private Vector2 dragMomentum;
    private double dragMomentumStart;
    private bool isDragging = false;
    private LngLatRecorder dragHistory = new LngLatRecorder(10000);

    void Start()
    {
        bounds = new Bounds(
            new Vector3(boundCenter.x, boundCenter.y, 0),
            new Vector3(boundSize.x, boundSize.y, 0)
        );
    }

    void Update()
    {
        CalculateTargetRaycast();
        if ((!isDragging) && dragMomentum.sqrMagnitude > 0)
        {
            lastLngLat += dragMomentum * Time.deltaTime;
            dragMomentum = dragMomentumDecay * dragMomentum;

            if (dragMomentum.sqrMagnitude < dragMomentumDecayThreshold)
                dragMomentum = Vector2.zero;

            lastLngLat = ClampLngLat(lastLngLat);
            rotationInterpolator.RotateTo(lastLngLat, 0.1f);
        }
    }

    public void StopDragMomentum()
    {
        if (zoomFollowCursor)
            dragMomentum = Vector2.zero;
    }

    public void RotateToCursor(Vector3 oldCameraPos, float factor)
    {
        if (!targetExists || !zoomFollowCursor) return;
        if (Time.realtimeSinceStartupAsDouble - dragMomentumStart < dragMomentumTimeImunity) return;

        Vector3 newCameraPos = ((oldCameraPos - targetPoint) * Mathf.Pow(factor, correctionFactor)) + targetPoint;
        Vector3 interpolatedTarget = newCameraPos - universalTransform.position;

        lastLngLat = ClampLngLat(Vector3ToLngLat(Quaternion.Inverse(lightBoxAngleCorrectionTransform.rotation) * interpolatedTarget));
        rotationInterpolator.RotateTo(lastLngLat, 0f);
    }

    private void CalculateTargetRaycast()
    {

        RaycastHit raycastHit;
        Ray lastestRay = cam.ScreenPointToRay(latestScreenPosition);

        if (targetExists = terrainCollider.Raycast(lastestRay, out raycastHit, 5000))
            targetPoint = raycastHit.point;

        if (isDragging) CalculateDrag();
    }

    private Vector3 CorrectPointToLngLat(Vector3 value)
    {
        return Quaternion.Inverse(lightBoxAngleCorrectionTransform.rotation) * value;
    }

    private void CalculateDrag()
    {
        Vector2 deltaLngLat = dragLngLat - Vector3ToLngLat(CorrectPointToLngLat(targetPoint));
        Vector2 camLngLat = Vector3ToLngLat(
            CorrectPointToLngLat((cam.gameObject.transform.position - universalTransform.position).normalized)
          * terrainContainerTransform.localScale.magnitude
        );
        lastLngLat = ClampLngLat(camLngLat + deltaLngLat);
        rotationInterpolator.RotateTo(lastLngLat, 0.1f);
        dragHistory.Record(lastLngLat);
    }

    private void CalculateFinalMomentum()
    {
        List<LngLatRecorder.Frame> frames = dragHistory.Rewind(dragMomentumTimeDelta, dragMomentumIgnoreLastSeconds);

        dragMomentum = Vector2.zero;
        if (frames.Count == 0) return;

        dragMomentum = (frames[frames.Count - 1].lngLat - frames[0].lngLat) / (float)(frames[frames.Count - 1].time - frames[0].time);
    }

    public void OnMouseMovement(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            latestScreenPosition = context.action.ReadValue<Vector2>();
        }
    }

    public void OnMouseDrag(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (targetExists)
            {
                dragMomentum = Vector2.zero;
                dragLngLat = Vector3ToLngLat(CorrectPointToLngLat(targetPoint));
                isDragging = true;
            }
        }
        if (context.canceled)
        {
            if (isDragging)
            {
                CalculateFinalMomentum();
                dragHistory.Clear();
                dragMomentumStart = Time.realtimeSinceStartupAsDouble;
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

    private class LngLatRecorder
    {
        public struct Frame
        {
            public Vector2 lngLat;
            public double time;
        }

        Frame[] frames;
        int index;

        public LngLatRecorder(int size)
        {
            frames = new Frame[size];
            index = 0;
        }

        public void Record(Vector2 lngLat)
        {
            frames[index] = new Frame() { lngLat = lngLat, time = Time.realtimeSinceStartupAsDouble };
            if (++index == frames.Length) index = 0;
        }

        public List<Frame> Rewind(double delta, double ignoreFirst)
        {
            double ignoreThreshold = Time.realtimeSinceStartupAsDouble - ignoreFirst;
            double deltaThreshold = ignoreThreshold - delta;

            List<Frame> output = new List<Frame>();
            int i = index - 1;
            if (i < 0) i = frames.Length - 1;

            while (frames[i].time > ignoreThreshold)
            {
                if (--i < 0) i = frames.Length - 1;
                if (i == ((index + 1 == frames.Length) ? 0 : (index + 1))) break;
            }

            while (frames[i].time > deltaThreshold)
            {
                output.Add(frames[i]);
                if (--i < 0) i = frames.Length - 1;
                if (i == ((index + 1 == frames.Length) ? 0 : (index + 1)))
                {
                    break;
                }
            }

            return output;
        }

        public void Clear()
        {
            System.Array.Clear(frames, 0, frames.Length);
            index = 0;
        }
    }
}
