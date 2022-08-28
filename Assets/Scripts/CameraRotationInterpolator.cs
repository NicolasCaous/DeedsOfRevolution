using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationInterpolator : MonoBehaviour
{
    public Transform longitudeTransform;
    public Transform latitudeTransform;
    public AnimationCurve curve;

    private Vector2 fromLngLat, toLngLat;
    private float t = 1, deltaFactor;

    // Update is called once per frame
    void Update()
    {
        if (t < 1)
        {
            t += Time.deltaTime * deltaFactor;

            float lng = Mathf.LerpAngle(fromLngLat.x, toLngLat.x, curve.Evaluate(t));
            float lat = Mathf.Lerp(fromLngLat.y, toLngLat.y, curve.Evaluate(t));

            longitudeTransform.localRotation = Quaternion.Euler(0, 0, lng);
            latitudeTransform.localRotation = Quaternion.Euler(lat, 0, 0);
        }
    }

    public void RotateTo(Vector2 lngLat, float seconds)
    {
        if (seconds == 0)
        {
            longitudeTransform.localRotation = Quaternion.Euler(0, 0, lngLat.x);
            latitudeTransform.localRotation = Quaternion.Euler(lngLat.y, 0, 0);
            t = 1;
            return;
        }

        float rawLng = longitudeTransform.localRotation.eulerAngles.z;
        float rawLat = latitudeTransform.localRotation.eulerAngles.x;
        fromLngLat = new Vector2(
             rawLng > 180 ? rawLng - 360 : rawLng,
             rawLat > 180 ? rawLat - 360 : rawLat
        );
        toLngLat = lngLat;
        deltaFactor = 1 / seconds;
        t = 0;
    }
}
