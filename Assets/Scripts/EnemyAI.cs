using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


//Added 2 classes and an enum for later use, perhaps for applying ice/fire effects
public enum StatModType
{
    Flat = 100, Percent = 200,
}

public class EnemyStat
{
    private bool isDirty = true;
    private float _value;

    public float BaseValue;
    private float lastBaseValue = float.MinValue;
    private readonly List<StatModifier> statModifiers;
    public readonly IReadOnlyCollection<StatModifier> StatModifiers;
    public EnemyStat(float baseValue)
    {
        BaseValue = baseValue;
        statModifiers = new List<StatModifier>();
        StatModifiers = statModifiers.AsReadOnly();
    }

    public float Value
    {
        get
        {
            if (isDirty || lastBaseValue != BaseValue)
            {
                lastBaseValue = BaseValue;
                _value = CalculateFinalValue();
                isDirty = false;
            }
            return _value;
        } 
    }

    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        isDirty = true;
        return statModifiers.Remove(mod);
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];
            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModType.Percent)
            {
                finalValue *= 1 + mod.Value;
            }            
        }
        return (float)Math.Round(finalValue, 4);
    }
}

public class StatModifier
{
    public readonly float Value;
    public readonly StatModType Type;
    public readonly int Order;

    public StatModifier(float value, StatModType type, int order)
    {
        Value = value;
        Type = type;
        Order = order;
    }

    //default order if not set
    public StatModifier(float value, StatModType type) : this(value, type, (int)type) { }
}

public class EnemyAI : MonoBehaviour, IPooledObject
{
    //TODO: break this ut to base enemy class, with inheriting class of specific types to determine specific attrs
    public int experienceReward;

    public float OriginalSpeed = 10f; //readonly prevents accidental changes, but can be first set during runtime
    public float speed = 10f;
    public Transform healthbar;
    public GameObject damageText;
    public Transform Enemy;
    private string BulletImpactEffectName = "BulletImpactEffect";
    private string CreepDestroyedEffectName = "CreepDestroyedEffect";
    public Text currentHealthText;
    public GameObject ImpactContainer;

    private Vector3 target;
    private int waypointIndex = 0;
    public float maxHealth = 25f;
    public float currentHealth;
    public int reward = 5;
    public float turnSpeed = 10f;

    public bool isDead = false;
    public int spawnNumber;
    public bool IsIceImmune = false;
    public string homePool;
    public bool showHealth = false;

    public float totalDistanceTravelled = 0f;
    public float recentDistanceTravelled = 0f;
    public Color TargettedColor;
    public bool isTargetted = false;
    public int waveIndex;
    private Vector3 randomWaypointOffset;
    public float offset = 0.1f;
    public GameObject healthCanvas;
    private float healthMultiplier;
    private float iceEffectDuration = 2f; //TODO: have this calculated rather than hardcoded; calc includes towers stats and creep reduction stats
    private float originalIceEffectDuration = 2f;
    WaypointManager wm;
    SpawnManager sm;
    Settings se;
    private ObjectPooler objectPooler;
    //bools for each possible area effect
    public bool onIce = false;  //TODO: maybe think about allowing this to stack
    public bool onFire = false;
    public float fireEffectDuration = 2f;
    private float originalFireEffectDuration = 2f;
    private float burnDuration = 0f;

    /*
     * Action is equivalent to :
     * public delegate void OnDamageReceived(float damage);
     * public static event OnDamageReceived onDamageReceived;
     * 
     * TODO: possilby restructure OnKilled to allow optional parameters
    */

    public static Action<float> OnDamageReceived;
    public static Action<int, int> OnKilled;
    public static Action OnReachedBase;

    void Awake()
    {
        se = Settings.Instance;
        wm = WaypointManager.Instance;
        sm = SpawnManager.Instance;
        objectPooler = ObjectPooler.Instance;
        UIManager.OnDisplayHealth += DisplayHealth;
    }

    private void OnDisable()
    {
        UIManager.OnDisplayHealth -= DisplayHealth;
    }

    public void OnObjectSpawn()
    {
        randomWaypointOffset = new Vector3(UnityEngine.Random.Range(-offset, offset), UnityEngine.Random.Range(-offset, offset), 0f);
        transform.position += randomWaypointOffset;
        isDead = false;
        healthMultiplier = Mathf.Pow(1.1f, sm.waves[waveIndex].DifficultyMultiplier);
        speed = OriginalSpeed;
        currentHealth = maxHealth * healthMultiplier;
        waypointIndex = 1;
        target = wm.Points[waypointIndex].ToVector3() + randomWaypointOffset;
        totalDistanceTravelled = 0f;
        recentDistanceTravelled = 0f;

        //display health or not depends on uimanager sending settings event, but initially we read the main settings
        if (se.playerSettings.healthVisible) DisplayHealth(true);


        UIManager.OnDisplayHealth += DisplayHealth;

        this.GetComponentInChildren<SpriteRenderer>().sortingOrder = sm.EnemiesOnScreen;
        //healthbar.GetComponent<SpriteRenderer>().sortingOrder = sm.EnemiesOnScreen + 2;

        //TODO:somehow need to kill any leftover damage/death effects from before spawn recall

    }


    void Update()
    {
        MoveSprite();
        //UpdateVisualStatus();
        AffectedByAnything();
        //RestoreStatsAfterEffect();
    }

    void AffectedByAnything()
    {
        //the order of this is ice beats fire

        if (onIce && !IsIceImmune)
        {
            //start timer for being frozen
            iceEffectDuration -= Time.deltaTime;
            if (iceEffectDuration <= 0)
            {
                onIce = false;
                iceEffectDuration = originalIceEffectDuration;
                speed = OriginalSpeed;
                Enemy.GetComponent<SpriteRenderer>().color = Color.white;
                return;
            }
            speed = OriginalSpeed * 0.5f;
            Enemy.GetComponent<SpriteRenderer>().color = Color.cyan;
            return;
        }
        //else
        //{
        //    speed = OriginalSpeed;
        //    Enemy.GetComponent<SpriteRenderer>().color = Color.white;
        //}
        if (onFire)
        {
            fireEffectDuration -= Time.deltaTime;
            if (fireEffectDuration <= 0)
            {
                onFire = false;
                fireEffectDuration = originalFireEffectDuration;
                speed = OriginalSpeed;
                Enemy.GetComponent<SpriteRenderer>().color = Color.white;
                return;
            }

            Enemy.GetComponent<SpriteRenderer>().color = Color.red;
            speed = OriginalSpeed * 1.05f;

            //take damage from fire 10% of damage every 0.1s;
            burnDuration -= Time.deltaTime;
            if (burnDuration <=0f)
            {
                TakeDamage(1f, null, false);
                burnDuration = 0.2f;
            }

        }
    }

    void RestoreStatsAfterEffect()
    {
        //TODO:being called all the time? not good
        speed = OriginalSpeed;
        //onIce = false;
    }

    public void DisplayHealth(bool displayHealth)
    {
        showHealth = displayHealth;
        if (displayHealth)
        {
            CalcHealthBar();
            healthbar.gameObject.SetActive(true);
            currentHealthText.gameObject.SetActive(true);
            currentHealthText.text = currentHealth.ToString("F01");
        }
        else
        {
            healthbar.gameObject.SetActive(false);
            currentHealthText.gameObject.SetActive(false);
        }
    }

    void MoveSprite()
    {
        //sprites have rigidbody, supposed to be better to move them taking that into account
        Vector3 dir = target - this.transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);
        
        
        //now we can slerp our sprite child from where we're already looking to our intendedrotation
        transform.Rotate(Vector3.zero);
        Quaternion intendedRotation = Quaternion.LookRotation(Vector3.forward, target - transform.position);
        Enemy.transform.rotation = Quaternion.Slerp(Enemy.transform.rotation, intendedRotation, Time.deltaTime * turnSpeed);

        //TODO: sometimes the sprites twitch when they#re rotating, no idea why
        //it semes to only happen when the sprite z rotation is close to -180, it flicks to +180 wierdly

        recentDistanceTravelled = (dir.normalized * speed * Time.deltaTime).magnitude;
        totalDistanceTravelled += recentDistanceTravelled;  //TODO: pause doesnt stop this increment

        //we're close enough to our targetted waypoint, advance!
        if (Vector3.SqrMagnitude(this.transform.position - target) <= dir.normalized.magnitude * speed * Time.deltaTime)
        {
            GetNextWayPoint();
        }

    }

    void GetNextWayPoint()
    {
        if (waypointIndex >= wm.Points.Count - 1)
        {
            OnReachedBase?.Invoke();

            //TODO make base quiver
            BulletPool.Instance.ReturnToPool(homePool, gameObject);


            return;
        }
        
        waypointIndex++;
        target = wm.Points[waypointIndex].ToVector3() + randomWaypointOffset;
    }

    public void ApplyTowerEffect(string effectType)
    {
        switch (effectType)
        {
            case ("ice"):
                //speed = OriginalSpeed * 0.2f;
                //onIce = true;
                break;
            case ("fire"):
                break;
            case ("poison"):
                break;
            default:
                break;
        }
    }

    void CalcHealthBar()
    {
        float scal = 0.9f * (currentHealth / (healthMultiplier * maxHealth));
        healthbar.transform.localScale = new Vector3(scal, 0.1f, 1);
        float green = (currentHealth / maxHealth);
        float red = 1 - green;
        Color tempColor = new Color(red + 0.25f, green + 0.15f, 0f);
        healthbar.GetComponent<SpriteRenderer>().color = tempColor;
        currentHealthText.text = currentHealth.ToString("F01");
    }

    IEnumerator KillEffect(GameObject go, float delay, string poolname)
    {
        
        yield return new WaitForSeconds(delay);
        BulletPool.Instance.ReturnToPool(poolname, go);
    }

    public void TakeDamage(float damage, Transform AttackTower, bool KnockBack)
    {
        currentHealth -= damage;
        OnDamageReceived?.Invoke(damage);

        //on the offchance the exit has been reached
        if (this.gameObject.activeInHierarchy == false) return;

        if (se.playerSettings.DisplayEffects)
        {
            //GameObject effectIns = Instantiate(justBeenShotEffect, transform.position, transform.rotation);
            GameObject effectIns = BulletPool.Instance.SpawnFromPool(BulletImpactEffectName, transform.position, Quaternion.identity, this.transform);
            StartCoroutine(KillEffect(effectIns, 2f, BulletImpactEffectName));
        }

        if (se.playerSettings.DisplayDamageText) InitDamageTextPrefab(damage.ToString("F1"));   //TODO pool this out
        if (se.playerSettings.healthVisible) CalcHealthBar();


        //perp is the tower that caused the damage, we need it to register the kill or tally damage stats
        //TODO delegate this out i guess
        TowerScript tempTscript = null;

        if (AttackTower == null)
        {
            AttackTower = GameObject.Find("TheUnknownTower").transform;
        }
        else { tempTscript = AttackTower.GetComponent<TowerScript>(); }


        //TODO: theunknowntower holds any kills or stats for towers that have been sold while their projectiles are still in the air
        //i need to implement some minimal towerscript thing that only holds the stats for the tower and split that away from the tower
        //mechanics script
        //TODO: maybe modularise out a lot of tower scripts, like AOE, upgrades, purchasable perks, top tier perks etc.

        //FORNOW just dont do these bits if tower is unknown
        //TODO EVENT

        //award a little experience for a hit
        if (tempTscript != null) { tempTscript.DamageInflicted += damage; tempTscript.experience++; tempTscript.CheckExperience(); }

        if (currentHealth <= 0)
        {
            //update individual tower stats, add a full experience reward for the kill shot
            if (tempTscript != null) { tempTscript.KillCount++; tempTscript.experience += experienceReward; tempTscript.CheckExperience(); }
            Dead();
        }
        if (KnockBack)   //TODO: proper knockback perk implementation
        {
            float knockbackPower = -2f;
            Vector3 dir = target - this.transform.position;
            transform.Translate(dir.normalized * knockbackPower * Time.deltaTime, Space.World);
        }


    }

    void InitDamageTextPrefab(string text)
    {
        //TODO: maybe think about pooling this per creep?worth it?
        GameObject temp = Instantiate(damageText) as GameObject;
        RectTransform tempRect = temp.GetComponent<RectTransform>();
        temp.transform.SetParent(healthCanvas.transform);   //TODO cache this more sensibly
        tempRect.transform.localPosition = damageText.transform.localPosition;
        tempRect.transform.localScale = damageText.transform.localScale;
        tempRect.transform.localRotation = damageText.transform.localRotation;
        temp.GetComponent<Text>().text = text; //TODO: format this!
        Destroy(temp.gameObject, 1);
    }

    void Dead()
    {
        isDead = true;
        OnKilled?.Invoke(reward, waveIndex);
        UIManager.OnDisplayHealth -= DisplayHealth;
        if (se.playerSettings.DisplayEffects)
        {
            //TODO: BUG: not appearing, no idea why; same init as impacteffect and it works fine
            //GameObject effectIns = objectPooler.SpawnFromPool(CreepDestroyedEffectName, transform.position, Quaternion.identity, this.transform);            
            //StartCoroutine(KillEffect(effectIns, 2f, CreepDestroyedEffectName));

            GameObject temp = BulletPool.Instance.SpawnFromPool(CreepDestroyedEffectName, transform.position, Quaternion.identity, this.transform.parent.transform);
            StartCoroutine(KillEffect(temp, 2f, CreepDestroyedEffectName));
        }
        BulletPool.Instance.ReturnToPool(homePool, gameObject);
    }
}

/*
void BezierTrnsform()
{
    //// parameter t ranges from 0f to 1f
    //// this code might not compile!
    //Vector3 GetBezierPosition(float t)
    //{
    //    Vector3 p0 = transformBegin.position;
    //    Vector3 p1 = p0 + transformBegin.forward;
    //    Vector3 p3 = transformEnd.position;
    //    Vector3 p2 = p3 - transformEnd.back;

    //    // here is where the magic happens!
    //    return Mathf.Pow(1f - t, 3f) * p0 + 3f * Mathf.Pow(1f - t, 2f) * t * p1 + 3f * (1f - t) * Mathf.Pow(t, 2f) * p2 + Mathf.Pow(t, 3f) * p3;
    //}
}

    void CheckIfSpawnedInTowerRange()
    {
        ////FIXED: this was all unnecessary, the real problem was the targetting of FIRST, not picking up travelled distance> furthest distance because there was only 1 spawn on  screen.
        ////WAAAARGGGHHHH!!!

        ////The triggers dont detect if an object spawns within its trigger volume, so we need to do it manually on spawning
        ////get list of towers
        ////check each towers range bounds against this spawns coords
        ////ignore if false
        ////if true call appropriate towershoot function
        //GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        //foreach (GameObject tower in towers)
        //{            
        //    CircleCollider2D cc = tower.GetComponent<CircleCollider2D>();
        //    Bounds towerBounds = cc.bounds;
        //    Bounds creepBounds = this.GetComponentInChildren<BoxCollider2D>().bounds;
        //    Bounds dd = this.transform.GetChild(0).GetComponent<BoxCollider2D>().bounds;
            
        //    Debug.Log("t : " + towerBounds + " c : " + creepBounds + " cc : " + dd);
        //    if (creepBounds.Intersects(towerBounds))
        //    {
        //        tower.GetComponent<TowerScript>().ManuallyAddTarget(this.transform.GetChild(0).gameObject); //we only want the gameobject with the collider, ie the Creep
        //        //TODO: this hardcoding int index of getchild is annoying, it means I need to be really careful how each creep is designed/imped.
        //        //Is there a better way of checking this?
        //    }
        //}
    }



*/
