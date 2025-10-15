using UnityEngine;

public class MapChunk : MonoBehaviour
{
    // �ܼ��� �ڱ� ���� Portal���� ã���ֱ⸸ �ϸ� ��
    public Portal[] GetPortals() => GetComponentsInChildren<Portal>(true);

    public Portal FindPortal(ExitDir d)
    {
        foreach (var p in GetPortals()) if (p.direction == d) return p;
        return null;
    }
}
