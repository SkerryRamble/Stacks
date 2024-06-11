using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SubWave //: ScriptableObject
//a wave can consist of 1 or more subwaves of different enemy types
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float delayTilNextSubWave;
    [SerializeField] private float spawnInterval;
    [SerializeField] private int numberOfEnemies;

    public GameObject EnemyPrefab { get => enemyPrefab; set => enemyPrefab = value; }
    public float DelayTilNextSubWave { get => delayTilNextSubWave; set => delayTilNextSubWave = value; }
    public float SpawnInterval { get => spawnInterval; set => spawnInterval = value; }
    public int NumberOfEnemies { get => numberOfEnemies; set => numberOfEnemies = value; }

}

[System.Serializable]
public class Wave //: ScriptableObject
//a wave consists of subwaves (1 enemy type, specific amount, spawn rate) which can start at different times throughout the wave(initialDelay)
{
    public bool Active { get; set; } = false;
    public int SurvivingEnemies { get; set; }

    [SerializeField] private List<SubWave> subWaves;
    [SerializeField] private int waveReward;
    [SerializeField] private float difficultyMultiplier;

    public List<SubWave> SubWaves { get => subWaves; set => subWaves = value; }
    public int WaveReward { get => waveReward; set => waveReward = value; }
    public float DifficultyMultiplier { get => difficultyMultiplier; set => difficultyMultiplier = value; }
}

[CreateAssetMenu(fileName = "New Level", menuName = "Level")]
public class Level : ScriptableObject
{
    [Header("Level Starting Resources")]
    [SerializeField] private int levelID; //TODO: have this automated out to auto increment somehow
    [SerializeField] private int startingCash;
    [SerializeField] private int startingLives;    
    public int StartingCash { get => startingCash; set => startingCash = value; }
    public int StartingLives { get => startingLives; set => startingLives = value; }
    public int LevelID { get => levelID; set => levelID = value; }

    public int TotalLevelEnemies { get; set; }
    [Header("Location coords")]
    [SerializeField] private List<Vector2S> pathPoints;
    [SerializeField] private List<Vector2S> towerBasePoints;
    public List<Vector2S> PathPoints { get => pathPoints; set => pathPoints = value; }
    public List<Vector2S> TowerBasePoints { get => towerBasePoints; set => towerBasePoints = value; }

    [Header("Wave Sequence")]
    public List<Wave> waves;
}

//public class ListOfAllLevels
//{
//    public List<Level> Levels;
//}




