using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradesPanelUIManager : MonoBehaviour
{
    //TODO BUG: tested on phone and saw perks doubling between quitting a game and restarting it; also sniper seemed to 'inherit' bullet towers perks...

    //TODO: be nice to see what effect the perks have onscreen, ie we're at 120% damage, next upgrade gets you 125%...

    GlobalObjectScript gos;
    public TextMeshProUGUI perksRemaining;
    public TextMeshProUGUI StashedCash;
    public GameObject TowerPerksPanelPrefab;

    public List<TowerScript> TowersList;

    public static Action OnPerksReclaimed;

    private void Awake()
    {
        gos = GlobalObjectScript.Instance;

        if (gos.metaUpgrades == null) 
        {
            Debug.LogError("No upgrades class found"); 
        }

        PerkPanelTowerScript.OnPerkPurchased += PerkPurchased;
    }

    private void OnDestroy()
    {
        PerkPanelTowerScript.OnPerkPurchased -= PerkPurchased;
    }

    private void Start()
    {
        DisplayCurrentPerkAmount();
        DisplayCurrentStashedCash();
        //foreach (var item in gos.metaUpgrades)
        //{
        //    var temp = Instantiate(TowerPerksPanelPrefab, this.transform);
        //    temp.GetComponent<PerkPanelTowerScript>().towerType = item.TowerType;
        //    //towertype is only a descriptive string like ice bullet missile
        //    //we need to get a towerscript with access to the tower sprites

        //}

        foreach (var item in TowersList)
        {
            var temp = Instantiate(TowerPerksPanelPrefab, this.transform);
            temp.GetComponent<PerkPanelTowerScript>().towerScript = item;
        }

    }

    void PerkPurchased()
    {
        DisplayCurrentPerkAmount();
    }
    
    public void DisplayCurrentPerkAmount()
    {
        perksRemaining.text = gos.storage.CurrentPerks.ToString();
    }

    public void DisplayCurrentStashedCash()
    {
        StashedCash.text = gos.GlobalStats.StashedCash.ToString();
    }

    public void ReturnToMainMenu()
    {
        gos.SaveTheAllFile();
        GlobalObjectScript.Instance.BackToMenuButtonClicked();
        SceneManager.LoadScene("StartMenu");
    }

    public void ConvertCash()
    {
        //can we afford it
        if (gos.GlobalStats.StashedCash >= gos.PerkCost)
        {
            gos.GlobalStats.StashedCash -= gos.PerkCost;
            gos.storage.CurrentPerks++;
            gos.storage.maxPerksSoFar++;
        }
        DisplayCurrentStashedCash();
        DisplayCurrentPerkAmount();
    }

    public void ResetPerks()
    {
        //TODO: pop up warning, and cost of reset = 10%
        //TODO BUG: ow that perks cost more than 1 potentially this method below doesnt work, what we should do is store maxcurrentperks and revert to that and zero the stuff below
        //int perksToReclaim = 0;
        foreach (MetaUpgrades listItem in gos.metaUpgrades)
        {
            //perksToReclaim += listItem.CurrentDamageUpgradeTier;
            //perksToReclaim += listItem.CurrentFirerateUpgradeTier;
            //perksToReclaim += listItem.CurrentRangeUpgradeTier;
            //perksToReclaim += listItem.CurrentUpgradePriceUpgradeTier;
            //perksToReclaim += listItem.CurrentBuildPriceUpgradeTier;

            listItem.CurrentDamageUpgradeTier = 0;
            listItem.CurrentFirerateUpgradeTier = 0;
            listItem.CurrentRangeUpgradeTier = 0;
            listItem.CurrentBuildPriceUpgradeTier = 0;
            listItem.CurrentUpgradePriceUpgradeTier = 0;
        }
        gos.storage.CurrentPerks = gos.storage.maxPerksSoFar;
        DisplayCurrentPerkAmount();
        OnPerksReclaimed?.Invoke();
    }
}
