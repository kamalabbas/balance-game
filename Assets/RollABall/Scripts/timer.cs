using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    private const string TimeLimitPrefKey = "TimeLimitSeconds";
    public float timeLimit = 270f; // Default: Easy (4:30)
    public Text timerText; // Reference to the UI Text component
    public PlayerController playerController; 
    public float timeRemaining;
    private bool timerRunning = false;

    void Start()
    {
        if (PlayerPrefs.HasKey(TimeLimitPrefKey))
        {
            float savedLimit = PlayerPrefs.GetFloat(TimeLimitPrefKey);
            if (savedLimit > 0f)
            {
                timeLimit = savedLimit;
            }
        }

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

    public void SetTimeLimit(float newTimeLimitSeconds, bool restartTimer = true)
    {
        if (newTimeLimitSeconds <= 0f)
        {
            return;
        }

        timeLimit = newTimeLimitSeconds;

        if (restartTimer)
        {
            StartTimer();
        }
        else
        {
            ResetTimer();
        }
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

        if (playerController != null && playerController.winText != null)
        {
            playerController.winText.text = "Time's Up";
        }

        // Stop the game
        Time.timeScale = 0f;

        // Call the LoadSceneAfterDelay function in the PlayerController script
        if (playerController != null)
        {
            StartCoroutine(playerController.LoadSceneAfterDelay(3f, 1)); // Wait 3 seconds before reloading the scene
        }
    }
}
