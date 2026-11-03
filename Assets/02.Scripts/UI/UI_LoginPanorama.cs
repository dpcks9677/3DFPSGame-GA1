using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 로그인 화면 배경. 맵의 여러 지점에서 찍은 파노라마 스카이박스를 차례로 돌려 가며 보여준다.
// 한 지점에서 천천히 둘러본 뒤 검정으로 페이드 아웃하고, 다음 지점으로 넘어가 페이드 인한다.
public class UI_LoginPanorama : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Material[] _skyboxes;                 // 지점별 파노라마 스카이박스
    [SerializeField] private Image _fade;                          // 화면 전체를 덮는 검정 이미지
    [SerializeField] private float _rotateSpeed = 6f;              // 카메라가 도는 속도 (도/초)
    [SerializeField] private float _holdTime = 18f;                // 한 지점을 보여주는 시간 (초)
    [SerializeField] private float _fadeTime = 1.2f;               // 페이드 아웃/인 시간 (초)
    [SerializeField] private float _pitchSwayAngle = 4f;           // 위아래로 살짝 흔들리는 각도 (도)
    [SerializeField] private float _pitchSwaySpeed = 0.25f;        // 위아래 흔들림 속도

    private float _yaw;

    private void Start()
    {
        if (_skyboxes.Length == 0) return;

        StartCoroutine(Tour_Coroutine());
    }

    private void Update()
    {
        // 카메라는 제자리에서 천천히 돌면서 위아래로 살짝 흔들린다.
        _yaw += _rotateSpeed * Time.deltaTime;
        float pitch = Mathf.Sin(Time.time * _pitchSwaySpeed) * _pitchSwayAngle;
        _camera.transform.rotation = Quaternion.Euler(pitch, _yaw, 0f);
    }

    private IEnumerator Tour_Coroutine()
    {
        int index = 0;
        while (true)
        {
            RenderSettings.skybox = _skyboxes[index];
            _yaw = Random.Range(0f, 360f);  // 매번 다른 방향에서 시작한다.

            yield return Fade_Coroutine(1f, 0f);
            yield return new WaitForSeconds(_holdTime);
            yield return Fade_Coroutine(0f, 1f);

            index = (index + 1) % _skyboxes.Length;
        }
    }

    private IEnumerator Fade_Coroutine(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < _fadeTime)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(from, to, elapsed / _fadeTime));
            yield return null;
        }

        SetFadeAlpha(to);
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = _fade.color;
        color.a = alpha;
        _fade.color = color;
    }
}
