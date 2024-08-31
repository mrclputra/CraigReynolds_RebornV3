using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [SerializeField] Config config;
    public List<Boid> neighbors;

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration; // accessed in parallelization

    private Vector3 wanderTarget;

    // https://www.c-sharpcorner.com/UploadFile/pranayamr/random-number-in-multithreading/
    private static readonly ThreadLocal<System.Random> random = new ThreadLocal<System.Random>(() => new System.Random());
    // what the fuck lmao

    private void Start()
    {
        // on object instantiate
        velocity = Random.onUnitSphere * config.maxVelocity; // start with some random velocity
        acceleration = Vector3.zero;
        position = transform.position;

        wanderTarget = Vector3.zero;
    }

    private void Update()
    {
        // update velocity based on acceleration
        velocity += acceleration * Time.deltaTime * 3f; // adjust time modifier here
        velocity = Vector3.ClampMagnitude(velocity, config.maxVelocity);

        // update position based on velocity
        position += velocity * Time.deltaTime;
        transform.position = position;

        // update rotation to face velocity
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = targetRotation; // apply slerp?
        }
    }

    public void Combine()
    {
        Vector3 result = Vector3.zero;

        if (config.wanderEnabled)
            result += Wander() * config.wanderWeight;
        if(config.cohesionEnabled)
            result += Cohesion() * config.cohesionWeight;
        if (config.alignmentEnabled)
            result += Alignment() * config.alignmentWeight;
        if (config.separationEnabled)
            result += Separation() * config.separationWeight;
        result += AvoidBounds() * config.boundsWeight;

        acceleration = result.normalized;
        //acceleration = Vector3.ClampMagnitude(result.normalized, config.maxAcceleration); // if u want to limit acceleration (dont)
    }

    /*
    vector calculation functions
    */

    public Vector3 Cohesion()
    {
        // returns vector to average position of neighbors
        Vector3 result = Vector3.zero;

        if (neighbors == null)
            return result;

        foreach (Boid boid in neighbors)
            result += boid.position;

        result /= neighbors.Count;
        result -= position;
        return result.normalized;
    }

    public Vector3 Alignment()
    {
        // returns vector to average velocity of neighbors
        Vector3 result = Vector3.zero;

        if (neighbors == null)
            return result; // move this to main combine?

        foreach (Boid boid in neighbors)
            result += boid.velocity;

        result /= neighbors.Count;
        return result.normalized;
    }
    public Vector3 Separation()
    {
        // returns vector away from all neighbors
        Vector3 result = Vector3.zero;

        if (neighbors == null) 
            return result;

        foreach (Boid boid in neighbors)
        {
            Vector3 target = this.position - boid.position;
            if (target.magnitude > 0)
                result += target / target.sqrMagnitude; // scale the force by distance
        }

        return result.normalized;
    }

    public Vector3 Wander()
    {
        float x, y, z;
        lock (random) // acquire thread lock on rng
        {
            x = (float)(random.Value.NextDouble() * 2.0 - 1.0);
            y = (float)(random.Value.NextDouble() * 2.0 - 1.0);
            z = (float)(random.Value.NextDouble() * 2.0 - 1.0);
        }

        wanderTarget += new Vector3(x, y, z);
        wanderTarget = Vector3.ClampMagnitude(wanderTarget, config.wanderRadius);

        return wanderTarget.normalized;
    }

    //public Vector3 AvoidBounds()
    //{
    //    // cube implementation
    //    float edgeDistance = config.boundarySize * 0.3f;

    //    float xForce = position.x < -edgeDistance ? 1 : position.x > edgeDistance ? -1 : 0;
    //    float yForce = position.y < -edgeDistance ? 1 : position.y > edgeDistance ? -1 : 0;
    //    float zForce = position.z < -edgeDistance ? 1 : position.z > edgeDistance ? -1 : 0;

    //    float proximity = Mathf.Min(
    //        Mathf.Abs(edgeDistance - Mathf.Abs(position.x)),
    //        Mathf.Abs(edgeDistance - Mathf.Abs(position.y)),
    //        Mathf.Abs(edgeDistance - Mathf.Abs(position.z))
    //    );

    //    float proximityMultiplier = Mathf.Clamp01(1.0f - proximity / edgeDistance);
    //    float forceMultiplier = proximityMultiplier * 100f;

    //    return new Vector3(xForce * forceMultiplier, yForce * forceMultiplier, zForce * forceMultiplier);
    //}

    private Vector3 AvoidBounds()
    {
        // spherical implementation
        Vector3 avoidanceVector = Vector3.zero;
        float boundaryRadius = config.boundarySize * 0.6f; // adjust bound size multiplier here
        float edgeDistance = 5f;
        float edgeStrength = 12f;

        Vector3 directionToCenter = -position;
        float distanceFromCenter = directionToCenter.magnitude;

        if (distanceFromCenter > boundaryRadius - edgeDistance)
        {
            float forceMagnitude = edgeStrength * Mathf.Pow(edgeDistance - (boundaryRadius - distanceFromCenter), 2);
            avoidanceVector = directionToCenter.normalized * forceMagnitude;
        }

        return avoidanceVector;
    }
}
