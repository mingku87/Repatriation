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
        var moveDirection = Vector3.zero;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveRight))) moveDirection.x = 1;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveLeft))) moveDirection.x = -1;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveUp))) moveDirection.z = 1;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveDown))) moveDirection.z = -1;

        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();
            transform.position += moveDirection * constant.speed * Time.deltaTime;
        }
    }
}
