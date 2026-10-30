using UnityEngine;

// 근접 모드에서 마우스 왼쪽 버튼을 누르면 왼손으로 지팡이를 휘두른다.
[RequireComponent(typeof(PlayerWeaponMode))]
public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("지팡이")]
    [SerializeField] private GameObject _staff;                        // 왼손 뼈 아래의 Staff
    [SerializeField] private SkinnedMeshRenderer _soldierMesh;         // 몸과 총이 한 메시인 군인 (1번 머티리얼이 총)
    [SerializeField] private Transform _staffHoldPose;                 // 쉴 때 지팡이를 쥐는 자리 (왼손 뼈 아래)
    [SerializeField] private Transform _staffSwingPose;                // 휘두를 때 지팡이를 칼처럼 쥐는 자리 (왼손 뼈 아래)
    [SerializeField, Range(0f, 1f)] private float _fadeStart = 0.7f;   // 동작이 이만큼 지나면 팔을 돌려놓기 시작한다. (0~1)
    [SerializeField] private float _fadeTime = 0.2f;                   // 팔이 총으로 돌아오는 시간 (초)

    [SerializeField] private Collider _staffCollider;  // 지팡이의 충돌체 (근접 공격 판정)
    [SerializeField] private float _damage = 25f;      // 한 번 맞힐 때의 대미지
    [SerializeField] private TrailRenderer _slashTrail;  // 휘두르는 동안 지팡이 끝에 남는 궤적

    private PlayerWeaponMode _weaponMode;
    private Animator _animator;
    private int _meleeLayer;      // Melee Layer의 번호
    private float _meleeWeight;   // Melee Layer의 지금 무게
    private Material[] _withGun;     // 몸과 총을 모두 그리는 머티리얼 목록
    private Material[] _withoutGun;  // 몸만 그리는 머티리얼 목록 (머티리얼이 없는 총 서브메시는 그려지지 않는다)

    private void Awake()
    {
        _weaponMode = GetComponent<PlayerWeaponMode>();
        _animator = GetComponentInChildren<Animator>();
        _meleeLayer = _animator.GetLayerIndex("Melee Layer");  // 이름으로 레이어 번호를 찾는다.

        _withGun = _soldierMesh.sharedMaterials;
        _withoutGun = new[] { _withGun[0] };
    }

    private void OnEnable()
    {
        _weaponMode.OnWeaponModeChanged += RefreshWeapon;
    }

    private void OnDisable()
    {
        _weaponMode.OnWeaponModeChanged -= RefreshWeapon;
    }

    private void Start()
    {
        RefreshWeapon(_weaponMode.Mode);
        DeactivateCollider();  // 처음에는 판정과 궤적을 꺼 둔다.
        StopTrail();
    }

    private void Update()
    {
        UpdateMeleeWeight();  // 게임 상태와 상관없이 팔은 돌려놓는다.

        // 게임 중이 아니면 아무것도 하지 않는다.
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        // 지팡이는 근접 모드에서만 휘두른다. 다른 모드의 왼쪽 버튼은 PlayerGunFire가 쓴다.
        if (_weaponMode.Mode != WeaponMode.Melee)
        {
            return;
        }

        // 1. 커서가 숨어 있을 때 마우스 왼쪽 버튼을 누른 순간, 쉬고 있으면 휘두른다.
        if (!Cursor.visible && Input.GetMouseButtonDown(0) && IsResting())
        {
            _meleeWeight = 1f;
            DeactivateCollider();  // 지난번에 켜진 채 남은 판정과 궤적이 있으면 끄고 시작한다.
            StopTrail();
            _animator.SetTrigger("Melee");
        }
    }

    // Melee Layer가 Empty에 있고 다른 상태로 넘어가는 중도 아니면, 쉬고 있는 것이다.
    private bool IsResting()
    {
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(_meleeLayer);
        return state.IsName("Empty") && !_animator.IsInTransition(_meleeLayer);
    }

    // 근접 모드에서는 총을 숨기고 지팡이를 든다. 다른 모드에서는 지팡이를 숨기고 총을 든다.
    private void RefreshWeapon(WeaponMode mode)
    {
        bool isMelee = mode == WeaponMode.Melee;
        _staff.SetActive(isMelee);
        _soldierMesh.sharedMaterials = isMelee ? _withoutGun : _withGun;
    }

    // 다 휘둘렀거나 쉬고 있으면 Melee Layer의 무게를 천천히 0으로 줄인다.
    private void UpdateMeleeWeight()
    {
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(_meleeLayer);
        bool isFinishing = state.IsName("Melee") && state.normalizedTime >= _fadeStart;
        if (isFinishing || IsResting())
        {
            _meleeWeight = Mathf.MoveTowards(_meleeWeight, 0f, Time.deltaTime / _fadeTime);
        }

        _animator.SetLayerWeight(_meleeLayer, _meleeWeight);

        // 쉴 때는 들고 있는 자리에, 휘두를 때는 칼처럼 쥔 자리에 지팡이를 둔다. 무게를 따라 그 사이를 옮긴다.
        Transform staff = _staff.transform;
        staff.localPosition = Vector3.Lerp(_staffHoldPose.localPosition, _staffSwingPose.localPosition, _meleeWeight);
        staff.localRotation = Quaternion.Slerp(_staffHoldPose.localRotation, _staffSwingPose.localRotation, _meleeWeight);
    }

    public void ActivateCollider() => _staffCollider.enabled = true;
    public void DeactivateCollider() => _staffCollider.enabled = false;

    // 궤적을 그리기 시작한다. 지팡이가 머리 위로 올라간 프레임의 이벤트가 불러 준다.
    public void StartTrail() => _slashTrail.emitting = true;
    public void StopTrail() => _slashTrail.emitting = false;

    // 지팡이에 무언가가 닿으면 MeleeWeapon이 불러 준다.
    public void Hit(Collider other)
    {
        // 1. 내 몸(Player의 CharacterController)에 닿은 것은 무시한다.
        if (other.transform.IsChildOf(transform)) return;

        // 2. 대미지를 받을 수 없으면 무시한다.
        if (!other.TryGetComponent(out IDamageable damageable)) return;

        // 3. 한 번 휘두를 때 하나만 맞힌다. 판정을 바로 끈다.
        DeactivateCollider();

        // 4. 대상에서 플레이어에 가장 가까운 점을 맞은 자리로 삼는다.
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        GiveDamage(damageable, hitPoint);
    }

    // 대미지 정보를 만들어 넘긴다. 맞은 자리의 방향은 플레이어 쪽이다.
    private void GiveDamage(IDamageable damageable, Vector3 hitPoint)
    {
        DamageInfo damageInfo = new DamageInfo();
        damageInfo.Value = _damage;
        damageInfo.HitPoint = hitPoint;
        damageInfo.HitNormal = (transform.position - hitPoint).normalized;
        damageable.TakeDamage(damageInfo);
    }
}
