using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFruits : MonoBehaviour
{
    [Header("Fruit Settings")]
    public string fruitName = "Apple";
    public int pointValue = 10;

    [Header("Cleanup")]
    public float destroyBelowY = -10f;
    public float maxLifetime = 20f;

    private float spawnTime;
    
    void Start()
    {
        spawnTime = Time.time;
        
        // Ensure we have required components
        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }
        
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<SphereCollider>();
        }
        
        // Set tag
        gameObject.tag = "Fruit";
    }
    
    void Update()
    {
        // Destroy if fallen too far
        if (transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }
        
        // Destroy if existed too long
        if (Time.time - spawnTime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
    
    public string GetFruitName()
    {
        return fruitName;
    }
    
    public int GetPoints()
    {
        return pointValue;
    }
}
