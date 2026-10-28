using UnityEngine;

// Animator가 붙은 자식 오브젝트에서 애니메이션 이벤트를 받아 부모의 컴포넌트로 전달한다.
public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerBombFire _bombFire;

    private void Awake()
    {
        _bombFire = GetComponentInParent<PlayerBombFire>();
    }

    // 수류탄 던지기 클립에서 팔을 뒤로 젖힌 프레임에 호출된다.
    public void OnGrenadeCharge()
    {
        _bombFire.OnThrowCharge();
    }

    // 수류탄 던지기 클립에서 손을 놓는 프레임에 호출된다.
    public void OnGrenadeRelease()
    {
        _bombFire.OnThrowRelease();
    }

    // 수류탄 던지기 클립이 끝날 때쯤 호출된다.
    public void OnGrenadeEnd()
    {
        _bombFire.OnThrowEnd();
    }
}
