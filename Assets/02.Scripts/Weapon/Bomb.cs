using System;
using UnityEngine;

// 충돌 시 폭발 이펙트를 남기고 사라짐
public class Bomb : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    private void OnCollisionEnter(Collision collision)
    {
        ParticleSystem effect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
        effect.Play();

        // TODO: 오브젝트 풀링 적용
        Destroy(gameObject);
    }
}
