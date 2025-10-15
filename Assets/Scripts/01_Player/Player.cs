using System;
using UnityEngine;

public class Player : SingletonObject<Player>
{
    public static PlayerSettings constant => PlayerConstant.Settings[InGameManager.difficulty];
    private static PlayerStatusController playerStatusController;
    private Inventory inventory;
    private bool inputBlocked = false;

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

    //MapLoader 등 외부에서 호출
    public void SetInputBlocked(bool blocked)
    {
        inputBlocked = blocked;
        if (blocked)
        {
            // 입력 차단 시, 이동 애니 값은 여기서 만지지 않음
            // (포탈 전환 코루틴이 방향/러닝 애니를 직접 세팅하도록 남겨 둠)
        }
    }

    private void Move()
    {
        //차단 중이면 Update에서 아무 것도 하지 않음
        if (inputBlocked) return;

        float moveX = 0.0f;
        float moveY = 0.0f;

        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveRight))) moveX = 1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveLeft))) moveX = -1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveUp))) moveY = 1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveDown))) moveY = -1.0f;

        GetComponent<Animator>().SetFloat(PlayerConstant.AnimatorFloatMoveX, moveX);
        GetComponent<Animator>().SetFloat(PlayerConstant.AnimatorFloatMoveY, moveY);

        var moveDirection = new Vector3(moveX, moveY, 0.0f);
        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();
            transform.position += moveDirection * GetCurrentStatus(Status.Speed) * Time.deltaTime;
        }
    }

    // PlayerStatusController Methods
    public static void AddOnStatChangedEvent(Action<Status, float, float> OnStatChanged) => playerStatusController.AddOnStatChangedEvent(OnStatChanged);
    public static void RemoveOnStatChangedEvent(Action<Status, float, float> OnStatChanged) => playerStatusController.RemoveOnStatChangedEvent(OnStatChanged);
    public static float GetCurrentStatus(Status status) => playerStatusController.GetCurrentStatus(status);
    public static float GetMaxStatus(Status status) => playerStatusController.GetMaxStatus(status);
    public static float SetMaxStatus(Status status, float value) => playerStatusController.SetMaxStatus(status, value);
    public static float SetCurrentStatus(Status status, float value) => playerStatusController.SetCurrentStatus(status, value);
    public static float ChangeCurrentStatus(Status status, float change) => playerStatusController.ChangeCurrentStatus(status, change);
}