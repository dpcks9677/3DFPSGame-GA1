using System.Collections;
using UnityEngine;


// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [Header("스탯")]
    [SerializeField] private int _maxBulletCount = 30;
    [SerializeField] private float _reloadingTime = 3f;
    [SerializeField] private float _range = 100f;            // 사거리
    [SerializeField] private float _damage = 10f;            // 공격력
    [SerializeField] private float _fireInterval = 0.1f;     // 발사 속도

    [Header("레이저")]
    [SerializeField] private LineRenderer _laser;
    [SerializeField] private Transform _laserStart;   // 레이저 시작 위치 (총구)
    [SerializeField] private float _laserDuration = 0.05f;

    [SerializeField] private float _fireClipLength = 0.5f;   // 사격 애니메이션 클립 한 사이클의 길이
    [SerializeField] private GameObject[] _muzzleFlashes;
    [SerializeField] private float _flashTime = 0.05f;

    public int BulletCount => _bulletCount;                  // 남은 총알 수 (밖에서는 읽기만 한다)
    public int MaxBulletCount => _maxBulletCount;            // 탄창에 들어가는 총알 수
    public event System.Action<int, int> OnBulletChanged;    // 총알 수가 바뀌면 (남은 수, 탄창 크기)를 알린다.

    private float _fireTimer = 0f;
    private int _bulletCount = 0;
    private Coroutine _reloadCoroutine = null;
    private Coroutine _laserCoroutine = null;
    private Animator _animator;
    private PlayerBombFire _bombFire;
    private PlayerWeaponMode _weaponMode;

    private void Awake()
    {
        _bulletCount = _maxBulletCount;
    }

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _bombFire = GetComponent<PlayerBombFire>();
        _weaponMode = GetComponent<PlayerWeaponMode>();

        // 애니메이션 한 사이클이 발사 간격과 같아지도록 재생 속도를 맞춘다.
        _animator.SetFloat("FireSpeed", _fireClipLength / _fireInterval);
    }


    private void Update()
    {
        // 게임 중이 아니면 쏘는 동작을 멈추고 아무것도 하지 않는다.
        if (GameManager.Instance.State != GameState.Playing)
        {
            _animator.SetBool("Fire", false);
            return;
        }

        // 1. 발사 타이머를 지난 시간만큼 줄인다.
        _fireTimer -= Time.deltaTime;

        // 2. 커서가 숨어 있고, 버튼을 누르고 있고, 총알이 남아 있는 동안은 쏘는 동작을 한다. (달리는 중에도, 수류탄을 던지는 중과 총을 숨긴 근접 모드에서는 제외)
        bool isMouseVisible = Cursor.visible;
        bool isFiring = !isMouseVisible && Input.GetButton("Fire1") && _bulletCount > 0 && !_bombFire.IsThrowing
                        && _weaponMode.Mode != WeaponMode.Melee;
        _animator.SetBool("Fire", isFiring);

        // 3. 쏘는 중이고 쏠 때가 되었으면 쏜다.
        if (isFiring && _fireTimer <= 0f)
        {
            CancelReload();  // 재장전 중이었다면 멈춘다.
            _fireTimer = _fireInterval;
            _bulletCount--;
            OnBulletChanged?.Invoke(_bulletCount, _maxBulletCount);  // 한 발 줄었다고 알린다.
            Shoot();
        }

        // 4. R 키를 누르면 재장전한다. (재장전 중이거나 탄창이 가득 차 있으면 하지 않는다.)
        if (Input.GetKeyDown(KeyCode.R) && _reloadCoroutine == null && _bulletCount < _maxBulletCount)
        {
            _reloadCoroutine = StartCoroutine(Reloading_Coroutine());
        }
    }

    private void CancelReload()
    {
        if (_reloadCoroutine == null) return;

        StopCoroutine(_reloadCoroutine);
        Debug.Log("재장전 취소!");
        _reloadCoroutine = null;
    }

    private IEnumerator Reloading_Coroutine()
    {
        Debug.Log("재장전 시작!");

        yield return new WaitForSeconds(_reloadingTime);

        _bulletCount = _maxBulletCount;
        OnBulletChanged?.Invoke(_bulletCount, _maxBulletCount);  // 탄창을 채웠다고 알린다.

        Debug.Log("재장전 완료!");
        _reloadCoroutine = null;
    }


    private void Shoot()
    {
        StartCoroutine(MuzzleFlash_Coroutine());
        // Ray: 레이저(시작위치, 날아갈 방향)
        // Physics.RayCast: 레이저를 발사
        // RayCastHit: 레이저가 물체와 부딛히면 그 정보를 저장하는 구조체

        // 2. 레이(광선)를 생성하고 발사될 위치와 진행 방향 설정하기
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 3. 레이가 부딛힌 대상의 정보를 저장할 변수 생성하기
        RaycastHit hitInfo = new RaycastHit();

        // 4. 레이를 발사하고, 만일 부딛힌 물체가 있다면 이펙트 표시하기
        bool isHit = Physics.Raycast(ray, out hitInfo, _range);

        Vector3 laserEnd = isHit ? hitInfo.point : ray.origin + ray.direction * _range;
        ShowLaser(_laserStart.position, laserEnd);

        if (isHit)
        {
            // Debug.Log($"{hitInfo.collider.name}이 총알을 맞았다. 대미지: {_damage}");

            // 맞은 자리에 총알 자국 이펙트 만들기
            ParticleSystem effect = Instantiate(_hitEffectPrefab);
            effect.transform.position = hitInfo.point; // 맞은 자리
            effect.transform.forward = hitInfo.normal; // 수직 위치로 회전
            effect.Play();

            // 대미지 구조체 만들기
            DamageInfo damageInfo = new DamageInfo();
            damageInfo.Value = _damage;
            damageInfo.HitPoint = hitInfo.point;
            damageInfo.HitNormal = hitInfo.normal;

            hitInfo.collider.GetComponent<IDamageable>()?.TakeDamage(damageInfo);

            /*
             hitInfo.collider.GetComponent<Zombie>()?.TakeDamage(_damage);
            hitInfo.collider.GetComponent<Drum>()?.TakeDamage(_damage);
            hitInfo.collider.GetComponent<Box>()?.TakeDamage(_damage);
            */

            // 황유찬 :         Player
            // 김민쉬:     Zombie, Boss
            // 박현서:  Drum, Box, Bird
        }
    }

    private void ShowLaser(Vector3 start, Vector3 end)
    {
        _laser.SetPosition(0, start);
        _laser.SetPosition(1, end);
        _laser.enabled = true;

        if (_laserCoroutine != null)
        {
            StopCoroutine(_laserCoroutine);
        }
        _laserCoroutine = StartCoroutine(HideLaser_Coroutine());
    }

    private IEnumerator HideLaser_Coroutine()
    {
        yield return new WaitForSeconds(_laserDuration);
        _laser.enabled = false;
        _laserCoroutine = null;
    }

    private IEnumerator MuzzleFlash_Coroutine()
    {
        GameObject flash = _muzzleFlashes[Random.Range(0, _muzzleFlashes.Length)];
        flash.SetActive(true);
        yield return new WaitForSeconds(_flashTime);
        flash.SetActive(false);
    }
}
