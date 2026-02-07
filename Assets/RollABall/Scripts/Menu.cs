using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{   
    public void OnPlayButton()
    {
        PlayerPrefs.SetInt("PlayerLives", 2);
        SceneManager.LoadScene(1);
    }

    public void OnQuitButton() 
    {
        Application.Quit();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.SetInt("PlayerLives", 2);
            SceneManager.LoadScene(1);
        }
    }
}
