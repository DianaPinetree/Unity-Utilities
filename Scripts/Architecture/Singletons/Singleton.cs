using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{

    [Header("Singleton")]
    public bool dontDestroyOnLoad = true;

    private static T _instance;
    public static T instance
    {
        get
        {
#if UNITY_EDITOR
            if (_instance == null && !EditorApplication.isPlayingOrWillChangePlaymode)
            { 
                _instance = GameObject.FindObjectOfType<T>();
            }
#endif
            return _instance;
        }
    }

    bool SetupSingleton()
    {
        if (_instance == null)
        {
            //if i am the first intance, make me the singleton
            _instance = (T)this;

            if(_instance != null)
                if(dontDestroyOnLoad && transform.parent == null) { DontDestroyOnLoad(gameObject); }
                    

            return true;
        }
        else {
            //if a singleton already exists destroy this instance
            if (this != _instance) Destroy(gameObject);

            return false;            
        }
    }

    private void Awake()
    {
        if( SetupSingleton() )
        {
            OnAwake();
        }
    }

    private void OnDestroy()
    {
        if( _instance != this ) return;

        _instance = null;
        MyDestroy();
    }

    protected abstract void OnAwake();
    protected abstract void MyDestroy();
}
