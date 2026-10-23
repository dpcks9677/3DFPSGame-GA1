using Lean.Pool;
using UnityEngine;

// 충돌 시 폭발 이펙트를 남기고 풀로 반납됨
public class Bomb : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private float _effectLifetime = 3f;
    [SerializeField] private float _damage = 100f;
    [SerializeField] private float _explosionRange = 4.5f;

    private bool _exploded;

    public void OnSpawn()
    {
        _exploded = false;
    }

    public void OnDespawn()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 반납 전에 충돌이 여러 번 들어와도 한 번만 폭발
        if (_exploded) return;
        _exploded = true;

        GameObject effect = LeanPool.Spawn(_explosionEffectPrefab.gameObject, transform.position, Quaternion.identity);
        effect.GetComponent<ParticleSystem>().Play();
        ApplyAreaDamage();
        LeanPool.Despawn(effect, _effectLifetime);

        LeanPool.Despawn(gameObject);
    }

    private void ApplyAreaDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRange, LayerMask.GetMask("Drum", "Zombie"));

        foreach (Collider col in colliders)
        {
            DamageInfo damageInfo = new DamageInfo();
            damageInfo.Value = _damage;
            damageInfo.HitPoint = col.ClosestPoint(transform.position);  // 폭발 중심에서 가장 가까운 콜라이더 표면 위치
            damageInfo.HitNormal = (transform.position - damageInfo.HitPoint).normalized;  // 표면에서 폭발 쪽을 향하는 방향

            col.GetComponent<IDamageable>()?.TakeDamage(damageInfo);
        }
    }
}
