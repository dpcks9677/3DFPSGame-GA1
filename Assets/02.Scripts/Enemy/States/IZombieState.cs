// 좀비 상태 하나를 객체로 표현한다. 전이는 Zombie.ChangeState로 한다.
public interface IZombieState
{
    // 이 상태로 들어올 때 한 번 호출된다.
    void Enter();

    // 이 상태인 동안 매 프레임 호출된다.
    void Tick();
}
