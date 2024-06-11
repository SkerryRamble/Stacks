using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This works better than Object Pooler, but still needs some work
//TODO: bullet particle effects behve odd, appearing on newly spawned creeps
//need to think about pool for particle effects and how they naturally stop

public class BulletPool : MonoSingleton<BulletPool>
{
    [System.Serializable]
    public class BPool
    {
        //TODO: could probably do away with poolid and just use prefab.name
        public string poolID { get; set; }
        public Stack<GameObject> PooledObjects { get => pooledObjects; set => pooledObjects = value; }
        public GameObject prefab;
        public int size;
        public bool CanExpand;
        private Stack<GameObject> pooledObjects;
    }

    //public BPool bPool;
    public List<BPool> PoolList;
    private int index;

    private void Start()
    {
        CreatePools();
        index = 0;
    }

    private void CreatePools()
    {
        foreach (BPool item in PoolList)
        {
            item.PooledObjects = new Stack<GameObject>();
            item.poolID = item.prefab.name;
            //Access Pool and instantiate 
            for (int i = 0; i < item.size; i++)
            {
                AddToStack(item);
            }
        }
    }

    private void AddToStack(BPool item)
    {
        GameObject obj = Instantiate(item.prefab, this.transform);
        obj.SetActive(false);
        obj.name += index;
        index++;
        item.PooledObjects.Push(obj);
    }

    public GameObject SpawnFromPool(string poolID, Vector3 position, Quaternion rotation, Transform parent)
    {
        //need to get pool somehow
        BPool bPool = PoolList.Find(x => x.poolID == poolID);
        //TODO: check bpool is not null

        if (bPool.PooledObjects.Count == 0)
        {
            if (bPool.CanExpand)  AddToStack(bPool);
            else
            {
                //print("Stack is empty!");
                return null;
            }
        }

        //Pluck a GO from the pool and posit, rotate it etc.
        GameObject objectToSpawn = bPool.PooledObjects.Pop();

        
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.transform.parent = parent;
        

        //register the GO to the IPooledObject interface so we can change more params(OnObjectSpawn) when it spawns anew as Awake won't be called anymore
        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null) pooledObj.OnObjectSpawn();

        objectToSpawn.SetActive(true);
        return objectToSpawn;
    }

    public void ReturnToPool(string poolID, GameObject objectToReturn)
    {
        if (objectToReturn == null) return; //hackfix, impact effects havent been pooled properly yet
        BPool bPool = PoolList.Find(x => x.poolID == poolID);
        objectToReturn.SetActive(false);
        bPool.PooledObjects.Push(objectToReturn);
    }

}
