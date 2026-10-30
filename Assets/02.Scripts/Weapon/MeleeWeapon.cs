using UnityEngine;

// 지팡이(트리거)에 닿은 것을 Player의 PlayerMeleeAttack에게 알려 준다.
// 트리거 이벤트는 콜라이더가 붙은 오브젝트의 스크립트가 받는다.
public class MeleeWeapon : MonoBehaviour
{
    private PlayerMeleeAttack _meleeAttack;

    private void Awake()
    {
        _meleeAttack = GetComponentInParent<PlayerMeleeAttack>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _meleeAttack.Hit(other);
    }
}
