using UnityEngine;
using System;

public class Player : MonoBehaviour, IDamageable
{

    private Animator _animator;
    private int _injuredLayerIndex;


    [Header("스탯")]
    [SerializeField] private float _maxHealth = 100f;
    private float _health;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    // Action 자료형: 함수(들)를 저장할 수 있다.
    public event Action OnDamaged = null;

    private void Start()
    {
        _health = _maxHealth;

        _animator = GetComponentInChildren<Animator>();
        _injuredLayerIndex = _animator.GetLayerIndex("Injured Layer");
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_health <= 0f) return;

        _health -= damageInfo.Value;

        float healthPercent = _health / _maxHealth;
        float injured = Mathf.InverseLerp(0.7f, 0.2f, healthPercent);
        _animator.SetLayerWeight(2, injured);

        OnDamaged?.Invoke();

        Debug.Log($"플레이어가 맞았다. 대미지 {damageInfo.Value}, 남은 체력 {_health}");
        if (_health <= 0f)
        {
            Debug.Log("플레이어가 쓰러졌다!");
            GameManager.Instance.GameOver();
        }
    }
}
