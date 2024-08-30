using UnityEngine;

[CreateAssetMenu(fileName = "Configuration", menuName = "Configuration File", order = 0)]
public class Config : ScriptableObject
{
    [Header("Environment")]
    public float boundarySize;
    public float spawnRadius;

    //[Header("Boids")]
}
