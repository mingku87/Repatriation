using System.Collections;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    public SceneName currentScene { get; private set; }

    void Start()
    {
        currentScene = Util.ParseEnumFromString<SceneName>(SceneManager.GetActiveScene().name);
    }
}
