using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    public static PlayerSettings constant => PlayerConstant.Settings[InGameManager.difficulty];
    [HideInInspector] public PlayerStatusController playerStatusController;

    void Start()
    {
        Time.timeScale = 5;
        Initialize();
        InvokeRepeating(nameof(UpdatePlayerStatus), 0f, 1f);
    }

    void Update()
    {

    }

    public void Initialize()
    {
        playerStatusController = new();
        playerStatusController.Initialize();
    }

    private void UpdatePlayerStatus()
    {
        playerStatusController.UpdatePlayerStatus();
    }

    private void Move()
    {

    }
}
