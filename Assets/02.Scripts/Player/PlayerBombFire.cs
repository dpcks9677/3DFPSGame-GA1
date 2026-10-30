using UnityEngine;

// B 키를 누르고 있으면 힘을 모으고, 떼면 시선이 바라보는 방향으로 수류탄을 던지고 싶다.
public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;
    [SerializeField] private float _throwPower = 15f;          // 최대 던지는 힘
    [SerializeField] private float _minThrowPower = 5f;        // 차징 없이 바로 뗐을 때의 힘
    [SerializeField] private float _maxChargeTime = 1.5f;      // 최대 힘에 도달하는 차징 시간
    [SerializeField] private float _throwAnimSpeed = 1.5f;     // 던지는 모션 재생 속도

    [SerializeField] private int _maxBombCount = 3;            // 가질 수 있는 폭탄 수

    private Animator _animator;
    private int _bombCount;                                    // 남은 폭탄 수
    private bool _isThrowing = false;
    private bool _isCharging = false;
    private float _chargeTime = 0f;
    private float _currentThrowPower = 0f;

    public bool IsThrowing => _isThrowing;
    public int BombCount => _bombCount;                        // 남은 폭탄 수 (밖에서는 읽기만 한다)
    public event System.Action<int> OnBombChanged;             // 폭탄 수가 바뀌면 남은 수를 알린다.

    private PlayerWeaponMode _weaponMode;

    private void Awake()
    {
        _weaponMode = GetComponent<PlayerWeaponMode>();
        _bombCount = _maxBombCount;  // 폭탄을 가득 가지고 시작한다.
    }

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        if (_weaponMode.Mode != WeaponMode.Normal)
        {
            return;
        }

        // 1. B 키 입력 받기
        // 던지는 중에는 다시 던질 수 없고, 폭탄이 남아 있어야 한다.
        if (Input.GetKeyDown(KeyCode.B) && !_isThrowing && _bombCount > 0)
        {
            _isThrowing = true;
            _isCharging = true;
            _chargeTime = 0f;
            _animator.SetFloat("ThrowSpeed", _throwAnimSpeed);
            _animator.SetTrigger("Throw");

            _bombCount--;
            OnBombChanged?.Invoke(_bombCount);  // 하나 줄었다고 알린다.
        }

        if (!_isCharging) return;

        // 2. 누르고 있는 동안 힘을 모은다. (팔을 뒤로 젖힌 채 대기)
        _chargeTime += Time.deltaTime;

        // 3. 버튼을 떼면 모은 힘을 확정하고 던지는 모션을 이어서 재생한다.
        if (Input.GetKeyUp(KeyCode.B))
        {
            _isCharging = false;
            _currentThrowPower = Mathf.Lerp(_minThrowPower, _throwPower, _chargeTime / _maxChargeTime);
            _animator.SetFloat("ThrowSpeed", _throwAnimSpeed);
        }
    }

    // 팔을 뒤로 젖힌 시점에 호출된다. 아직 누르고 있으면 모션을 멈추고 대기한다. (PlayerAnimationEvents 경유)
    public void OnThrowCharge()
    {
        if (_isCharging) _animator.SetFloat("ThrowSpeed", 0f);
    }

    // 던지는 애니메이션에서 손을 놓는 순간 호출된다. (PlayerAnimationEvents 경유)
    public void OnThrowRelease()
    {
        Shoot();
    }

    // 던지는 애니메이션이 끝날 때쯤 호출된다. (PlayerAnimationEvents 경유)
    public void OnThrowEnd()
    {
        _isThrowing = false;
    }

    private void Shoot()
    {
        // 4. 발사 위치에서 수류탄 생성하기
        Bomb bomb = Instantiate(_bombPrefab, _firePoint.position, Quaternion.identity);

        // 5. 생성된 수류탄을 시선 방향으로 던지기
        Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
        rigidbody.AddForce(Camera.main.transform.forward * _currentThrowPower, ForceMode.Impulse);
    }

}
