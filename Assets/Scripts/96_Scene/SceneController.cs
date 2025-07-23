using System.Collections;
using UnityEngine.SceneManagement;

public class SceneController : SingletonObject<SceneController>
{
    public SceneName currentScene { get; private set; }

    void Start()
    {
        currentScene = Util.ParseEnumFromString<SceneName>(SceneManager.GetActiveScene().name);
    }
}
