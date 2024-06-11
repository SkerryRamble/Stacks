using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class UIManager : MonoSingleton<UIManager>
{
    //TODO: creeps killed isnt reset for each new level
    [SerializeField]
    private Text LivesLeftText, BankText, CreepsOnScreenText, PausedText, CycleSpeedText, CreepsKilledText, CreepsLeftText;

    private int creepsKilledGlobalOffset;

    public Transform PausedPanel;
    private Color toggleOnColor = Color.green;
    private Color toggleOffColor = Color.white;
    public Transform rangeButton;
    public Transform effectsButton;
    public Transform damageTextButton;
    public Transform healthbarButton;
    public GameObject WinPicture;
    public GameObject LosePicture;
    public Button PressAnyWhereButton;
    
    public static Action OnBuildMissile;
    public static Action OnBuildBullet;
    public static Action OnBuildIce;
    public static Action OnBuildFire;
    public static Action OnBuildSniper;
    public static Action<bool> OnDisplayHealth;

    GlobalObjectScript gos;
    LevelManager lm;
    SpawnManager sm;
    BuildManager bm;
    Settings ps;

    private void Start()
    {
        gos = GlobalObjectScript.Instance;
        lm = LevelManager.Instance;
        sm = SpawnManager.Instance;
        bm = BuildManager.Instance;
        ps = Settings.Instance;

        //Begin the level with the pause screen up and other splash screens off
        lm.paused = true;
        ForcePause(lm.paused);
        RefreshToggleButtons();
        WinPicture.SetActive(false);
        LosePicture.SetActive(false);
        PressAnyWhereButton.transform.gameObject.SetActive(false);

        creepsKilledGlobalOffset = gos.GlobalStats.CreepsKilled;
    }

    public void ReloadCurrentLevel()
    {
        //levelid etc should be fine
        SceneManager.LoadScene("Level");
    }

    private void Update()
    {
        UpdateScreenStats();
        GetKeys();
    }

    private void UpdateScreenStats()
    {
        if (LivesLeftText.text != lm.Lives.ToString()) LivesLeftText.text = string.Format("{00}", lm.Lives);
        if (BankText.text != lm.Bank.ToString()) BankText.text = string.Format("{00}", lm.Bank);
        if (CreepsKilledText.text != (gos.GlobalStats.CreepsKilled - creepsKilledGlobalOffset).ToString()) CreepsKilledText.text = (gos.GlobalStats.CreepsKilled - creepsKilledGlobalOffset).ToString();  //TODO: make level version of this, globabl stat is historical
        if (CreepsOnScreenText.text != sm.EnemiesOnScreen.ToString()) CreepsOnScreenText.text = string.Format("{00}", sm.EnemiesOnScreen);
        if (CreepsLeftText.text != sm.creepsStillToCome.ToString()) CreepsLeftText.text = (gos.currentLevel.TotalLevelEnemies - sm.enemiesSpawned).ToString();
    }

    public void SwitchSpeed()
    {
        //check if we're breaking the pause
        if (lm.paused)
        {
            lm.paused = !lm.paused;
            ForcePause(lm.paused);
            PausedText.text = ">";
        }

        //first cycle through gamespeed
        switch (lm.GameSpeed)
        {
            case (int)StaticEnums.GameSpeed.NormalSpeed:
                lm.GameSpeed = (int)StaticEnums.GameSpeed.DoubleSpeed;
                CycleSpeedText.text = "x2";
                break;
            case (int)StaticEnums.GameSpeed.DoubleSpeed:
                lm.GameSpeed = (int)StaticEnums.GameSpeed.TripleSpeed;
                CycleSpeedText.text = "x3";
                break;
            case (int)StaticEnums.GameSpeed.TripleSpeed:
                lm.GameSpeed = (int)StaticEnums.GameSpeed.HalfSpeed;
                CycleSpeedText.text = "x0.5";
                break;
            case (int)StaticEnums.GameSpeed.HalfSpeed:
                lm.GameSpeed = (int)StaticEnums.GameSpeed.NormalSpeed;
                CycleSpeedText.text = "x1";
                break;
        }
        //next set timescale to cycled timespeed
        Time.timeScale = lm.GameSpeed / 10f;
    }

    void GetKeys()
    {
        //keypresses only work if not paused; except unpause and speedswitch
        if (Input.GetKeyDown(KeyCode.Space)) TogglePause();
        if (Input.GetKeyDown(KeyCode.Q) && !lm.JustStarted) SwitchSpeed();

        if (!lm.paused)
        {
            if (Input.GetKeyDown(KeyCode.S)) bm.BuildMode = StaticEnums.BuildMode.Selling;
            if (Input.GetKeyDown(KeyCode.U)) bm.BuildMode = StaticEnums.BuildMode.Upgrading;
            if (Input.GetKeyDown(KeyCode.N)) SpawnManager.Instance.CallWave(true);
            if (Input.GetKeyDown(KeyCode.R)) ToggleRangeVisible();            
            if (Input.GetKeyDown(KeyCode.D)) ToggleDisplayDamageText();
            if (Input.GetKeyDown(KeyCode.H)) ToggleHealthBars();
            if (Input.GetKeyDown(KeyCode.E)) ToggleDisplayEffects();
            //Select Towers to build hotkeys
            //TODO: hotkeysa are broken at mo, the shoppanelcellscript appears to destroy itself after being deselected or seomthing so only direct clikcing works
            if (Input.GetKeyDown(KeyCode.Alpha1))
            { OnBuildBullet?.Invoke(); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) 
                OnBuildIce?.Invoke();
            if (Input.GetKeyDown(KeyCode.Alpha3)) 
                OnBuildMissile?.Invoke();
            if (Input.GetKeyDown(KeyCode.Alpha4)) 
                OnBuildSniper?.Invoke();
            if (Input.GetKeyDown(KeyCode.Alpha5))
            { OnBuildFire?.Invoke(); }
        }
    }

    public void TogglePause()
    {//we must togglepause to get past the Ready? overlay which means we are past the juststarted point and can reactivate the Q keypress
        lm.JustStarted = false;    
        lm.paused = !lm.paused;
        ForcePause(lm.paused);
    }

    public void ForcePause(bool PauseMe)
    {
        if (PauseMe)
        {
            Time.timeScale = 0f;
            PausedText.text = "II";
            PausedPanel.gameObject.SetActive(true);

            //take the pause opportunity to save stuff
            //if (!lm.JustStarted) Settings.Instance.SavePlayerPrefs();
            //TODO:save all
           
        }
        else
        {
            Time.timeScale = lm.GameSpeed / 10f;
            PausedText.text = ">";
            PausedPanel.gameObject.SetActive(false);
        }
    }

    public void ToggleRangeVisible()
    {
        ps.playerSettings.rangeVisible = !ps.playerSettings.rangeVisible;
        ChangeButtonColor(ps.playerSettings.rangeVisible, rangeButton);
    }

    public void ToggleHealthBars()
    {
        ps.playerSettings.healthVisible = !ps.playerSettings.healthVisible;
        ChangeButtonColor(ps.playerSettings.healthVisible, healthbarButton);
        OnDisplayHealth?.Invoke(ps.playerSettings.healthVisible);
    }

    public void ToggleDisplayDamageText()
    {
        ps.playerSettings.DisplayDamageText = !ps.playerSettings.DisplayDamageText;
        ChangeButtonColor(ps.playerSettings.DisplayDamageText, damageTextButton);
    }

    public void ToggleDisplayEffects()
    {
        ps.playerSettings.DisplayEffects = !ps.playerSettings.DisplayEffects;
        ChangeButtonColor(ps.playerSettings.DisplayEffects, effectsButton);
    }

    public void ShowPath()
    {
        WaypointManager.Instance.HighlightPath();
    }

    private void ChangeButtonColor(bool toggle, Transform buttonTransform)
    {
        //was hoping catching the button object like this would work, but still need to process keypresses.s..
        //sigh, much simpler just to attach each buttons transform and process them individually like that.
        //Transform tf = EventSystem.current.currentSelectedGameObject.transform;

        //Color temp = buttonTransform.GetComponent<Image>().color;
        if (toggle)
        {
            buttonTransform.GetComponent<Image>().color = toggleOnColor;
        }
        else
        {
            buttonTransform.GetComponent<Image>().color = toggleOffColor;
        }
    }

    public void RefreshToggleButtons()
    {
        ChangeButtonColor(ps.playerSettings.DisplayDamageText, damageTextButton);
        ChangeButtonColor(ps.playerSettings.rangeVisible, rangeButton);
        ChangeButtonColor(ps.playerSettings.DisplayEffects, effectsButton);
        ChangeButtonColor(ps.playerSettings.healthVisible, healthbarButton);
    }

    public void GameOverSplash()
    {
        PressAnyWhereButton.transform.gameObject.SetActive(true);

        if (lm.GameStatus == StaticEnums.GameEndStatus.Won) //won
        {
            WinPicture.SetActive(true);
        }
        if (lm.GameStatus == StaticEnums.GameEndStatus.Lost)
        {
            LosePicture.SetActive(true);
        }
    }

    public void EndLevel()
    {
        //technically we 'lost' the game here
        gos.GlobalStats.GamesLost++;

        //TODO players visual settings arent being saved? if we quit the game early

        //preserve stats etc
        gos.SaveTheAllFile();
        gos.SaveTheAllFileJSON();
        Settings.Instance.SavePlayerPrefs();
        GlobalObjectScript.Instance.BackToMenuButtonClicked();
        SceneManager.LoadScene("StartMenu");
    }

    public void SelectTower(GameObject tower)
    {
        bm.SetTowerToBuild(tower);
    }

    
    public void SaveGame()
    {
        //TODO: no idea;
    }

    public void SaveAndQuit()
    {
        SaveGame();
        EndLevel();
    }
}
