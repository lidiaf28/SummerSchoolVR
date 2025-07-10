using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Properties")]
    public float damage = 1f;
    public bool piercing = false;
    public int maxPierceCount = 3;
    
    [Header("Arrow Orientation")]
    [Tooltip("Which local axis points forward (tip of arrow)")]
    public Vector3 forwardAxis = Vector3.up;
    
    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;
    
    private Rigidbody rb;
    private Collider arrowCollider;
    private Vector3 velocity;
    private float lifetime;
    private float spawnTime;
    private int currentPierceCount = 0;
    private List<GameObject> hitFruits = new List<GameObject>();
    private bool initialized = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;
        
        arrowCollider = GetComponent<Collider>();
        if (arrowCollider == null)
        {
            BoxCollider capsule = gameObject.AddComponent<BoxCollider>();
        }
        arrowCollider.isTrigger = true;
    }
    
    public void Initialize(Vector3 direction, float speed, float lifetime)
    {
        this.velocity = direction * speed;
        this.lifetime = lifetime;
        this.spawnTime = Time.time;
        this.initialized = true;
        
        if (rb != null)
        {
            rb.velocity = velocity;
        }
        
        RotateArrowToDirection(direction);
    }
    
    void RotateArrowToDirection(Vector3 direction)
    {
        if (Mathf.Abs(direction.z) < 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            if (forwardAxis == Vector3.up)
            {
                transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            }
            else if (forwardAxis == Vector3.right)
            {
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            else if (forwardAxis == Vector3.forward)
            {
                transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.up);
            }
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
    
    void Update()
    {
        if (!initialized) return;
        
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            RotateArrowToDirection(rb.velocity.normalized);
        }
        
        if (Time.time - spawnTime > lifetime)
        {
            DestroyArrow();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fruit") || other.GetComponent<SimpleFruits>() != null)
        {
            if (hitFruits.Contains(other.gameObject))
                return;
                
            hitFruits.Add(other.gameObject);
            
            SimpleFruits fruit = other.GetComponent<SimpleFruits>();
            if (fruit != null)
            {
                ExplodeFruit(fruit);
                
                currentPierceCount++;
                
                if (!piercing || currentPierceCount >= maxPierceCount)
                {
                    DestroyArrow();
                }
            }
        }
        else if (other.CompareTag("Ground"))
        {
            DestroyArrow();
        }
    }
    
    void ExplodeFruit(SimpleFruits fruit)
    {
        Vector3 fruitPos = fruit.transform.position;
        string fruitName = fruit.GetFruitName();
        int points = fruit.GetPoints();
        
        if (hitEffectPrefab != null)
        {
            
            GameObject effect = Instantiate(hitEffectPrefab, fruitPos, Quaternion.identity);
            Destroy(effect, 2f);
        }
        else
        {
            CreateDefaultExplosion(fruitPos, fruit.GetComponent<Renderer>());
        }
        
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, fruitPos);
        }
        
        Debug.Log($"Hit {fruitName}! +{points} points");
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore(points);
            gameManager.UpdateUI();
        }
        
        Destroy(fruit.gameObject);
    }
    
    void CreateDefaultExplosion(Vector3 position, Renderer fruitRenderer)
    {
        GameObject particleObj = new GameObject("Fruit Explosion");
        particleObj.transform.position = position;
        
        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.duration = 0.5f;
        main.startLifetime = 1f;
        main.startSpeed = 5f;
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        if (fruitRenderer != null && fruitRenderer.material != null)
        {
            main.startColor = fruitRenderer.material.color;
        }
        
        var emission = particles.emission;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, 100)
        });
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);
        
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        
        Destroy(particleObj, 2f);
    }
    
    void DestroyArrow()
    {
        if (hitEffectPrefab != null)
        {
            GameObject puff = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            puff.transform.localScale = Vector3.one * 0.3f;
            Destroy(puff, 1f);
        }
        
        Destroy(gameObject);
    }
}
