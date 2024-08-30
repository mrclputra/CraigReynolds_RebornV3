using UnityEngine;

[CreateAssetMenu(fileName = "Configuration", menuName = "Configuration File", order = 0)]
public class Config : ScriptableObject
{
    [Header("Environment")]
    public float boundarySize;
    public float spawnRadius;

    [Header("Boids")]
    public float boidCount;
    //[Space(5)]
    public float maxVelocity;
    //public float maxAcceleration;
    public float boidViewRadius;
    public float boidViewFOV;

    [Space(10)]
    public float boundsWeight;

    [Space(10)]
    public float wanderRadius;
    public float wanderJitter;
    public float wanderWeight;
    public bool wanderEnabled; // wander toggle

    //[Space(10)]
    // add more here
}
