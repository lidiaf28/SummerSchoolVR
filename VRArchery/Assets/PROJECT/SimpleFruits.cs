using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFruits : MonoBehaviour
{
    [Header("Fruit Settings")]
    public string fruitName = "Apple";
    public int pointValue = 10;
    public Color fruitColor = Color.red;

    [Header("Cleanup")]
    public float destroyBelowY = -10f;
    public float maxLifetime = 20f;

    [Header("Visual Effects")]
    public GameObject explosionPrefab; 
    public float explosionScale = 1f;
    private Renderer fruitRenderer;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;

        fruitRenderer = GetComponent<Renderer>();
        if (fruitRenderer != null && fruitRenderer.material != null)
        {
            fruitColor = fruitRenderer.material.color;
        }

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.5f; 
        }

        if (GetComponent<Collider>() == null)
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.radius = 0.5f; 
        }

        if (!gameObject.CompareTag("Fruit"))
        {
            try
            {
                gameObject.tag = "Fruit";
            }
            catch
            {
                Debug.LogWarning($"'Fruit' tag not found. Please create it in the Tags manager.");
            }
        }
        gameObject.layer = LayerMask.NameToLayer("Fruit");
    }

    void Update()
    {
        if (transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }

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

    public Color GetFruitColor()
    {
        return fruitColor;
    }
    
    public void CreateJuiceSplash(Vector3 hitPoint)
    {
        GameObject splash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        splash.transform.position = hitPoint;
        splash.transform.localScale = Vector3.one * 0.1f;
        
        Destroy(splash.GetComponent<Collider>());
        
        Renderer renderer = splash.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = fruitColor;
        }
        
        StartCoroutine(AnimateSplash(splash));
    }
    
    IEnumerator AnimateSplash(GameObject splash)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = splash.transform.localScale;
        Vector3 endScale = startScale * 3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            splash.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            Renderer renderer = splash.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1f - t;
                renderer.material.color = color;
            }
            
            yield return null;
        }
        
        Destroy(splash);
    }
}
