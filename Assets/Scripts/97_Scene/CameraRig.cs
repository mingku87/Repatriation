using UnityEngine;

public class CameraRig : MonoBehaviour
{
    public static CameraRig Instance { get; private set; }
    void Awake() => Instance = this;

    public Transform player;                 // 플레이어 트랜스폼 연결
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float followSpeed = 6f;           // 부드럽게 따라가는 속도

    enum Mode { FollowPlayer, LockToTarget }
    Mode mode = Mode.FollowPlayer;

    Transform lockTarget;                    // 포탈 앵커 등

    void LateUpdate()
    {
        if (mode == Mode.FollowPlayer)
        {
            if (!player) return;
            Vector3 target = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);
        }
        else // LockToTarget
        {
            if (!lockTarget) return;
            Vector3 target = lockTarget.position + offset;
            transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);
        }

        // 회전 고정
        transform.rotation = Quaternion.identity;
    }

    // --- 외부에서 호출할 API ---
    public void FollowPlayer(bool snap = false)
    {
        mode = Mode.FollowPlayer;
        if (snap && player) transform.position = player.position + offset;
    }

    public void LockTo(Transform target, bool snap = true)
    {
        lockTarget = target;
        mode = Mode.LockToTarget;
        if (snap && target) transform.position = target.position + offset;
    }

    public void SetOffset(Vector3 newOffset) => offset = newOffset;
    public void SetFollowSpeed(float s) => followSpeed = s;
}
