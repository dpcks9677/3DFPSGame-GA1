using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;
using UnityEngine.AI;

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

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour, IDamageable, IPoolable
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

    private Vector3 _startPosition;
    private GameObject _player;
    private NavMeshAgent _agent;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    // 상태 객체들이 읽는 값
    public GameObject Player => _player;
    public Vector3 StartPosition => _startPosition;
    public float DistanceToPlayer => Vector3.Distance(transform.position, _player.transform.position);
    public float DetectDistance => _detectDistance;
    public float ComebackDistance => _combackDistance;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackInterval => _attackInterval;
    public float PatrolInterval => _patrolInterval;
    public float PatrolRadius => _patrolRadius;
    public float PatrolTimeout => _patrolTimeout;


    // 대미지를 받을 때마다 구독자(체력바 등)에게 알린다.
    // (using System 하면 Random이 UnityEngine.Random과 모호해지므로 System.Action으로 쓴다.)
    public event System.Action OnDamaged = null;

    private ZombieState _state = ZombieState.Idle;

    // Hit, Death는 코루틴과 함께 Zombie가 직접 처리하므로 상태 객체가 없다. (현재 상태 객체는 null)
    private readonly Dictionary<ZombieState, IZombieState> _states = new Dictionary<ZombieState, IZombieState>();
    private IZombieState _currentState = null;

    // 풀에서 꺼내진 좀비는 죽으면 Destroy 대신 풀로 반납한다. (씬에 직접 둔 좀비는 false)
    private bool _pooled = false;
    private readonly List<GameObject> _bloodSprays = new List<GameObject>();

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _moveSpeed;

        _states[ZombieState.Idle] = new ZombieIdleState(this);
        _states[ZombieState.Patrol] = new ZombiePatrolState(this);
        _states[ZombieState.Trace] = new ZombieTraceState(this);
        _states[ZombieState.Comeback] = new ZombieComebackState(this);
        _states[ZombieState.Attack] = new ZombieAttackState(this);

        // 구독자가 Start에서 체력을 읽어도 되도록 Awake에서 초기화한다.
        _health = _maxHealth;
    }

    private void Start()
    {
        // 씬에 직접 둔 좀비는 여기서 초기화한다. (풀에서 꺼낸 좀비는 OnSpawn에서 이미 초기화했다.)
        if (!_pooled)
        {
            Init();
        }
    }

    // 풀에서 꺼내질 때마다 호출된다.
    public void OnSpawn()
    {
        _pooled = true;
        Init();
    }

    // 풀로 반납될 때 다음 사용을 위해 체력과 남은 이펙트를 정리한다.
    public void OnDespawn()
    {
        foreach (GameObject bloodSpray in _bloodSprays)
        {
            Destroy(bloodSpray);
        }
        _bloodSprays.Clear();
        _health = _maxHealth;
    }

    // 상태를 처음으로 되돌린다.
    private void Init()
    {
        _startPosition = transform.position;
        _agent.enabled = true;

        // 게임을 시작하면 플레이어를 찾는다.
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
        }

        ChangeState(ZombieState.Idle);

        ZombieManager.Instance.Register();
    }


    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        _currentState?.Tick();
    }

    // 상태를 바꾼다. 상태 객체가 없는 상태(Hit, Death)면 현재 상태 객체는 null이 된다.
    public void ChangeState(ZombieState next)
    {
        if (_state != next)
        {
            Debug.Log($"{_state} -> {next}");
        }
        _state = next;

        _states.TryGetValue(next, out _currentState);
        _currentState?.Enter();
    }

    // 목표 지점을 향해 바라보며 이동한다. (높이 무시)
    public void MoveToward(Vector3 target)
    {

        _agent.SetDestination(target);
    }

    // 경로를 지우고 그 자리에 멈춘다. (에이전트가 켜져 있을 때만 호출해야 한다.)
    private void StopAgent()
    {
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
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
            _bloodSprays.Add(bloodSpray);

            // 잠깐 멈칫 하고 싶다!
            ChangeState(ZombieState.Hit);
            StartCoroutine(Hit_Coroutine());
        }
        else
        {
            ChangeState(ZombieState.Death);
            ZombieManager.Instance.Unregister();
            StartCoroutine(Death_Coroutine());
        }

    }

    private IEnumerator Hit_Coroutine()
    {
        yield return new WaitForSeconds(0.3f); // 잠깐 멈칫한다!

        StopAgent();

        // 맞았으니 쫒아온다.
        ChangeState(ZombieState.Trace);
    }

    private IEnumerator Death_Coroutine()
    {
        StopAgent();
        _agent.enabled = false;
        yield return new WaitForSeconds(0.5f);

        if (_pooled)
        {
            LeanPool.Despawn(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
