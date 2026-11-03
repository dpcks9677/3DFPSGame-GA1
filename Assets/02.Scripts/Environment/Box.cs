using UnityEngine;

// Todo. 공격 받으면 산산조각 나듯 부셔진다. 
public class Box : MonoBehaviour, IDamageable
{

    public bool IsDead => false;  // 아직 부서지는 동작이 없으므로 죽지 않는다.

    public void TakeDamage(DamageInfo damageInfo)
    {
        // Todo. 뭔가 해야함
    }
}
