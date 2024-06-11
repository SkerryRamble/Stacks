
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TowerBaseController : MonoBehaviour
{
    private AudioSource source;
    public AudioClip built;
    public AudioClip upgraded;
    public AudioClip sold;
    public AudioClip towerSelected;
    BuildManager bm;

    [Tooltip("Cant assign via editor, must be assigned via script")]
    public GameObject TowerSummaryUIPanel;
    public Transform TowersContainerTransform;
    public GameObject SingleTowerInfoPrefab;

    private float towerInUseFadedAlpha = 0.05f;
    private Color BuildColor = Color.green;
    private Color UpgradeColour = Color.blue;
    private Color SellAllowedColor = Color.red;
    private Color OriginalColor = Color.white;
    private Vector4 BuildHDRColor = new Vector4(0, 32, 0, 0);
    private Vector4 UpgradeHDRColor = new Vector4(0, 0, 32, 0);
    private Vector4 SellHDRColor = new Vector4(32, 0, 0, 0);
    private Vector4 OriginalHDRColor = new Vector4(1, 1, 1, 0);
    private SpriteRenderer sr;
    public int MaxTowersAllowedHere;   //changes according to TowerBase_X
    private int currentTowersBuiltHere = 0;
    private LevelManager lm;
    private Animator animator;
    private Animator towerSummaryPanelAnimator;


    public static Action OnTowerBuilt;
    public static Action OnTowerUpgraded;
    public static Action OnTowerSold;

    //TODO: for now upgrade price has been overtaken by build price. The way it works is there is an initial cost to build, and subsequent 'build' costs for that tower are
    //considered upgrade costs. So any perks relating to build are also for upgrade. Probably can remove upgrade stuff as serious dose of CBA for all that. I'll leave the dormant code
    //in case i change my mind

    private void Start()
    {
        bm = BuildManager.Instance;
        lm = LevelManager.Instance;
        sr = this.GetComponent<SpriteRenderer>();
        animator = this.GetComponent<Animator>();
        source = GetComponent<AudioSource>();

        TowerSummaryUIPanel = GameObject.Find("TowerSummaryUIPanel");
        towerSummaryPanelAnimator = TowerSummaryUIPanel.GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        //TowerBase has been clicked on, so we:
        // 1 - Pop up the Tower Summary panel with any towers built on this locations details
        // 2 - Play a sound effect and apply a little animation to the panel for feedback to the player to let them know their selection had an effect    

        InitTowerSummaryPrefab();
        towerSummaryPanelAnimator.SetTrigger("towerSelected");

        //Depending on what buildmode we're in, we do that thing.
        switch (bm.BuildMode)
        {
            case StaticEnums.BuildMode.Nothing:
                //Sound effect is applied here when build mode is nothing, audio feedback that the player clicked something but nothing happened
                source.PlayOneShot(towerSelected);
                return;
            case StaticEnums.BuildMode.Building:
                //Sound effect calls are in the Build/Sell/Upgrade methods, as they can be called from elsewhere
                BuildTower();
                break;
            case StaticEnums.BuildMode.Selling:
                SellTower();
                break;
            case StaticEnums.BuildMode.Upgrading:
                UpgradeTower();
                break;
            default:
                break;
        }
    }

    private void BuildTower()
    {
        //The build manager hols the tower type we want to build
        GameObject towerToBuild = bm.GetTowerToBuild();

        //Any built item will start at upgrade tier of zero. 
        int upgradeTier = 0;

        //The towerscript of the tower we want to build has info about its cost etc
        TowerScript tscript = towerToBuild.GetComponent<TowerScript>();

        //we need the tower and we can get its base stats
        MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == tscript.ThisTowerType);
        float buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];

        //Can we afford to build this tower
        if (lm.Bank >= tscript.costToBuild[upgradeTier] * buildPriceMult)
        //if (lm.Bank >= (tscript.costToBuild[upgradeTier] * tscript.buildPriceMult)) //TODO any multpliers probably need to come from metaupgrades thing
            //Is there space on this stack to build any towers?
            if (currentTowersBuiltHere < MaxTowersAllowedHere)
            {
                PlaceTower();
                source.PlayOneShot(towerSelected);
            }
    }

    private void SellTower()
    {
        //Are there any towers to sell?
        if (TowersContainerTransform.childCount <= 0) return;

        //Get the top tower on the stack, and its towerscript
        Transform t = TowersContainerTransform.GetChild(TowersContainerTransform.childCount - 1);
        TowerScript tscript = t.GetComponent<TowerScript>();

        MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == tscript.ThisTowerType);
        float buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];

        //reimburse the player for a value % of the towers worth
        lm.Bank += Mathf.RoundToInt(lm.SellPercentage * (tscript.costToBuild[0] * buildPriceMult)); //TODO fix this, need an acucmulated tower worht maybe?

        //remove and destroy the tower
        t.parent = null;
        Destroy(t.gameObject);

        //keep track of how many towers are now on this stack
        currentTowersBuiltHere--;

        //let whoever needs to know that this tower has been sold
        OnTowerSold?.Invoke();

        //sound effect for tower sold; at the minute the nicest sound effect is towerSelected which sounds vague enough to do many sound effect jobs
        source.PlayOneShot(towerSelected);
        
        //If there are no towers left we restore the towerbase animation and colour to their originals
        if (TowersContainerTransform.childCount < 1)
        {
            OriginalColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, 1f);
            HighlightBuildLocation(OriginalColor, OriginalHDRColor);
            animator.speed = 1;
        }

        //Refresh the tower summary panel
        InitTowerSummaryPrefab();
    }

    private void UpgradeTower()
    {
        if (TowersContainerTransform.childCount > 0)
        {
            Transform t = TowersContainerTransform.GetChild(TowersContainerTransform.childCount - 1);
            TowerScript tscript = t.GetComponent<TowerScript>();

            MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == tscript.ThisTowerType);
            //float upgradePriceMult = temp.upgradePriceMetaUpgrade[temp.CurrentUpgradePriceUpgradeTier];
            float buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];

            if (tscript.currentUpgradeTier < tscript.maxUpgradePublic - 1
                && lm.Bank >= (tscript.costToBuild[tscript.currentUpgradeTier + 1] * buildPriceMult))
            {
                lm.Bank -= Mathf.RoundToInt(tscript.costToBuild[tscript.currentUpgradeTier + 1] * buildPriceMult);
                tscript.UpgradeMe();
                OnTowerUpgraded?.Invoke();
                source.PlayOneShot(towerSelected);
            }
        }
    }

    private void UpgradeSpecificTower(TowerScript tscript)
    {
        if (TowersContainerTransform.childCount > 0)
        {
            MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == tscript.ThisTowerType);
            //float upgradePriceMult = temp.upgradePriceMetaUpgrade[temp.CurrentUpgradePriceUpgradeTier];
            float buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];
            if (tscript.currentUpgradeTier < tscript.maxUpgradePublic - 1
                && lm.Bank >= (tscript.costToBuild[tscript.currentUpgradeTier + 1] * buildPriceMult))
            {
                lm.Bank -= Mathf.RoundToInt(tscript.costToBuild[tscript.currentUpgradeTier + 1] * buildPriceMult);
                tscript.UpgradeMe();
                OnTowerUpgraded?.Invoke();
                source.PlayOneShot(towerSelected);
            }
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (TowersContainerTransform.childCount > 0)// && TowerSummaryCanvas.transform.childCount == 0)
            {
                InitTowerSummaryPrefab();
            }
            return;
        }

        MouseHighlight();
    }

    void MouseHighlight()
    {

        //The initial pause menu hides the towerbases but they're still causing mouse events and bm might not yet be up for that
        if (bm == null) { return; }
        switch (bm.BuildMode)
        {
            case StaticEnums.BuildMode.Nothing:
                HighlightBuildLocation(OriginalColor, OriginalHDRColor);
                return;
            case StaticEnums.BuildMode.Building:
                GameObject towerToBuild = bm.GetTowerToBuild();

                MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == towerToBuild.GetComponent<TowerScript>().ThisTowerType);
                float buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];

                if (lm.Bank >= (towerToBuild.GetComponent<TowerScript>().costToBuild[0]) * buildPriceMult)
                    if (currentTowersBuiltHere < MaxTowersAllowedHere)
                    {
                        HighlightBuildLocation(BuildColor, BuildHDRColor);
                    }
                break;
            case StaticEnums.BuildMode.Selling:
                if (TowersContainerTransform.childCount > 0)
                {
                    HighlightBuildLocation(SellAllowedColor, SellHDRColor);
                }
                break;
            case StaticEnums.BuildMode.Upgrading:
                if (TowersContainerTransform.childCount > 0)
                {
                    Transform t = TowersContainerTransform.GetChild(TowersContainerTransform.childCount - 1);
                    TowerScript tscript = t.GetComponent<TowerScript>();

                    MetaUpgrades temp2 = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == tscript.ThisTowerType);
                    float upgradePriceMult = temp2.upgradePriceMetaUpgrade[temp2.CurrentUpgradePriceUpgradeTier];

                    if (tscript.currentUpgradeTier < tscript.maxUpgradePublic - 1
                        && lm.Bank >= (tscript.costToBuild[tscript.currentUpgradeTier + 1] * upgradePriceMult))
                    {
                        HighlightBuildLocation(UpgradeColour, UpgradeHDRColor);
                    }
                    else
                    {
                        HighlightBuildLocation(OriginalColor, OriginalHDRColor);
                    }
                }
                break;
            default:
                HighlightBuildLocation(OriginalColor, OriginalHDRColor);
                break;
        }
    }

    private void OnMouseExit()
    {
        if (bm.BuildMode == StaticEnums.BuildMode.Nothing) return; //TODO probably need to make this more robust for other buildmodes
        HighlightBuildLocation(OriginalColor, OriginalHDRColor);
    }

    private void HighlightBuildLocation(Color _col, Vector4 _HDRcol)
    {
        sr.color = _col;
        sr.material.SetColor("Color_FB31EF4", _HDRcol);
    }

    private void PlaceTower()
    {
        currentTowersBuiltHere = TowersContainerTransform.childCount;
        GameObject towerToBuild = bm.GetTowerToBuild();

        //scale z down/up according to child count
        //We want it so the higher up the tower the greater the range but the more expensive and slower things are, as a result of climbing more stairs or something
        //the range increases ballistically with height, but the area of effect things dont as they are field effects rather than ballistic

        //heightadjustedposition is purely for visual sorting for rendering
        Vector3 heightadjustedposition = new Vector3(this.transform.position.x, this.transform.position.y, -currentTowersBuiltHere);
        GameObject placedTower = (GameObject)Instantiate(towerToBuild, heightadjustedposition, Quaternion.identity, TowersContainerTransform);

        currentTowersBuiltHere = TowersContainerTransform.childCount;
        float scaleTower = 1.0f - ((currentTowersBuiltHere-1) * 0.15f); //scale tower size, according to tower height  

        //we only want to scale the sprite, not the colliders etc
        Vector3 originalScale = placedTower.GetComponent<TowerScript>().Barrel.transform.localScale;
        placedTower.GetComponent<TowerScript>().Barrel.transform.localScale = originalScale * scaleTower;
        //placedTower.transform.localScale = new Vector3(scaleTower, scaleTower, scaleTower);

        //TODO: bleurgh should probably let the tower calc all this on its own and just pass in stack level from here...
        float stackedRangeMult = 1f + (currentTowersBuiltHere * 0.1f);
        float stackedRateMult = 1f - (currentTowersBuiltHere * 0.1f);

        placedTower.GetComponent<TowerScript>().stackedRangeMult = stackedRangeMult;
        placedTower.GetComponent<TowerScript>().stackedRateMult = stackedRateMult;
        placedTower.GetComponent<TowerScript>().levelText.transform.position += new Vector3(-((currentTowersBuiltHere - 1) * 0.2f), (currentTowersBuiltHere - 1) * 0.2f, 0f);
        placedTower.GetComponent<TowerScript>().UpdateTowerStatsRealtime();

        placedTower.GetComponent<TowerScript>().showRange = Settings.Instance.playerSettings.rangeVisible;

        //TODO: offchance that this could fail so prob check if need an extra bank check here
        MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == placedTower.GetComponent<TowerScript>().ThisTowerType);
        float buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];
        lm.Bank -= Mathf.RoundToInt(placedTower.GetComponent<TowerScript>().costToBuild[0] * buildPriceMult);
        
        //turn off build mode to prevent double purchase on nervous clickers/tappers
        bm.BuildMode = StaticEnums.BuildMode.Nothing;

        //we have at least 1 tower so turn off towerbase rotation animation and fade the alpha a bit
        OriginalColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, towerInUseFadedAlpha);
        animator.speed = 0;

        HighlightBuildLocation(OriginalColor, OriginalHDRColor);

        //whoever needs to know a tower was built will now know
        OnTowerBuilt?.Invoke();
    }

    void InitTowerSummaryPrefab()   //heavily comment this code as its not clear or intuitive
    {
        foreach (Transform child in TowerSummaryUIPanel.transform)
        {
            Destroy(child.gameObject);
        }

        //Add each tower's info panel to the main panel; loop through towers contained in the towers container
        for (int i = 0; i < TowersContainerTransform.childCount; i++)
        {
            //we pass each towers TowerScript comp and a prefab of the towerinfopanel to the main info panel
            TowerScript tscr = TowersContainerTransform.GetChild(i).GetComponent<TowerScript>();
            //we also want to check if the tower is not the highest on its towerbase, to disable selling if so
            bool sellEnabled = true ? i == TowersContainerTransform.childCount - 1 : false;
            InitSingleTowerSummaryPrefab(TowerSummaryUIPanel.transform, SingleTowerInfoPrefab, tscr, sellEnabled);
        }
    }

    void InitSingleTowerSummaryPrefab(Transform _parent, GameObject _prefab, TowerScript _tscr, bool SellEnabled)
    {
        //quick check to see if the tower is maxxed; affects upgrade button
        bool maxxed = false;
        int upgradeTier = _tscr.currentUpgradeTier + 1;
        if (upgradeTier >= _tscr.maxUpgradePublic) { maxxed = true; }

        GameObject SingleTowerSummaryPrefabClone = Instantiate(_prefab, _prefab.transform.position, _prefab.transform.rotation, _parent);

        //access the towerinfopanel script; ref buttons/text etc via this
        SingleTowerInfoScript towerInfo = SingleTowerSummaryPrefabClone.GetComponent<SingleTowerInfoScript>();

        MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == _tscr.ThisTowerType);
        float damageMult = temp.damageMetaUpgrade[temp.CurrentDamageUpgradeTier];
        float rangeMult = temp.rangeMetaUpgrade[temp.CurrentRangeUpgradeTier];
        float firerateMult = temp.firerateMetaUpgrade[temp.CurrentFirerateUpgradeTier];
        float buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];
        float upgradePriceMult = temp.upgradePriceMetaUpgrade[temp.CurrentUpgradePriceUpgradeTier];

        //assign icon
        towerInfo.towerIcon.sprite = _tscr.upgradeSprites[_tscr.currentUpgradeTier];
        towerInfo.towerLevelText.text = _tscr.currentLevel.ToString();

        //info text for stats
        towerInfo.tmpCurrentPrice.text = (buildPriceMult * _tscr.costToBuild[_tscr.currentUpgradeTier]).ToString();
        towerInfo.tmpCurrentDamage.text = (damageMult * _tscr.baseDamageArray[_tscr.currentUpgradeTier]).ToString();
        towerInfo.tmpCurrentRange.text = (rangeMult * _tscr.range[_tscr.currentUpgradeTier]).ToString();
        towerInfo.tmpCurrentRate.text = (firerateMult * _tscr.fireRate[_tscr.currentUpgradeTier]).ToString();

        //upgrade stats; using a ternary to compactly determine if we're maxxed use n/a as string instead
        towerInfo.tmpNextPrice.text = maxxed == false ? (buildPriceMult * _tscr.costToBuild[upgradeTier]).ToString() : "n/a";
        towerInfo.tmpNextDamage.text = maxxed == false ? (damageMult * _tscr.baseDamageArray[upgradeTier]).ToString() : "n/a";
        towerInfo.tmpNextRange.text = maxxed == false ? (rangeMult * _tscr.range[upgradeTier]).ToString() : "n/a";
        towerInfo.tmpNextRate.text = maxxed == false ? (firerateMult * _tscr.fireRate[upgradeTier]).ToString() : "n/a";

        //Buttons are a little more complicated; they need listeners attached

        //Sell button will only call the SellTower method, and will only be visible is SellEnabled is true (calculated earlier)
        towerInfo.sellButton.gameObject.SetActive(SellEnabled);
        if (SellEnabled) towerInfo.sellButton.onClick.AddListener(delegate { SellTower(); });

        //Upgrade button will only appear if the tower is not maxxed out; and will call the UpgradeSpecificTower method and refresh the tower summary panel
        towerInfo.upgradeButton.gameObject.SetActive(!maxxed);
        if (!maxxed) towerInfo.upgradeButton.onClick.AddListener(delegate { UpgradeSpecificTower(_tscr); InitTowerSummaryPrefab(); });

        //Rotate targetting needs to change its button text to show current targetting priority and the button causes a sound effect and the targetting rotation
        towerInfo.targettingButton.GetComponentInChildren<Text>().text = _tscr.GetTargettingButtonText();
        towerInfo.targettingButton.onClick.AddListener(delegate
        {
            source.PlayOneShot(towerSelected);
            _tscr.RotateTargetting(towerInfo.targettingButton);
        });
    }
}
