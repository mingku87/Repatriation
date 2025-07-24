using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] private Image progressBarImage;
    private float progress;
    private Timer timer;

    private void Start()
    {
        progress = 0.0f;
        timer = new Timer();
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        // 한 프레임 대기 후 시작
        yield return null;

        var sceneName = SceneController.Instance.currentScene.ToString();
        AsyncOperation loadingOp = SceneManager.LoadSceneAsync(sceneName);
        loadingOp.allowSceneActivation = false;
        timer.Initialize();

        while (!loadingOp.isDone)
        {
            // 타이머 업데이트
            timer.UnScaledTick();

            // 실제 로딩 진행도 0~0.9 까지만 채워지므로, 0.9 이전엔 loadingOp.progress 사용
            if (loadingOp.progress < 0.9f)
            {
                progress = Mathf.Lerp(progress, loadingOp.progress, timer.time);
                if (progress >= loadingOp.progress)
                    timer.Initialize();
            }
            else
            {
                // 0.9 이상 → 1.0까지 천천히
                progress = Mathf.Lerp(progress, 1f, timer.time);
                if (Mathf.Approximately(progress, 1f))
                    loadingOp.allowSceneActivation = true;
            }

            // 프로그레스 바에 반영
            if (progressBarImage != null)
                progressBarImage.fillAmount = progress;

            yield return null;
        }
    }
}