using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Ready,
    Playing,
    GameOver,
    Pause
}

// 게임 상태를 관리한다. 씬이 바뀌어도 유지되므로 씬에 있는 UI를 직접 참조하지 않고,
// 상태가 바뀔 때 OnStateChanged 이벤트로 알린다. (UI는 UI_GameState가 구독해서 갱신)
public class GameManager : SingletonBehaviour<GameManager>
{
    private const float ReadyDuration = 1.5f;

    private GameState _state = GameState.Ready;
    public GameState State => _state;

    private bool _isHitStopping = false;  // 히트스톱으로 시간이 느려져 있는 중

    public event Action<GameState> OnStateChanged;


    private void Start()
    {
        // 씬 재시작 시 새로 생긴 중복 인스턴스는 Awake에서 파괴되므로 초기화하지 않는다.
        if (!ReferenceEquals(Instance, this)) return;

        // 첫 씬은 여기서 시작하고, 이후 씬이 다시 로드될 때마다 OnSceneLoaded에서 다시 시작한다.
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartGame();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_state == GameState.Playing)
            {
                Pause();
            }
            else if (_state == GameState.Pause)
            {
                Resume();
            }
        }
    }

    private void StartGame()
    {
        StopAllCoroutines();
        _isHitStopping = false;  // 히트스톱 코루틴이 끊겨도 Restart가 timeScale을 1로 되돌려 둔다.
        StartCoroutine(StartSequence_Coroutine());
    }

    // 타격감을 위해 잠깐 시간을 거의 멈춘다. 이미 히트스톱 중이면 무시한다.
    public void HitStop(float scale, float duration)
    {
        if (_state != GameState.Playing || _isHitStopping) return;

        StartCoroutine(HitStop_Coroutine(scale, duration));
    }

    private IEnumerator HitStop_Coroutine(float scale, float duration)
    {
        _isHitStopping = true;
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        _isHitStopping = false;

        // 그 사이 일시정지(0)가 되었다면 그대로 둔다.
        if (_state != GameState.Pause)
        {
            Time.timeScale = 1f;
        }
    }

    private IEnumerator StartSequence_Coroutine()
    {
        ChangeState(GameState.Ready);
        yield return new WaitForSeconds(ReadyDuration);
        ChangeState(GameState.Playing);
    }

    private void ChangeState(GameState state)
    {
        _state = state;
        OnStateChanged?.Invoke(_state);
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        CursorManager.Instance.UnlockCursor();
    }

    // 일시 정지
    public void Pause()
    {
        if (_state != GameState.Playing) return;

        Time.timeScale = 0f;
        CursorManager.Instance.UnlockCursor();
        ChangeState(GameState.Pause);
    }

    // 계속하기
    public void Resume()
    {
        if (_state != GameState.Pause) return;

        Time.timeScale = 1f;
        CursorManager.Instance.LockCursor();
        ChangeState(GameState.Playing);
    }

    // 다시 시작
    public void Restart()
    {
        Time.timeScale = 1f;
        CursorManager.Instance.LockCursor();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 게임 종료
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
