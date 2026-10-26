using System;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _maxHealth = 100f;
    private float _health;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    // Action 자료형은 함수들을 저장할 수 있다.
    public event Action OnDamaged = null;

    private void Start()
    {
        _health = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (Health <= 0f) return;

        _health -= damageInfo.Value;

        OnDamaged?.Invoke();

        Debug.Log($"플레이어가 맞았다. 대미지 {damageInfo.Value}, 남은 체력 {_health}");


        if (_health <= 0f)
        {
            Debug.Log("플레이어 사망");
            GameManager.Instance.GameOver();
        }
    }
}
