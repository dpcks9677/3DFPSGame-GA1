using System.Collections;
using UnityEngine;

// FSM(유한 상태 머신) : 유한개의 상태가 있고, 상태마다 행동을 다르게 함으로써 AI를 구현하는 유명한 방법
// - "상태": 객체가 가질 수 있는 어떤 조건이나 상황
// - "전이": 조건에 따라 현재 상태에서 다른 상태로 넘어가는 것
// - 그리고 위 방식에 따라 규칙을 세워둔 것을 "머신"


// 좀비가 가질 수 있는 상태
public enum ZombieState
{
    Idle,       // 대기: 제자리에서 플레이어를 기다린다.
    Trace,      // 추격: 플레이어를 쫓아간다.
    Comeback,    // 복귀: 처음 자리로 돌아간다.
    Attack,     // 공격: 일정 시간마다 플레이어를 때린다.
    Hit,        // 피격: 공격(총알, 폭탄)을 받아 잠깐 멈칫한다.
    Death,      // 죽음: 쓰러지고 잠시 뒤 사라진다.
    Patrol
}

public class Zombie : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _detectDistance = 8;    // 플레이어를 발견하는 거리
    [SerializeField] private float _combackDistance = 15;
    [SerializeField] private float _attackDamage = 10f;  // 처음 자리에서 이만큼 멀어지면 돌아가는 거리
    [SerializeField] private float _attackRange = 1.5f;     // 공격 범위
    [SerializeField] private float _attackInterval = 1.2f;  // 공격 간격
    [SerializeField] private float _moveSpeed = 5;          // 이동속도
    [SerializeField] private float _maxHealth = 30;
    [SerializeField] private float _health;
    [SerializeField] private float _patrolInterval = 3f;    // Idle에서 이만큼 기다리면 순찰을 나간다.
    [SerializeField] private float _patrolRadius = 10f;     // 처음 자리 기준 순찰 반경
    [SerializeField] private float _patrolTimeout = 5f;     // 순찰이 이 시간을 넘기면 포기하고 Idle로 돌아간다.

    [Header("이펙트")]
    [SerializeField] private GameObject _bloodSprayPrefab;

    private float _attackTimer = 0f;
    private float _idleTimer = 0f;
    private float _patrolTimer = 0f;
    private Vector3 _patrolTarget;
    private CharacterController _controller;
    private Vector3 _startPosition;
    private GameObject _player;

    public float MaxHealth => _maxHealth;
    public float Health => _health;


    // 대미지를 받을 때마다 구독자(체력바 등)에게 알린다.
    // (using System 하면 Random이 UnityEngine.Random과 모호해지므로 System.Action으로 쓴다.)
    public event System.Action OnDamaged = null;

    private ZombieState _state = ZombieState.Idle;

    private void Awake()
    {
        // 구독자가 Start에서 체력을 읽어도 되도록 Awake에서 초기화한다.
        _health = _maxHealth;
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _startPosition = transform.position;

        // 게임을 시작하면 플레이어를 찾는다.
        _player = GameObject.FindGameObjectWithTag("Player");
    }


    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        switch (_state)
        {
            case ZombieState.Idle:
                Idle();
                break;

            case ZombieState.Trace:
                Trace();
                break;

            case ZombieState.Comeback:
                Comeback();
                break;

            case ZombieState.Attack:
                Attack();
                break;

            case ZombieState.Patrol:
                Patrol();
                break;
        }
    }

    // 대미지를 받는다.
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_state == ZombieState.Death)
        {
            return;
        }

        _health -= damageInfo.Value;

        OnDamaged?.Invoke();

        StopAllCoroutines();

        if (_health > 0f)
        {
            // 좀비의 자식으로 블러드 스프레이를 생성! -> 위치 수정!
            GameObject bloodSpray = Instantiate(_bloodSprayPrefab, transform);
            //bloodSpray.transform.SetParent(transform);
            bloodSpray.transform.position = damageInfo.HitPoint;
            bloodSpray.transform.forward = damageInfo.HitNormal;

            // 잠깐 멈칫 하고 싶다!
            Debug.Log($"{_state} -> Hit");
            _state = ZombieState.Hit;
            StartCoroutine(Hit_Coroutine());
        }
        else
        {
            Debug.Log($"{_state} -> Death");
            _state = ZombieState.Death;
            StartCoroutine(Death_Coroutine());
        }

    }

    private IEnumerator Hit_Coroutine()
    {
        yield return new WaitForSeconds(0.3f); // 잠깐 멈칫한다!

        // 맞았으니 쫒아온다.
        Debug.Log($"{_state} -> Trace");
        _state = ZombieState.Trace;
    }

    private IEnumerator Death_Coroutine()
    {
        _controller.enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    // 대기: 제자리에서 플레이어를 기다린다.
    private void Idle()
    {
        // Todo. Idle 애니메이션 실행

        // 플레이어 가까워지면 Trace 상태로 전이한다.
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance <= _detectDistance)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        // 일정 시간 기다렸으면 순찰을 나간다.
        _idleTimer += Time.deltaTime;
        if (_idleTimer >= _patrolInterval)
        {
            _idleTimer = 0f;
            _patrolTimer = 0f;

            // 처음 자리 기준 반경 안에서 목표 지점을 정한다. (높이 없음)
            _patrolTarget = _startPosition + new Vector3(
                Random.Range(-_patrolRadius, _patrolRadius), 0f, Random.Range(-_patrolRadius, _patrolRadius));

            Debug.Log($"{_state} -> Patrol");
            _state = ZombieState.Patrol;
        }
    }

    // 추격: 플레이어를 쫓아간다.
    private void Trace()
    {
        // Todo. Run 애니메이션 실행


        // 처음 자리에서 너무 멀어지면 돌아간다.
        if (Vector3.Distance(transform.position, _startPosition) > _combackDistance)
        {
            Debug.Log($"{_state} -> Comeback");
            _state = ZombieState.Comeback;
            return;
        }

        // 플레이어가 공격 거리 안이면 공격한다.
        if (Vector3.Distance(transform.position, _player.transform.position) <= _attackRange)
        {
            _attackTimer = 0f;
            Debug.Log($"{_state} -> Attack");
            _state = ZombieState.Attack;
            return;
        }


        Vector3 direction = _player.transform.position - transform.position;
        direction.y = 0f;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);
    }

    // 복귀: 처음 자리로 돌아간다.
    private void Comeback()
    {
        // 처음 자리에 도착하면 Idle
        if (Vector3.Distance(transform.position, _startPosition) < 1f)
        {
            Debug.Log($"{_state} -> Idle");
            _idleTimer = 0f;
            _state = ZombieState.Idle;
            return;
        }

        // 처음 자리로 돌아간다
        Vector3 direction = _startPosition - transform.position;
        direction.y = 0f;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);
    }

    // 공격: 일정 시간마다 플레이어를 때린다.
    private void Attack()
    {
        // 플레이어가 공격 사정 거리 밖으로 나가면 쫓아!
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance > _attackRange)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackInterval)
        {
            // Todo. 공격 애니메이션 실행
            // Todo. 플레이어를 때려!
            DamageInfo damageInfo = new DamageInfo()
            {
                Value = _attackDamage,
                HitPoint = _player.transform.position,
                HitNormal = (transform.position - _player.transform.position).normalized
            };

            _player.GetComponent<IDamageable>().TakeDamage(damageInfo);

            Debug.Log("플레이어를 때렸습니다.");
            _attackTimer = 0f;
        }
    }

    // 순찰: 정해둔 목표 지점으로 이동한다.
    private void Patrol()
    {
        // 플레이어가 가까워지면 Trace 상태로 전이한다.
        if (Vector3.Distance(transform.position, _player.transform.position) <= _detectDistance)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        // 목표 지점에 도착했거나 너무 오래 걸리면 Idle로 돌아간다.
        _patrolTimer += Time.deltaTime;
        Vector3 direction = _patrolTarget - transform.position;
        direction.y = 0f;
        if (direction.magnitude < 1f || _patrolTimer >= _patrolTimeout)
        {
            Debug.Log($"{_state} -> Idle");
            _idleTimer = 0f;
            _state = ZombieState.Idle;
            return;
        }

        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);
    }
}