using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
//sniper bullet should have shorter smoke trail, and more fuzzy
//bullet streak maybe red/yellow
public class BulletScript : MonoBehaviour, IPooledObject
{

    public float bulletFlightRange; //TODO: fix this based on tower range...
    public GameObject target;
    private float damage;
    private Vector3 bulletOrigin;
    public Vector3 directionOfTravel;
    public float speed;
    public bool knockback = false;
    public Transform TowerOwner;

    public void OnObjectSpawn()
    {
        bulletOrigin = transform.position;
    }

    public void Seek(GameObject _target, float _damage, float _towerRange)
    {
        target = _target;
        damage = _damage;
        bulletFlightRange = _towerRange;
        directionOfTravel = (target.transform.position - transform.position).normalized;
        this.GetComponent<TrailRenderer>().Clear();
    }

    private void FixedUpdate()
    {
        UpdateBulletPosition();
        CheckForImpact();
    }

    private void CheckForImpact()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, directionOfTravel, speed * Time.deltaTime);
        if (hit.collider != null)
        {
            CollidedWithTarget(hit.collider.gameObject.transform);
        }
    }

    //TODO: change this to raycast directly ahead of path of bullet
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy")) CollidedWithTarget(collision.gameObject.transform);
    //}

    private void UpdateBulletPosition()
    {
        float rotZ = Mathf.Atan2(directionOfTravel.y, directionOfTravel.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        if (Vector3.Distance(bulletOrigin, transform.position) > bulletFlightRange) { KillBullet(true); }
    }

    void KillBullet(bool didWeCrash)
    {
        //ObjectPooler.Instance.ReturnToPool("Bullet", gameObject);
        BulletPool.Instance.ReturnToPool("Bullet", gameObject);
    }

    void CollidedWithTarget(Transform tt)
    {        
        EnemyAI targetScript = tt.gameObject.GetComponentInParent<EnemyAI>();
        if (targetScript == null) return;
        targetScript.TakeDamage(damage, TowerOwner, knockback);
        KillBullet(false);
    }
}
