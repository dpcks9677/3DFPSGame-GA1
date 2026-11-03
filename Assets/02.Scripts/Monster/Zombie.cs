using System;
using System.Collections;
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
}

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour, IDamageable
{
    private const float BloodSprayLifetime = 1f;  // 블러드 스프레이가 남아 있는 시간

    [Header("스탯")]
    [SerializeField] private float _detectDistance = 8;    // 플레이어를 발견하는 거리
    [SerializeField] private float _combackDistance = 15;   // 처음 자리에서 이만큼 멀어지면 돌아가는 거리
    [SerializeField] private float _attackDamage = 10f;     // 공격 대미지
    [SerializeField] private float _attackRange = 1.5f;     // 공격 범위
    [SerializeField] private float _attackInterval = 1.2f;  // 공격 간격
    [SerializeField] private float _moveSpeed = 5;          // 이동속도
    [SerializeField] private float _maxHealth = 30;
    [SerializeField] private float _health;

    [Header("이펙트")]
    [SerializeField] private GameObject _bloodSprayPrefab;
    [SerializeField] private float _knockbackDistance = 0.4f;  // 맞았을 때 뒤로 밀리는 거리 (m)
    [SerializeField] private float _knockbackTime = 0.15f;     // 뒤로 밀리는 시간 (초)

    private float _attackTimer = 0f;
    private Vector3 _startPosition;
    private GameObject _player;
    private NavMeshAgent _agent;
    private Animator _animator;


    public float MaxHealth => _maxHealth;
    public float Health => _health;

    // 좀비가 죽는 순간 발행하는 이벤트 (스포너, UI 등이 구독한다)
    public event Action<Zombie> Died;


    private ZombieState _state = ZombieState.Idle;

    private void Start()
    {
        _health = _maxHealth;

        // 길을 찾아 걷게 해주는 컴포넌트 NavMeshAgent를 찾아서 참조
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _moveSpeed;

        _animator = GetComponentInChildren<Animator>();

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

        StopAllCoroutines();

        if (_health > 0f)
        {
            // 좀비의 자식으로 블러드 스프레이를 생성! -> 위치 수정!
            GameObject bloodSpray = Instantiate(_bloodSprayPrefab, transform);
            //bloodSpray.transform.SetParent(transform);
            bloodSpray.transform.position = damageInfo.HitPoint;
            bloodSpray.transform.forward = damageInfo.HitNormal;
            Destroy(bloodSpray, BloodSprayLifetime);  // 반복 재생 파티클이라 직접 지우지 않으면 맞을 때마다 쌓인다.

            // 잠깐 멈칫 하고 싶다!
            Debug.Log($"{_state} -> Hit");
            _animator.SetTrigger("Hit");
            _state = ZombieState.Hit;
            StartCoroutine(Hit_Coroutine());
        }
        else
        {
            Debug.Log($"{_state} -> Death");
            _animator.SetTrigger("Death");
            _state = ZombieState.Death;
            Died?.Invoke(this);
            StartCoroutine(Death_Coroutine());
        }

        // StopAllCoroutines 뒤에 시작해야 끊기지 않는다. 생존/사망 모두 밀려난다.
        StartCoroutine(Knockback_Coroutine());
    }

    // 플레이어 반대 방향으로 짧게 밀려난다. 끝으로 갈수록 느려진다.
    private IEnumerator Knockback_Coroutine()
    {
        Vector3 direction = transform.position - _player.transform.position;
        direction.y = 0f;
        direction.Normalize();

        float elapsed = 0f;
        while (elapsed < _knockbackTime)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;

            // 남은 시간 비율에 비례해 속도를 줄인다. (처음 빠르게, 나중에 느리게)
            float speed = _knockbackDistance / _knockbackTime * 2f * (1f - elapsed / _knockbackTime);
            _agent.Move(direction * speed * deltaTime);
            yield return null;
        }
    }

    private IEnumerator Hit_Coroutine()
    {
        StopAgent();
        yield return new WaitForSeconds(0.5f); // 잠깐 멈칫한다!

        // 맞았으니 쫒아온다.
        Debug.Log($"{_state} -> Trace");
        _state = ZombieState.Trace;
    }

    private IEnumerator Death_Coroutine()
    {
        StopAgent();
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // 대기: 제자리에서 플레이어를 기다린다.
    private void Idle()
    {
        StopAgent();

        // 플레이어 가까워지면 Trace 상태로 전이한다.
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance <= _detectDistance)
        {
            Debug.Log($"{_state} -> Trace");
            _animator.SetTrigger("IdleToMove");
            _state = ZombieState.Trace;
        }
    }

    // 추격: 플레이어를 쫓아간다.
    private void Trace()
    {
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
            _animator.SetTrigger("MoveAttackDelay");
            _state = ZombieState.Attack;
            return;
        }

        // SetDestination은 목적지 설정
        // - 호출할 때마다 목적지를 향해 조금씩 이동 (X)
        // - 한 번 세팅하면 내부에서 계속해서 이동  (O)
        _agent.SetDestination(_player.transform.position);
    }

    // 복귀: 처음 자리로 돌아간다.
    private void Comeback()
    {
        // 처음 자리에 도착하면 Idle
        if (Vector3.Distance(transform.position, _startPosition) < _agent.stoppingDistance)
        {
            Debug.Log($"{_state} -> Idle");
            _animator.SetTrigger("MoveToIdle");
            _state = ZombieState.Idle;
            return;
        }

        // 처음 자리로 돌아간다
        _agent.SetDestination(_startPosition);
    }

    // 공격: 일정 시간마다 플레이어를 때린다.
    private void Attack()
    {
        StopAgent();

        // 플레이어가 공격 사정 거리 밖으로 나가면 쫓아!
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance > _attackRange)
        {
            Debug.Log($"{_state} -> Trace");
            _animator.SetTrigger("AttackMove");
            _state = ZombieState.Trace;
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackInterval)
        {
            _animator.SetTrigger("StartAttack");

            _attackTimer = 0f;
        }
    }

    public void AttackAction()
    {
        DamageInfo info = new DamageInfo()
        {
            Value = _attackDamage,
            HitPoint = _player.transform.position,
            HitNormal = (transform.position - _player.transform.position).normalized,
        };

        _player.GetComponent<IDamageable>().TakeDamage(info);
    }

    private void StopAgent()
    {
        _agent.ResetPath();                    // 에이전트의 목적지 삭제
        _agent.velocity = Vector3.zero;        // 미끄럼 방지를 위해 가속도 제로!
    }
}
