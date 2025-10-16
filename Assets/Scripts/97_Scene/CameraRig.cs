using UnityEngine;

public class CameraRig : MonoBehaviour
{
    public static CameraRig Instance { get; private set; }
    void Awake() => Instance = this;

    public Transform player;                 // �÷��̾� Ʈ������ ����
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float followSpeed = 6f;           // �ε巴�� ���󰡴� �ӵ�

    enum Mode { FollowPlayer, LockToTarget }
    Mode mode = Mode.FollowPlayer;

    Transform lockTarget;                    // ��Ż ��Ŀ ��

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

        // ȸ�� ����
        transform.rotation = Quaternion.identity;
    }

    // --- �ܺο��� ȣ���� API ---
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
