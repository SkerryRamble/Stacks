using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: merge this back with bullet script and call it projectile script
//impl laser and flame and cone fire and other ideas

    //BUG TODO: after a while the missiles teleport again, no idea what's going on
    //looks like it something to do wth pooling, after a while only the first 5 or so missiles are available, so they 'steal'
    //each others spawn...

public class MissileScript : MonoBehaviour, IPooledObject
{
    public GameObject impactEffect;
    public GameObject crashEffect;

    public Transform homingTarget;
    private Vector3 circlingTarget;
    private bool idling = false;
    private float damage;

    private Vector3 bulletOrigin;
    private Vector3 directionOfTravel;

    //for missiles etc.
    public float timeInFlight;
    public float maxFlightTime;
    public float speed = 1.5f;
    public float startTime;
    public float distance;
    public float UserSetRotateSpeed = 135f;
    private float rotateSpeed;
    //public float circleRadius = 1f;
    //public float circleAngle;

    private const bool bulletCrashed = true;
    private const bool bulletDidntCrash = false;
    public TowerScript Tower;
    public Transform TowerOwner;

    public void OnObjectSpawn()
    {
        timeInFlight = 0f;
        startTime = Time.time;
        rotateSpeed = UserSetRotateSpeed;
        this.GetComponent<TrailRenderer>().Clear();
    }


    public void Seek(GameObject _target, float _damage, TowerScript _tower)
    {
        homingTarget = _target.transform;
        damage = _damage;
        Tower = _tower;
        idling = false;
        directionOfTravel = (homingTarget.transform.position - transform.position).normalized;
        
    }

    private void FixedUpdate()
    {
        if (homingTarget == null || !homingTarget.parent.gameObject.activeSelf) { RetargetMissile();}
        UpdateBulletPosition();
        CheckForImpact();
        timeInFlight += Time.fixedDeltaTime;
        
        if (timeInFlight > maxFlightTime) { killBullet(bulletCrashed); }
    }

    private void CheckForImpact()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, directionOfTravel, speed * Time.deltaTime);
        if (hit.collider != null)
        {
            CollidedWithTarget(hit.collider.gameObject.transform);
        }
    }

    void RetargetMissile()  //TODO: pop this into an event callback thing, otherwise it gets called every update (between waves)
    {
        idling = true;
        if (Tower.Target != null) 
        {
            //Debug.Log("retargetted missile");
            homingTarget = Tower.Target.transform;
            idling = false;
        }        
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.tag == "Enemy")
    //    {
    //        CollidedWithTarget(collision.gameObject.transform);
    //    }
    //}

    private void CircleTower()
    {
        directionOfTravel = Tower.transform.position - transform.position;
    }

    private void UpdateBulletPosition()
    {
        //circle missile around parent tower if not valid target
        if (idling) { directionOfTravel = Tower.transform.position - transform.position; }
        else
        {
            directionOfTravel = homingTarget.position - transform.position;
        }
        directionOfTravel.Normalize();

        float rotZ = Mathf.Atan2(directionOfTravel.y, directionOfTravel.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 0f, rotZ), rotateSpeed * Time.deltaTime);
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void killBullet(bool didWeCrash)
    {
        BulletPool.Instance.ReturnToPool("Missile", gameObject);

        if (didWeCrash)
        {
            GameObject effectIns = (GameObject)Instantiate(crashEffect, transform.position, transform.rotation);
            Destroy(effectIns, 2f);
        }
        //Destroy(this.gameObject); //TODO return to pool
        //Invoke("ReturnBulletToPool", 2f);  //cant return bullet to pool until particle system effects are finished
    }

    void ReturnBulletToPool()
    {
        BulletPool.Instance.ReturnToPool("Missile", gameObject);
    }


    void HitTarget()
    {
        //GameMasterScript.instance.source.pitch = Random.Range(lowPitchRange, highPitchRange);
        //float hitVol = coll.relativeVelocity.magnitude * velToVol;
        //if (coll.relativeVelocity.magnitude < velocityClipSplit)
        //GameMasterScript.instance.source.PlayOneShot(GameMasterScript.instance.hitSound, 0.9f);

        GameObject effectIns = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 2f);

        killBullet(bulletDidntCrash);

        if (homingTarget == null) { return; } //might have died already
        //hows our enemy?
        EnemyAI targetScript = homingTarget.gameObject.GetComponentInParent<EnemyAI>();
        targetScript.TakeDamage(damage, TowerOwner, false);
        if (targetScript.isDead)
        {
            //maybe not set it null here, interferes with bullet not knowing what to do with a null target; crash or expire?
            homingTarget = null;
        }
    }

    void CollidedWithTarget(Transform target)
    {
        //GameMasterScript.instance.source.pitch = Random.Range(lowPitchRange, highPitchRange);
        //float hitVol = coll.relativeVelocity.magnitude * velToVol;
        //if (coll.relativeVelocity.magnitude < velocityClipSplit)
        //GameMasterScript.instance.source.PlayOneShot(GameMasterScript.instance.hitSound, 0.9f);

        GameObject effectIns = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 2f);

        killBullet(bulletDidntCrash);

        
            if (target == null) { return; } //might have died already:TODO still erroring
                                            //hows our enemy?
            //EnemyAI targetScript = target.gameObject.GetComponentInParent<EnemyAI>();
            EnemyAI targetScript = target.gameObject.GetComponentInParent<EnemyAI>();
        
        //TODO: rethink this unknown tower idea
        //if (TowerOwner == null) TowerOwner = GameObject.Find("TheUnknownTower").transform;
        if(targetScript == null)
        {
            homingTarget = null; return;
        }
        targetScript.TakeDamage(damage, TowerOwner, false);

        
        //replace this all with delegate thing maybe?
        if (targetScript.isDead)
        {
            //maybe not set it null here, interferes with bullet not knowing what to do with a null target; crash or expire?
            homingTarget = null;
        }
        

    }
}
