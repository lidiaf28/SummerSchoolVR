using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ArrowShooter : MonoBehaviour
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public Transform shootPoint; 
    public float arrowSpeed = 20f;
    public float arrowLifetime = 5f;
    
    [Header("Shooting Settings")]
    public bool canShoot = true;
    public float shootCooldown = 0.3f;
    public int maxArrowsInScene = 10;
    
    [Header("Camera Reference")]
    public Camera mainCamera;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    
    private List<GameObject> activeArrows = new List<GameObject>();
    private float lastShootTime;
    
    private Vector2 mousePosition;
    
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (shootPoint == null)
        {
            GameObject shootPointObj = new GameObject("ShootPoint");
            shootPointObj.transform.parent = transform;
            shootPointObj.transform.position = new Vector3(0, -4, 0); 
            shootPoint = shootPointObj.transform;
        }
    }
    
    void Update()
    {
        UpdateMousePosition();
        
        if (CheckMouseClick() && canShoot)
        {
            if (Time.time - lastShootTime >= shootCooldown)
            {
                ShootArrow();
            }
        }
        
        activeArrows.RemoveAll(arrow => arrow == null);
    }
    
    void UpdateMousePosition()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            mousePosition = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            mousePosition = Touchscreen.current.touches[0].position.ReadValue();
        }
        #else
        mousePosition = Input.mousePosition;
        #endif
    }
    
    bool CheckMouseClick()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
        else if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            return Touchscreen.current.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began;
        }
        return false;
        #else
        return Input.GetMouseButtonDown(0);
        #endif
    }
    
    void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow prefab not assigned!");
            return;
        }
        
        if (activeArrows.Count >= maxArrowsInScene)
        {
            if (activeArrows[0] != null)
                Destroy(activeArrows[0]);
            activeArrows.RemoveAt(0);
        }
        
        Vector3 clickPosition = GetClickWorldPosition();
        if (clickPosition == Vector3.zero) return;
        
        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        
        Vector3 direction = (clickPosition - shootPoint.position).normalized;
        
        Arrow arrowComponent = arrow.GetComponent<Arrow>();
        if (arrowComponent == null)
            arrowComponent = arrow.AddComponent<Arrow>();
            
        arrowComponent.Initialize(direction, arrowSpeed, arrowLifetime);
        
        activeArrows.Add(arrow);
        
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        lastShootTime = Time.time;
    }
    
    Vector3 GetClickWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.back, Vector3.zero);
        
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            return ray.GetPoint(rayDistance);
        }
        
        return Vector3.zero;
    }
    
    public void EnableShooting(bool enable)
    {
        canShoot = enable;
    }
}