using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RollABall.Licensing;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{   
    private const string TimeLimitPrefKey = "TimeLimitSeconds";
    private const string ModePrefKey = "MenuSelectedModeLabel";
    private const float EasyTimeLimitSeconds = 270f;   // 4:30
    private const float MediumTimeLimitSeconds = 180f; // 3:00
    private const float HardTimeLimitSeconds = 90f;    // 1:30

    [Header("UI")]
    public TMP_Text modeText;
    public TMP_Text timeText;

    [Header("UI (Legacy Text)")]
    public Text modeTextLegacy;
    public Text timeTextLegacy;

    private string selectedModeLabel = "Easy";
    private float selectedTimeLimitSeconds = EasyTimeLimitSeconds;

    void Start()
    {
        // In builds, this will create machine_code.txt when not activated.
        // In Editor, IsActivated() always returns true (no license needed).
        LicenseService.IsActivated(out _);

        LoadSelectionFromPrefs();
        UpdateMenuText();
    }

    public void OnPlayButton()
    {
        SaveSelectionToPrefs();
        StartGameWithTimeLimit(selectedTimeLimitSeconds);
    }

    public void OnEasyButton()
    {
        SetMode("Easy");
        SaveSelectionToPrefs();
        UpdateMenuText();
    }

    public void OnMediumButton()
    {
        SetMode("Medium");
        SaveSelectionToPrefs();
        UpdateMenuText();
    }

    public void OnHardButton()
    {
        SetMode("Hard");
        SaveSelectionToPrefs();
        UpdateMenuText();
    }

    public void OnQuitButton() 
    {
        ClearSelectionPrefs();
        Application.Quit();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SaveSelectionToPrefs();
            StartGameWithTimeLimit(selectedTimeLimitSeconds);
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SetMode("Easy");
            SaveSelectionToPrefs();
            UpdateMenuText();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            SetMode("Medium");
            SaveSelectionToPrefs();
            UpdateMenuText();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            SetMode("Hard");
            SaveSelectionToPrefs();
            UpdateMenuText();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SetTimeLimitSeconds(EasyTimeLimitSeconds);
            SaveSelectionToPrefs();
            UpdateMenuText();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SetTimeLimitSeconds(MediumTimeLimitSeconds);
            SaveSelectionToPrefs();
            UpdateMenuText();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SetTimeLimitSeconds(HardTimeLimitSeconds);
            SaveSelectionToPrefs();
            UpdateMenuText();
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
        PlayerPrefs.SetString(ModePrefKey, selectedModeLabel);
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }

    private void SetMode(string modeLabel)
    {
        selectedModeLabel = modeLabel;
    }

    private void SetTimeLimitSeconds(float timeLimitSeconds)
    {
        selectedTimeLimitSeconds = timeLimitSeconds;
    }

    private void LoadSelectionFromPrefs()
    {
        selectedTimeLimitSeconds = PlayerPrefs.GetFloat(TimeLimitPrefKey, EasyTimeLimitSeconds);
        selectedModeLabel = PlayerPrefs.GetString(ModePrefKey, "Easy");
    }

    private void SaveSelectionToPrefs()
    {
        PlayerPrefs.SetFloat(TimeLimitPrefKey, selectedTimeLimitSeconds);
        PlayerPrefs.SetString(ModePrefKey, selectedModeLabel);
        PlayerPrefs.Save();
    }

    private void ClearSelectionPrefs()
    {
        PlayerPrefs.DeleteKey(TimeLimitPrefKey);
        PlayerPrefs.DeleteKey(ModePrefKey);
        PlayerPrefs.Save();
    }

    private void UpdateMenuText()
    {
        if (modeText != null)
        {
            modeText.text = selectedModeLabel + " Mode";
        }

        if (modeTextLegacy != null)
        {
            modeTextLegacy.text = selectedModeLabel + " Mode";
        }

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(selectedTimeLimitSeconds / 60f);
            int seconds = Mathf.FloorToInt(selectedTimeLimitSeconds % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (timeTextLegacy != null)
        {
            int minutes = Mathf.FloorToInt(selectedTimeLimitSeconds / 60f);
            int seconds = Mathf.FloorToInt(selectedTimeLimitSeconds % 60f);
            timeTextLegacy.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
