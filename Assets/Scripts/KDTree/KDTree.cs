using System.Collections.Generic;
using UnityEngine;

public class KDTree
{
    private KDTreeNode root;

    public void Build(List<Boid> boids)
    {
        root = BuildTree(boids, 0);
    }

    private KDTreeNode BuildTree(List<Boid> boids, int depth)
    {
        // recursive

        if(boids.Count == 0) return null;

        int axis = depth % 3; // 0 = x-axis, 1 = y-axis, 2 = z-axis

        boids.Sort((a, b) => CompareByAxis(a, b, axis));

        int medianIndex = boids.Count / 2;
        KDTreeNode node = new KDTreeNode(boids[medianIndex]);

        node.left = BuildTree(boids.GetRange(0, medianIndex), depth + 1);
        node.right = BuildTree(boids.GetRange(medianIndex + 1, boids.Count - medianIndex - 1), depth + 1);

        return node;
    }

    private int CompareByAxis(Boid a, Boid b, int axis)
    {
        if (axis == 0)
            return a.position.x.CompareTo(b.position.x);
        if (axis == 1)
            return a.position.y.CompareTo(b.position.y);
        return a.position.z.CompareTo(b.position.z);
    }

    public List<Boid> Query(Vector3 position, float radius, float fov)
    {
        List<Boid> result = new List<Boid>();

        // start recursive search
        // O(log n + k), where k is number of neighbors found
        QueryRecursive(root, position, radius, fov, 0, result);
        return result;
    }

    private void QueryRecursive(KDTreeNode node, Vector3 position, float radius, float fov, int depth, List<Boid> result)
    {
        // base case if current node is null
        if (node == null) return;

        // check if boid at this node is within search radius
        if (Vector3.Distance(node.boid.position, position) <= radius)
        {
            // compute direction and angle from query position to boid position
            Vector3 directionToBoid = (node.boid.position - position).normalized;
            float angle = Vector3.Angle(directionToBoid, Vector3.forward);

            // check if in field of view
            if (angle <= fov / 2)
                result.Add(node.boid);
        }

        // determine which axis to use for the current depth of the KD-tree
        // then compute distance distance from the query position to the splitting plane of the current node
        int axis = depth % 3;   
        float diff = axis == 0 ? position.x - node.boid.position.x :
                          axis == 1 ? position.y - node.boid.position.y :
                                      position.z - node.boid.position.z;

        // choose near and far nodes based on relative position to splitting plane
        KDTreeNode nearNode = diff < 0 ? node.left : node.right;
        KDTreeNode farNode = diff < 0 ? node.right : node.left;

        // search near node
        QueryRecursive(nearNode, position, radius, fov, depth + 1, result);

        // also search far if distance to splitting plane is within radius
        if (Mathf.Abs(diff) <= radius)
        {
            QueryRecursive(farNode, position, radius, fov, depth + 1, result);
        }
    }
}
