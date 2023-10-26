using System;
using System.Collections;
using System.Collections.Generic;
using static CollisionDetection;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

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
        tree = Octree.Create(Vector3.zero, 2);
        
        for (int i = 0; i < nStartingParticles; i++)
        {
            float offsetX = Random.Range(-4, 4);
            float offsetY = Random.Range(-4, 4);
            float offsetZ = Random.Range(-4, 4);
            Sphere sphere = Instantiate(particlePrefab, new Vector3(offsetX, offsetY, offsetZ), Quaternion.identity).GetComponent<Sphere>();
            
            float velX = Random.Range(-2, 2);
            float velY = Random.Range(-2, 2);
            float velZ = Random.Range(-2, 2);
            sphere.velocity = new Vector3(velX, velY, velZ);
            spheres.Add(sphere);
        }
    }

    private void TreeCollisionResolution()
    {
        tree.ResolveCollisions();
        
        PlaneCollider[] planes = FindObjectsOfType<PlaneCollider>();
        for (int i = 0; i < spheres.Count; i++)
        {
            Sphere s1 = spheres[i];
            foreach (PlaneCollider plane in planes)
            {
                ApplyCollisionResolution(s1, plane);
            }
        }
    }

    private void StandardCollisionResolution()
    {
        PlaneCollider[] planes = FindObjectsOfType<PlaneCollider>();
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
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks = 0;
        
        tree.Clear();
        foreach (var sphere in spheres)
        {
            tree.Insert(sphere);   
        }
        
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
