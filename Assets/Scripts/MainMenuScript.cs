using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenuScript : MonoBehaviour
{
    //public static Action OnMenuButtonClicked;

    public void StartGame()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("LevelSelector");
    }

    public void Upgrades()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("Upgrades");
    }

    public void Settings()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("Settings");
    }

    public void Instructions()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("Instructions");
    }

    public void Statistics()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("Statistics");
    }

    public void LevelEditor()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("LevelEditor");
    }

    public void Future()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("Future");
    }

    public void About()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("About");
    }

    public void Achievements()
    {
        GlobalObjectScript.Instance.MenuButtonClicked();
        SceneManager.LoadScene("Achievements");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}

