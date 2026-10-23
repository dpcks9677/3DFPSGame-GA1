// 데미지를 받을 수 있는 것들에 대한 약속 (인터페이스)

using System.Numerics;

public interface IDamageable
{
    public void TakeDamage(DamageInfo damageInfo);
}
