using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoomController : MonoBehaviour
{
    public Camera cam;
    public CameraRotationController rotationController;

    public float scrollFactor = 0.04f;

    [Range(0, 1)]
    public float value = 0.5f;
    [Range(0, 1)]
    public float target = 0.5f;

    public float lerpMS = 250f;
    private float elapsed = 0f;

    public AnimationCurve heightCurve;
    public float maxHeight = 20f;
    public float minHeight = 1f;

    public AnimationCurve angleCurve;
    public float maxAngle = 85f;
    public float minAngle = 70f;

    void Start()
    {
        UpdatePositionAndRotatiion();
    }

    void Update()
    {
        if (value == target) return;

        elapsed += Time.deltaTime;
        if (elapsed > 1f/(1000f / lerpMS)) elapsed = 1f/(1000f / lerpMS);

        value = Mathf.Lerp(value, target, elapsed * (1000f / lerpMS));

        UpdatePositionAndRotatiion();
    }

    private void UpdatePositionAndRotatiion()
    {
        Vector3 oldWorldPos = transform.position;
        Vector3 oldPos = transform.localPosition;
        transform.localPosition = new Vector3(
            oldPos.x,
            Mathf.Lerp(minHeight, maxHeight, heightCurve.Evaluate(value)),
            oldPos.z
        );

        transform.localRotation = Quaternion.Euler(
            Mathf.Lerp(minAngle, maxAngle, angleCurve.Evaluate(value)),
            0,
            0
        );

        rotationController.RotateToCursor(oldWorldPos, transform.localPosition.y / oldPos.y);
    }

    public void OnMouseScroll(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Vector2 scroll = context.action.ReadValue<Vector2>();

            target += -1 * scrollFactor * scroll.y;
            target = Mathf.Clamp(target, 0, 1);
            elapsed = 0f;
        }
    }
}
