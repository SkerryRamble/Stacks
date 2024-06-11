using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class LevelSelectorController : MonoBehaviour
{
    /*
     * Simply drag and drop all our level scriptable objects to the inspector property of List Levels
     * It will automatically create the correct size list of all our levels
     * 
     * Start calls CreateButtons which loops through all our levels and instantiates a button for each level
     * Each button has a listener added to the onClick which allows us to choose our method called when clicked, and pass levelID through too
     * 
     * LoadScene refreshes the GlobalObjectScript's currentLevel to the level we selected (using the levelID) and loads up the
     * Level scene which will build its contents based on the config in the currentLevel stored in the GlobalObjectScript
     *
     */

    public List<Level> levels;
    public GameObject LevelButtonPrefab;

    private void Start()
    {
        CreateButtons();
    }

    public void CreateButtons()
    {
        foreach (Level level in levels)
        {
            GameObject temp = Instantiate(LevelButtonPrefab, this.transform);
            temp.GetComponentInChildren<TextMeshProUGUI>().text = level.name.ToString();
            Button btn = temp.GetComponentInChildren<Button>();
            btn.onClick.AddListener(delegate { this.LoadScene(level.LevelID); });
        }
    }

    public void LoadScene(int levelID)
    {
        GlobalObjectScript.Instance.currentLevel = levels.Find(l => l.LevelID == levelID);
        //GlobalObjectScript.Instance.LevelSelectedSound();
        SceneManager.LoadScene("Level");
    }

    public void ReturnToMainMenu()
    {
        GlobalObjectScript.Instance.BackToMenuButtonClicked();
        SceneManager.LoadScene("StartMenu");
    }
}
