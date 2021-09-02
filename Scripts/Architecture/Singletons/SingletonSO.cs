using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonSO<T> : ScriptableObject where T : ScriptableObject
{
    protected static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var type = typeof(T);
                T[] instances = Resources.LoadAll<T>(string.Empty);
                _instance = instances != null ? instances[0] : default;
                if (_instance == null)
                {
                    Debug.LogError("You must create an asset instance in your project in Resources order for the singleton to work");
                }
                else if (instances.Length > 1)
                {
                    Debug.LogError("There are multiple asset instances of your singleton, assure there is only 1");
                }
            }

            return _instance;
        }
    }
}
