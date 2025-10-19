using UnityEngine;
using Game.Dialogue;

public class NPCInteractable : InteractableBase
{
    [Header("Dialogue")]
    [SerializeField] private string npcId = "NPC001";
    [SerializeField] private bool lockPlayerWhileTalking = true;

    private DialogueManager manager;
    private MonoBehaviour playerControllerLike;

    // ��ٿ
    float _lastInteractTime;
    const float InteractCooldown = 0.2f; // 200ms

    // �̺�Ʈ �ߺ� ��� ����
    bool _subscribedEnd;

    void Awake()
    {
        manager = FindObjectOfType<DialogueManager>();
        // playerControllerLike = FindObjectOfType<PlayerController>();
    }

    public override void Interact(Transform interactor)
    {
        // ��ٿ
        if (Time.unscaledTime - _lastInteractTime < InteractCooldown) return;
        _lastInteractTime = Time.unscaledTime;

        Debug.Log($"[NPC] ��ȭ �õ�: {name} ({npcId})", this);

        if (manager == null) manager = FindObjectOfType<DialogueManager>();
        if (manager == null)
        {
            Debug.LogWarning("[NPCInteractable] DialogueManager�� ���� �����ϴ�.");
            return;
        }

        // �̹� ��ȭ ���̸� ����
        if (manager.IsActive) return;

        // (����) ���� ���
        if (lockPlayerWhileTalking && playerControllerLike != null)
            playerControllerLike.enabled = false;

        // ���� ������: �ߺ� ����
        if (!_subscribedEnd)
        {
            manager.onDialogueEnd.RemoveListener(OnDialogueEnded);
            manager.onDialogueEnd.AddListener(OnDialogueEnded);
            _subscribedEnd = true;
        }

        manager.StartDialogue(npcId);
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.onDialogueEnd.RemoveListener(OnDialogueEnded);
        _subscribedEnd = false;
    }

    private void OnDialogueEnded(string endNpcId, string dialogueId)
    {
        if (endNpcId != npcId) return;

        if (lockPlayerWhileTalking && playerControllerLike != null)
            playerControllerLike.enabled = true;

        // �� ���� �赵�� ����
        if (manager != null)
            manager.onDialogueEnd.RemoveListener(OnDialogueEnded);
        _subscribedEnd = false;
    }

    public override string GetPrompt() => "F - ��ȭ";
}
