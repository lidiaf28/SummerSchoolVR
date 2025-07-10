using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownTimerUI : MonoBehaviour
{
    [Header("Timer Display")]
    public TextMeshProUGUI timerText;
    public TextMeshPro worldSpaceTimerText; 

    [Header("Visual Settings")]
    public bool showMilliseconds = false;
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;
    public float warningThreshold = 30f; 
    public float criticalThreshold = 10f;

    [Header("Effects")]
    public bool pulseOnCritical = true;
    public float pulseSpeed = 2f;
    public float pulseScale = 1.2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip tickSound;
    public AudioClip warningSound;
    public AudioClip criticalSound;
    public float tickInterval = 1f;

    private float lastTickTime = 0f;
    private bool warningPlayed = false;
    private bool criticalPlayed = false;
    private Vector3 originalScale;
    
    void Start()
    {
        if (timerText != null)
            originalScale = timerText.transform.localScale;
        else if (worldSpaceTimerText != null)
            originalScale = worldSpaceTimerText.transform.localScale;
    }
    
    public void UpdateTimerDisplay(float remainingTime)
    {
        string timeString = FormatTime(remainingTime);
        
        if (timerText != null)
            timerText.text = timeString;
        if (worldSpaceTimerText != null)
            worldSpaceTimerText.text = timeString;
        
        UpdateTimerColor(remainingTime);
        
        HandleTimerEffects(remainingTime);
        
        if (audioSource != null && tickSound != null && remainingTime > 0)
        {
            if (Time.time - lastTickTime >= tickInterval)
            {
                audioSource.PlayOneShot(tickSound);
                lastTickTime = Time.time;
            }
        }
    }
    
    public void UpdateTimerText(string timeString)
    {
        if (timerText != null)
            timerText.text = timeString;
        if (worldSpaceTimerText != null)
            worldSpaceTimerText.text = timeString;
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        
        if (showMilliseconds)
        {
            int milliseconds = Mathf.FloorToInt((timeInSeconds % 1) * 100);
            return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        }
        else
        {
            return $"{minutes:00}:{seconds:00}";
        }
    }
    
    void UpdateTimerColor(float remainingTime)
    {
        Color targetColor = normalColor;
        
        if (remainingTime <= criticalThreshold)
        {
            targetColor = criticalColor;
            if (!criticalPlayed && audioSource != null && criticalSound != null)
            {
                audioSource.PlayOneShot(criticalSound);
                criticalPlayed = true;
            }
        }
        else if (remainingTime <= warningThreshold)
        {
            targetColor = warningColor;
            if (!warningPlayed && audioSource != null && warningSound != null)
            {
                audioSource.PlayOneShot(warningSound);
                warningPlayed = true;
            }
        }
        
        if (timerText != null)
            timerText.color = targetColor;
        if (worldSpaceTimerText != null)
            worldSpaceTimerText.color = targetColor;
    }
    
    void HandleTimerEffects(float remainingTime)
    {
        if (pulseOnCritical && remainingTime <= criticalThreshold && remainingTime > 0)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            Vector3 targetScale = Vector3.Lerp(originalScale, originalScale * pulseScale, pulse);
            
            if (timerText != null)
                timerText.transform.localScale = targetScale;
            if (worldSpaceTimerText != null)
                worldSpaceTimerText.transform.localScale = targetScale;
        }
        else
        {
            if (timerText != null)
                timerText.transform.localScale = originalScale;
            if (worldSpaceTimerText != null)
                worldSpaceTimerText.transform.localScale = originalScale;
        }
    }
    
    public void OnTimerComplete()
    {
        if (timerText != null)
            timerText.text = "TIME'S UP!";
        if (worldSpaceTimerText != null)
            worldSpaceTimerText.text = "TIME'S UP!";
        
        warningPlayed = false;
        criticalPlayed = false;
    }
}
