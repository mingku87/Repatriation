using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] float progress;
    private Timer timer;

    private void Start()
    {
        StartCoroutine(LoadScene());
        timer = new Timer();
    }

    IEnumerator LoadScene()
    {
        yield return null;
        //SceneController.Instance.RunChangeSceneProcess(SceneController.Instance.currentScene);
        AsyncOperation loadingSceneProcess = SceneManager.LoadSceneAsync(SceneController.Instance.currentScene.ToString());
        loadingSceneProcess.allowSceneActivation = false;
        timer.Initialize();
        while (!loadingSceneProcess.isDone)
        {
            yield return null;
            timer.UnScaledTick();
            if (loadingSceneProcess.progress < 0.9f)
            {
                progress = Mathf.Lerp(progress, loadingSceneProcess.progress, timer.time);
                if (progress >= loadingSceneProcess.progress)
                {
                    timer.Initialize();
                }
            }
            else
            {
                progress = Mathf.Lerp(progress, 1f, timer.time);
                if (progress == 1.0f)
                {
                    loadingSceneProcess.allowSceneActivation = true;
                    yield return null;
                }
            }
        }
    }
}