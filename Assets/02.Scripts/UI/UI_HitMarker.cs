using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 총이나 지팡이로 맞혔을 때 크로스헤어 위에 잠깐 뜨는 히트마커
public class UI_HitMarker : MonoBehaviour
{
    [SerializeField] private CanvasGroup _marker;       // 알파와 크기를 바꿀 히트마커
    [SerializeField] private Color _normalColor = Color.white;  // 맞혔을 때의 색
    [SerializeField] private Color _killColor = Color.red;      // 처치했을 때의 색
    [SerializeField] private float _duration = 0.15f;           // 사라지기까지의 시간 (실제 초)
    [SerializeField] private float _startScale = 1.4f;          // 처음 커져 있는 배율

    private Graphic[] _graphics;
    private Coroutine _showCoroutine;
    private PlayerGunFire _gunFire;
    private PlayerMeleeAttack _meleeAttack;

    private void Awake()
    {
        _graphics = _marker.GetComponentsInChildren<Graphic>(true);
        _marker.alpha = 0f;
    }

    private void Start()
    {
        _gunFire = FindAnyObjectByType<PlayerGunFire>();
        _meleeAttack = FindAnyObjectByType<PlayerMeleeAttack>();

        if (_gunFire != null)
        {
            _gunFire.OnHit += Show;
        }

        if (_meleeAttack != null)
        {
            _meleeAttack.OnHit += Show;
        }
    }

    private void OnDestroy()
    {
        if (_gunFire != null)
        {
            _gunFire.OnHit -= Show;
        }

        if (_meleeAttack != null)
        {
            _meleeAttack.OnHit -= Show;
        }
    }

    // 맞혔다고 알려오면 색을 정하고 다시 보여준다.
    private void Show(bool killed)
    {
        Color color = killed ? _killColor : _normalColor;
        foreach (Graphic graphic in _graphics)
        {
            graphic.color = color;
        }

        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
        }
        _showCoroutine = StartCoroutine(Show_Coroutine());
    }

    // 알파는 1에서 0으로, 크기는 _startScale에서 1로 줄인다. 히트스톱 중에도 같은 속도로 진행한다.
    private IEnumerator Show_Coroutine()
    {
        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _marker.alpha = 1f - t;
            _marker.transform.localScale = Vector3.one * Mathf.Lerp(_startScale, 1f, t);
            yield return null;
        }

        _marker.alpha = 0f;
        _showCoroutine = null;
    }
}
