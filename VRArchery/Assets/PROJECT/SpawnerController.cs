using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpawnerController : MonoBehaviour
{
    [Header("Spawner Reference")]
    public Spawner fruitSpawner;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public Button startButton;
    public Button stopButton;

    void Start()
    {
        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopButtonClicked);

        UpdateUI();
    }
    
    void Update()
    {
        // Update timer display
        if (fruitSpawner != null && fruitSpawner.IsSpawning())
        {
            UpdateTimerDisplay();
        }
    }
    
    void OnStartButtonClicked()
    {
        if (fruitSpawner != null)
        {
            fruitSpawner.StartSpawning();
            UpdateUI();
        }
    }
    
    void OnStopButtonClicked()
    {
        if (fruitSpawner != null)
        {
            fruitSpawner.StopSpawning();
            UpdateUI();
        }
    }
    
    void UpdateUI()
    {
        if (fruitSpawner == null) return;
        
        bool isSpawning = fruitSpawner.IsSpawning();
        
        if (startButton != null)
            startButton.interactable = !isSpawning;
            
        if (stopButton != null)
            stopButton.interactable = isSpawning;
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            float remainingTime = fruitSpawner.GetRemainingTime();
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"Time Remaining: {minutes:00}:{seconds:00}";
        }
    }
}
