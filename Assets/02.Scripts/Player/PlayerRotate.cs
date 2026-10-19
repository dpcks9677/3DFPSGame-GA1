using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1000f;
    private float _accumulationX = 0f; // 좌우 회전각

    private void Update()
    {
        // 1. 마우스 입력을 받는다
        float mouseX = Input.GetAxis("Mouse X");

        // 2. 마우스 입력만큼 회전값을 누적
        _accumulationX += mouseX * _rotationSpeed * Time.deltaTime;

        // 3. 누적값을 회전에 적용
        transform.eulerAngles = new Vector3(0f, _accumulationX, 0f);
    }
}
