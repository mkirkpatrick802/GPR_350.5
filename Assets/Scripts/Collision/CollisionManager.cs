using System;
using System.Collections;
using System.Collections.Generic;
using static CollisionDetection;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollisionManager : MonoBehaviour
{
    Octree tree;

    public enum CollisionType
    {
        Standard,
        Octree
    }

    public static CollisionType collisionType = CollisionType.Standard;

    [SerializeField]
    public uint nStartingParticles = 100;

    [SerializeField]
    private GameObject particlePrefab;

    private List<Sphere> spheres = new List<Sphere>();
    private void Start()
    {
        tree = Octree.Create(Vector3.zero, 5);
        
        for (int i = 0; i < nStartingParticles; i++)
        {
            Sphere sphere = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity).GetComponent<Sphere>();
            tree.Insert(sphere);
            spheres.Add(sphere);
        }
    }

    private void TreeCollisionResolution()
    {
        tree.ResolveCollisions();
    }

    private void StandardCollisionResolution()
    {
        /*PlaneCollider[] planes = FindObjectsOfType<PlaneCollider>();
        if(spheres.Count < 1) return;
        
        for (int i = 0; i < spheres.Count; i++)
        {
            Sphere s1 = spheres[i];
            for (int j = i + 1; j < spheres.Count; j++)
            {
                Sphere s2 = spheres[j];
                ApplyCollisionResolution(s1, s2);
            }
            foreach (PlaneCollider plane in planes)
            {
                ApplyCollisionResolution(s1, plane);
            }
        }*/
    }

    private void FixedUpdate()
    {
        CollisionChecks = 0;

        switch (collisionType)
        {
            case CollisionType.Standard:
                StandardCollisionResolution();
                break;
            case CollisionType.Octree:
                TreeCollisionResolution();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            collisionType = collisionType == CollisionType.Standard ? CollisionType.Octree : CollisionType.Standard;
        }
    }
}
