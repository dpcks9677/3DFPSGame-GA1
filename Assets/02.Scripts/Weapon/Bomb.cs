using Lean.Pool;
using UnityEngine;

// 충돌 시 폭발 이펙트를 남기고 풀로 반납됨
public class Bomb : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private float _effectLifetime = 3f;

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
        LeanPool.Despawn(effect, _effectLifetime);

        LeanPool.Despawn(gameObject);
    }
}
