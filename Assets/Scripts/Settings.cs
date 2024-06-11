using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class PlayerSettings
{//small class to wrap up settings for easy persistence.
    //bleurgh, playerprefs doesnt work like that, must be done line by line
    public bool rangeVisible = false;
    public bool healthVisible = true;
    public bool DisplayDamageText = true;
    public bool DisplayEffects = true;
}

public class Settings : MonoSingleton<Settings>
{ 
    public GameObject BuildPointsContainer;
    public PlayerSettings playerSettings;   

    public override void Init()
    {
        LevelManager.OnGameFinished += GameFinished;
        LevelManager.OnGameStarted += LoadPlayerPrefs;
    }

    private void OnDisable()
    {
        LevelManager.OnGameFinished -= GameFinished;
        LevelManager.OnGameStarted -= LoadPlayerPrefs;
    }

    private void GameFinished(StaticEnums.GameEndStatus status)
    {
        SavePlayerPrefs();
    }

    public void SavePlayerPrefs()
    {
        if (playerSettings == null) { Debug.LogError("No prefs exists to save."); return; }

        PlayerPrefs.SetString("rangeVisible", playerSettings.rangeVisible.ToString());
        PlayerPrefs.SetString("healthVisible", playerSettings.healthVisible.ToString());
        PlayerPrefs.SetString("DisplayDamageText", playerSettings.DisplayDamageText.ToString());
        PlayerPrefs.SetString("DisplayEffects", playerSettings.DisplayEffects.ToString());
        //Debug.Log("Saved player settings");
    }

    public void LoadPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("rangeVisible"))
        {
            playerSettings.rangeVisible = PlayerPrefs.GetString("rangeVisible").ToString() == "False" ? false : true;
        }
        if (PlayerPrefs.HasKey("healthVisible"))
        {
            playerSettings.healthVisible = PlayerPrefs.GetString("healthVisible").ToString() == "False" ? false : true;
        }
        if (PlayerPrefs.HasKey("DisplayDamageText"))
        {
            playerSettings.DisplayDamageText = PlayerPrefs.GetString("DisplayDamageText").ToString() == "False" ? false : true;
        }
        if (PlayerPrefs.HasKey("DisplayEffects"))
        {
            playerSettings.DisplayEffects = PlayerPrefs.GetString("DisplayEffects").ToString() == "False" ? false : true;
        }
        //Debug.Log("Loaded player settings");
        UIManager.Instance.RefreshToggleButtons();
    }
}
