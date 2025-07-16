using UnityEngine;

public class InGameManager : SingletonObject<InGameManager>
{
    public static Difficulty difficulty;
    public static DifficultySettings settings => InGameConstant.Settings[difficulty];

    public void Initialize()
    {
        InGameUIManager.Instance.Initialize();
    }

    void Update()
    {
        TimeManager.AddPlayTime(Time.deltaTime);
    }
}
