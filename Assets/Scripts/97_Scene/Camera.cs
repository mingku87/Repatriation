using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform Target;          // 현재 따라갈 대상 (기본: Player)
    public float speed = 6f;
    public float z = -10f;
    public float yOffest = 0f;
    public Vector2 center;
    public Vector2 size;

    private float orthographicSize;
    private float horizontalGraphicSize;

    public bool isCamera = true;

    // ★ 내부 상태
    Transform _player;                // 플레이어 캐시
    bool _lockedToTarget = false;     // 포탈 등 외부 타겟에 잠금 여부

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
        // 플레이어 존재 시 항상 캐시 갱신
        if (Player.Instance != null) _player = Player.Instance.transform;

        // 잠금 상태가 아니라면 기본 타겟을 플레이어로 유지
        if (!_lockedToTarget && _player != null) Target = _player;

        cameraMovement();
        // maxCameraMovement(); // 필요 시 사용
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

        // 회전 고정(아이소 2D)
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
    // ★ 외부에서 호출할 간단 API
    // =============================

    /// <summary>도착 포탈(앵커)에 잠깐 고정</summary>
    public void LockTo(Transform target, bool snap = true)
    {
        if (target == null) return;
        _lockedToTarget = true;
        Target = target;
        if (snap)
            transform.position = new Vector3(target.position.x, target.position.y + yOffest, z);
    }

    /// <summary>다시 플레이어 추적 모드로</summary>
    public void FollowPlayer(bool snap = false)
    {
        _lockedToTarget = false;
        if (_player != null) Target = _player;
        if (snap && Target != null)
            transform.position = new Vector3(Target.position.x, Target.position.y + yOffest, z);
    }

    /// <summary>추적 속도/오프셋 런타임 조정</summary>
    public void SetSpeed(float s) => speed = s;
    public void SetYOffset(float y) => yOffest = y;
}
