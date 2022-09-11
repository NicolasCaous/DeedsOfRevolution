using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public int lastFrames = 30;

    private float fps = 0f;
    private float meanFps = 0f;

    private float[] lastFPSs;
    private int count = 0;

    private void Start()
    {
        lastFPSs = new float[lastFrames];
    }

    void Update()
    {
        fps = 1f / Time.deltaTime;

        lastFPSs[count++] = fps;
        if (count == lastFrames) count = 0;

        meanFps = 0f;
        for (int i = 0; i < lastFrames; i++) meanFps += lastFPSs[i];
        meanFps /= lastFrames;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 50), "FPS: ");
        GUI.Label(new Rect(100, 0, 100, 50), fps.ToString("0.00"));
        GUI.Label(new Rect(0, 15, 100, 50), "mean-" + lastFrames.ToString() + " FPS:");
        GUI.Label(new Rect(100, 15, 100, 50), meanFps.ToString("0.00"));
    }
}
