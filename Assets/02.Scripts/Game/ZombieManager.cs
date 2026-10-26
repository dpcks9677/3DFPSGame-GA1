using System;
using UnityEngine;

// 살아있는 좀비 수를 관리한다. 좀비가 스스로 등록/해제하고, 수가 바뀌면 구독자에게 알린다.
public class ZombieManager : MonoBehaviour
{
    private static ZombieManager _instance = null;
    public static ZombieManager Instance => _instance;

    private int _count = 0;
    public int Count => _count;

    public event Action OnCountChanged = null;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public void Register()
    {
        _count++;
        OnCountChanged?.Invoke();
    }

    public void Unregister()
    {
        _count--;
        OnCountChanged?.Invoke();
    }
}
