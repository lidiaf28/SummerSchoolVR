using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Fruit Prefabs")]
    [Tooltip("Array of fruit prefabs in order: Watermelon, Apple, Banana, Melon, Orange, Pineapple, Grape, Tomato, Carrot, Coconut")]
    public GameObject[] fruitPrefabs = new GameObject[10];
    
    [Header("Spawn Settings")]
    [Tooltip("Number of fruits to spawn per cycle")]
    public int fruitsPerCycle = 3;
    
    [Tooltip("Delay between spawn cycles in seconds")]
    public float spawnCycleDelay = 3f;
    
    [Tooltip("Total spawning duration in seconds")]
    public float spawningDuration = 60f;
    
    [Header("Launch Settings")]
    [Tooltip("Minimum upward launch angle in degrees")]
    public float minLaunchAngle = 45f;
    
    [Tooltip("Maximum upward launch angle in degrees")]
    public float maxLaunchAngle = 135f;
    
    [Tooltip("Launch force applied to fruits")]
    public float launchForce = 100f;
    
    [Tooltip("Random horizontal spread in degrees")]
    public float horizontalSpread = 30f;
    
    [Header("Spawn Positions")]
    [Tooltip("Empty GameObjects that act as spawn points")]
    public Transform[] spawnPoints = new Transform[2];

    // Internal variables
    private bool isSpawning = false;
    private float spawnTimer = 0f;
    private Coroutine spawnCoroutine;

    // Fruit type enum
    public enum FruitType
    {
        Watermelon = 0,
        Apple = 1,
        Banana = 2,
        Melon = 3,
        Orange = 4,
        Pineapple = 5,
        Grape = 6,
        Tomato = 7,
        Carrot = 8,
        Coconut = 9
    }

    void Start()
    {
        ValidateSetup();
    }

    void ValidateSetup()
    {
        // Check if we have all fruit prefabs
        for (int i = 0; i < fruitPrefabs.Length; i++)
        {
            if (fruitPrefabs[i] == null)
            {
                Debug.LogWarning($"Fruit prefab at index {i} ({(FruitType)i}) is not assigned!");
            }
        }
        
        // Check spawn points
        if (spawnPoints.Length < 2)
        {
            Debug.LogError("Need at least 2 spawn points! Currently have: " + spawnPoints.Length);
        }
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError($"Spawn point {i} is not assigned!");
            }
        }
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnTimer = 0f;
            spawnCoroutine = StartCoroutine(SpawnCycle());
            Debug.Log("Fruit spawning started!");
        }
    }

    IEnumerator SpawnCycle()
    {
        while (isSpawning && spawnTimer < spawningDuration)
        {
            // Spawn fruits for this cycle
            SpawnFruitsForCycle();
            
            // Wait for next cycle
            yield return new WaitForSeconds(spawnCycleDelay);
            
            // Update timer
            spawnTimer += spawnCycleDelay;
        }
        
        // Auto-stop when duration is reached
        StopSpawning();
        Debug.Log("Spawning duration completed!");
    }

    void SpawnFruitsForCycle()
    {
        for (int i = 0; i < fruitsPerCycle; i++)
        {
            // Choose random spawn point
            Transform spawnPoint = GetRandomSpawnPoint();
            
            // Choose random fruit
            GameObject fruitPrefab = GetRandomFruitPrefab();
            
            if (spawnPoint != null && fruitPrefab != null)
            {
                // Spawn the fruit
                SpawnFruit(fruitPrefab, spawnPoint);
            }
        }
    }

    Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Length == 0) return null;
        
        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex];
    }
    
    GameObject GetRandomFruitPrefab()
    {
        // Filter out null prefabs
        var validFruits = new System.Collections.Generic.List<GameObject>();
        foreach (var fruit in fruitPrefabs)
        {
            if (fruit != null)
                validFruits.Add(fruit);
        }
        
        if (validFruits.Count == 0) return null;
        
        int randomIndex = Random.Range(0, validFruits.Count);
        return validFruits[randomIndex];
    }
    
    void SpawnFruit(GameObject fruitPrefab, Transform spawnPoint)
    {
        // Instantiate fruit at spawn point
        GameObject fruit = Instantiate(fruitPrefab, spawnPoint.position, Random.rotation);
        
        // Get or add Rigidbody
        Rigidbody rb = fruit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = fruit.AddComponent<Rigidbody>();
        }
        
        // Calculate launch direction
        Vector3 launchDirection = CalculateLaunchDirection();
        
        // Apply launch force
        rb.velocity = launchDirection * launchForce;
        
        // Add some random spin
        rb.angularVelocity = Random.insideUnitSphere * 5f;
        
        // Log spawn info
        Debug.Log($"Spawned {fruitPrefab.name} at {spawnPoint.name} with angle {Vector3.Angle(Vector3.up, launchDirection)}°");
    }

    Vector3 CalculateLaunchDirection()
    {
        // Get random angle between min and max (45-135 degrees)
        float angle = Random.Range(minLaunchAngle, maxLaunchAngle);
        
        // Convert to radians
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate base direction (assuming forward is towards player)
        // Angle of 90 degrees = straight up
        // Less than 90 = forward and up
        // More than 90 = backward and up
        float x = Mathf.Cos(angleRad);
        float y = Mathf.Sin(angleRad);
        
        // Add horizontal spread
        float horizontalAngle = Random.Range(-horizontalSpread, horizontalSpread) * Mathf.Deg2Rad;
        float z = Mathf.Sin(horizontalAngle) * Mathf.Abs(x); // Scale by x to maintain angle
        x *= Mathf.Cos(horizontalAngle);
        
        return new Vector3(x, y, z).normalized;
    }

    // Public methods for external control
    public float GetRemainingTime()
    {
        return Mathf.Max(0, spawningDuration - spawnTimer);
    }
    
    public bool IsSpawning()
    {
        return isSpawning;
    }
    
    public void SetSpawningDuration(float duration)
    {
        spawningDuration = duration;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        
        // Draw spawn points
        Gizmos.color = Color.green;
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.3f);
                
                // Draw launch angle cone
                DrawLaunchCone(point);
            }
        }
    }
    
    void DrawLaunchCone(Transform spawnPoint)
    {
        Gizmos.color = Color.yellow;
        
        // Draw min and max angle lines
        Vector3 minDir = Quaternion.Euler(0, 0, -minLaunchAngle) * Vector3.right;
        Vector3 maxDir = Quaternion.Euler(0, 0, -maxLaunchAngle) * Vector3.right;
        
        Gizmos.DrawRay(spawnPoint.position, minDir * 2f);
        Gizmos.DrawRay(spawnPoint.position, maxDir * 2f);
        
        // Draw arc between angles
        int segments = 10;
        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;
            
            float angle1 = Mathf.Lerp(minLaunchAngle, maxLaunchAngle, t1);
            float angle2 = Mathf.Lerp(minLaunchAngle, maxLaunchAngle, t2);
            
            Vector3 dir1 = Quaternion.Euler(0, 0, -angle1) * Vector3.right * 2f;
            Vector3 dir2 = Quaternion.Euler(0, 0, -angle2) * Vector3.right * 2f;
            
            Gizmos.DrawLine(spawnPoint.position + dir1, spawnPoint.position + dir2);
        }
    }
    
    public void StopSpawning()
    {
        if (isSpawning)
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            Debug.Log("Fruit spawning stopped!");
        }
    }
}

