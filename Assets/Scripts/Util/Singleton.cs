using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    var container = new GameObject(nameof(T));
                    _instance = container.AddComponent<T>();
                    DontDestroyOnLoad(_instance);
                }
            }

            return _instance;
        }
    }

    public virtual void Awake()
    {
        RemoveDuplicates();
    }

    public static void Reset()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }
    }

    private void RemoveDuplicates()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Init()
    {
    }
}