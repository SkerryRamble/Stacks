using UnityEngine;
using System.Collections;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null) Debug.LogError(typeof(T).ToString() + " is NULL");
            return _instance;           
        }
    }

    private void Awake()
    {
        _instance = this as T;
        Init();
    }

    public virtual void Init()
    {
        //nothing here so inheriting classes can override this and won't need base.Init();
        //if there was some code here, they would need base.Init()
    }

}
