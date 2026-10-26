using UnityEngine;

// 공격: 일정 시간마다 플레이어를 때린다.
public class ZombieAttackState : IZombieState
{
    private readonly Zombie _zombie;
    private float _attackTimer = 0f;

    public ZombieAttackState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void Enter()
    {
        _attackTimer = 0f;
    }

    public void Tick()
    {
        // 플레이어가 공격 사정 거리 밖으로 나가면 쫓아!
        if (_zombie.DistanceToPlayer > _zombie.AttackRange)
        {
            _zombie.ChangeState(ZombieState.Trace);
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _zombie.AttackInterval)
        {
            // Todo. 공격 애니메이션 실행
            GameObject player = _zombie.Player;
            DamageInfo damageInfo = new DamageInfo()
            {
                Value = _zombie.AttackDamage,
                HitPoint = player.transform.position,
                HitNormal = (_zombie.transform.position - player.transform.position).normalized
            };

            player.GetComponent<IDamageable>().TakeDamage(damageInfo);

            Debug.Log("플레이어를 때렸습니다.");
            _attackTimer = 0f;
        }
    }
}
