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
        if (inputBlocked) return;

        float moveX = 0.0f;
        float moveY = 0.0f;

        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveRight))) moveX = 1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveLeft))) moveX = -1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveUp))) moveY = 1.0f;
        if (Input.GetKey(KeySetting.GetKey(PlayerAction.MoveDown))) moveY = -1.0f;

        // 애니메이터는 "입력" 기준 그대로 유지
        GetComponent<Animator>().SetFloat(PlayerConstant.AnimatorFloatMoveX, moveX);
        GetComponent<Animator>().SetFloat(PlayerConstant.AnimatorFloatMoveY, moveY);

        Vector3 moveDir = Vector3.zero;

        if (moveX != 0 && moveY != 0)
        {
            // ✅ 대각선 입력일 때 각도 스냅
            float angleDeg = 0f;
            if (moveX > 0 && moveY > 0) angleDeg = 30f;   // ↗
            else if (moveX > 0 && moveY < 0) angleDeg = 330f;  // ↘ (또는 -30도)
            else if (moveX < 0 && moveY > 0) angleDeg = 150f;  // ↖
            else if (moveX < 0 && moveY < 0) angleDeg = 210f;  // ↙

            float rad = angleDeg * Mathf.Deg2Rad;
            moveDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f); // 길이 1
        }
        else
        {
            // 단일 축 입력은 기존대로 (상하좌우)
            moveDir = new Vector3(moveX, moveY, 0f).normalized;
        }

        if (moveDir != Vector3.zero)
        {
            transform.position += moveDir * GetCurrentStatus(Status.Speed) * Time.deltaTime;
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