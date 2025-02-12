using UnityEngine;

public class KDTreeNode
{
    public Boid boid;
    public KDTreeNode left;
    public KDTreeNode right;
    public Bounds bounds;

    // constructor
    public KDTreeNode(Boid boid, Bounds bounds)
    {
        this.boid = boid;
        this.bounds = bounds;
        left = null;
        right = null;
    }
}