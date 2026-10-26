using UnityEngine;

// 순찰: 정해둔 목표 지점으로 이동한다.
public class ZombiePatrolState : IZombieState
{
    private readonly Zombie _zombie;
    private Vector3 _patrolTarget;
    private float _patrolTimer = 0f;

    public ZombiePatrolState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void Enter()
    {
        _patrolTimer = 0f;

        // 처음 자리 기준 반경 안에서 목표 지점을 정한다. (높이 없음)
        float radius = _zombie.PatrolRadius;
        _patrolTarget = _zombie.StartPosition + new Vector3(
            Random.Range(-radius, radius), 0f, Random.Range(-radius, radius));
    }

    public void Tick()
    {
        // 플레이어가 가까워지면 Trace 상태로 전이한다.
        if (_zombie.DistanceToPlayer <= _zombie.DetectDistance)
        {
            _zombie.ChangeState(ZombieState.Trace);
            return;
        }

        // 목표 지점에 도착했거나 너무 오래 걸리면 Idle로 돌아간다.
        _patrolTimer += Time.deltaTime;
        Vector3 direction = _patrolTarget - _zombie.transform.position;
        direction.y = 0f;
        if (direction.magnitude < 1f || _patrolTimer >= _zombie.PatrolTimeout)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        _zombie.MoveToward(_patrolTarget);
    }
}
