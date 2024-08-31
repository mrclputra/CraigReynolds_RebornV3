using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // simulation UI
    [SerializeField] private Toggle cohesionToggle;
    [SerializeField] private Toggle alignmentToggle;
    [SerializeField] private Toggle separationToggle;

    [SerializeField] private Toggle visualizeTreeToggle;
    [SerializeField] private Toggle visualizeBoundsToggle;
    [SerializeField] private Toggle visualizeFOVToggle;

    [SerializeField] private Slider boidCountSlider;
    [SerializeField] private Button resetButton;

    [SerializeField] private BoidManager boidManager;
    [SerializeField] private Config config;
    [SerializeField] private World world;

    private int lastBoidCount;

    private void Start()
    {
        // reset slider
        boidCountSlider.value = config.boidCount;
        lastBoidCount = config.boidCount;

        // disable on start
        cohesionToggle.isOn = config.cohesionEnabled; 
        alignmentToggle.isOn = config.alignmentEnabled;
        separationToggle.isOn = config.separationEnabled;

        visualizeBoundsToggle.isOn = world.drawBounds;
        visualizeFOVToggle.isOn = boidManager.drawFOV;

        boidCountSlider.onValueChanged.AddListener((v) => {
            lastBoidCount = Mathf.RoundToInt(v);
            boidManager.UpdateBoidCount(v);
        });

        resetButton.onClick.AddListener(() => {
            // reset boid positions
            boidManager.UpdateBoidCount(0);
            boidManager.UpdateBoidCount(lastBoidCount);
        });

        cohesionToggle.onValueChanged.AddListener(updateCohesion);
        alignmentToggle.onValueChanged.AddListener(updateAlignment);
        separationToggle.onValueChanged.AddListener(updateSeparation);

        visualizeBoundsToggle.onValueChanged.AddListener(updateBoundsDraw);
        visualizeFOVToggle.onValueChanged.AddListener(updateFOVDraw);
    }

    private void updateCohesion(bool value) { config.cohesionEnabled = value; }
    private void updateAlignment(bool value) { config.alignmentEnabled = value; }
    private void updateSeparation(bool value) {  config.separationEnabled = value;}
    private void updateBoundsDraw(bool value) { world.drawBounds = value; }
    private void updateFOVDraw(bool value) { boidManager.drawFOV = value; }
}
