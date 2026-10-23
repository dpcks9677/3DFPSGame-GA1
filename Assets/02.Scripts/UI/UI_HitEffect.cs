using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UI_HitEffect : MonoBehaviour
{
    [SerializeField] private float _fadeInTime = 0.05f;
    [SerializeField] private float _fadeOutTime = 0.3f;
    private Image _image;
    private float _maxAlpha;    // 인스펙터에서 정한 이미지의 알파값을 최대치로 쓴다.
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _maxAlpha = _image.color.a;
        SetAlpha(0f);
    }

    public void Flash()
    {
        // 연속으로 맞으면 진행 중인 연출을 끊고 처음부터 다시 시작한다.
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(Flash_Coroutine());
    }

    private IEnumerator Flash_Coroutine()
    {
        // 페이드 인: 0 -> 최대 알파
        float elapsed = 0f;
        while (elapsed < _fadeInTime)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, _maxAlpha, elapsed / _fadeInTime));
            yield return null;
        }

        // 페이드 아웃: 최대 알파 -> 0
        elapsed = 0f;
        while (elapsed < _fadeOutTime)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(_maxAlpha, 0f, elapsed / _fadeOutTime));
            yield return null;
        }

        SetAlpha(0f);
        _flashCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        Color color = _image.color;
        color.a = alpha;
        _image.color = color;
    }
}
