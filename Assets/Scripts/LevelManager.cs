using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoSingleton<LevelManager>
{
    public Transform TowerBaseLocations;
    public Transform WaypointLocations;

#if UNITY_EDITOR
    public bool DEBUG = false;
    public int DEBUGLIVES;
    public int DEBUGBANK;

#endif

    public bool paused = false;
    public int GameSpeed { get; set; } = (int)StaticEnums.GameSpeed.NormalSpeed;
    public int Bank { get; set; }
    public int Lives { get; set; }
    public int CurrentWave { get; set; }
    public float SellPercentage { get; set; } = 0.5f;
    public bool JustStarted = true;
    public Level CurrentLevel;
    public StaticEnums.GameEndStatus GameStatus { get; set; }
    public GameObject SkullFog;

    public static Action<StaticEnums.GameEndStatus> OnGameFinished;
    public static Action OnPaused;
    public static Action OnGameStarted;

    private void ReachedBase() { Lives--; }
    private void EnemyKilled(int reward, int nully) { Bank += reward; }
    private void WaveDestroyed(int reward)  { Bank += reward; }
    private void WaveCalledEarly(int reward) { Bank += reward; }

    public override void Init()
    {        
        EnemyAI.OnReachedBase += ReachedBase;
        EnemyAI.OnKilled += EnemyKilled;
        SpawnManager.OnWaveDestroyed += WaveDestroyed;
        SpawnManager.OnWaveCalledEarly += WaveCalledEarly;
    }

    private void OnDisable()
    {
        EnemyAI.OnReachedBase -= ReachedBase;
        EnemyAI.OnKilled -= EnemyKilled;
        SpawnManager.OnWaveDestroyed -= WaveDestroyed;
        SpawnManager.OnWaveCalledEarly -= WaveCalledEarly;
    }

    

    private void Start()
    {        
        GameStarted();
    }

    void GameStarted()
    {
        //we start paused and reset speed and current wave
        //SkullFog.SetActive(false);
        paused = true;
        JustStarted = true;
        CurrentWave = 0;
        GameSpeed = (int)StaticEnums.GameSpeed.NormalSpeed;
        GameStatus = StaticEnums.GameEndStatus.OnGoing;

        CurrentLevel = GlobalObjectScript.Instance.currentLevel;
        if (CurrentLevel == null)
        {
            //Debug.LogError("No level found to load");
            //return to startmenu


            SceneManager.LoadScene("StartMenu");
            return;
        }

        Lives = CurrentLevel.StartingLives; // + perksBonusLives if(level allows perks)
        Bank = CurrentLevel.StartingCash; // + perksBonusLives if(level allows perks)

#if UNITY_EDITOR
        if (DEBUG) Lives = DEBUGLIVES;
        if (DEBUG) Bank = DEBUGBANK;
#endif

        DrawLevel();
        RepositionCamera();
        OnGameStarted?.Invoke();
    }

    void RepositionCamera()
    {
        GetLevelMetaLocation();
    }


    void GetLevelMetaLocation()
    {
        //maybe work ou how to incorporate this into the levellocation class stuff
        //we need minmax x y and slap the camera in the middle, and zoom on the mag dist
        float minx = Mathf.Infinity;
        float miny = minx;
        float maxx = Mathf.NegativeInfinity;
        float maxy = maxx;

        foreach (Vector2S item in CurrentLevel.PathPoints)
        {
            if (item.x < minx) minx = item.x;
            if (item.y < miny) miny = item.y;
            if (item.x > maxx) maxx = item.x;
            if (item.y > maxy) maxy = item.y;
        }

        float width = maxx - minx;
        float height = maxy - miny;

        //pop out to other method someday
        Camera.main.transform.position = new Vector3(minx + (width / 2), miny + (height / 2), -25f);
        Camera.main.orthographicSize = Mathf.Min(15, Mathf.Max(width, height));
    }

    private void DrawLevel()
    {
        WaypointManager.Instance.PlacePoints();
        TowerBaseLocationsController.Instance.PlacePoints();
    }
    
    private void Update()
    {
        if (Lives <= 0 && GameStatus == StaticEnums.GameEndStatus.OnGoing)
        {
            GameOver(StaticEnums.GameEndStatus.Lost);
        }
    }

    public void GameOver(StaticEnums.GameEndStatus status)
    {
        GameStatus = status;
        GlobalObjectScript.Instance.GlobalStats.StashedCash += Bank;
        OnGameFinished?.Invoke(status);
        GlobalObjectScript.Instance.SaveTheAllFile();
        KillAllObjects();
        //Instantiate(SkullFog); //TODO: add a skull fog on losing...
        UIManager.Instance.GameOverSplash();
    }

    private void KillAllObjects()
    {
        //careful in case end pic is animated this will break it
        Time.timeScale = 0f;
    }
}
