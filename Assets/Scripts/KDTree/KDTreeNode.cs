using System.Collections.Generic;
using UnityEngine;

public class KDTreeNode
{
    public Boid boid;
    public KDTreeNode left;
    public KDTreeNode right;

    // constructor
    public KDTreeNode(Boid boid)
    {
        this.boid = boid;
        left = null;
        right = null;
    }
}
