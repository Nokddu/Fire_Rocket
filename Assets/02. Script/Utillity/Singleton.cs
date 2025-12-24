using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isQuitting;

    public static T Instance
    {
        get
        {
            if (isQuitting) return null;

            if (instance == null)
            {
                instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                // 정말 자동생성이 필요한 타입만 여기서 만들거나,
                // 아예 자동생성 금지로 두는 걸 추천
                if (instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    instance = go.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual bool DontDestroy => true; // 타입별로 끄고 켤 수 있게

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // 중복 제거
            return;
        }

        instance = this as T;

        if (DontDestroy)
            DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
