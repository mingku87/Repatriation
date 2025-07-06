using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    void Update()
    {
        TimeManager.AddPlayTime(Time.deltaTime);
    }
}
