using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RollABall.Licensing;

public class Menu : MonoBehaviour
{   
    private const string TimeLimitPrefKey = "TimeLimitSeconds";
    private const float EasyTimeLimitSeconds = 270f;   // 4:30
    private const float MediumTimeLimitSeconds = 180f; // 3:00
    private const float HardTimeLimitSeconds = 90f;    // 1:30

    public void OnPlayButton()
    {
        StartGameWithTimeLimit(EasyTimeLimitSeconds);
    }

    public void OnEasyButton()
    {
        StartGameWithTimeLimit(EasyTimeLimitSeconds);
    }

    public void OnMediumButton()
    {
        StartGameWithTimeLimit(MediumTimeLimitSeconds);
    }

    public void OnHardButton()
    {
        StartGameWithTimeLimit(HardTimeLimitSeconds);
    }

    public void OnQuitButton() 
    {
        Application.Quit();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartGameWithTimeLimit(EasyTimeLimitSeconds);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartGameWithTimeLimit(EasyTimeLimitSeconds);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            StartGameWithTimeLimit(MediumTimeLimitSeconds);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            StartGameWithTimeLimit(HardTimeLimitSeconds);
        }
    }

    private void StartGameWithTimeLimit(float timeLimitSeconds)
    {
        if (!LicenseService.IsActivated(out _))
        {
            return;
        }

        PlayerPrefs.SetInt("PlayerLives", 2);
        PlayerPrefs.SetFloat(TimeLimitPrefKey, timeLimitSeconds);
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }
}
