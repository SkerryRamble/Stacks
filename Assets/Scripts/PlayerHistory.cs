using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerHistory : MonoSingleton<PlayerHistory>
{
    public Dictionary<string, Sprite> achievementArtworkDict = new Dictionary<string, Sprite>();
    public static Action<AchievementData> AchievementGranted;
    private GlobalObjectScript gos;

    //TODO remove events on destroy or similar

    private void Start()
    {    
        gos = GlobalObjectScript.Instance;
        EnemyAI.OnDamageReceived += DamageDealt;
        EnemyAI.OnKilled += CreepKilled;
        EnemyAI.OnReachedBase += ReachedBase;
        TowerBaseController.OnTowerBuilt += TowerBuilt;
        TowerBaseController.OnTowerSold += TowerSold;
        TowerBaseController.OnTowerUpgraded += TowerUpgraded;
        LevelManager.OnGameFinished += GameFinished;
        TowerScript.OnMissileFired += MissileFired;
        TowerScript.OnBulletShot += BulletShot;
        SpawnManager.OnWaveDestroyed += WaveDestroyed;
        SpawnManager.OnWaveCalledEarly += WaveCalledEarly;
        //ClearGameStats();
    }

    private void OnDisable()
    {
        EnemyAI.OnDamageReceived -= DamageDealt;
        EnemyAI.OnKilled -= CreepKilled;
        EnemyAI.OnReachedBase -= ReachedBase;
        TowerBaseController.OnTowerBuilt -= TowerBuilt;
        TowerBaseController.OnTowerSold -= TowerSold;
        TowerBaseController.OnTowerUpgraded -= TowerUpgraded;
        LevelManager.OnGameFinished -= GameFinished;
        TowerScript.OnMissileFired -= MissileFired;
        TowerScript.OnBulletShot -= BulletShot;
        SpawnManager.OnWaveDestroyed -= WaveDestroyed;
        SpawnManager.OnWaveCalledEarly -= WaveCalledEarly;
    }

    #region Action Handlers
    private void GameFinished(StaticEnums.GameEndStatus gameEndStatus)
    {
        bool isDirty = false;
        if (gameEndStatus == StaticEnums.GameEndStatus.Won)
        {
            gos.GlobalStats.GamesWon++;

            
            foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
            {
                if (item.statToMeasure == "winGames" && !item.IsAchieved && gameEndStatus == StaticEnums.GameEndStatus.Won)
                {
                    item.progress++;
                    isDirty = true;
                }
            }
        }

        if (gameEndStatus == StaticEnums.GameEndStatus.Lost) gos.GlobalStats.GamesLost++;

       
        if (isDirty) CheckProgress();
    }

    private void WaveDestroyed(int reward)
    {
        gos.GlobalStats.CoinsEarned += reward;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "wavesDestroyed" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }

    private void WaveCalledEarly(int reward)
    {
        gos.GlobalStats.CoinsEarned += reward;
        gos.GlobalStats.WavesCalledEarly++;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "wavesCalledEarly" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }

    private void MissileFired()
    {
        //gos.currentLevelStats.MissileFired++;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "missilesFired" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }

    private void BulletShot()
    {
        gos.GlobalStats.BulletsShot++;

        //bit much to do this mid game, maybe once in a while, or just at the end of the level?
        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "bulletsShot" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }

    private void ReachedBase()
    {
        gos.GlobalStats.LivesLost++;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "baseReached" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }

    private void TowerBuilt()
    {
        gos.GlobalStats.BuildingsBuilt++;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "towersBuilt" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }

    private void TowerSold()
    {
        gos.GlobalStats.BuildingsSold++;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "towersSold" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }
    
    private void TowerUpgraded()
    {
        gos.GlobalStats.BuildingsUpgraded++;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "towersUpgraded" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }

    private void CreepKilled(int reward, int nully)    //TODO: find out how to make optional parameter in Action;probably need to use delegate instead
    {
        gos.GlobalStats.CreepsKilled++;
        gos.GlobalStats.CoinsEarned += reward;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "enemiesKilled" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }

    private void DamageDealt(float damage)
    {
        gos.GlobalStats.DamageDealt += damage;

        bool isDirty = false;
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.statToMeasure == "damageReceived" && !item.IsAchieved)
            {
                item.progress++;
                isDirty = true;
            }
        }
        if (isDirty) CheckProgress();
    }
    #endregion

    public void Statistics()
    {
        //TODO: store general stats about tower kills, games played, won lost total damage etc

    }

    public void ClearGameStats()
    {
        gos.GlobalStats = new GameStatistics();
    }
    
    public void EraseHistory()
    {//TODO: are you sure hcheck
        
    }

    public void CheckProgress()
    {
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (item.progress >= item.successThreshold && !item.IsAchieved)
            {
                //Debug.Log(item.name + " has been achieved!");
                item.IsAchieved = true;
                item.IsHidden = false; //Make visible any secret achievements;
                GlobalObjectScript.Instance.storage.CurrentPerks += item.reward;
                GlobalObjectScript.Instance.storage.maxPerksSoFar += item.reward;
                AchievementGranted?.Invoke(item);
            }
        }
    }
}
