using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _jumpPower = 7f;
    [SerializeField] private float _dashSpeed = 12f;

    private const float Gravity = -20f; // 중력 가속도
    private float _yVelocity = 0f; // 떨어지는 속도

    [SerializeField] private int _validJumpCount = 2;

    [SerializeField] private int _stamina = 3;
    [SerializeField] private int _maxStamina = 5;
    private float _staminaRefillCounter = 0f;

    public float MaxStamina => _maxStamina;
    public float Stamina => _stamina;

    private float _dashTime = 0f;

    [SerializeField] private CharacterController _characterController;


    // 코요테 타임 변수
    [SerializeField] private float _coyoteTime = 0.12f; // 인스펙터에서 튜닝
    private float _coyoteCounter = 0f;
    private bool _wasGrounded = true;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 0. 스테미너 리필 카운터
        if (_stamina < 5)
        {
            _staminaRefillCounter += Time.deltaTime;
            if (_staminaRefillCounter >= 3f)
            {
                addStamina(1);
                _staminaRefillCounter = 0f;
            }
        }
        else
        {
            _staminaRefillCounter = 0f;
        }

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

        // 코요테 타임
        bool grounded = _characterController.isGrounded;

        if (grounded)
        {
            _coyoteCounter = _coyoteTime;   // 바닥에 있으면 계속 리필
            if (_yVelocity < 0f)
            {
                _yVelocity = -2f;
                _validJumpCount = 2;
            }
        }
        else
        {
            _coyoteCounter -= Time.deltaTime;

            // 점프 없이 바닥을 벗어난 첫 프레임 → 지상 점프 1회분 소모
            if (_wasGrounded && _yVelocity <= 0f)
                _validJumpCount = Mathf.Min(_validJumpCount, 1);
        }
        _wasGrounded = grounded;



        // 4. 점프
        if (Input.GetButtonDown("Jump") && _stamina > 0)
        {
            bool coyoteJump = _coyoteCounter > 0f;

            if (coyoteJump || _validJumpCount > 0)
            {
                _yVelocity = _jumpPower;    // += 가 아니라 = (아래 주의 참고)
                _coyoteCounter = 0f;        // 코요테 점프 중복 방지
                if (!coyoteJump) _validJumpCount--;
                else _validJumpCount = Mathf.Max(0, _validJumpCount - 1);
                addStamina(-1);
            }
        }


        Vector3 velocity;

        // 5. 속도를 구한다
        if (Input.GetKey(KeyCode.LeftShift) && _stamina > 0)
        {
            _dashTime += Time.deltaTime;
            velocity = direction * _dashSpeed; // 대쉬
        }
        else
        {
            velocity = direction * _moveSpeed; // 일반
        }

        // 5-1 대쉬 시 스테미나 감소 로직
        if (_dashTime >= 1f)
        {
            addStamina(-1);
            _dashTime = 0f;
        }

        velocity.y = _yVelocity;

        // 6. 방향과 속력에 따라 이동한다.
        _characterController.Move(velocity * Time.deltaTime);

    }

    private void addStamina(int value)
    {
        _stamina += value;
        Debug.Log($"stamina: {_stamina}");
    }
}
