using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class WrapMeUp
{
    public Incontravertibles perksStorage;
    public List<MetaUpgrades> metaUpgrades = new List<MetaUpgrades>();
    public GameStatistics GlobalStats = new GameStatistics();
    public List<AchievementData> achievementsList = new List<AchievementData>();
}

[System.Serializable]
public class GameStatistics
{
    public int StashedCash;    
    public int CreepsKilled;
    public int BuildingsUpgraded;
    public float CoinsEarned;
    public int CreepsFrozen;
    public int RocketsFired;
    public int BulletsShot;
    public float DamageDealt;
    public int BuildingsBuilt;
    public int BuildingsSold;
    public int LivesLost;
    public int GamesWon;
    public int GamesLost;
    public int WavesCalledEarly;
}

[System.Serializable]
public class Incontravertibles
{
    private int currentPerks;
    public int dummyPerk;
    public int CurrentPerks { get => currentPerks / 2; set { currentPerks = value * 2; dummyPerk = value; } }
    public int maxPerksSoFar;
    //public int HashPerk { get => hashPerk; set => hashPerk = currentPerks.GetHashCode(); }

    //we store 2 values here, a dummy perk with the 'correct' value, and the real perk will be modified each access to throw off data peekers for value changes to see which bits to flip;
    //secretly we'll use the perk /2;
    //public const int maxUnusedPerks = 10; //maybe use to stop hackers figuring out how to manipulate the save files and give free perks
    //private int hashPerk;

    
    //also should implement a checksum on save and save date feature and encrypt too; not necessary but good to learn how to do this stuff
    //maybe stamp the time here on any change and checksum the time, store that, then when reading any sensitive values
    //check the checksum of the timestamp of its change against the checksum stored
}

[System.Serializable]
public class BasicStats
{

}

[System.Serializable]
public class MetaUpgrades
{
    public const int maxMetaUpgradeTiers = 10 + 1; //we have an initial 0 upgrade state, so we account for that
    public string TowerType;

    private int currentDamageUpgradeTier;
    private int currentFirerateUpgradeTier;
    private int currentRangeUpgradeTier;
    private int currentBuildPriceTier;
    private int currentUpgradePriceTier;
    public int CurrentDamageUpgradeTier { get => currentDamageUpgradeTier; set => currentDamageUpgradeTier = Mathf.Min(maxMetaUpgradeTiers, value); }
    public int CurrentRangeUpgradeTier { get => currentRangeUpgradeTier; set => currentRangeUpgradeTier = Mathf.Min(maxMetaUpgradeTiers, value); }
    public int CurrentFirerateUpgradeTier { get => currentFirerateUpgradeTier; set => currentFirerateUpgradeTier = Mathf.Min(maxMetaUpgradeTiers, value); }
    public int CurrentBuildPriceUpgradeTier { get => currentBuildPriceTier; set => currentBuildPriceTier = Mathf.Min(maxMetaUpgradeTiers, value); }
    public int CurrentUpgradePriceUpgradeTier { get => currentUpgradePriceTier; set => currentUpgradePriceTier = Mathf.Min(maxMetaUpgradeTiers, value); }

    public float[] rangeMetaUpgrade = new float[maxMetaUpgradeTiers];    
    public float[] damageMetaUpgrade = new float[maxMetaUpgradeTiers];    
    public float[] firerateMetaUpgrade = new float[maxMetaUpgradeTiers];
    public float[] buildPriceMetaUpgrade = new float[maxMetaUpgradeTiers];
    public float[] upgradePriceMetaUpgrade = new float[maxMetaUpgradeTiers];

}

[System.Serializable]
public class AchievementData
{
    public string name;
    public string description;
    public bool IsLocked = false;
    public bool IsHidden = false;
    public bool IsAchieved = false;
    public int reward = 1; //how many perks to give
    public float progress = 0f;  //stat to increase
    public float successThreshold;
    public string statToMeasure;
}
[System.Serializable]
public struct Vector2S
{
    public float x;
    public float y;

    public Vector2S(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Vector2S))
        {
            return false;
        }

        var s = (Vector2S)obj;
        return x == s.x &&
               y == s.y;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, 0f);
    }


    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public static bool operator ==(Vector2S a, Vector2S b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vector2S a, Vector2S b)
    {
        return a.x != b.x && a.y != b.y;
    }

    public static implicit operator Vector2(Vector2S x)
    {
        return new Vector2(x.x, x.y);
    }

    public static implicit operator Vector2S(Vector2 x)
    {
        return new Vector2S(x.x, x.y);
    }

    
}
