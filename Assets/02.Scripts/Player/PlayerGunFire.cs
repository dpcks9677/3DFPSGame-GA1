using UnityEngine;

public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [Header("스탯")]
    [SerializeField] private float _range = 100f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _fireInterval = 0.1f;
    [SerializeField] private const int MAX_MAGAZINE = 50;
    private int _magazine = MAX_MAGAZINE;
    private float _fireTimer = 0f;


    private void Update()
    {
        // Ray: 레이저(시작위치 ,날아갈 방향)
        // Physics.RayCast(): 레이저 발사
        // RayCastHit: 레이저와 물체가 부딪히면

        // 0. 발사 타이머를 지난시간만큼 줄인다
        _fireTimer -= Time.deltaTime;

        // 1. 좌클릭 입력 받기
        if (Input.GetButton("Fire1") && _fireTimer <= 0f && _magazine > 0)
        {
            _fireTimer = _fireInterval;
            Shoot();
            _magazine--;
            Debug.Log($"남은 탄창: {_magazine}");
        }

        // 5. 재장전
        if (Input.GetButton("Reload"))
        {
            Debug.Log("재장전 실행");
            _magazine = MAX_MAGAZINE;
            Debug.Log($"장전완료 / 남은 탄약: {_magazine}");
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
        if (isHit)
        {
            //Debug.Log($"{hitInfo.collider.name}이 총알을 맞았다. 데미지: {_damage}");
            ParticleSystem effect = Instantiate(_hitEffectPrefab);
            effect.transform.position = hitInfo.point;
            effect.transform.forward = hitInfo.normal;
            effect.Play();
        }
    }
}
