// ������������������������������������������������������������������������������������������������
// ��� ��ȭ ���� �ý���(GameConditionService, DialogueManager ��)�� ���� �������̽� ����
// ������������������������������������������������������������������������������������������������

using UnityEngine;

namespace Game.Dialogue
{
    /// <summary>
    /// ���� ������ ���� ���� (�б� ����)
    /// DialogueManager�� �����Ͽ� ���� ��� ���θ� �Ǵ���.
    /// </summary>
    public interface IDialogueConditionService
    {
        /// <summary>
        /// �÷��̾ Ư�� �������� �����ϰ� �ִ°�?
        /// </summary>
        bool HasItem(string itemId);

        /// <summary>
        /// Ư�� ������Ʈ�� ���ʷ� ��ȣ�ۿ��ߴ°�?
        /// true = �̹� ��ȣ�ۿ� �Ϸ� (�� �� �̻�)
        /// false = ���� ó��
        /// </summary>
        bool IsFirstObjectInteract(string objectKey);

        /// <summary>
        /// Ư�� NPC�� ���ʷ� ��ȣ�ۿ��ߴ°�?
        /// true = �̹� ��ȭ�� �� ����
        /// false = ó�� ��ȭ
        /// </summary>
        bool IsFirstNpcInteract(string npcId);

        /// <summary>
        /// PlayerPrefs ����� ���� Ű �÷���(��: ����Ʈ ���� ����)
        /// </summary>
        bool IsKeySatisfied(string keyName);
    }

    /// <summary>
    /// ���� ���¸� ���/���� ���� ����
    /// DialogueManager�� ���� ��ȭ�� ��ȣ�ۿ� �Ϸ� �� ȣ����.
    /// </summary>
    public interface IConditionWriteback
    {
        /// <summary>
        /// Ư�� ������Ʈ�� ��ȣ�ۿ� �Ϸ�� ���
        /// </summary>
        void MarkObjectInteracted(string objectKey);

        /// <summary>
        /// Ư�� NPC�� ��ȣ�ۿ� �Ϸ�� ���
        /// </summary>
        void MarkNpcInteracted(string npcId);

        /// <summary>
        /// PlayerPrefs ������� ���� Ű �÷��׸� ����
        /// (ex: ����Ʈ �Ϸ� ����, �ƾ� ��� ���� ��)
        /// </summary>
        void SetKey(string keyName, bool value = true);
    }
}
