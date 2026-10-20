using UnityEngine;

//role: 마우스 움직이면 카메라를 회전
public class CameraRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1000f;
    private float _accumulationX = 0f; // 좌우 회전각
    private float _accumulationY = 0f; // 위아래 회전각

    // false면 마우스 입력을 누적하지도, 카메라에 적용하지도 않음 (고정 시점용)
    public bool ApplyRotation { get; set; } = true;

    // 고정 시점에서 돌아올 때 누적값을 현재 방향에 맞춰 초기화
    public void ResetRotation(float yaw, float pitch)
    {
        _accumulationX = yaw;
        _accumulationY = pitch;
    }

    private void Update()
    {
        if (!ApplyRotation) return;

        // 1. 마우스 입력을 받는다
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 2. 마우스 입력만큼 회전값을 누적
        _accumulationX += mouseX * _rotationSpeed * Time.deltaTime;
        _accumulationY += -mouseY * _rotationSpeed * Time.deltaTime;

        // 3. 누적하기 전, 내 변수에서 먼저 -90 ~ 90 제한
        _accumulationY = Mathf.Clamp(_accumulationY, -90, 90);

        // 4. 누적값을 카메라에 적용
        transform.eulerAngles = new Vector3(_accumulationY, _accumulationX, 0f);
    }
}
