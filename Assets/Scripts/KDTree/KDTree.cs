using System.Collections.Generic;
using UnityEngine;

public class KDTree
{
    public KDTreeNode root;

    public void Build(List<Boid> boids)
    {
        // TODO: link to spawnradius or world bounds through config
        Bounds initialBounds = new Bounds(Vector3.zero, Vector3.one * 10f * 2);
        root = BuildTree(boids, 0, initialBounds);
    }

    private KDTreeNode BuildTree(List<Boid> boids, int depth, Bounds pBounds)
    {
        // recursive
        if (boids.Count == 0) return null;

        int axis = depth % 3; // 0 = x-axis, 1 = y-axis, 2 = z-axis
        boids.Sort((a, b) => CompareByAxis(a, b, axis));

        int medianIndex = boids.Count / 2;
        Boid medianBoid = boids[medianIndex];

        // split parent bounds along current axis
        Bounds leftBounds = new Bounds(pBounds.center, pBounds.size);
        Bounds rightBounds = new Bounds(pBounds.center, pBounds.size);

        if (axis == 0) // x-axis
        {
            leftBounds.max = new Vector3(medianBoid.position.x, pBounds.max.y, pBounds.max.z);
            rightBounds.min = new Vector3(medianBoid.position.x, pBounds.min.y, pBounds.min.z);
        }
        else if (axis == 1) // y-axis
        {
            leftBounds.max = new Vector3(pBounds.max.x, medianBoid.position.y, pBounds.max.z);
            rightBounds.min = new Vector3(pBounds.min.x, medianBoid.position.y, pBounds.min.z);
        }
        else // z-axis
        {
            leftBounds.max = new Vector3(pBounds.max.x, pBounds.max.y, medianBoid.position.z);
            rightBounds.min = new Vector3(pBounds.min.x, pBounds.min.y, medianBoid.position.z);
        }

        // create the current node with median boid and its bounds
        KDTreeNode node = new KDTreeNode(medianBoid, pBounds);

        // build left and right subtrees
        node.left = BuildTree(boids.GetRange(0, medianIndex), depth + 1, leftBounds);
        node.right = BuildTree(boids.GetRange(medianIndex + 1, boids.Count - medianIndex - 1), depth + 1, rightBounds);

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

        // check if the node's bounds intersect with the search radius
        // skip this node if its bounds are outside the search radius
        if (!node.bounds.Intersects(new Bounds(position, Vector3.one * radius * 2)))
            return;

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
        // then compute distance from the query position to the splitting plane of the current node
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
