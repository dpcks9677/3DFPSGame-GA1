using UnityEngine;

// 복귀: 처음 자리로 돌아간다.
public class ZombieComebackState : IZombieState
{
    private readonly Zombie _zombie;

    public ZombieComebackState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void Enter()
    {
    }

    public void Tick()
    {
        // 처음 자리에 도착하면 Idle
        if (Vector3.Distance(_zombie.transform.position, _zombie.StartPosition) < 1f)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        _zombie.MoveToward(_zombie.StartPosition);
    }
}
