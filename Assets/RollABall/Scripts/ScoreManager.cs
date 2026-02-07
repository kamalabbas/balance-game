using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class ScoreManager : MonoBehaviour
{
    public GameObject scoreCanvas; // The Canvas that contains the InputField, Button, and Text
    public InputField nameInputField;
    public Button submitButton;
    public Text scoreBoardText;

    private List<ScoreEntry> scoreEntries = new List<ScoreEntry>();
    private const string scoreFile = "scores.json";

    [System.Serializable]
    public class ScoreEntry
    {
        public string playerName;
        public int score;
    }

    [System.Serializable]
    private class ScoreData
    {
        public List<ScoreEntry> scores;
    }

    void Start()
    {
        scoreCanvas.SetActive(false); // Hide canvas initially
        LoadScores();
    }

    public void ShowScoreCanvas()
    {
        scoreCanvas.SetActive(true); // Show canvas
        UpdateScoreBoard();
    }

    void OnSubmitScore()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            // Create a new score entry
            ScoreEntry newEntry = new ScoreEntry
            {
                playerName = nameInputField.text,
                score = 100 // Replace with the actual score
            };

            // Add the new score entry
            scoreEntries.Add(newEntry);

            // Save the scores
            SaveScores();

            // Update the scoreboard UI
            UpdateScoreBoard();
        }
    }

    void SaveScores()
    {
        // Sort the scores in descending order
        scoreEntries.Sort((x, y) => y.score.CompareTo(x.score));

        string json = JsonUtility.ToJson(new { scores = scoreEntries }, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, scoreFile), json);
    }

    void LoadScores()
    {
        string path = Path.Combine(Application.persistentDataPath, scoreFile);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var loadedData = JsonUtility.FromJson<ScoreData>(json);
            scoreEntries = new List<ScoreEntry>(loadedData.scores);
        }
    }

    void UpdateScoreBoard()
    {
        scoreBoardText.text = "Scoreboard:\n";
        foreach (var entry in scoreEntries)
        {
            scoreBoardText.text += $"{entry.playerName}: {entry.score}\n";
        }
    }
}
