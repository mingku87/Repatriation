using System;
using UnityEngine;

public class Player : SingletonObject<Player>
{
    public static PlayerSettings constant => PlayerConstant.Settings[InGameManager.difficulty];
    private static PlayerStatusController playerStatusController;
    private Inventory inventory;

    void Start()
    {
        InGameManager.Instance.Initialize();
        Initialize();
        InvokeRepeating(nameof(UpdatePlayerStatus), 0f, 1f);
    }

    void Update()
    {
        // Test
        if (Input.GetKeyDown(KeyCode.M)) Time.timeScale = Time.timeScale == 1 ? 15 : 1;

        Move();
    }

    public void Initialize()
    {
        playerStatusController = new();
        playerStatusController.Initialize();

        inventory = new();
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
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveUp))) moveDirection.y = 1;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveDown))) moveDirection.y = -1;

        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();
            transform.position += moveDirection * constant.speed * Time.deltaTime;
        }
    }

    // PlayerStatusController Methods
    public static void AddOnStatChangedEvent(Action<Status, float, float> OnStatChanged) { playerStatusController.AddOnStatChangedEvent(OnStatChanged); }
    public static void RemoveOnStatChangedEvent(Action<Status, float, float> OnStatChanged) { playerStatusController.RemoveOnStatChangedEvent(OnStatChanged); }
    public static float GetCurrentStatus(Status status) { return playerStatusController.GetCurrentStatus(status); }
    public static float GetMaxStatus(Status status) { return playerStatusController.GetMaxStatus(status); }
    public static float SetMaxStatus(Status status, float value) { return playerStatusController.SetMaxStatus(status, value); }
    public static float SetCurrentStatus(Status status, float value) { return playerStatusController.SetCurrentStatus(status, value); }
    public static float ChangeCurrentStatus(Status status, float change) { return playerStatusController.ChangeCurrentStatus(status, change); }
}
