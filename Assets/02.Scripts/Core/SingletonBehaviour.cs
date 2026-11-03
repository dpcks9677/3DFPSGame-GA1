using UnityEngine;

// 씬이 바뀌어도 유지되는 싱글톤 MonoBehaviour의 부모 클래스.
// 이미 인스턴스가 있으면 새로 생긴 쪽을 파괴한다.
public class SingletonBehaviour<T> : MonoBehaviour where T : class
{
    private static T _instance = null;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}
