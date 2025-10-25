using System.Linq;
using UnityEngine;

public class MapChunk : MonoBehaviour
{
    // ���� �����Ǹ� �ڵ����� MapLoader�� �ڽ��� ���
    void Awake()
    {
        if (MapLoader.Instance)
            MapLoader.Instance.TryAdoptChunk(this);

        // ���� ��Ż/������ Owner �ڵ� ����(������/��Ÿ�� ���)
        foreach (var p in GetComponentsInChildren<Portal>(true))
            if (p && p.Owner == null) p.Owner = this;

        foreach (var d in GetComponentsInChildren<DoorPortal>(true))
            if (d && d.Owner == null) d.Owner = this;
    }

    public Portal[] GetPortals()
        => GetComponentsInChildren<Portal>(true);

    public Portal FindPortal(ExitDir d)
    {
        foreach (var p in GetPortals())
            if (p.direction == d) return p;
        return null;
    }

    public DoorPortal FindDoorById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return GetComponentsInChildren<DoorPortal>(true)
               .FirstOrDefault(x => x.doorId == id);
    }
}