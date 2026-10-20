using UnityEngine;

// role: V 키로 6가지 시점(FPS, TPS, 숄더, 쿼터, 탑다운, 사이드)을 순환
// 0~2: 마우스 회전 시점 / 3~5: 월드 고정 시점
// 고정 시점은 시점 오브젝트의 localPosition(플레이어 기준 오프셋)과 localRotation(월드 회전)을 그대로 사용한다.
// 플레이어 회전과 무관하며, CameraPosition은 로컬 회전 identity라고 가정한다.
public class CameraFollow : MonoBehaviour
{
    private const int FirstFixedViewIndex = 3;
    private const int SideViewIndex = 5;

    [SerializeField] private Transform _cameraPosition; // 시점 오브젝트들의 부모 (비어 있으면 자동 탐색)
    [SerializeField] private Transform[] _cameraArray;  // FPS, TPS, 숄더, 쿼터, 탑다운, 사이드 순서
    [SerializeField] private Transform _player;         // 고정 시점의 기준 대상

    private int _currentCamera = 0;
    private CameraRotate _cameraRotate;
    private PlayerRotate _playerRotate;

    // 현재 월드 고정 시점(쿼터/탑다운/사이드)인지
    public bool IsFixedView => _currentCamera >= FirstFixedViewIndex;

    // 횡스크롤 사이드뷰인지 (좌우 이동만 허용, 조준은 플레이어 깊이 평면)
    public bool IsSideView => _currentCamera == SideViewIndex;

    private void Awake()
    {
        _cameraRotate = GetComponent<CameraRotate>();

        // 씬에 배열이 연결돼 있지 않으면 CameraPosition의 자식을 순서대로 사용
        if (_cameraArray == null || _cameraArray.Length == 0)
        {
            if (_cameraPosition == null)
            {
                GameObject found = GameObject.Find("CameraPosition");
                if (found != null) _cameraPosition = found.transform;
            }

            if (_cameraPosition != null)
            {
                _cameraArray = new Transform[_cameraPosition.childCount];
                for (int i = 0; i < _cameraArray.Length; i++)
                {
                    _cameraArray[i] = _cameraPosition.GetChild(i);
                }
            }
        }

        if (_player == null && _cameraPosition != null)
        {
            _player = _cameraPosition.parent;
        }

        if (_player != null)
        {
            _playerRotate = _player.GetComponent<PlayerRotate>();
        }

        if (_cameraArray == null || _cameraArray.Length == 0)
        {
            Debug.LogWarning("[CameraFollow] 카메라 위치가 없습니다.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            bool wasFixedView = IsFixedView;
            _currentCamera = (_currentCamera + 1) % _cameraArray.Length;
            if (wasFixedView && !IsFixedView) ResetMouseRotation();
            ApplyCursorState();
        }

        // 고정 시점에서는 마우스 회전을 끈다
        if (_cameraRotate != null)
        {
            _cameraRotate.ApplyRotation = _currentCamera < FirstFixedViewIndex;
        }
    }

    // 고정 시점에서 마우스 회전 시점으로 돌아올 때: 플레이어가 바라보던 방향으로 맞추고 상하 각도는 정면으로
    private void ResetMouseRotation()
    {
        if (_player == null) return;

        float yaw = _player.eulerAngles.y;
        if (_cameraRotate != null) _cameraRotate.ResetRotation(yaw, 0f);
        if (_playerRotate != null) _playerRotate.ResetRotation(yaw);
    }

    // 고정 시점: 마우스 커서로 조준하므로 커서 해제 / 그 외: 잠금
    private void ApplyCursorState()
    {
        bool fixedView = IsFixedView;
        Cursor.lockState = fixedView ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = fixedView;
    }

    private void LateUpdate()
    {
        Transform view = _cameraArray[_currentCamera];

        if (_currentCamera < FirstFixedViewIndex || _player == null)
        {
            // FPS/TPS/숄더: 위치만 따라가고 회전은 CameraRotate가 담당
            transform.position = view.position;
            return;
        }

        // 쿼터/탑다운/사이드: 플레이어 회전과 무관하게 월드 기준 오프셋·회전 유지
        transform.position = _player.position + view.localPosition;
        transform.rotation = view.localRotation;
    }
}
