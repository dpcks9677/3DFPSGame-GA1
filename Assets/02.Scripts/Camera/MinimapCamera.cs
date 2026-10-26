using UnityEngine;

// 미니맵 카메라: 대상의 머리 위에서 아래를 내려다보며 따라간다.
public class MinimapCamera : MonoBehaviour
{
    private Camera _camera;

    [SerializeField] private Transform _target;    // 따라갈 대상 (Player)
    [SerializeField] private float _height = 8f;  // 대상보다 얼마나 위에 있을지 (m)

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _camera.orthographicSize -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            _camera.orthographicSize += 1;
        }
    }

    private void LateUpdate()
    {
        // 1. 대상의 머리 위로 옮긴다.
        transform.position = _target.position + Vector3.up * _height;

        // 2. 아래를 보되, 대상이 보는 쪽이 미니맵의 위쪽이 되게 돌린다.
        transform.eulerAngles = new Vector3(90f, _target.eulerAngles.y, 0f);
    }
}