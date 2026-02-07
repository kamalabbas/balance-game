using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    public float timeLimit = 120f; // 2 minutes
    public Text timerText; // Reference to the UI Text component
    public PlayerController playerController; 
    public float timeRemaining;
    private bool timerRunning = false;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                timerRunning = false;
                OnTimerEnd();
            }

            UpdateTimerText();
        }
    }

    public void StartTimer()
    {
        timeRemaining = timeLimit;
        timerRunning = true;
        UpdateTimerText();
    }

    public void ResetTimer()
    {
        timeRemaining = timeLimit;
        timerRunning = false;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text =  "Timer: " + string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnTimerEnd()
    {
        // Implement what happens when the timer ends, e.g., end game logic
        // For instance, you might trigger a game over condition or reset the game
        Debug.Log("Time's up!");

        playerController.winText.text = "Time's Up";

        // Stop the game
        Time.timeScale = 0f;

        // Call the LoadSceneAfterDelay function in the PlayerController script
        StartCoroutine(playerController.LoadSceneAfterDelay(3f, 1)); // Wait 5 seconds before reloading the scene
    }
}
