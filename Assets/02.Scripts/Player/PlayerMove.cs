using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _jumpPower = 7f;
    [SerializeField] private float _dashSpeed = 12f;
    [SerializeField] private int _validJumpCount = 2;
    private const float Gravity = -20f; // 중력 가속도
    private float _yVelocity = 0f; // 떨어지는 속도

    [SerializeField] private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 1. 키보드 입력을 받는다
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. 키보드 입력에 따라 방향을 구한다.
        Vector3 direction = new Vector3(h, 0, v);
        direction.Normalize();

        // 좌표계를 월드 좌표계로 변경
        direction = transform.TransformDirection(direction);

        // 3. 중력 누적 (바닥에 닿으면 속도 초기화)
        if (_characterController.isGrounded && _yVelocity < 0f)
        {
            _yVelocity = -2f; // 바닥에 붙어 있게 살짝 눌러줌
            _validJumpCount = 2;
        }
        _yVelocity += Gravity * Time.deltaTime;

        // 4. 점프
        if (Input.GetButtonDown("Jump") && _validJumpCount > 0)
        {
            _yVelocity += _jumpPower;
            _validJumpCount--;
            Debug.Log(_validJumpCount);
        }

        Vector3 velocity;

        // 5. 속도를 구한다
        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity = direction * _dashSpeed;
        }
        else
        {
            velocity = direction * _moveSpeed;
        }

        velocity.y = _yVelocity;

        // 6. 방향과 속력에 따라 이동한다.
        _characterController.Move(velocity * Time.deltaTime);

    }
}
