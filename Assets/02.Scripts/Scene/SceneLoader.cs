using UnityEngine.SceneManagement;

// 씬을 옮길 때 로딩 씬을 지나가게 한다. 어느 씬으로 갈지 기억해 둔다.
public static class SceneLoader
{
    private const string LoadingSceneName = "LoadingScene";

    // 로딩 씬이 뒤에서 불러올 씬의 이름. 밖에서는 읽기만 한다.
    public static string NextSceneName { get; private set; }

    // sceneName으로 가는 길에 로딩 씬을 먼저 연다.
    public static void Load(string sceneName)
    {
        NextSceneName = sceneName;
        SceneManager.LoadScene(LoadingSceneName);  // 로딩 씬은 작아서 바로 열린다.
    }
}
