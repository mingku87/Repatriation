using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform Target;          // ���� ���� ��� (�⺻: Player)
    public float speed = 6f;
    public float z = -10f;
    public float yOffest = 0f;
    public Vector2 center;
    public Vector2 size;

    private float orthographicSize;
    private float horizontalGraphicSize;

    public bool isCamera = true;

    // �� ���� ����
    Transform _player;                // �÷��̾� ĳ��
    bool _lockedToTarget = false;     // ��Ż �� �ܺ� Ÿ�ٿ� ��� ����

    void Start()
    {
        orthographicSize = Camera.main.orthographicSize;
        horizontalGraphicSize = orthographicSize * Screen.width / Screen.height;

        if (Player.Instance != null)
        {
            _player = Player.Instance.transform;
            if (Target == null) Target = _player;
        }
    }

    void LateUpdate()
    {
        // �÷��̾� ���� �� �׻� ĳ�� ����
        if (Player.Instance != null) _player = Player.Instance.transform;

        // ��� ���°� �ƴ϶�� �⺻ Ÿ���� �÷��̾�� ����
        if (!_lockedToTarget && _player != null) Target = _player;

        cameraMovement();
        // maxCameraMovement(); // �ʿ� �� ���
    }

    void cameraMovement()
    {
        if (Target == null) return;

        Vector3 dst;
        if (isCamera)
        {
            dst = new Vector3(Target.position.x, Target.position.y + yOffest, z);
            transform.position = Vector3.Lerp(transform.position, dst, Time.deltaTime * speed);
        }
        else
        {
            dst = new Vector3(Target.position.x, transform.position.y, z);
            transform.position = Vector3.Lerp(transform.position, dst, Time.deltaTime * speed);
        }

        // ȸ�� ����(���̼� 2D)
        transform.rotation = Quaternion.identity;
    }

    void maxCameraMovement()
    {
        float lx = size.x * 0.5f - horizontalGraphicSize;
        float clampX = Mathf.Clamp(transform.position.x, -lx + center.x, lx + center.x);

        float ly = size.y * 0.5f - orthographicSize;
        float clampY = Mathf.Clamp(transform.position.y, -ly + center.y, ly + center.y);

        transform.position = new Vector3(clampX, clampY, z);
    }

    // =============================
    // �� �ܺο��� ȣ���� ���� API
    // =============================

    /// <summary>���� ��Ż(��Ŀ)�� ��� ����</summary>
    public void LockTo(Transform target, bool snap = true)
    {
        if (target == null) return;
        _lockedToTarget = true;
        Target = target;
        if (snap)
            transform.position = new Vector3(target.position.x, target.position.y + yOffest, z);
    }

    /// <summary>�ٽ� �÷��̾� ���� ����</summary>
    public void FollowPlayer(bool snap = false)
    {
        _lockedToTarget = false;
        if (_player != null) Target = _player;
        if (snap && Target != null)
            transform.position = new Vector3(Target.position.x, Target.position.y + yOffest, z);
    }

    /// <summary>���� �ӵ�/������ ��Ÿ�� ����</summary>
    public void SetSpeed(float s) => speed = s;
    public void SetYOffset(float y) => yOffest = y;
}
