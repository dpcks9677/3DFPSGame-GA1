using UnityEngine;

// 서브타이틀이 커졌다 작아졌다 한다.
public class UI_SubtitlePulse : MonoBehaviour
{
    [SerializeField] private float _minScale = 0.9f;
    [SerializeField] private float _maxScale = 1.1f;
    [SerializeField] private float _speed = 4f;  // 초당 회전 속도 (라디안). 4면 약 1.6초에 한 번 커졌다 작아진다.

    private void Update()
    {
        float t = (Mathf.Sin(Time.unscaledTime * _speed) + 1f) * 0.5f;
        transform.localScale = Vector3.one * Mathf.Lerp(_minScale, _maxScale, t);
    }
}
