using UnityEngine;

[CreateAssetMenu(fileName = "Configuration", menuName = "Configuration File", order = 0)]
public class Config : ScriptableObject
{
    [Header("Environment")]
    public float boundarySize;
    public float spawnRadius;

    //[Header("Octree")]
    //public int maxBoids;
    //public int maxDepth;

    [Header("Boids")]
    public int boidCount;
    //[Space(5)]
    public float maxVelocity;
    //public float maxAcceleration;
    public float boidViewRadius;
    public float boidViewFOV;
    public float boundsWeight;

    [Space(10)]
    public float wanderRadius;
    public float wanderJitter;
    public float wanderWeight;
    public bool wanderEnabled; // wander toggle

    [Space(5)]
    public float cohesionWeight;
    public bool cohesionEnabled;

    [Space(5)]
    public float alignmentWeight;
    public bool alignmentEnabled;

    [Space(5)]
    public float separationWeight;
    public bool separationEnabled;

    //[Space(10)]
    // add more here
}
