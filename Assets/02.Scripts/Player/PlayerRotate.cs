using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1000f;
    private float _accumulationX = 0f; // 좌우 회전각
    private CameraFollow _cameraFollow;

    private void Awake()
    {
        _cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
    }

    // 고정 시점에서 돌아올 때 누적값을 현재 방향에 맞춰 초기화
    public void ResetRotation(float yaw)
    {
        _accumulationX = yaw;
    }

    private void Update()
    {
        if (_cameraFollow != null && _cameraFollow.IsSideView)
        {
            AimAtMouseSide();
            return;
        }

        if (_cameraFollow != null && _cameraFollow.IsFixedView)
        {
            AimAtMouseOnGround();
            return;
        }

        // 1. 마우스 입력을 받아 회전값을 누적 (고정 시점에서는 커서 조준이라 누적하지 않음)
        float mouseX = Input.GetAxis("Mouse X");
        _accumulationX += mouseX * _rotationSpeed * Time.deltaTime;

        // 2. 누적값을 회전에 적용
        transform.eulerAngles = new Vector3(0f, _accumulationX, 0f);
    }

    // 쿼터/탑다운: 마우스 커서가 가리키는 바닥 지점을 바라본다
    private void AimAtMouseOnGround()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, transform.position);

        if (!ground.Raycast(ray, out float distance)) return;

        Vector3 look = ray.GetPoint(distance) - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude < 0.01f) return;

        transform.rotation = Quaternion.LookRotation(look);
    }

    // 사이드뷰: 플레이어 깊이 평면 위의 커서 지점이 왼쪽/오른쪽인지에 따라 좌우만 바라본다
    private void AimAtMouseSide()
    {
        Transform cam = Camera.main.transform;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane depthPlane = new Plane(-cam.forward, transform.position);

        if (!depthPlane.Raycast(ray, out float distance)) return;

        float deltaX = ray.GetPoint(distance).x - transform.position.x;
        if (Mathf.Abs(deltaX) < 0.01f) return;

        transform.rotation = Quaternion.LookRotation(Vector3.right * Mathf.Sign(deltaX));
    }
}
