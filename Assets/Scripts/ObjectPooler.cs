using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoSingleton<ObjectPooler>
{
    [System.Serializable]
    public class Pool
    {
        public string Tag { get; set; }
        public GameObject prefab;
        public int size;
        public bool CanExpand;
    }

    //Each pool contains the basic info about a pool: id tag, prefb to be pooled, how many and if it can expand or not
    //Each pool is then added to the grand list of pools
    //the pool is only used for set up, after that its ignored and the string tab is used to identify the dictionary
    //Now we have a pooldictionary which is the meat of the pool and contains the hundreds of prefab instances, id'd by a tag, with a queue to hold the instances
    //So it looks quite silly to have a pool object (and subsequent list of) to hold metadata about a group of pooldictionaries which are the actual containers
    //their only link is via the prefab.name.tag

    //what i really need is a simple pool what knows what it contains and how many etc.

    public List<Pool> pools;
    //public Dictionary<string, Queue<GameObject>> poolDictionary;
    public Dictionary<string, Stack<GameObject>> poolDictionary;
    

    //Each pool is a dictionary of tag and queue of prefabs;
    //all pools are added to the pools list

    //TODO: add autoincrease pool size flag and impl; include prewarm int too

    /*
    if (shouldExpand) {
  GameObject obj = (GameObject)Instantiate(objectToPool);
    obj.SetActive(false);
  pooledObjects.Add(obj);
  return obj;
} else {
  return null;
}

    */

    private void Start()
    {
        CreatePools();
        
    }

    private void CreatePools() 
    { 
        poolDictionary = new Dictionary<string, Stack<GameObject>>();

        foreach (Pool pool in pools)
        {
            Stack<GameObject> objectPool = new Stack<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                pool.Tag = pool.prefab.name;
                GameObject obj = Instantiate(pool.prefab, this.transform);
                obj.SetActive(false);
                objectPool.Push(obj);
            }
            poolDictionary.Add(pool.Tag, objectPool);

        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent)
    {
        //Check pool exists
        if (!poolDictionary.ContainsKey(tag)) { Debug.LogError("PoolDictionary contains no " + tag); return null; }

        //check are we at max pool usage; if so then if we CanExpand then make another object or else just reuse an existing one
        if (poolDictionary[tag].Count == 0)
        {
            //print("Stack empty!");
            //assume we can expand the pool here

            return null;
        }

        //Pluck a GO from the pool and posit, rotate it etc.
        GameObject objectToSpawn = poolDictionary[tag].Pop();
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.transform.parent = parent;

        //register the GO to the IPooledObject interface so we can change more params(OnObjectSpawn) when it spawns anew as Awake won't be called anymore
        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null) pooledObj.OnObjectSpawn();

        //stick the object tospawn back in the pool after we've activated it
        //poolDictionary[tag].Push(objectToSpawn);
        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag)) { Debug.LogError("PoolDictionary contains no " + tag); return; }
        objectToReturn.SetActive(false);
        poolDictionary[tag].Push(objectToReturn);
    }
}
