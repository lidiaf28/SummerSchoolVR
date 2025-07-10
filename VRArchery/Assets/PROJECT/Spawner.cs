using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
    [Header("Fruit Prefabs")]
    public GameObject[] fruitPrefabs = new GameObject[13];

    [Header("Spawn Settings")]
    public int fruitsPerCycle = 3;
    public float spawnCycleDelay = 3f;
    public float spawningDuration = 60f;

    [Header("Launch Settings")]
    public float minLaunchAngle = 45f;
    public float maxLaunchAngle = 135f;
    public float launchForce = 10f;
    public float horizontalSpread = 30f;

    [Header("Spawn Positions")]
    public Transform[] spawnPoints = new Transform[2];

    [Header("Layer Settings")]
    public string fruitLayerName = "Fruit";
    public bool autoSetLayer = true;

    [Header("Timer Events")]
    public UnityEvent<float> onTimerUpdate; 
    public UnityEvent<string> onTimerTextUpdate; 
    public UnityEvent onTimerComplete;

    private float elapsedTime = 0f;
    private float remainingTime = 0f;
    private bool isSpawning = false;
    private bool timerRunning = false;
    private Coroutine spawnCoroutine;

    public float RemainingTime => remainingTime;
    public float ElapsedTime => elapsedTime;
    public bool IsSpawning => isSpawning;
    public bool IsTimerRunning => timerRunning;

    void Start()
    {
        ValidateSetup();
        remainingTime = spawningDuration;
    }

    void Update()
    {
        if (timerRunning && remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            elapsedTime += Time.deltaTime;
            if (remainingTime < 0)
            {
                remainingTime = 0;
                HandleTimerComplete();
            }

            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        onTimerUpdate?.Invoke(remainingTime);

        string timeString = FormatTime(remainingTime);
        onTimerTextUpdate?.Invoke(timeString);
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        int milliseconds = Mathf.FloorToInt((timeInSeconds % 1) * 100);

        return $"{minutes:00}:{seconds:00}"; 
    }
    
    void HandleTimerComplete()
    {
        timerRunning = false;
        StopSpawning();
        onTimerComplete?.Invoke();
        Debug.Log("Timer completed! Game Over!");
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            timerRunning = true;
            elapsedTime = 0f;
            remainingTime = spawningDuration;
            
            spawnCoroutine = StartCoroutine(SpawnCycle());
            Debug.Log($"Fruit spawning started! Duration: {spawningDuration} seconds");
        }
    }
    
    public void StopSpawning()
    {
        if (isSpawning)
        {
            isSpawning = false;
            timerRunning = false;
            
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            Debug.Log("Fruit spawning stopped!");
        }
    }
    
    public void PauseTimer()
    {
        timerRunning = false;
    }
    
    public void ResumeTimer()
    {
        if (isSpawning)
        {
            timerRunning = true;
        }
    }
    
    public void AddTime(float seconds)
    {
        remainingTime += seconds;
        UpdateTimerDisplay();
    }
    
    public void SetDuration(float newDuration)
    {
        spawningDuration = newDuration;
        if (!isSpawning)
        {
            remainingTime = newDuration;
            UpdateTimerDisplay();
        }
    }
    
    IEnumerator SpawnCycle()
    {
        while (isSpawning && remainingTime > 0)
        {
            SpawnFruitsForCycle();
            yield return new WaitForSeconds(spawnCycleDelay);
        }
    }
    
    void SpawnFruitsForCycle()
    {
        for (int i = 0; i < fruitsPerCycle; i++)
        {
            Transform spawnPoint = GetRandomSpawnPoint();
            GameObject fruitPrefab = GetRandomFruitPrefab();
            
            if (spawnPoint != null && fruitPrefab != null)
            {
                SpawnFruit(fruitPrefab, spawnPoint);
            }
        }
    }
    
    void SpawnFruit(GameObject fruitPrefab, Transform spawnPoint)
    {
        GameObject fruit = Instantiate(fruitPrefab, spawnPoint.position, Random.rotation);
        
        if (autoSetLayer)
        {
            int fruitLayer = LayerMask.NameToLayer(fruitLayerName);
            if (fruitLayer != -1)
            {
                fruit.layer = fruitLayer;
                foreach (Transform child in fruit.transform)
                {
                    child.gameObject.layer = fruitLayer;
                }
            }
        }
        
        Rigidbody rb = fruit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = fruit.AddComponent<Rigidbody>();
        }
        
        Vector3 launchDirection = CalculateLaunchDirection();
        rb.velocity = launchDirection * launchForce;
        rb.angularVelocity = Random.insideUnitSphere * 5f;
    }
    
    Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
    
    GameObject GetRandomFruitPrefab()
    {
        var validFruits = new System.Collections.Generic.List<GameObject>();
        foreach (var fruit in fruitPrefabs)
        {
            if (fruit != null) validFruits.Add(fruit);
        }
        
        if (validFruits.Count == 0) return null;
        return validFruits[Random.Range(0, validFruits.Count)];
    }
    
    Vector3 CalculateLaunchDirection()
    {
        float angle = Random.Range(minLaunchAngle, maxLaunchAngle);
        float angleRad = angle * Mathf.Deg2Rad;
        
        float x = Mathf.Cos(angleRad);
        float y = Mathf.Sin(angleRad);
        
        float horizontalAngle = Random.Range(-horizontalSpread, horizontalSpread) * Mathf.Deg2Rad;
        float z = Mathf.Sin(horizontalAngle) * Mathf.Abs(x);
        x *= Mathf.Cos(horizontalAngle);
        
        return new Vector3(x, y, z).normalized;
    }
    
    void ValidateSetup()
    {
        int fruitLayer = LayerMask.NameToLayer(fruitLayerName);
        if (fruitLayer == -1)
        {
            Debug.LogError($"Fruit layer '{fruitLayerName}' not found!");
        }
        
        for (int i = 0; i < fruitPrefabs.Length; i++)
        {
            if (fruitPrefabs[i] == null)
            {
                Debug.LogWarning($"Fruit prefab at index {i} is not assigned!");
            }
        }
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError($"Spawn point {i} is not assigned!");
            }
        }
    }
}

