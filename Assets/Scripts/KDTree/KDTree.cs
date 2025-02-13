using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class KDTree
{
    public KDTreeNode root;

    public void Build(List<Boid> boids)
    {
        Bounds initialBounds = new Bounds(Vector3.zero, Vector3.one * 30f);
        root = BuildTree(boids, 0, boids.Count, 0, initialBounds);
    }

    private KDTreeNode BuildTree(List<Boid> boids, int start, int end, int depth, Bounds pBounds)
    {
        if (start >= end) return null;

        int axis = depth % 3; // 0=x, 1=y, z=2
        int medianIndex = (start + end) / 2;

        // partition list around median
        Quickselect(boids, start, end - 1, medianIndex, axis);

        Boid medianBoid = boids[medianIndex];

        // split parent bounds around current axis
        Bounds leftBounds = pBounds;
        Bounds rightBounds = pBounds;

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

        // build left and right subtrees, recursive
        node.left = BuildTree(boids, start, medianIndex, depth + 1, leftBounds);
        node.right = BuildTree(boids, medianIndex + 1, end, depth + 1, rightBounds);

        return node;

    }

    private void Quickselect(List<Boid> boids, int left, int right, int k, int axis)
    {
        while (left < right)
        {
            int pivotIndex = Partition(boids, left, right, axis);
            if (pivotIndex == k) return;
            if (pivotIndex < k) left = pivotIndex + 1;
            else right = pivotIndex - 1;
        }
    }

    private int Partition(List<Boid> boids, int left, int right, int axis)
    {
        Boid pivot = boids[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (CompareByAxis(boids[j], pivot, axis) <= 0)
            {
                i++;
                Swap(boids, i, j);
            }
        }

        Swap(boids, i + 1, right);
        return i + 1;
    }

    private void Swap(List<Boid> boids, int i, int j)
    {
        Boid temp = boids[i];
        boids[i] = boids[j];
        boids[j] = temp;
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
        QueryRecursive(root, position, radius, fov, 0, result);
        return result;
    }

    private void QueryRecursive(KDTreeNode node, Vector3 position, float radius, float fov, int depth, List<Boid> result)
    {
        if (node == null) return;

        // check if node bounds intersect with search radius
        // used for pruning subtrees outside said radius
        if (!node.bounds.Intersects(new Bounds(position, Vector3.one * radius * 2)))
            return;

        // check if boid at this node is within the search radius
        float distanceSq = (node.boid.position - position).sqrMagnitude;
        if (distanceSq <= radius * radius)
        {
            // compute direction and angle from query position to boid position
            Vector3 directionToBoid = (node.boid.position - position).normalized;
            float angle = Vector3.Angle(directionToBoid, Vector3.forward);

            // check if in fov
            if (angle <= fov / 2) result.Add(node.boid);
        }

        // determine which axis to use for the current depth of the kd tree
        // x -> y -> z
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