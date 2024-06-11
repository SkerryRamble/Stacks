using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEditorScript : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        GlobalObjectScript.Instance.BackToMenuButtonClicked();
        SceneManager.LoadScene("StartMenu");
    }
}
