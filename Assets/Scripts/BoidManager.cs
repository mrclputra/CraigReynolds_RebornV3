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

    public bool drawVisualizer = false; // draw kdtree flag
    public bool drawFOV = false;

    private void Awake()
    {
        // spawn in boids
        for (int i = 0; i < config.boidCount; i++)
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

        // hello world

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
        float radius = config.boidViewRadius;
        float fov = config.boidViewFOV;
        return kdTree.Query(self.position, radius, fov);
    }

    private void OnRenderObject()
    {
        // ensure all boid positions are updated before drawings
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

            GL.End();
            GL.PopMatrix();
        }

        if (!drawVisualizer) return;
        DrawKDTreePlanes(kdTree.root, 0);
    }

    private void DrawKDTreePlanes(KDTreeNode node, int depth)
    {
        if (node == null) return;

        int axis = depth % 3;
        DrawPlane(node.boid.position, axis, node.bounds);

        // recursively draw planes for left and right children
        DrawKDTreePlanes(node.left, depth + 1);
        DrawKDTreePlanes(node.right, depth + 1);
    }

    private void DrawPlane(Vector3 position, int axis, Bounds bounds)
    {
        // plane color
        GL.PushMatrix();
        GL.Begin(GL.QUADS);
        lineMaterial.SetPass(0);
        // GL.Color(new Color(0, 1, 0, 0.2f)); // TODO: to be changed

        // set color based on axis
        if (axis == 0) GL.Color(new Color(1, 0, 0, 0.2f));
        else if (axis == 1) GL.Color(new Color(0, 1, 0, 0.2f));
        else GL.Color(new Color(0, 0, 1, 0.2f));

        // draw planes
        if (axis == 0) // x-axis split
        {
            Vector3 topLeft = new Vector3(position.x, bounds.min.y, bounds.min.z);
            Vector3 topRight = new Vector3(position.x, bounds.min.y, bounds.max.z);
            Vector3 bottomLeft = new Vector3(position.x, bounds.max.y, bounds.min.z);
            Vector3 bottomRight = new Vector3(position.x, bounds.max.y, bounds.max.z);

            GL.Vertex(topLeft);
            GL.Vertex(topRight);
            GL.Vertex(bottomRight);
            GL.Vertex(bottomLeft);
        }
        else if (axis == 1) // y-axis split
        {
            Vector3 topLeft = new Vector3(bounds.min.x, position.y, bounds.min.z);
            Vector3 topRight = new Vector3(bounds.max.x, position.y, bounds.min.z);
            Vector3 bottomLeft = new Vector3(bounds.min.x, position.y, bounds.max.z);
            Vector3 bottomRight = new Vector3(bounds.max.x, position.y, bounds.max.z);

            GL.Vertex(topLeft);
            GL.Vertex(topRight);
            GL.Vertex(bottomRight);
            GL.Vertex(bottomLeft);
        }
        else // z-axis split
        {
            Vector3 topLeft = new Vector3(bounds.min.x, bounds.min.y, position.z);
            Vector3 topRight = new Vector3(bounds.max.x, bounds.min.y, position.z);
            Vector3 bottomLeft = new Vector3(bounds.min.x, bounds.max.y, position.z);
            Vector3 bottomRight = new Vector3(bounds.max.x, bounds.max.y, position.z);

            GL.Vertex(topLeft);
            GL.Vertex(topRight);
            GL.Vertex(bottomRight);
            GL.Vertex(bottomLeft);
        }

        GL.End();

        // draw lines
        GL.Begin(GL.LINES);
        lineMaterial.SetPass(0);

        GL.Color(new Color(0, 0, 0, 1));

        if (axis == 0) // x-axis split
        {
            Vector3 topLeft = new Vector3(position.x, bounds.min.y, bounds.min.z);
            Vector3 topRight = new Vector3(position.x, bounds.min.y, bounds.max.z);
            Vector3 bottomLeft = new Vector3(position.x, bounds.max.y, bounds.min.z);
            Vector3 bottomRight = new Vector3(position.x, bounds.max.y, bounds.max.z);

            GL.Vertex(topLeft); GL.Vertex(topRight);
            GL.Vertex(topRight); GL.Vertex(bottomRight);
            GL.Vertex(bottomRight); GL.Vertex(bottomLeft);
            GL.Vertex(bottomLeft); GL.Vertex(topLeft);
        }
        else if (axis == 1) // y-axis split
        {
            Vector3 topLeft = new Vector3(bounds.min.x, position.y, bounds.min.z);
            Vector3 topRight = new Vector3(bounds.max.x, position.y, bounds.min.z);
            Vector3 bottomLeft = new Vector3(bounds.min.x, position.y, bounds.max.z);
            Vector3 bottomRight = new Vector3(bounds.max.x, position.y, bounds.max.z);

            GL.Vertex(topLeft); GL.Vertex(topRight);
            GL.Vertex(topRight); GL.Vertex(bottomRight);
            GL.Vertex(bottomRight); GL.Vertex(bottomLeft);
            GL.Vertex(bottomLeft); GL.Vertex(topLeft);
        }
        else // z-axis split
        {
            Vector3 topLeft = new Vector3(bounds.min.x, bounds.min.y, position.z);
            Vector3 topRight = new Vector3(bounds.max.x, bounds.min.y, position.z);
            Vector3 bottomLeft = new Vector3(bounds.min.x, bounds.max.y, position.z);
            Vector3 bottomRight = new Vector3(bounds.max.x, bounds.max.y, position.z);

            GL.Vertex(topLeft); GL.Vertex(topRight);
            GL.Vertex(topRight); GL.Vertex(bottomRight);
            GL.Vertex(bottomRight); GL.Vertex(bottomLeft);
            GL.Vertex(bottomLeft); GL.Vertex(topLeft);
        }

        GL.End();
        GL.PopMatrix();
    }
}
