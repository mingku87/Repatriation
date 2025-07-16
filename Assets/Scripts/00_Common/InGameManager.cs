using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : SingletonObject<InGameManager>
{
    public static Difficulty difficulty;
    public static DifficultySettings settings => InGameConstant.Settings[difficulty];

    void Update()
    {
        TimeManager.AddPlayTime(Time.deltaTime);
    }
}
