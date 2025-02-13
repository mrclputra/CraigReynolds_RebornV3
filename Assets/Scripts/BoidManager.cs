using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private Material lineMaterial;

    private List<Boid> boids = new List<Boid>();

    // vector lines enabled flag, DONT TOUCH
    // ensures opengl calls aren't drawn until boid positions are updated
    private bool shouldDraw = false;

    public bool drawFOV = false; // draw FOV lines flag

    private void Awake()
    {
        // spawn in boids
        for (int i = 0; i < config.boidCount; i++)
        {
            Spawn(boidPrefab);
        }
    }

    private void Update()
    {
        // parallel operations here
        // make sure that no Unity API calls are made inside and during parallelization
        Parallel.ForEach(boids, boid =>
        {
            // update neighbor list
            boid.neighbors = GetNeighbors(boid);

            // compute acceleration values
            boid.Combine();
        });

        shouldDraw = true;
    }

    public void UpdateBoidCount(float value)
    {
        int targetCount = Mathf.RoundToInt(value);

        if (targetCount > boids.Count)
        {
            // increase number of boids
            int toSpawn = targetCount - boids.Count;
            for (int i = 0; i < toSpawn; i++)
            {
                Spawn(boidPrefab);
            }
        }
        else if (targetCount < boids.Count)
        {
            // reduce number of boids
            int toDelete = boids.Count - targetCount;
            for (int i = 0; i < toDelete; i++)
            {
                DeleteBoid(boids[boids.Count - 1]); // remove last boid in list
            }
        }
    }

    private void Spawn(GameObject boidPrefab)
    {
        GameObject obj = Instantiate(
                boidPrefab,
                Random.insideUnitSphere * (config.spawnRadius / 2f),
                Quaternion.identity
            );

        obj.gameObject.SetActive(true);
        boids.Add(obj.GetComponent<Boid>());
    }

    private void DeleteBoid(Boid boid)
    {
        if (boids.Remove(boid))
            Destroy(boid.gameObject);
    }

    private List<Boid> GetNeighbors(Boid self)
    {
        List<Boid> neighbors = new List<Boid>();
        float radiusSq = config.boidViewRadius * config.boidViewRadius;
        float cosFOV = Mathf.Cos(config.boidViewFOV * 0.5f * Mathf.Deg2Rad);

        // calculate self's forward direction from velocity
        Vector3 selfForward = self.velocity.normalized;

        foreach (Boid other in boids)
        {
            if (other == self) continue;

            Vector3 toOther = other.position - self.position;
            float distSq = toOther.sqrMagnitude;

            // check if within radius
            if (distSq > radiusSq) continue;

            // check if within FOV
            Vector3 dirToOther = toOther.normalized;
            float dot = Vector3.Dot(selfForward, dirToOther);
            if (dot < cosFOV) continue;

            neighbors.Add(other);
        }

        return neighbors;
    }

    private void OnRenderObject()
    {
        // ensure all boid positions are updated before drawing calls
        if (!shouldDraw) return;

        foreach (Boid boid in boids)
        {
            // begin GL API calls
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            // draw velocities
            GL.Color(new Color(0, 0, 0, 0.5f));
            GL.Vertex(boid.transform.position);
            GL.Vertex(boid.transform.position + (boid.velocity / 5f));

            // draw boid neighbor connections
            if (drawFOV)
            {
                GL.Color(new Color(1, 0, 0, 0.4f));
                foreach (Boid neighbor in boid.neighbors)
                {
                    GL.Vertex(boid.position);
                    GL.Vertex(neighbor.position);
                }
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}