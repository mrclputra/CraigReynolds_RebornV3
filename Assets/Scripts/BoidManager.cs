using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private Material lineMaterial;

    private List<Boid> boids = new List<Boid>();
    private KDTree kdTree;

    private bool shouldDraw = false; // draw lines flag
    public bool drawFOV = false;

    private void Awake()
    {
        // spawn in boids
        for(int i = 0; i < config.boidCount; i++)
        {
            Spawn(boidPrefab);
        }

        kdTree = new KDTree();
        kdTree.Build(boids);
    }

    private void FixedUpdate()
    {
        // rebuild kdtree
        kdTree.Build(boids);

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

        if(targetCount > boids.Count)
        {
            // increase number of boids
            int toSpawn = targetCount - boids.Count;
            for (int i = 0; i < toSpawn; i++)
            {
                Spawn(boidPrefab);
            }
        }
        else if(targetCount < boids.Count)
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
        if(boids.Remove(boid))
            Destroy(boid.gameObject);
    }

    //private List<Boid> GetNeighbors(Boid self)
    //{
    //    // returns a list of boids of input boid based on config.viewradius
    //    // this implementation utilizes the boids array

    //    List<Boid> neighbors = new List<Boid> ();

    //    foreach(Boid boid in boids)
    //    {
    //        if(boid == self) continue; // skip self

    //        Vector3 offset = boid.position - self.position;
    //        float sqrDistance = offset.sqrMagnitude;

    //        // check if in view range
    //        if(sqrDistance <= config.boidViewRadius * config.boidViewRadius)
    //        {
    //            float angle = Vector3.Angle(boid.velocity, offset.normalized);

    //            // check if in field of view
    //            if(angle <= config.boidViewFOV / 2)
    //                neighbors.Add(boid);
    //        }
    //    }

    //    return neighbors;
    //}

    private List<Boid> GetNeighbors(Boid self)
    {
        float radius = config.boidViewRadius;
        float fov = config.boidViewFOV;
        return kdTree.Query(self.position, radius, fov);
    }

    private void OnRenderObject()
    {
        // ensure all boid positions are updated before drawings
        if(!shouldDraw) return;

        foreach(Boid boid in boids)
        {
            // begin GL API calls
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            // draw velocities
            GL.Color(new Color(0, 0, 0, 0.5f));
            GL.Vertex(boid.transform.position);
            GL.Vertex(boid.transform.position + (boid.velocity / 5f));

            //draw boid neighbor connections
            if (drawFOV)
            {
                GL.Color(new Color(1, 0, 0, 0.2f));
                foreach (Boid neighbor in boid.neighbors)
                {
                    GL.Vertex(boid.position);
                    GL.Vertex(neighbor.position);
                }
            }

            // draw kd-tree planes


            GL.End();
            GL.PopMatrix();
        }
    }
}
