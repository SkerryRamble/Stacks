using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolList : MonoSingleton<ObjectPoolList>
{
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int poolSize;
    public bool CanExpand;

    private void Start()
    {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }
}
