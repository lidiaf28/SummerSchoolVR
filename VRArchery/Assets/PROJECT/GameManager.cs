using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Score")]
    public int currentScore = 0;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;

    [Header("Game Components")]
    public Spawner fruitSpawner;
    public ArrowShooter arrowShooter;

    [Header("Score Events")]
    public UnityEvent<int> onScoreChanged;

    [Header("Effects")]
    public AudioClip scoreSound;
    public AudioClip gameOverSound;
    private AudioSource audioSource;

    private bool isGameActive = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        UpdateUI();
        
        if (fruitSpawner != null)
        {
            fruitSpawner.onTimerComplete.AddListener(OnGameOver);
        }
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    public void StartGame()
    {
        isGameActive = true;
        currentScore = 0;
        UpdateUI();
        
        if (arrowShooter != null)
            arrowShooter.EnableShooting(true);
            
        if (fruitSpawner != null)
            fruitSpawner.StartSpawning();
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    public void AddScore(int points)
    {
        if (!isGameActive) return;
        
        currentScore += points;
        onScoreChanged?.Invoke(currentScore);
        
        if (audioSource != null && scoreSound != null)
             audioSource.PlayOneShot(scoreSound);
        
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
    }
    
    void OnGameOver()
    {
        isGameActive = false;
        
        if (arrowShooter != null)
            arrowShooter.EnableShooting(false);
            
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (audioSource != null && gameOverSound != null)
            audioSource.PlayOneShot(gameOverSound);
            
        StartCoroutine(CleanupFruits());
    }
    
    IEnumerator CleanupFruits()
    {
        yield return new WaitForSeconds(1f);
        
        SimpleFruits[] fruits = FindObjectsOfType<SimpleFruits>();
        foreach (var fruit in fruits)
        {
            Arrow tempArrow = new GameObject("TempArrow").AddComponent<Arrow>();
            tempArrow.transform.position = fruit.transform.position;
            typeof(Arrow).GetMethod("ExplodeFruit", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)
                .Invoke(tempArrow, new object[] { fruit });
            Destroy(tempArrow.gameObject);
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void RestartGame()
    {
        StartGame();
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
