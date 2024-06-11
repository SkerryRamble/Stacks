using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using UnityEngine.EventSystems;

public class ShopPanelCellScript : MonoBehaviour, IDeselectHandler
{
    public GameObject Tower;
    private TowerScript tscript;
    public Button button;
    public TextMeshProUGUI price;
    public TextMeshProUGUI damage;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI range;

    public AudioSource source;
    public AudioClip towerSelected;
    public AudioClip dudSound;
    private Color originalColor;
    [SerializeField] private Color PendingPurchaseColor;
    float buildPriceMult;

    private void Awake()
    {
        source = GetComponentInParent<AudioSource>();
    }

    private void Start()
    {
        source = GetComponentInParent<AudioSource>();

        originalColor = GetComponent<Image>().color;
        tscript = Tower.GetComponent<TowerScript>();

        if (tscript.ThisTowerType == "ice") UIManager.OnBuildIce += TowerHotkeyyed;
        if (tscript.ThisTowerType == "bullet") UIManager.OnBuildBullet += TowerHotkeyyed;
        if (tscript.ThisTowerType == "missile") UIManager.OnBuildMissile += TowerHotkeyyed;
        if (tscript.ThisTowerType == "sniper") UIManager.OnBuildSniper += TowerHotkeyyed;
        if (tscript.ThisTowerType == "fire") UIManager.OnBuildFire += TowerHotkeyyed;
        PopulateTowerDataInShopPanel();
    }

    private void OnDestroy()
    {
        if (tscript.ThisTowerType == "ice") UIManager.OnBuildIce -= TowerHotkeyyed;
        if (tscript.ThisTowerType == "bullet") UIManager.OnBuildBullet -= TowerHotkeyyed;
        if (tscript.ThisTowerType == "missile") UIManager.OnBuildMissile -= TowerHotkeyyed;
        if (tscript.ThisTowerType == "sniper") UIManager.OnBuildSniper -= TowerHotkeyyed;
        if (tscript.ThisTowerType == "fire") UIManager.OnBuildFire -= TowerHotkeyyed;
    }

    private void PopulateTowerDataInShopPanel()
    {
        //we need the tower and we can get its base stats
        MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == tscript.ThisTowerType);
        float damageMult = temp.damageMetaUpgrade[temp.CurrentDamageUpgradeTier];
        float rangeMult = temp.rangeMetaUpgrade[temp.CurrentRangeUpgradeTier];
        float firerateMult = temp.firerateMetaUpgrade[temp.CurrentFirerateUpgradeTier];
        buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];
        float upgradePriceMult = temp.upgradePriceMetaUpgrade[temp.CurrentUpgradePriceUpgradeTier];

        button.image.sprite = tscript.upgradeSprites[0];
        price.text = (buildPriceMult * tscript.costToBuild[0]).ToString();
        damage.text = (damageMult * tscript.baseDamageArray[0]).ToString();
        range.text = (rangeMult * tscript.range[0]).ToString();
        rate.text = (firerateMult * tscript.fireRate[0]).ToString();

        button.onClick.AddListener(delegate { SelectTower(Tower); });        
    }

    private void TowerHotkeyyed()
    {
        SelectTower(Tower);
    }

    public void SelectTower(GameObject tower)
    {
        if (source == null) source = GetComponentInParent<AudioSource>();

        //add check for cash being available
        
        if (LevelManager.Instance.Bank >= (buildPriceMult * tscript.costToBuild[0])){

            source.PlayOneShot(towerSelected);
            BuildManager.Instance.SetTowerToBuild(tower);
            EventSystem.current.SetSelectedGameObject(this.gameObject);
            this.GetComponent<Image>().color = PendingPurchaseColor;
        }
        else
        {
            source.PlayOneShot(dudSound);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        this.GetComponent<Image>().color = originalColor;
        BuildManager.Instance.SetTowerToBuild(null);
        BuildManager.Instance.BuildMode = StaticEnums.BuildMode.Nothing;
    }
}
