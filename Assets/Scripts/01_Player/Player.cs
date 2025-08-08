using System;
using UnityEngine;

public class Player : SingletonObject<Player>
{
    public static PlayerSettings constant => PlayerConstant.Settings[InGameManager.difficulty];
    private static PlayerStatusController playerStatusController;
    private Animator _animator;
    private Inventory inventory;
    private Rigidbody2D _rigidbody;

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
        if (Input.GetKeyDown(KeyCode.N)) SceneController.Instance.ChangeSceneWithLoading(SceneName.Title);

        Move();
    }

    public void Initialize()
    {
        playerStatusController = new();
        playerStatusController.Initialize();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        inventory = new();
    }

    private void UpdatePlayerStatus()
    {
        playerStatusController.UpdatePlayerStatus();
    }

    private void Move()
    {
        float moveX = 0.0f;
        float moveY = 0.0f;

        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveRight))) moveX = 1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveLeft))) moveX = -1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveUp))) moveY = 1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveDown))) moveY = -1.0f;

        if (moveX == 0 && moveY == 0)
        {
            _animator.SetBool(PlayerConstant.AnimatorBoolIsMove, false);
            return;
        }

        _animator.SetBool(PlayerConstant.AnimatorBoolIsMove, true);
        _animator.SetFloat(PlayerConstant.AnimatorFloatMoveX, moveX);
        _animator.SetFloat(PlayerConstant.AnimatorFloatMoveY, moveY);

        var moveDirection = new Vector2(moveX, moveY).normalized;
        var targetPos = _rigidbody.position + moveDirection * GetCurrentStatus(Status.Speed) * Time.fixedDeltaTime;
        _rigidbody.MovePosition(targetPos);
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