using UnityEngine;
using UnityEngine.UI;

public class TitleManager : SingletonObject<TitleManager>
{
    public void Initialize()
    {
    }

    public void StartGame()
    {

    }

    public void NewGame() => SceneController.Instance.StartGame();
    public void Settings() => SettingsUIController.Instance.ShowSettings();
    public void Exitgame() => Application.Quit();
}