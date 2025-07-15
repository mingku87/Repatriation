using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    public static PlayerSettings constant => PlayerConstant.Settings[InGameManager.difficulty];
    public PlayerStatusController playerStatusController;

    void Start()
    {

    }

    void Update()
    {

    }

    public void Initialize()
    {
        playerStatusController.Initialize();
    }

    private void Move()
    {

    }
}
