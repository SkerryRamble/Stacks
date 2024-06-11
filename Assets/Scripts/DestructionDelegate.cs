using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionDelegate : MonoBehaviour
{

    public delegate void EnemyDelegate(GameObject enemy);
    public EnemyDelegate enemyDelegate;

    private void OnDestroy()
    {
        if (enemyDelegate != null)
        {
            enemyDelegate(gameObject);
        }
    }
}
