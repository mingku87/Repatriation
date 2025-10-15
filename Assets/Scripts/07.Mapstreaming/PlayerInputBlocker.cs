using UnityEngine;

public class PlayerInputBlocker : MonoBehaviour
{
    [SerializeField] bool blocked;             // 내부 저장
    public bool IsBlocked => blocked;          // 읽기 전용

    public void SetBlocked(bool v)             // 외부에서 변경은 이걸로
    {
        blocked = v;
        // 필요하면 여기서 애니/속도 초기화도 함께 처리
    }
}
