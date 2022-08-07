using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    public Camera cam;

    public float scrollFactor = 0.04f;

    [Range(0, 1)]
    public float value = 0.5f;
    [Range(0, 1)]
    public float target = 0.5f;

    public float lerpMS = 250f;
    private float lerpTo;
    private float elapsed = 0f;

    public AnimationCurve heightCurve;
    public float maxHeight = 20f;
    public float minHeight = 1f;

    public AnimationCurve angleCurve;
    public float maxAngle = 85f;
    public float minAngle = 70f;
    // Start is called before the first frame update
    void Start()
    {
        lerpTo = target;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed > 1f/(1000f / lerpMS)) elapsed = 1f/(1000f / lerpMS);

        float oldTarget = target;
        target += -1 * scrollFactor * Input.mouseScrollDelta.y;
        target = Mathf.Clamp(target, 0, 1);
        bool hasMoved = oldTarget != target;

        if (hasMoved)
        {
            lerpTo = target;
            elapsed = 0f;
        }

        value = Mathf.Lerp(value, lerpTo, elapsed * (1000f / lerpMS));

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
    }
}
