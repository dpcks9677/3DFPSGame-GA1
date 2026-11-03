using System.Collections;
using TMPro;
using UnityEngine;

// 게임 상태에 따라 상태 텍스트와 팝업(일시정지/게임 오버)을 갱신하고, 팝업 버튼 입력을 GameManager에 전달한다.
// 씬과 함께 생성/파괴되므로 씬이 재시작되어도 항상 현재 씬의 UI를 참조한다.
public class UI_GameState : MonoBehaviour
{
    private const float StartTextDuration = 0.5f;

    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private GameObject _pausePopup;
    [SerializeField] private GameObject _gameOverPopup;

    private GameState _prevState;
    private Coroutine _startTextCoroutine;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += Refresh;

        _prevState = GameManager.Instance.State;
        Refresh(_prevState);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnStateChanged -= Refresh;
    }

    private void Refresh(GameState state)
    {
        _pausePopup.SetActive(state == GameState.Pause);
        _gameOverPopup.SetActive(state == GameState.GameOver);

        switch (state)
        {
            case GameState.Ready:
                ShowText("준비");
                break;

            case GameState.Playing:
                // 준비 → 시작일 때만 "시작!"을 잠깐 보여주고, 일시정지 해제 시에는 바로 숨긴다.
                if (_prevState == GameState.Ready)
                {
                    StopStartText();
                    _startTextCoroutine = StartCoroutine(ShowStartText_Coroutine());
                }
                else
                {
                    _stateText.gameObject.SetActive(false);
                }
                break;

            default:
                StopStartText();
                _stateText.gameObject.SetActive(false);
                break;
        }

        _prevState = state;
    }

    private void ShowText(string text)
    {
        _stateText.gameObject.SetActive(true);
        _stateText.text = text;
    }

    private void StopStartText()
    {
        if (_startTextCoroutine == null) return;

        StopCoroutine(_startTextCoroutine);
        _startTextCoroutine = null;
    }

    private IEnumerator ShowStartText_Coroutine()
    {
        ShowText("시작!");
        yield return new WaitForSeconds(StartTextDuration);
        _stateText.gameObject.SetActive(false);
        _startTextCoroutine = null;
    }

    // 팝업 버튼 (Button OnClick에 연결)
    public void OnClickResume()
    {
        GameManager.Instance.Resume();
    }

    public void OnClickRestart()
    {
        GameManager.Instance.Restart();
    }

    public void OnClickQuit()
    {
        GameManager.Instance.Quit();
    }
}
