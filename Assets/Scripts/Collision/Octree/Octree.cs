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
            
            /*
             * 
             * Bits count right to left
             * 
             * First bit represents the X axis
             * Second bit represents the Y axis
             * Third bit represents the Z axis
             * 
             * 0 = 000
             * 1 = 001
             * 2 = 010
             * 3 = 011
             * 4 = 100
             * 5 = 101
             * 6 = 110
             * 7 = 111
             * 
            */
            
            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = pos;
                
                //Move offset up on X axis if it has the first bit assigned, down if not
                offset += Vector3.right * ((i & 1) != 0 ? halfWidth : -halfWidth);
                
                //Move offset up on Y axis if it has the second bit assigned, down if not
                offset += Vector3.up * ((i & 2) != 0 ? halfWidth : -halfWidth);
                
                //Move offset up on the Z axis if it has the third bit assigned, down if not
                offset += Vector3.forward * ((i & 4) != 0 ? halfWidth : -halfWidth);
                
                //Add Child to List
                children[i] = Create(offset, halfWidth, depth - 1);
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
        /*
         *
         * Bits count right to left
         *
         * First bit represents the X axis
         * Second bit represents the Y axis
         * Third bit represents the Z axis
         *
         * 0 = 000
         * 1 = 001
         * 2 = 010
         * 3 = 011
         * 4 = 100
         * 5 = 101
         * 6 = 110
         * 7 = 111
         *
         */
        
        //Get the child that is absolutely holding the sphere
        int baseZone = 0;
        for (int i = 0; i < 3; i++)
        {
            if (sphere.Center[i] >= position[i])
            {
                //Shifts the binary number to the left in respect to i:
                //the result will either be 1, 2, 4
                baseZone += (1 << i);   
            }
        }
        
        //Add the sphere to the absolute child
        children[baseZone].Insert(sphere);

        //Get how far away the sphere is from the center of the octree
        Vector3 difference = sphere.Center - position;
        
        //Check the difference with the sphere radius, if the radius is larger,
        //then the sphere is bleeding into another child
        int offset = 0; //Offset is used to hold which children are occupied
        for (int i = 0; i < 3; i++)
        {
            if (sphere.Radius > difference[i])
            {
                offset += (1 << i);
            }
        }
        
        //If offset is less then one, then no other children contain this sphere
        if(offset < 1) return;
        
        //Cycle through all remaining children and see if the radius is bleeding into them using the calculated offset
        for (int i = 1; i < 8; i++)
        {
            //Compares the bits of offset and i to see if i is contained in offset.
            //If it is then that means baseZone - i is occupied.
            //If offset is 7 then all children are occupied.
            //If offset is 6 then the 4th and 2nd children are occupied
            //If offset is 4 then only the 4th child is occupied.
            if ((offset & i) == i) 
            {   
                children[baseZone - i].Insert(sphere);
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
        if(spheres.Count < 2) return;
        
        for (int i = 0; i < spheres.Count; i++)
        {
            var s1 = spheres[i];
            for (int j = i + 1; j < spheres.Count; j++)
            {
                var s2 = spheres[j];
                CollisionDetection.ApplyCollisionResolution(s1, s2);
            }
        }
    }

    /// <summary>
    /// Removes all objects from this node.
    /// </summary>
    public void Clear()
    {
        spheres.Clear();
    }
}
