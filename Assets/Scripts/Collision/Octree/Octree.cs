using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface Octree
{
    /// <summary>
    /// Inserts a particle into the octree, descending its children as needed.
    /// </summary>
    /// <param name="particle"></param>
    public void Insert(Sphere particle);

    /// <summary>
    /// Does all necessary collision detection tests.
    /// </summary>
    public void ResolveCollisions();

    /// <summary>
    /// Removes all objects from the Octree.
    /// </summary>
    public void Clear();

    /// <summary>
    /// Creates a new Octree, properly creating children.
    /// </summary>
    /// <param name="pos">The position of this Octree</param>
    /// <param name="halfWidth">The width of this Octree node, from the center to one edge (only needs to be used to calculate children's positions)</param>
    /// <param name="depth">The number of levels beneath this one to create (i.e., depth = 1 means create one node with 8 children. depth = 0 means create only this node. depth = 2 means create one node with 8 children, each of which are Octree's with depth 1.</param>
    /// <returns>The newly created Octree</returns>
    public static Octree Create(Vector3 pos, float halfWidth = 1f, uint depth = 1)
    {
        if (depth == 0)
        {
            return new OctreeObjects();
        }
        else
        {
            halfWidth /= 2;
            Octree[] children = new Octree[8];

            for (int i = 0; i < 8; i++)
            {
                //Move Pos on X Axis
                pos += Vector3.right * ((i & 1) != 0 ? halfWidth : -halfWidth);
                
                //Move Pos on Y Axis
                pos += Vector3.up * ((i & 2) != 0 ? halfWidth : -halfWidth);
                
                //Move Pos on Z Axis
                pos += Vector3.forward * ((i & 4) != 0 ? halfWidth : -halfWidth);
                
                //Add Child to List
                children[i] = Create(pos, halfWidth, depth - 1);
            }

            return new OctreeNode(pos, children);
        }
    }
}

/// <summary>
/// An octree that holds 8 children, all of which are Octree's.
/// </summary>
public class OctreeNode : Octree
{
    public Vector3 position;
    public Octree[] children;

    public OctreeNode()
    {
        position = new Vector3(0, 0, 0);
        children = new Octree[8];
    }
    
    public OctreeNode(Vector3 position, Octree[] children)
    {
        this.position = position;
        this.children = children;
    }

    /// <summary>
    /// Inserts the given particle into the appropriate children. The particle
    /// may need to be inserted into more than one child.
    /// </summary>
    /// <param name="sphere">The bounding sphere of the particle to insert.</param>
    public void Insert(Sphere sphere)
    {
        Vector3 difference = position - sphere.Center;
        for (int i = 0; i < 8; i++)
        {
            if (difference.x < sphere.Radius)
            {
                children[i].Insert(sphere);
                continue;
            }

            if (difference.y < sphere.Radius)
            {
                children[i].Insert(sphere);
                continue;
            }

            if (difference.z < sphere.Radius)
            {
                children[i].Insert(sphere);
                continue;
            }
        }
    }

    /// <summary>
    /// Resolves collisions in all children, as only leaf nodes can hold particles.
    /// </summary>
    public void ResolveCollisions()
    {
        for (int i = 0; i < 8; i++)
        {
            children[i].ResolveCollisions();
        }
    }

    /// <summary>
    /// Removes all particles in each child.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < 8; i++)
        {
            children[i].Clear();
        }
    }
}

/// <summary>
/// An octree that holds only particles.
/// </summary>
public class OctreeObjects : Octree
{
    private List<Sphere> spheres;
    
    public ICollection<Sphere> Objects => spheres;

    public OctreeObjects()
    {
        spheres = new List<Sphere>();
    }

    /// <summary>
    /// Inserts the particle into this node. It will be compared with all other
    /// particles in this node in ResolveCollisions().
    /// </summary>
    /// <param name="particle">The particle to insert.</param>
    public void Insert(Sphere particle)
    {
        spheres.Add(particle);
    }

    /// <summary>
    /// Calls CollisionDetection.ApplyCollisionResolution() on every pair of
    /// spheres in this node.
    /// </summary>
    public void ResolveCollisions()
    {
    }

    /// <summary>
    /// Removes all objects from this node.
    /// </summary>
    public void Clear()
    {
        spheres.Clear();
    }
}
