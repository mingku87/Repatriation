using System.Collections;
using UnityEngine.SceneManagement;

public class SceneController : SingletonObject<SceneController>
{
    public SceneName currentScene { get; private set; }

    void Start()
    {
        currentScene = Util.ParseEnumFromString<SceneName>(SceneManager.GetActiveScene().name);
    }

    public void StartGame()
    {
        ChangeSceneWithLoading(SceneName.Chapter1);
        Util.Log("Game started, loading Chapter 1.");
    }

    public void ChangeScene(SceneName sceneName) => SceneManager.LoadScene(sceneName.ToString());
    public void ChangeSceneWithLoading(SceneName targetScene) => StartCoroutine(ChangeSceneWithLoadingCoroutine(targetScene));
    public IEnumerator ChangeSceneWithLoadingCoroutine(SceneName targetScene)
    {
        currentScene = targetScene;
        ChangeScene(SceneName.Loading);
        yield return null;
    }
}