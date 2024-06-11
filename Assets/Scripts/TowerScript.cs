using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//TODO BUG upgrade price displays correctly but is incorrect in the purchase mechanism; monst likely to do with inconsisitencies with build v upgrade costs
//idea is initial build of a tower is build price, and further upgrades are upgrade cost but right now its muddled. needs reworked probly from scratch

//TODO: fix placement of onscreen level indicator
//TODO: update realtime tower level indicator in tower summary if its already open
//TODO: 

public class TowerScript : MonoBehaviour
{
    #region Variablesetc

    private AudioSource source;
    public AudioClip hitSound;

    public TextMeshProUGUI levelText;

    private float lowPitchRange = .75F;
    private float highPitchRange = 1.5F;

    public Collider2D[] _viableTargetColliders;
    //public List<GameObject> _viableTargets;
    private StaticEnums.TargettingPriority _targettingPriority;
    private const int _maxUpgrades = 3;

    [Header("Arrayed upgrades")]
    public int maxUpgradePublic = _maxUpgrades;

    public float[] range = new float[_maxUpgrades];
    public float[] turnSpeed = new float[_maxUpgrades];
    public float[] fireRate = new float[_maxUpgrades];
    public float[] baseDamageArray = new float[_maxUpgrades];

    public Sprite[] upgradeSprites = new Sprite[_maxUpgrades];
    public bool homing = false;
    public string areaEffect;
    public bool AimedShot;
    public GameObject Tracer;
    public GameObject smokeTrail;
    public bool isFiringAimed;
    public int[] costToBuild = new int[_maxUpgrades];
    public int currentUpgradeTier = 0;
    public float currentBuildWorth = 0;

    [Header("Unity Setup")]
    public string ThisTowerType;
    public string enemyTag = "Enemy";
    public string bulletPoolName = "Bullet";
    public string missilePoolName = "Missile";
    public float fireCountdown = 0f;
    public Transform firePoint;
    public GameObject rangeCircle;
    public bool showRange;
    public GameObject ProjectilesContainer;
    private ObjectPooler objectPooler;
    public GameObject Barrel;
    private Animator BarrelAnimator;
    private Animator IcePulseAnimator;
    public bool IsAnimated = false;
    public float DamageInflicted = 0f;
    public int KillCount = 0;
    public GameObject Target { get; set; }

    public float damageMult = 1f;
    public float expDamageMult = 1f;
    public float exDamagePerkMult = 1.01f;  // TODO impl this as a perk adjustable constant
    public float rangeMult = 1f;
    public float stackedRangeMult = 1f;
    public float stackedRateMult = 1f;
    public float firerateMult = 1f;
    public float buildPriceMult = 1f;
    public float upgradePriceMult = 1f;
    private GameObject smokeTrailClone;
    private Vector3 directionOfAimedTravel;
    public float AimedSpeed = 100f;

    //Idea is that towers can level up forever, the experience to level up increases faster than a waves given experience so it should become harder with progression
    public int experience = 0;
    public int currentLevel = 1;
    public int baseLevelExperienceNeeded = 100;
    public int realLevelExperienceNeeded;
    public int experienceStillNeeded;
    public float levelExperienceMult = 1.1f;

    public bool targetsAvailable = false;
    public bool reloaded = false;
    //public bool aimed = false;

    //TODO: experience accumulates but thats it, we need a reward for experiece: after total tower bullet type exp has cleared 1000000 say, we open the perk knockback
    //which is enabled on towers thereafer when they reach experience 5 and can afford the effect mid game, so its not enabled right away at wave 1

    #endregion

    public static Action OnBulletShot;
    public static Action OnMissileFired;

    private void Awake()
    {
        ProjectilesContainer = GameObject.Find("ProjectilesContainer");
        if (IsAnimated) BarrelAnimator = Barrel.GetComponent<Animator>();
        source = GetComponent<AudioSource>();
        if (areaEffect == "ice") IcePulseAnimator = this.transform.GetComponent<Animator>();
    }

    void Start()
    {
        //get all the metaupgrades data 
        MetaUpgrades temp = GlobalObjectScript.Instance.metaUpgrades.Find(x => x.TowerType == ThisTowerType);
        damageMult = temp.damageMetaUpgrade[temp.CurrentDamageUpgradeTier];
        rangeMult = temp.rangeMetaUpgrade[temp.CurrentRangeUpgradeTier];
        firerateMult = temp.firerateMetaUpgrade[temp.CurrentFirerateUpgradeTier];
        buildPriceMult = temp.buildPriceMetaUpgrade[temp.CurrentBuildPriceUpgradeTier];
        //upgradePriceMult = temp.upgradePriceMetaUpgrade[temp.CurrentUpgradePriceUpgradeTier];

        experience = 0;
        currentLevel = 1;
        levelText.text = currentLevel.ToString();

        //levelText.transform.position += (stackedLevel-1) * 3; //Reposition level text position +3pixels to xy coords for each stack level increase

        isFiringAimed = false;
        KillCount = 0;
        DamageInflicted = 0f;
        objectPooler = ObjectPooler.Instance;
        currentUpgradeTier = 0;
        currentBuildWorth = costToBuild[0] * buildPriceMult;
        //_viableTargets = new List<GameObject>();
        CircleCollider2D c = GetComponent<CircleCollider2D>();
        c.radius = stackedRangeMult * rangeMult * range[currentUpgradeTier] / 2;

        //update the effective stats, as metaupgrades cant change midgame, we can safely modify our base stats and use them that way
        targetsAvailable = false;
        reloaded = false;
        //aimed = false;

    }

    void Update()
    {
        ApplyVisualEffects();

        //Target update and reload loop
        //only check for targets if we don't have any
        //if (!targetsAvailable) CheckForTargets();
        //only reload if we arent
        if (!reloaded) Reload();
        //only aim if not currently aimed and has target
        //if (!aimed && targetsAvailable) Aim();

        //bug, creep killed between aim and next shot, aim is still valid, so tower shoots towards spawnpoint.
        //need to move aim to just before shoot, not have it as a waiting state


        //only shoot if reloaded and has a target aimed
        if (reloaded) AimAndShoot();
        //emergent property is that we won't reseek on targets until the entire target area is cleared






        //if (Target == null || _viableTargets.Count < 1) return;
        //if (Target == null || _viableTargetColliders.Length == 0) return;
        //if (areaEffect.Length == 0 && targetsAvailable) { RotateAndAim(); }
        //ShootCountdown();

        //TODO: BUG currently towers create viable target lists when enemies enter/exit range, it may happen a fast enemy overtakes a slow enemy to be 'first' but upon overtaking
        //will still not be targetted by the tower as no trigger to reprioritise occurs if no other enemies enter/exit range so the fast enemy scampers away freely
        //unless the initial slow enemy is destroyed.
        //quick fix could be to reprioritise on every shot, but thta may prove to be costly, so maybe every few shots?
        //FIX: current fix is to simply call retarget after every shot which is ok for most towers but perhaps a bit much for the faster shot towers like bullet
        //consider this a POLISH/OPTIMISE task

        if (isFiringAimed) FireAimed(); //for sniper smoke trail
    }


    //EXPT I want to investigate if it's better to simply monitor coords of all creeps, instead of relying on colliders    

    private bool CheckForTargets()
    {
        _viableTargetColliders = Physics2D.OverlapCircleAll(this.transform.position, stackedRangeMult * rangeMult * range[currentUpgradeTier] / 2, 1 << 8);
        //_viableTargetColliders = Physics2D.OverlapCircle(this.transform.position, 4f, 1 << 8);
        if (_viableTargetColliders.Length > 0)
        {
            return true;
            //Aim();
        }
        return false;
    }

    //private void Aim()
    //{
    //    if (_viableTargetColliders.Length > 0 && targetsAvailable)//not sure this check is necessary, the check is built in to the call here...?
    //    {
    //        UpdateTarget();
    //    }
    //}

    private void Reload()
    {
        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            reloaded = true;
            fireCountdown = 1f / (fireRate[currentUpgradeTier] * firerateMult * stackedRateMult);
        }
    }

    public void CheckExperience()
    {
        //check if expereience exceeds level threshold
        //levelthreshold increases by levelmult each level; simple compound interest style formula
        //tracking too many variables here really, but visually useful for now

        realLevelExperienceNeeded = Mathf.RoundToInt(Mathf.Pow((float)levelExperienceMult, (float)(currentLevel - 1)) * baseLevelExperienceNeeded);
        experienceStillNeeded = realLevelExperienceNeeded - experience;
        if (experience > realLevelExperienceNeeded)
        {
            currentLevel++;
            experience -= realLevelExperienceNeeded; //keep overshot experience       
            //TODO: update visuals in panel if shown, think about some way of showing tower exp on game screen
            levelText.text = currentLevel.ToString();
            expDamageMult *= exDamagePerkMult;
            //TODO: ice towers will have different exp upgrades like freeze time etc.
        }
    }

    void FireAimed()
    {

        smokeTrailClone.transform.Translate(directionOfAimedTravel.normalized * AimedSpeed * Time.deltaTime, Space.World);

        if (Target == null)
        {
            isFiringAimed = false;
            Destroy(smokeTrailClone);
            return;
        }
        if (Vector2.Distance(smokeTrailClone.transform.position, Target.transform.position) <= directionOfAimedTravel.normalized.magnitude * AimedSpeed * Time.deltaTime)
        {
            isFiringAimed = false;
            Destroy(smokeTrailClone, 1f);
        }
    }

    //void ApplyAreaEffect()
    //{
    //    //this is exp replaced by removing/adding flag to targets as they exit/enter range
    //    //the target itself handles its condition

    //    //this is wildly inefficient, need a better method of simply telling the enemy its frozen once per pulse and the enemy will
    //    //freeze itself on its own timer and unfreeze unless pulsed again

    //    //TODO: implement timer for effects, flag controlled too
    //    //Debug.Log("Applying area effect");
    //    //all viable targets take damage
    //    if (_viableTargets.Count > 0)
    //    {
    //        //Debug.Log("Applying area effect to list of viable targets");
    //        //temp store viabletargets
    //        List<GameObject> tempTargetsList = new List<GameObject>(_viableTargets);
    //        foreach (GameObject target in tempTargetsList)      //BUG: list can be modified outside this and throws colletion changed error; need to ensure our temp copy is a true copy, not a link to the original 
    //        {
    //            Debug.Log("foreach on list of targets, there are " + tempTargetsList.Count);
    //            if (target != null)
    //            {
    //                Debug.Log("Passed null check on target");
    //                //TODO BUG sometimes this doesnt activate, enemies slip through untouched
    //                EnemyAI enem = target.GetComponentInParent<EnemyAI>();
    //                //enem.TakeDamage(damage[currentUpgradeTier]);
    //                if (enem == null) break;
    //                Debug.Log("Passed null check on enemy script");
    //                enem.ApplyTowerEffect(areaEffect);  //being cancelled by gun 'applying' no effect?s
    //            }
    //        }
    //    }
    //}

    public void UpgradeMe()
    {
        currentUpgradeTier++;
        if (currentUpgradeTier > _maxUpgrades - 1) { currentUpgradeTier = _maxUpgrades - 1; return; }
        currentBuildWorth += buildPriceMult * costToBuild[currentUpgradeTier];
        //change sprte
        Barrel.GetComponent<SpriteRenderer>().sprite = upgradeSprites[currentUpgradeTier];
        UpdateTowerStatsRealtime();

        //if tower summary panel is active on this tower then update it visually
    }

    public void UpdateTowerStatsRealtime()
    {
        //TODO: the ice tower and similar will have always on effects of range, need to think of some way to portray this

        CircleCollider2D c = GetComponent<CircleCollider2D>();
        c.radius = stackedRangeMult * rangeMult * range[currentUpgradeTier] / 2;
    }

    private void ApplyVisualEffects()   //TODO ACtion Event this
    {
        if (Settings.Instance.playerSettings.rangeVisible)
        {
            rangeCircle.transform.localScale = new Vector3(stackedRangeMult * rangeMult * range[currentUpgradeTier], stackedRangeMult * rangeMult * range[currentUpgradeTier], 0);
            rangeCircle.SetActive(true);
        }
        else { rangeCircle.SetActive(false); }
    }

    private void RotateAndAim()
    {   //No gameplay affects, only visual
        Vector3 diff = Target.transform.position - transform.position;
        diff.Normalize();
        float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        Barrel.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }

    //TODO: sniper aiming is having problems with the new physics overlap method....no idea whats going on

    //private void ShootCountdown()
    //{
    //    if (fireCountdown < 0f && _viableTargetColliders.Length > 0)
    //    //if (fireCountdown < 0f && _viableTargets.Count > 0)
    //    {
    //        Shoot();
    //        fireCountdown = 1f / (fireRate[currentUpgradeTier] * firerateMult * stackedRateMult);
    //        UpdateTarget();
    //    }
    //    fireCountdown -= Time.deltaTime;
    //}

    //smoke trail is terrible, manually it looks great, so how to emulate that
    IEnumerator ShootSmokeTrail(GameObject smokeTrail)
    {
        float speed = 50;
        for (int i = 0; i < speed; i++)
        {
            smokeTrail.transform.position = Vector3.Lerp(this.transform.position, Target.transform.position, i / speed);
        }

        yield return new WaitForSeconds(0.1f);
    }

    void ShootAoE()
    {
        //a wee visual aid to the ice pulse
        if (areaEffect == "ice")
        {
            IcePulseAnimator.SetTrigger("ShotFired");
            source.PlayOneShot(hitSound);
        }
        //area of effect doesnt need to aim and can checkfor targets last inute
        CheckForTargets();
        //all viable targets take damage
        if (_viableTargetColliders.Length > 0)
        {
            foreach (Collider2D item in _viableTargetColliders)
            {
                //we know all colliders are enemies thaks to the layermask
                EnemyAI enemy = item.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    if (areaEffect == "ice") enemy.onIce = true;
                    if (areaEffect == "fire") enemy.onFire = true;
                    enemy.TakeDamage(baseDamageArray[currentUpgradeTier] * damageMult * expDamageMult, this.transform, false);
                }
            }
        }
    }


    void AimAndShoot()
    {

        if (!CheckForTargets()) return;
        UpdateTarget();
        RotateAndAim();

        //GameObject shotLockedTarget = Target;   //feeble attempt to ensure projectiles stick to their chosen creep, regardless of if class Target has changed
        //somehow this messes up a bit if the creep respawns mid aim r mid reload, not sure really. the tower still targets the 'dead' creep as it respawns from teh cave
        //i suspect it has to do with the container being disabled but not the collider bots so the detection of null is being effectively bypassed....

        if (areaEffect.Length > 0)
        {

            ShootAoE();

            reloaded = false;
            targetsAvailable = false;   //TODO: might not need to clear targets here
            //aimed = false;
            return;
        }

        if (AimedShot)
        {
            //the bullet shouldn't hit anything on the way to its target, so we cheat and draw visual-only effects to show projectile trajectory but instantly apply damage to Target

            //and a tracer which is an instant line of light
            GameObject tracerClone = Instantiate(Tracer, firePoint.position, Quaternion.identity, ProjectilesContainer.transform);
            LineRenderer lr = tracerClone.GetComponent<LineRenderer>();
            Vector3[] pos = { firePoint.position, Target.transform.position };
            lr.SetPositions(pos);
            Destroy(tracerClone, 0.2f);

            //Add a smoke trail to the bullet which is slower to appear and fade
            smokeTrailClone = Instantiate(smokeTrail, firePoint.position, Quaternion.identity, ProjectilesContainer.transform);
            isFiringAimed = true;
            directionOfAimedTravel = (Target.transform.position - firePoint.transform.position);

            //and finally a crack sound
            source.PlayOneShot(hitSound);

            //After all that we apply damage, important to do it after in case the target is destroyed by this shot and the tracer/smoke point to the next target
            EnemyAI targetScript = Target.GetComponentInParent<EnemyAI>();
            if (targetScript == null) return;
            targetScript.TakeDamage(baseDamageArray[currentUpgradeTier] * damageMult * expDamageMult, this.transform, false);
        }
        else if (homing)
        {
            //The way the prefabs are set up requires we use the firepoint position and the barrel rotation for the projectiles
            GameObject missileGO = BulletPool.Instance.SpawnFromPool(missilePoolName, firePoint.transform.position, Barrel.transform.rotation, ProjectilesContainer.transform);
            MissileScript missile = missileGO.GetComponent<MissileScript>();
            if (missile != null)
            {
                missile.TowerOwner = this.transform;
                OnMissileFired?.Invoke();
                missile.Seek(Target, baseDamageArray[currentUpgradeTier] * damageMult * expDamageMult, this);
            }
        }
        else
        {
            //GameObject bulletGO = objectPooler.SpawnFromPool(bulletPoolName, firePoint.transform.position, Barrel.transform.rotation, ProjectilesContainer.transform);
            GameObject bulletGO = BulletPool.Instance.SpawnFromPool(bulletPoolName, firePoint.transform.position, Barrel.transform.rotation, ProjectilesContainer.transform);
            BulletScript bullet = bulletGO.GetComponent<BulletScript>();
            if (bullet != null)
            {
                bullet.TowerOwner = this.transform;
                OnBulletShot?.Invoke();
                source.pitch = UnityEngine.Random.Range(lowPitchRange, highPitchRange);
                source.PlayOneShot(hitSound);
                float tempRange = stackedRangeMult * rangeMult * range[currentUpgradeTier] / 2;
                bullet.Seek(Target, baseDamageArray[currentUpgradeTier] * damageMult * expDamageMult, tempRange);
            }
        }

        reloaded = false;
        targetsAvailable = false;   //TODO: might not need to clear targets here
        //aimed = false;
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    DoWeHaveATarget(collision);
    //}

    //void DoWeHaveATarget(Collider2D collision) { 
    //    //Enemy enters range of tower, we add it to viableTargets list and apply any tower range effects;ice, stun, etc.
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        GameObject enemyInRange = collision.gameObject;   
    //        if (!_viableTargets.Contains(enemyInRange))
    //        {
    //            _viableTargets.Add(enemyInRange);
    //            EnemyAI enemyScript = enemyInRange.GetComponentInParent<EnemyAI>();

    //            if (enemyScript != null)
    //            {
    //                enemyScript.isTargetted = true;
    //                if (areaEffect == "ice") enemyScript.onIce = true; //AoE speed control;
    //            }
    //            UpdateTarget();
    //        }
    //    }
    //}

    //ontriggerstay is causing huge performance loss with larger numbers of enemy on screen, understandably. is it necessary?
    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    DoWeHaveATarget(collision);
    //}

    //public void ManuallyAddTarget(GameObject target)
    //{
    //    if (target.CompareTag("Enemy"))
    //    {
    //        if (!_viableTargets.Contains(target))
    //        {
    //            _viableTargets.Add(target);
    //            target.GetComponentInParent<EnemyAI>().isTargetted = true;
    //            UpdateTarget();
    //        }
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        GameObject target = collision.gameObject;
    //        if (_viableTargets.Contains(target))
    //        {
    //            _viableTargets.Remove(target);
    //            UpdateTarget();
    //        }
    //    }
    //}

    public void RotateTargetting(Button btn)
    {
        int max = Enum.GetNames(typeof(StaticEnums.TargettingPriority)).Length;
        _targettingPriority++;
        if ((int)_targettingPriority >= max) { _targettingPriority = 0; }
        btn.GetComponentInChildren<Text>().text = GetTargettingButtonText();
        UpdateTarget();
    }

    public string GetTargettingButtonText()
    {
        string targetString = string.Empty;
        switch (_targettingPriority)
        {
            case StaticEnums.TargettingPriority.First: targetString = "FIRST"; break;
            case StaticEnums.TargettingPriority.Last: targetString = "LAST"; break;
            case StaticEnums.TargettingPriority.Nearest: targetString = "CLOSEST"; break;
            case StaticEnums.TargettingPriority.Strongest: targetString = "STRONGEST"; break;
            case StaticEnums.TargettingPriority.Weakest: targetString = "WEAKEST"; break;
            default: targetString = string.Empty; break;
        }
        return targetString;
    }

    void UpdateTarget()
    {

        //if (_viableTargets.Count < 1) return;


        switch (_targettingPriority)
        {
            case StaticEnums.TargettingPriority.First:
                TargetFirst();
                break;
            case StaticEnums.TargettingPriority.Last:
                TargetLast();
                break;
            case StaticEnums.TargettingPriority.Nearest:
                TargetNearest();
                break;
            case StaticEnums.TargettingPriority.Strongest:
                TargetStrongest();
                break;
            case StaticEnums.TargettingPriority.Weakest:
                TargetWeakest();
                break;
            default:
                TargetFirst();
                break;
        }
    }


    //All the targetting methods follow a similar strategy
    //TODO: these could be refactored to RETURN the target;
    //or copy the viable target list for internal traversal and check for null at the end.
    //probably makes no real world difference though, just ocd subservience
    void TargetFirst()
    {
        float _furthestTravelled = 0f;
        GameObject _firstEnemy = null;

        //loop through all viable targets
        //foreach (GameObject enemy in _viableTargets)
        foreach (Collider2D item in _viableTargetColliders)
        {
            GameObject enemy = item.gameObject;
            //check each enemy in case its been destroyed by another tower in the meantime
            if (enemy != null)
            {
                //retrieve enemy script and again check for null(destoyed by another tower)
                EnemyAI enemyScript = enemy.GetComponentInParent<EnemyAI>();
                if (enemyScript != null)
                {
                    //check parameter against our criteria and store enemy as target if best candidate, otherwise skip to next enemy
                    float _travelled = enemyScript.totalDistanceTravelled;
                    if (_travelled >= _furthestTravelled)
                    {
                        _furthestTravelled = _travelled;
                        _firstEnemy = enemy;
                    }
                }
            }
        }

        //finally set target to best candidate, again checking it hasn't been destroyed in the interim
        if (_firstEnemy != null) { Target = _firstEnemy;}
        else { Target = null; }
    }

    void TargetStrongest()
    {
        float _biggestHealth = 0f;
        float _furthestTravelled = 0f;
        GameObject _healthiestEnemy = null;
        foreach (Collider2D item in _viableTargetColliders)
        {
            GameObject enemy = item.gameObject;
            //foreach (GameObject enemy in _viableTargets)
        //{
            if (enemy != null)
            {
                EnemyAI enemyScript = enemy.GetComponentInParent<EnemyAI>();
                if (enemyScript != null)
                {
                    //we want to target strongest, and first
                    float _currentHealth = enemyScript.maxHealth;
                    float _travelled = enemyScript.totalDistanceTravelled;
                    if (_currentHealth >= _biggestHealth && _travelled >= _furthestTravelled)
                    {
                        _furthestTravelled = _travelled;
                        _biggestHealth = _currentHealth;
                        _healthiestEnemy = enemy;
                    }
                }
            }
        }
        if (_healthiestEnemy != null) { Target = _healthiestEnemy; }
        else { Target = null; }
    }

    void TargetWeakest()
    {
        float _lowestHealth = Mathf.Infinity;
        GameObject _unhealthiestEnemy = null;
        float _furthestTravelled = 0f;
        foreach (Collider2D item in _viableTargetColliders)
        {
            GameObject enemy = item.gameObject;
            //foreach (GameObject enemy in _viableTargets)
       //{
            if (enemy != null)
            {
                EnemyAI enemyScript = enemy.GetComponentInParent<EnemyAI>();
                if (enemyScript != null)
                {
                    float _currentHealth = enemyScript.maxHealth;
                    float _travelled = enemyScript.totalDistanceTravelled;
                    if (_currentHealth <= _lowestHealth && _travelled >= _furthestTravelled)
                    {
                        _furthestTravelled = _travelled;
                        _lowestHealth = _currentHealth;
                        _unhealthiestEnemy = enemy;
                    }
                }
            }
        }

        if (_unhealthiestEnemy != null) { Target = _unhealthiestEnemy; }
        else { Target = null; }
    }

    void TargetLast()
    {
        float leastTravelled = Mathf.Infinity;
        GameObject lastEnemy = null;
        foreach (Collider2D item in _viableTargetColliders)
        {
            GameObject enemy = item.gameObject;
            //foreach (GameObject enemy in _viableTargets)
        //{
            if (enemy != null)
            {
                EnemyAI temp = enemy.GetComponentInParent<EnemyAI>();
                if (temp != null)
                {
                    float travelled = temp.totalDistanceTravelled;
                    if (travelled < leastTravelled)
                    {
                        leastTravelled = travelled;
                        lastEnemy = enemy;
                    }
                }
            }
        }

        if (lastEnemy != null) { Target = lastEnemy;  }
        else { Target = null; }
    }

    void TargetNearest()
    {
        float _shortestDistance = Mathf.Infinity;
        GameObject _nearestEnemy = null;
        foreach (Collider2D item in _viableTargetColliders)
        {
            GameObject enemy = item.gameObject;
            //foreach (GameObject _enemy in _viableTargets)
        //{
            if (enemy != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

                if (distanceToEnemy < _shortestDistance)
                {
                    _shortestDistance = distanceToEnemy;
                    _nearestEnemy = enemy;
                }
            }
        }

        if (_nearestEnemy != null && _shortestDistance <= stackedRangeMult * rangeMult * range[currentUpgradeTier])
        {
            Target = _nearestEnemy; 
        }
        else { Target = null; }
    }
}
