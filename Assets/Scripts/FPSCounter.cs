using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private Text fpsText;
    [SerializeField] private Text avgFpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private float accum = 0f; // FPS accumulated over interval
    private int frames = 0; // frames drawn over interval
    private float timeLeft; // time left for the current interval

    private float totalFps = 0f; // total FPS accumulated over time
    private int fpsSamples = 0; // number of FPS samples taken

    private void Start()
    {
        if (fpsText == null || avgFpsText == null)
        {
            Debug.LogError("FPSCounter: UI Text components not assigned!");
            enabled = false;
            return;
        }

        timeLeft = updateInterval;
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeLeft <= 0f)
        {
            float fps = accum / frames;
            fpsText.text = $"{fps:F1}";

            totalFps += fps;
            fpsSamples++;
            float avgFps = totalFps / fpsSamples;
            avgFpsText.text = $"{avgFps:F1}";

            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }
}