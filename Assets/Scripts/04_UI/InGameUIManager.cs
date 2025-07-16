using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUIManager : SingletonObject<InGameUIManager>
{
    public void Initialize()
    {
        TimerUI.Instance.Initialize();
    }
}
