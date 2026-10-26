using System;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _maxHealth = 100f;
    private float _health;
    [SerializeField] private UI_HitEffect _uIHitEffect;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    private void Start()
    {
        _health = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (Health <= 0f) return;

        _health -= damageInfo.Value;

        Debug.Log($"플레이어가 맞았다. 대미지 {damageInfo.Value}, 남은 체력 {_health}");

        _uIHitEffect.Flash();

        if (_health <= 0f)
        {
            Debug.Log("플레이어 사망");
            GameManager.Instance.GameOver();
        }
    }
}
