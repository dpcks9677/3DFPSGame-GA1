using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 로딩 화면. SceneLoader가 정한 씬을 뒤에서 불러오며 진행률을 막대로 보여 준다.
public class UI_Loading : MonoBehaviour
{
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private float _fillSpeed = 1f;  // 막대가 따라가는 빠르기 (1초에 막대 전체의 몇 배)
    [SerializeField] private float _minTime = 1f;    // 로딩 화면을 보여 줄 최소 시간 (초)

    private void Start()
    {
        StartCoroutine(Load_Coroutine());
    }

    private IEnumerator Load_Coroutine()
    {
        // 뒤에서 불러오기 시작한다. 다 불러와도 저절로 넘어가지 않게 막아 둔다.
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.NextSceneName);
        operation.allowSceneActivation = false;

        float shown = 0f;    // 막대에 보여 주는 값
        float elapsed = 0f;  // 로딩 화면을 보여 준 시간

        while (true)
        {
            // 불러오기는 0.9에서 멈추므로 0.9를 1로 늘려서 쓴다.
            float target = Mathf.Clamp01(operation.progress / 0.9f);

            // 막대는 진행률을 일정한 빠르기로 따라간다.
            shown = Mathf.MoveTowards(shown, target, _fillSpeed * Time.unscaledDeltaTime);
            elapsed += Time.unscaledDeltaTime;

            _progressSlider.value = shown;
            _progressText.text = $"불러오는 중 {shown * 100f:0}%";

            // 막대가 다 차고 최소 시간도 지났으면 새 씬으로 넘어간다.
            if (shown >= 1f && elapsed >= _minTime)
            {
                operation.allowSceneActivation = true;
                yield break;
            }

            yield return null;  // 다음 프레임까지 기다린다.
        }
    }
}
