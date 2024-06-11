using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//Can't use the monosingleton here as DontDestroyOnLoad requires special handling

public class GlobalObjectScript : MonoBehaviour
{
    // public TestScript testSOcon;

    //TheAllFile will contain all the bits below
    private string theAllFileFilename = "/theallfile.dat";
    public WrapMeUp TheAllFile = new WrapMeUp();
    public List<MetaUpgrades> metaUpgrades = new List<MetaUpgrades>();
    public Incontravertibles storage = new Incontravertibles(); //store perks
    //public AllAchievements achievements = new AllAchievements();
    public List<AchievementData> achievementsList = new List<AchievementData>();
    public List<AchievementDataSO> firstRunAchievementsList = new List<AchievementDataSO>();    //stores copy of zeroed achievements for first run
    public GameStatistics GlobalStats = new GameStatistics();       //Overall stats across games

    //game session level stats, temp only, to be copied to globalstats on pause or level end
    //public GameStatistics currentLevelStats = new GameStatistics();  //Current game stats; preserve between sessions if paused and closed mid game

    public Level currentLevel;
    public static GlobalObjectScript Instance;

    private AudioSource source;
    public AudioClip menuButtonSound;
    public AudioClip returnToMainMenuSound;

    public int PerkCost { get; set; } = 10000; //TODO: make this perkable
    public int MaxWaveBonus { get; set; } = 5; //TODO make this perkable

    //TODO: just check if allfile exists, if not then populate dummy version to be used in first game, then subsequent games can just load up data
    void Awake()
    {
        if (Instance == null)
        {   //if no clone of this exists then mark this to be not destroyed and identify it as the instance
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {   //or else destroy the imposter clone!
            Destroy(gameObject);
        }

        source = GetComponent<AudioSource>();

        DetectGraphicsSettings();

    }

    private void DetectGraphicsSettings()
    {
        //TODO: detect screen res for mobile 1920x1080 is too much for my shield, so we want to start with 720p minimum and upgrade auto if poss
        

    }

    public void MenuButtonClicked()
    {
        source.PlayOneShot(menuButtonSound);
    }

    public void BackToMenuButtonClicked()
    {
        source.PlayOneShot(returnToMainMenuSound);
    }

    private void Start()
    {

        //AchievementDataSO test = testSOcon.GetComponent<AchievementDataSO>();


        //TODO:See if game was paused then closed/saved to reload last currentlevel stats
        //TODO a lot needs to be done before then like record status of waves and position of creeps/towers etc.
        //TODO: probably need to implement the command pattern to record stuff or just devise a snapshot method
        //Snapshot with stats seems most efficient. loop through all creeps on screen with stats and store that, then do same for towers and game stats like cash etc
        //need methods to gather gamestate; save gamestate; read gamestate; recreate gamestate
        //breakdown>>> gather towers/upgrades/stats; waves completed; waves on screen; game stats...bleurgh

        //set up metaupgrades default, for use in first game. will be overwritten as levels are played
        PopulateMetaUpgrades();


        //load the all file and extract the bits we want into our specialised classes
        LoadTheAllFile();
        GlobalStats = TheAllFile.GlobalStats;
        storage = TheAllFile.perksStorage;

        if (TheAllFile.metaUpgrades.Count == metaUpgrades.Count)
        {
            metaUpgrades = TheAllFile.metaUpgrades;
        }


        //first time running? if theallfile already has a valid version of achievements we use that, otherwise copy over the SO versions to the current gos version
        //for some unknown reason the firstrunachievementslist is empty on build, it should have a bunch of SOs in it
        if (firstRunAchievementsList == null || firstRunAchievementsList.Count == 0)
        {
            Debug.LogError("no achievements loaded!");
        }



        if (TheAllFile.achievementsList.Count == firstRunAchievementsList.Count)
        { 
            //load achievements from file
            achievementsList = TheAllFile.achievementsList; 
        }
        else
        {
            //reset achievements from achievement template: firstrunachievements
            ResetAchievements();
           
        }

    }

    private void ResetAchievements()
    {
        //copy over firstrun data to current achievements in gos
        //first, clear gos achievements
        achievementsList = new List<AchievementData>();
        achievementsList.Clear();

        //should be empty, ie count=0, we need to create the items to add to it

        foreach (AchievementDataSO item in firstRunAchievementsList)
        {
            AchievementData temp = new AchievementData()
            {
                description = item.Description,
                IsAchieved = item.IsAchieved,
                IsHidden = item.IsHidden,
                IsLocked = item.IsLocked,
                name = item.AchievementName,
                progress = item.Progress,
                reward = item.Reward,
                statToMeasure = item.StatToMeasure,
                successThreshold = item.SuccessThreshold
            };
            achievementsList.Add(temp);
        }
    }

    //#if UNITY_EDITOR
    //    private void OnGUI()
    //    {
    //        GUIStyle styleBtn = GUI.skin.GetStyle("button");
    //        styleBtn.fontSize = 24;
    //        GUIStyle styleLabel = GUI.skin.GetStyle("label");
    //        styleLabel.fontSize = 24;
    //        GUI.Label(new Rect(1820, 980, 100, 100), AverageFPS.ToString(), "label");
    //    }
    //#endif

    //    #region FPS Calculations

    //    public int frameRange = 60;
    //    public int AverageFPS { get; private set; }
    //    public int HighestFPS { get; private set; }
    //    public int LowestFPS { get; private set; }
    //    int[] fpsBuffer;
    //    int fpsBufferIndex;

    //    void Update()
    //    {
    //        if (fpsBuffer == null || fpsBuffer.Length != frameRange)
    //        {
    //            InitializeBuffer();
    //        }
    //        UpdateBuffer();
    //        CalculateFPS();
    //    }

    //    void InitializeBuffer()
    //    {
    //        if (frameRange <= 0)
    //        {
    //            frameRange = 1;
    //        }
    //        fpsBuffer = new int[frameRange];
    //        fpsBufferIndex = 0;
    //    }

    //    void UpdateBuffer()
    //    {
    //        fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
    //        if (fpsBufferIndex >= frameRange)
    //        {
    //            fpsBufferIndex = 0;
    //        }
    //    }

    //    void CalculateFPS()
    //    {
    //        int sum = 0;
    //        int highest = 0;
    //        int lowest = int.MaxValue;
    //        for (int i = 0; i < frameRange; i++)
    //        {
    //            int fps = fpsBuffer[i];
    //            sum += fps;
    //            if (fps > highest)
    //            {
    //                highest = fps;
    //            }
    //            if (fps < lowest)
    //            {
    //                lowest = fps;
    //            }
    //        }
    //        AverageFPS = sum / frameRange;
    //        HighestFPS = highest;
    //        LowestFPS = lowest;
    //    }

    //    #endregion

    #region SaveLoad All
    public void LoadTheAllFileJSON()
    {
        string jsonLoadText = File.ReadAllText(Application.persistentDataPath + theAllFileFilename + ".txt");
        TheAllFile = JsonUtility.FromJson<WrapMeUp>(jsonLoadText);
    }

    public void SaveTheAllFileJSON()
    {
        //ensure all the components of theallfile are as current as they can be
        TheAllFile.perksStorage = storage;
        TheAllFile.achievementsList = achievementsList;
        TheAllFile.GlobalStats = GlobalStats;
        TheAllFile.metaUpgrades = metaUpgrades;

        string jsonSaveString = JsonUtility.ToJson(TheAllFile);
        File.WriteAllText(Application.persistentDataPath + theAllFileFilename + ".txt.", jsonSaveString);
    }

    public void LoadTheAllFile()
    {
        if (File.Exists(Application.persistentDataPath + theAllFileFilename))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + theAllFileFilename, FileMode.OpenOrCreate);
            TheAllFile = (WrapMeUp)bf.Deserialize(file);
            file.Close();
        }
    }

    public void SaveTheAllFile()
    {
        //ensure all the components of theallfile are as current as they can be
        TheAllFile.perksStorage = storage;
        TheAllFile.achievementsList = achievementsList;
        TheAllFile.GlobalStats = GlobalStats;
        TheAllFile.metaUpgrades = metaUpgrades;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + theAllFileFilename, FileMode.OpenOrCreate);
        bf.Serialize(file, TheAllFile);
        file.Close();
    }

    #endregion

    void PopulateMetaUpgrades() //also resets them, handy
    {
        metaUpgrades = new List<MetaUpgrades>();

        string[] TowerTypes = { "ice", "bullet", "missile", "sniper", "fire"};   //TODO use buildmanager to control towertypes

        for (int i = 0; i < TowerTypes.Length; i++)
        {
            MetaUpgrades temp = new MetaUpgrades();
            temp.TowerType = TowerTypes[i];
            temp.CurrentDamageUpgradeTier = 0;    //default, we'll overwrite this later if saved data says so
            temp.CurrentFirerateUpgradeTier = 0;    //default, we'll overwrite this later if saved data says so
            temp.CurrentRangeUpgradeTier = 0;    //default, we'll overwrite this later if saved data says so
            temp.CurrentBuildPriceUpgradeTier = 0;
            temp.CurrentUpgradePriceUpgradeTier = 0;

            //set up the gains based on max upgrade levels
            float normalGain = 1f/(MetaUpgrades.maxMetaUpgradeTiers - 1);
            float doubleGain = 2f/(MetaUpgrades.maxMetaUpgradeTiers - 1);
            float halfGain = 0.5f/(MetaUpgrades.maxMetaUpgradeTiers - 1);

            for (int j = 0; j < MetaUpgrades.maxMetaUpgradeTiers; j++)
            {
                temp.damageMetaUpgrade[j] = 1f + j * normalGain;
                temp.rangeMetaUpgrade[j] = 1f + j * doubleGain;
                temp.firerateMetaUpgrade[j] = 1f + j * normalGain;
                temp.buildPriceMetaUpgrade[j] = 1f - (j * halfGain);    //reduced
                temp.upgradePriceMetaUpgrade[j] = 1f - (j * halfGain);  //reduced
            }
            metaUpgrades.Add(temp);
        }

        //perks bits
        storage = new Incontravertibles { CurrentPerks = 0, maxPerksSoFar = 0 };
    }

    public void ResetPerks()
    {
        storage = new Incontravertibles { CurrentPerks = 0, maxPerksSoFar = 0 };
    }

    public void ResetStats()
    {
        GlobalStats = new GameStatistics();
        ResetAchievements();
        ResetPerks();
        SaveTheAllFile();
    }
}



