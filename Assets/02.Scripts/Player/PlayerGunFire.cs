using UnityEngine;

public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    private float _range = 100f;
    private float _damage = 10f;


    private void Update()
    {
        // Ray: 레이저(시작위치 ,날아갈 방향)
        // Physics.RayCast(): 레이저 발사
        // RayCastHit: 레이저와 물ㅊ가 부딪히면

        // 1. 좌클릭 입력 받기
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
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
