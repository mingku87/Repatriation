using UnityEngine;

public class PlayerInputBlocker : MonoBehaviour
{
    [SerializeField] bool blocked;             // ���� ����
    public bool IsBlocked => blocked;          // �б� ����

    public void SetBlocked(bool v)             // �ܺο��� ������ �̰ɷ�
    {
        blocked = v;
        // �ʿ��ϸ� ���⼭ �ִ�/�ӵ� �ʱ�ȭ�� �Բ� ó��
    }
}
