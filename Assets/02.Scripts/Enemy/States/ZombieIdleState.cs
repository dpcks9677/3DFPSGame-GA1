// 대기: 제자리에서 플레이어를 기다린다.
public class ZombieIdleState : IZombieState
{
    private readonly Zombie _zombie;
    private float _idleTimer = 0f;

    public ZombieIdleState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void Enter()
    {
        _idleTimer = 0f;
    }

    public void Tick()
    {
        // Todo. Idle 애니메이션 실행

        // 플레이어 가까워지면 Trace 상태로 전이한다.
        if (_zombie.DistanceToPlayer <= _zombie.DetectDistance)
        {
            _zombie.ChangeState(ZombieState.Trace);
            return;
        }

        // 일정 시간 기다렸으면 순찰을 나간다.
        _idleTimer += UnityEngine.Time.deltaTime;
        if (_idleTimer >= _zombie.PatrolInterval)
        {
            _zombie.ChangeState(ZombieState.Patrol);
        }
    }
}
