using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class PerkPanelTowerScript : MonoBehaviour
{
    //used to pass tower type to panel buttons and icons etc and build the panel

    //public string towerType;
    public TowerScript towerScript;
    public Image towerIcon; //can be gotten from the towerscript?
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI rateText;
    public TextMeshProUGUI priceText;
    //public Button rangeButton;
    //public Button rateButton;
    //public Button damageButton;
    //public Button priceButton;
    private MetaUpgrades thisTowerTypeUpgrades;
    private GlobalObjectScript gos;

    public int perkPurchasePrice = 1;   //TODO impl this, add each perk price to the metaupgrades object

    public static Action OnPerkPurchased;

    private void Awake()
    {
        gos = GlobalObjectScript.Instance;
        UpgradesPanelUIManager.OnPerksReclaimed += RefreshText;
    }

    private void OnDestroy()
    {
        UpgradesPanelUIManager.OnPerksReclaimed -= RefreshText;
    }

    private void Start()
    {
        thisTowerTypeUpgrades = gos.metaUpgrades.Find(x => x.TowerType == towerScript.ThisTowerType);
        towerIcon.sprite = towerScript.upgradeSprites[towerScript.maxUpgradePublic-1];

        RefreshText();
    
    }

    private void RefreshText()
    {


        damageText.text = thisTowerTypeUpgrades.CurrentDamageUpgradeTier.ToString();
        rangeText.text = thisTowerTypeUpgrades.CurrentRangeUpgradeTier.ToString();
        rateText.text = thisTowerTypeUpgrades.CurrentFirerateUpgradeTier.ToString();
        priceText.text = thisTowerTypeUpgrades.CurrentBuildPriceUpgradeTier.ToString();
    }

    public void BuyRange()
    {
        if (thisTowerTypeUpgrades.CurrentRangeUpgradeTier >= MetaUpgrades.maxMetaUpgradeTiers-1) return;
        if (PurchasePerkIfAffordable(thisTowerTypeUpgrades.CurrentRangeUpgradeTier + 1)) thisTowerTypeUpgrades.CurrentRangeUpgradeTier++;
        rangeText.text = thisTowerTypeUpgrades.CurrentRangeUpgradeTier.ToString();
    }

    public void BuyPrice()
    {
        if (thisTowerTypeUpgrades.CurrentBuildPriceUpgradeTier >= MetaUpgrades.maxMetaUpgradeTiers-1) return;
        if (PurchasePerkIfAffordable(thisTowerTypeUpgrades.CurrentBuildPriceUpgradeTier + 1)) thisTowerTypeUpgrades.CurrentBuildPriceUpgradeTier++;
        priceText.text = thisTowerTypeUpgrades.CurrentBuildPriceUpgradeTier.ToString();
    }

    public void BuyDamage()
    {
        if (thisTowerTypeUpgrades.CurrentDamageUpgradeTier >= MetaUpgrades.maxMetaUpgradeTiers-1) return;
        if (PurchasePerkIfAffordable(thisTowerTypeUpgrades.CurrentDamageUpgradeTier + 1)) thisTowerTypeUpgrades.CurrentDamageUpgradeTier++;
        damageText.text = thisTowerTypeUpgrades.CurrentDamageUpgradeTier.ToString();
    }

    public void BuyRate()
    {
        if (thisTowerTypeUpgrades.CurrentFirerateUpgradeTier >= MetaUpgrades.maxMetaUpgradeTiers-1) return;
        if (PurchasePerkIfAffordable(thisTowerTypeUpgrades.CurrentFirerateUpgradeTier + 1)) thisTowerTypeUpgrades.CurrentFirerateUpgradeTier++;
        rateText.text = thisTowerTypeUpgrades.CurrentFirerateUpgradeTier.ToString();
    }

    public bool PurchasePerkIfAffordable(int adjustedPrice)
    {
        if (gos.storage.CurrentPerks - adjustedPrice >= 0)
        {
            gos.storage.CurrentPerks -= adjustedPrice;
            OnPerkPurchased?.Invoke();
            return true;
        }
        return false;
    }

}
