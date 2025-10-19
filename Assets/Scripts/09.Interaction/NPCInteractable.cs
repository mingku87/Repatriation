using UnityEngine;
using Game.Dialogue;

public class NPCInteractable : InteractableBase
{
    [Header("Dialogue")]
    [SerializeField] private string npcId = "NPC001";
    [SerializeField] private bool lockPlayerWhileTalking = true;

    private DialogueManager manager;
    private MonoBehaviour playerControllerLike;

    // 디바운스
    float _lastInteractTime;
    const float InteractCooldown = 0.2f; // 200ms

    // 이벤트 중복 등록 방지
    bool _subscribedEnd;

    void Awake()
    {
        manager = FindObjectOfType<DialogueManager>();
        // playerControllerLike = FindObjectOfType<PlayerController>();
    }

    public override void Interact(Transform interactor)
    {
        // 디바운스
        if (Time.unscaledTime - _lastInteractTime < InteractCooldown) return;
        _lastInteractTime = Time.unscaledTime;

        Debug.Log($"[NPC] 대화 시도: {name} ({npcId})", this);

        if (manager == null) manager = FindObjectOfType<DialogueManager>();
        if (manager == null)
        {
            Debug.LogWarning("[NPCInteractable] DialogueManager가 씬에 없습니다.");
            return;
        }

        // 이미 대화 중이면 무시
        if (manager.IsActive) return;

        // (선택) 조작 잠금
        if (lockPlayerWhileTalking && playerControllerLike != null)
            playerControllerLike.enabled = false;

        // 종료 리스너: 중복 방지
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

        // 한 번만 듣도록 제거
        if (manager != null)
            manager.onDialogueEnd.RemoveListener(OnDialogueEnded);
        _subscribedEnd = false;
    }

    public override string GetPrompt() => "F - 대화";
}
