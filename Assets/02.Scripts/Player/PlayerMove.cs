using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _jumpPower = 7f;
    private const float Gravity = -10f; // 중력 가속도
    private float _yVelocity = 0f; // 떨어지는 속도

    [SerializeField] private CharacterController _characterController;
    private CameraFollow _cameraFollow;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
    }

    private void Update()
    {
        // 1. 키보드 입력을 받는다
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. 키보드 입력에 따라 방향을 구한다.
        Vector3 direction = new Vector3(h, 0, v);
        direction.Normalize();

        if (_cameraFollow != null && _cameraFollow.IsSideView)
        {
            // 사이드뷰(횡스크롤): 좌우(월드 X)로만 이동, 깊이(Z)는 고정
            direction = Vector3.right * h;
        }
        else if (_cameraFollow != null && _cameraFollow.IsFixedView)
        {
            // 쿼터/탑다운 고정 시점: 카메라(화면) 기준 이동 (W = 화면 위쪽)
            Transform cam = Camera.main.transform;
            Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.001f)
            {
                // 탑다운처럼 바로 아래를 볼 때는 카메라 up이 화면 위쪽
                forward = Vector3.ProjectOnPlane(cam.up, Vector3.up);
            }
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            direction = forward * direction.z + right * direction.x;
        }
        else
        {
            // 일반 시점: 플레이어 기준 이동 (좌표계를 월드 좌표계로 변경)
            direction = transform.TransformDirection(direction);
        }

        // 3. 중력 누적
        _yVelocity += _gravity * Time.deltaTime;

        // 4. 속도를 구한다
        Vector3 velocity = direction * _moveSpeed;
        velocity.y = _yVelocity;

        // 4. 방향과 속력에 따라 이동한다.
        _characterController.Move(velocity * _moveSpeed * Time.deltaTime);

    }
}
