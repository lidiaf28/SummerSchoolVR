using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpawnerController : MonoBehaviour
{
    [Header("Spawner Reference")]
    public Spawner fruitSpawner;
    public Button startButton;
    public Button stopButton;
    
    [Header("Timer UI Reference")]
    public CountdownTimerUI timerUI;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopButtonClicked);
            
        if (fruitSpawner != null && timerUI != null)
        {
            fruitSpawner.onTimerUpdate.AddListener(timerUI.UpdateTimerDisplay);
            fruitSpawner.onTimerTextUpdate.AddListener(timerUI.UpdateTimerText);
            fruitSpawner.onTimerComplete.AddListener(timerUI.OnTimerComplete);
            
            Debug.Log("Timer UI connected to Spawner events");
        }
        else
        {
            if (fruitSpawner == null)
                Debug.LogError("FruitSpawner is not assigned in SpawnerController!");
            if (timerUI == null)
                Debug.LogError("TimerUI is not assigned in SpawnerController!");
        }
        UpdateButtonStates();
    }
    
    void OnStartButtonClicked()
    {
        if (fruitSpawner != null)
        {
            fruitSpawner.StartSpawning();
            UpdateButtonStates();
        }
    }
    
    void OnStopButtonClicked()
    {
        if (fruitSpawner != null)
        {
            fruitSpawner.StopSpawning();
            UpdateButtonStates();
        }
    }
    
    void UpdateButtonStates()
    {
        if (fruitSpawner != null)
        {
            bool isSpawning = fruitSpawner.IsSpawning;
            
            if (startButton != null)
                startButton.interactable = !isSpawning;
                
            if (stopButton != null)
                stopButton.interactable = isSpawning;
        }
    }
    
    void OnDestroy()
    {
        if (fruitSpawner != null && timerUI != null)
        {
            fruitSpawner.onTimerUpdate.RemoveListener(timerUI.UpdateTimerDisplay);
            fruitSpawner.onTimerTextUpdate.RemoveListener(timerUI.UpdateTimerText);
            fruitSpawner.onTimerComplete.RemoveListener(timerUI.OnTimerComplete);
        }
    }
}
