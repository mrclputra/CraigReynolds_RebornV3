using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private Material lineMaterial;

    private List<Boid> boids = new List<Boid>();
    private bool shouldDraw = false; // draw lines flag

    private void Awake()
    {
        // spawn in boids
        for(int i = 0; i < config.boidCount; i++)
        {
            GameObject obj = Instantiate(
                boidPrefab,
                Random.insideUnitSphere * (config.spawnRadius / 2f),
                Quaternion.identity
            );

            obj.gameObject.SetActive( true );
            boids.Add(obj.GetComponent<Boid>());
        }
    }

    private void FixedUpdate()
    {
        // parallel operations
        Parallel.ForEach(boids, boid =>
        {
            // update neighbor list
            boid.neighbors = GetNeighbors(boid);

            // compute acceleration values
            boid.Combine();
        });

        shouldDraw = true;
    }

    private List<Boid> GetNeighbors(Boid self)
    {
        // returns a list of boids of input boid based on config.viewradius
        // this implementation utilizes the boids array

        List<Boid> neighbors = new List<Boid> ();

        foreach(Boid boid in boids)
        {
            if(boid == self) continue; // skip self

            Vector3 offset = boid.position - self.position;
            float sqrDistance = offset.sqrMagnitude;

            // check if in view range
            if(sqrDistance <= config.boidViewRadius * config.boidViewRadius)
            {
                float angle = Vector3.Angle(boid.velocity, offset.normalized);

                // check if in field of view
                if(angle <= config.boidViewFOV / 2)
                    neighbors.Add(boid);
            }
        }

        return neighbors;
    }

    private void OnRenderObject()
    {
        // ensure all boid positions are updated
        if(!shouldDraw) return;

        foreach(Boid boid in boids)
        {
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            // draw velocities
            GL.Color(Color.gray);
            GL.Vertex(boid.transform.position);
            GL.Vertex(boid.transform.position + (boid.velocity / 2));

            // draw neighbor connections
            //GL.Color(Color.red);
            //foreach(Boid neighbor in boid.neighbors)
            //{
            //    GL.Vertex(boid.position);
            //    GL.Vertex(neighbor.position);
            //}

            GL.End();
            GL.PopMatrix();
        }
    }
}
