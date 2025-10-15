using UnityEngine;

public class TriggerProbe2D : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[TriggerProbe2D] hit {other.name}, tag={other.tag}");
        var who = other.attachedRigidbody ? other.attachedRigidbody.gameObject
                                          : other.transform.root.gameObject;
        if (who.CompareTag("Player"))
        {
            // �θ��� Portal�� ����
            var portal = GetComponentInParent<Portal>();
            if (portal) portal.OnHitFromChild(who.transform);
        }
    }
}