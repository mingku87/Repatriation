using UnityEngine;

public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null) Debug.LogError($"ScriptableObject<{typeof(T)}> instance not found in scene.");
            }
            return _instance;
        }
    }
}