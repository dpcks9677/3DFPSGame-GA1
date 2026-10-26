using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [Header("레이저")]
    [SerializeField] private LineRenderer _laser;
    [SerializeField] private Vector3 _laserOffset = new Vector3(0.2f, -0.2f, 0.5f);  // 카메라 기준 레이저 시작 위치
    [SerializeField] private float _laserDuration = 0.05f;

    [Header("스탯")]
    [SerializeField] private float _range = 100f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _fireInterval = 0.1f;
    [SerializeField] private const int MAX_MAGAZINE = 30;
    [SerializeField] private float _reloatingTime = 3f;
    private int _magazine = MAX_MAGAZINE;
    private float _fireTimer = 0f;
    private Awaitable _reloadAwaitable = null;
    private Coroutine _laserCoroutine = null;


    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        // Ray: 레이저(시작위치 ,날아갈 방향)
        // Physics.RayCast(): 레이저 발사
        // RayCastHit: 레이저와 물체가 부딪히면

        // 0. 발사 타이머를 지난시간만큼 줄인다
        _fireTimer -= Time.deltaTime;

        bool isMouseVisible = Cursor.visible;

        // 1. 좌클릭 입력 받기
        if (!isMouseVisible && Input.GetButton("Fire1") && _fireTimer <= 0f && _magazine > 0)
        {
            _fireTimer = _fireInterval;
            Shoot();
            _magazine--;
            Debug.Log($"남은 탄창: {_magazine}");

            if (_reloadAwaitable != null)
            {
                _reloadAwaitable.Cancel();
                Debug.Log("재장전 취소됨");
                _reloadAwaitable = null;
            }
        }

        // 5. 재장전
        if (Input.GetButtonDown("Reload") && _reloadAwaitable == null)
        {
            ReloadingAsync();
        }
    }

    private void Shoot()
    {
        Transform cameraTransfrom = Camera.main.transform;
        // 2. 레이 생성 후 발사될 위치와 진행방향 설정
        Ray ray = new Ray(cameraTransfrom.position, cameraTransfrom.forward);

        // 3. 레이가 부딪힌 대상의 정보를 저장하기
        RaycastHit hitInfo = new RaycastHit();

        // 4. 레이를 발사하고 부딪힌 물체가 있다면 이펙트 표시
        bool isHit = Physics.Raycast(ray, out hitInfo, _range);

        Vector3 laserEnd = isHit ? hitInfo.point : ray.origin + ray.direction * _range;
        ShowLaser(cameraTransfrom.TransformPoint(_laserOffset), laserEnd);

        if (isHit)
        {
            //Debug.Log($"{hitInfo.collider.name}이 총알을 맞았다. 데미지: {_damage}");

            // 총알 파티클 생성
            ParticleSystem effect = Instantiate(_hitEffectPrefab);
            effect.transform.position = hitInfo.point;
            effect.transform.forward = hitInfo.normal;
            effect.Play();

            DamageInfo damageInfo = new DamageInfo();
            damageInfo.Value = _damage;
            damageInfo.HitPoint = hitInfo.point;
            damageInfo.HitNormal = hitInfo.normal;

            // 피격 대상이 좀비면 TakeDamage() 실행
            hitInfo.collider.GetComponent<IDamageable>()?.TakeDamage(damageInfo);
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

    private async Awaitable ReloadingAsync()
    {
        await Awaitable.WaitForSecondsAsync(_reloatingTime);
        _magazine = MAX_MAGAZINE;
        Debug.Log("장전완료");
        _reloadAwaitable = null;
    }
}
