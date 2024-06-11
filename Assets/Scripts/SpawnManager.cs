using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SpawnManager : MonoSingleton<SpawnManager>
{
    public List<Wave> waves;
    public float timeBetweenWaves;
    public float waveSpawnCountdown;
    public Text waveCountDownText;  //assigned in editor
    //public int waveIndex;    
    public float lastSpawnTime;
    public int enemiesSpawned = 0;
    public bool AllEnemiesSpawned = false;
    public int EnemiesOnScreen;
    public int creepsStillToCome;
    private ObjectPooler objectPooler;
    private LevelManager lm;
    public static Action<int> OnWaveDestroyed;
    public static Action<int> OnWaveCalledEarly;
    public bool ENDLESS;

    public List<GameObject> endlessPrefabs;

    //TODO: store a preset of wave types for use in endless mode, need a way to increase number and toughness of spawns related to wave number, limit number somehow
    //maybe follow a pattern of single enemy type per wave up til wave 10 which will be bosses
    private void CreateEndlessWavesDecade()
    {
        List<Wave> endlessWaves = new List<Wave>();

        //general idea: waves come in batches of 10; first wave will be regular and last wave will be boss
        //all waves in between will be a mix of other waves in the list, order, interval and number randomised

        for (int i = 0; i < 10; i++)
        {
            Wave wave1 = new Wave { Active = false, DifficultyMultiplier = lm.CurrentWave, SubWaves = PopulateEndlessSubWave(i), WaveReward = 5 };
            endlessWaves.Add(wave1);
        }
    }

    List<SubWave> PopulateEndlessSubWave(int index)
    {
        int prefabIndex = 0;
        if (index == 0) prefabIndex = 0;    //regular
        if (index == 9) prefabIndex = endlessPrefabs.Count; //boss
        if (index != 0 && index != 9) prefabIndex = UnityEngine.Random.Range(1, 8);

        SubWave subWave1 = new SubWave { EnemyPrefab = endlessPrefabs[prefabIndex], DelayTilNextSubWave = 0f, NumberOfEnemies = 10, SpawnInterval = 1 };
        List<SubWave> sw = new List<SubWave>();
        sw.Add(subWave1);
        return sw;
    }

    public override void Init()
    {
        AllEnemiesSpawned = false;
        EnemyAI.OnKilled += EnemyKilled;
    }

    private void OnDisable()
    {
        EnemyAI.OnKilled -= EnemyKilled;
    }

    private void EnemyKilled(int reward, int _waveindex)
    {
        //TODO: check this is being called when it shuld be
        waves[_waveindex].SurvivingEnemies--;
        if (waves[_waveindex].SurvivingEnemies == 0)
        {
            WaveDestroyed(_waveindex);
        }
    }

    private void WaveDestroyed(int _waveindex)
    {
        waves[_waveindex].Active = false;
        OnWaveDestroyed?.Invoke(waves[_waveindex].WaveReward);        
    }

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
        lm = LevelManager.Instance;
        if (GlobalObjectScript.Instance.currentLevel == null)
        {
            //got in here by mistake, quit to main menu
            //print("No level loaded");
            SceneManager.LoadScene("StartMenu");
            return;
        }
        waves = GlobalObjectScript.Instance.currentLevel.waves;
        GlobalObjectScript.Instance.currentLevel.TotalLevelEnemies = 0;

        //endless mode cant calculate how many enemies are left in the level
        if (ENDLESS) GlobalObjectScript.Instance.currentLevel.TotalLevelEnemies = -1;
        else
        {
            foreach (Wave wave in waves)
            {
                foreach (SubWave subWave in wave.SubWaves)
                {
                    GlobalObjectScript.Instance.currentLevel.TotalLevelEnemies += subWave.NumberOfEnemies;
                }
            }
        }
        lastSpawnTime = Time.time;
        
    }

    void Update()
    {
        Countdown();

        //quick way to count enemies on screen?
        //TODO: move this to be called only on enemy death or emergence
        EnemiesOnScreen = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (!ENDLESS)
        {
            //Win game IF last wave called AND no enemies on screen AND all enemies spawned AND we're not paused or anything
            if (lm.CurrentWave >= waves.Count && EnemiesOnScreen <= 0 && AllEnemiesSpawned && lm.GameStatus == StaticEnums.GameEndStatus.OnGoing)
            {
                lm.GameOver(StaticEnums.GameEndStatus.Won);
            }
        }
    }

    void Countdown()
    {
        //only call a wave if we have waves left to call or we're in endless mode
        if (lm.CurrentWave < waves.Count || ENDLESS)
        {
            waveSpawnCountdown -= Time.deltaTime;
            waveCountDownText.text = string.Format("{0:00.0}", waveSpawnCountdown);            
            if (waveSpawnCountdown <= 0f) CallWave(false);
        }
        else
        {
            waveCountDownText.text = "!";
        }
    }

    public void CallWave(bool IsCalledEarly)
    {

        if (ENDLESS)
        {
            //sort this out with a clearer head, probably better to refactor this entire module
            //everything is intertwined: reward system, remaining enemies, which wave is active, etc and to add another endless layer is complicating things needlessly
        }

        //Turtles all the way down...
        //CallWave resets the wave counter and starts a coroutine to spawn the wave, which starts a coroutine to spawn its subwaves, which starts
        // a coroutine to call each creep

        //Calculate early wave bonus, then reset wave countdown: either 5, or half the time remaining, whichever is biggest - Generous Gim
        int earlyReward = Mathf.Max(GlobalObjectScript.Instance.MaxWaveBonus, Mathf.FloorToInt(waveSpawnCountdown /2));
        waveSpawnCountdown = 0;

        if (lm.CurrentWave >= waves.Count) return;  //can be called from outside        

        //if we get here award the bonus and carry on
        if (IsCalledEarly) OnWaveCalledEarly?.Invoke(earlyReward);

        //Calculate surviving enemies of forthcoming waves; ie. all enemies in the wave
        //We do this before any waves are called as we can't guarantee super sayan towers won't kill an entire subwave before the next subwave, and cuasing double wave bonuses
        waves[lm.CurrentWave].SurvivingEnemies = 0;        
        foreach(SubWave subWave in waves[lm.CurrentWave].SubWaves)
        {
            waves[lm.CurrentWave].SurvivingEnemies += subWave.NumberOfEnemies;
        }

        //mark wave as active for various other systems to know
        waves[lm.CurrentWave].Active = true;

        //start wave spawning
        StartCoroutine(SpawnWave(lm.CurrentWave));

        //reset general wave countdown
        waveSpawnCountdown = timeBetweenWaves;

        //as we're using coroutines that have stored their own internal currentwave index we can safely increment the global index
        lm.CurrentWave++;
    }

    IEnumerator SpawnWave(int _currentWave)
    {        
        foreach (SubWave subWave in waves[_currentWave].SubWaves)
        {
            StartCoroutine(SpawnSubWave(subWave.SpawnInterval, subWave.NumberOfEnemies, _currentWave, subWave.EnemyPrefab.name));            
            yield return new WaitForSeconds(subWave.DelayTilNextSubWave);
        }
    }
    
    IEnumerator SpawnSubWave(float spawnInterval, int numberOfEnemies, int _currentWave, string tag)
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            SpawnEnemy(_currentWave, tag);            
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy(int _currentWave, string tag)
    {
        Vector3 spawnPoint = WaypointManager.Instance.Points[0].ToVector3();
        //todo: calc rotation based on points0  and points 1
        GameObject temp = BulletPool.Instance.SpawnFromPool(tag, spawnPoint, Quaternion.identity, this.transform);

        //TODO: add null check

        //TODO rotate spawn immediately towrds next target, annoying having them spin every time they#re instantiated
        EnemyAI temp2 = temp.GetComponent<EnemyAI>();
        temp2.waveIndex = _currentWave;
        temp2.homePool = tag;
        enemiesSpawned++;
        if (enemiesSpawned >= GlobalObjectScript.Instance.currentLevel.TotalLevelEnemies && !ENDLESS) { AllEnemiesSpawned = true; }
        if (!ENDLESS) creepsStillToCome = GlobalObjectScript.Instance.currentLevel.TotalLevelEnemies - enemiesSpawned;
        else { creepsStillToCome = -1; }
    }
}
