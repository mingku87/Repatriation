using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Game.Dialogue;  // IDialogueConditionService, IConditionWriteback

#region UnityEvents
[System.Serializable] public class DialogueStartEvent : UnityEvent<string /*npcId*/, string /*dialogueId*/> { }
[System.Serializable] public class DialogueShowEvent : UnityEvent<string /*speakerName*/, string /*text*/, SpeakerKind> { }
[System.Serializable] public class DialogueEndEvent : UnityEvent<string /*npcId*/, string /*dialogueId*/> { }
#endregion

/// <summary>
/// TSV로 임포트된 DialogueMetaTable / DialogueLinesTable을 사용해
/// 조건 평가 → 대화 선택 → 라인 진행을 관리.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Tables")]
    public DialogueMetaTable metaTable;
    public DialogueLinesTable linesTable;

    [Header("Condition Service (선택)")]
    [Tooltip("IDialogueConditionService / IConditionWriteback 구현체.\n비워두면 GameConditionService를 자동으로 붙임.")]
    public MonoBehaviour conditionServiceBehaviour;

    // 외부에서 접근하지 않도록 프로퍼티로 래핑
    IDialogueConditionService Cond =>
        (conditionServiceBehaviour as IDialogueConditionService) ?? _condAuto;
    IConditionWriteback CondWrite =>
        (conditionServiceBehaviour as IConditionWriteback) ?? _condAuto;

    GameConditionService _condAuto; // 자동 장착용 기본 구현

    [Header("Config")]
    [Tooltip("플레이어 말풍선 표시명. 비워두면 UI의 defaultPlayerName을 사용.")]
    public string playerNameOverride = "";

    [Header("Events")]
    public DialogueStartEvent onDialogueStart;
    public DialogueShowEvent onShowLine;
    public DialogueEndEvent onDialogueEnd;

    public bool IsActive { get; private set; }
    public string CurrentNpcId { get; private set; }
    public string CurrentDialogueId { get; private set; }

    int cursor = -1;
    readonly List<DialogueLine> _activeLines = new();

    // Localization 재진입 가드(만일 Localization 쪽에서 역호출되더라도 스택오버 방지)
    bool _resolving;

    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // 조건 서비스 자동 장착(인스펙터가 비었을 때만)
        if (conditionServiceBehaviour == null)
            _condAuto = gameObject.AddComponent<GameConditionService>();
    }

    // ─────────────────────────────────────────────────────────────
    // 외부 API
    // ─────────────────────────────────────────────────────────────

    /// <summary>npcId로 시작(조건 평가 포함)</summary>
    public void StartDialogue(string npcId)
    {
        if (IsActive) { Debug.Log("[DM] Already active – ignored."); return; }
        if (metaTable == null || linesTable == null)
        {
            Debug.LogWarning("[DM] Table refs are null.");
            return;
        }

        var id = SelectDialogueId(npcId);
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning($"[DM] SelectDialogueId('{npcId}') == null");
            return;
        }

        StartDialogueById(npcId, id);
    }

    /// <summary>특정 dialogueId로 직접 시작</summary>
    public void StartDialogueById(string npcId, string dialogueId)
    {
        var lines = linesTable?.GetLines(dialogueId);
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning($"[DM] GetLines('{dialogueId}') → 0");
            return;
        }

        // 1) dialogueIndex 기준 정렬
        lines.Sort((a, b) => a.dialogueIndex.CompareTo(b.dialogueIndex));

        IsActive = true;
        CurrentNpcId = npcId;
        CurrentDialogueId = dialogueId;

        _activeLines.Clear();
        _activeLines.AddRange(lines);

        // 2) 첫 줄을 dialogueIndex==1로 맞추기 (없으면 최소값)
        int first = _activeLines.FindIndex(l => l.dialogueIndex == 1);
        if (first < 0)
        {
            int minIdx = 0;
            for (int i = 1; i < _activeLines.Count; i++)
                if (_activeLines[i].dialogueIndex < _activeLines[minIdx].dialogueIndex)
                    minIdx = i;
            first = minIdx;
        }

        // 커서는 '현재 줄'을 가리키게 둔다 (첫 줄 표시 후, 다음 입력 때 2번으로 넘어가도록)
        cursor = first;

        // 최초 상호작용 기록
        CondWrite?.MarkNpcInteracted(npcId);

        // 이벤트: 시작 알림
        onDialogueStart?.Invoke(npcId, dialogueId);

        // ✅ Next()를 호출하지 않고, 첫 줄을 '그대로' 보여준다
        ShowLineAt(cursor);
    }

    /// <summary>다음 라인 진행</summary>
    public void Next()
    {
        if (!IsActive) return;

        // 점프 처리 (현재 라인이 점프 정보를 갖고 있을 때)
        if (cursor >= 0 && cursor < _activeLines.Count)
        {
            var cur = _activeLines[cursor];
            if (cur.nextDialogue > 0 && cur.nextDialogue - 1 != cursor)
            {
                // nextDialogue는 1-base라고 가정
                cursor = cur.nextDialogue - 2; // 아래 cursor++를 고려해 -2
            }
        }

        cursor++;
        if (cursor >= _activeLines.Count)
        {
            End();
            return;
        }

        var line = _activeLines[cursor];
        string text = Resolve(line.textKey);

        // (플레이어 이름 오버라이드 지원)
        string whoName = (line.speakerKind == SpeakerKind.Player)
            ? playerNameOverride
            : (string.IsNullOrEmpty(line.speakerNpcId) ? CurrentNpcId : line.speakerNpcId);

        onShowLine?.Invoke(whoName, text, line.speakerKind);
    }

    void ShowLineAt(int index)
    {
        if (index < 0 || index >= _activeLines.Count) return;

        var line = _activeLines[index];
        string text = Resolve(line.textKey);

        string whoName = (line.speakerKind == SpeakerKind.Player)
            ? playerNameOverride
            : (string.IsNullOrEmpty(line.speakerNpcId) ? CurrentNpcId : line.speakerNpcId);

        onShowLine?.Invoke(whoName, text, line.speakerKind);
    }

    /// <summary>대화 종료</summary>
    public void End()
    {
        if (!IsActive) return;

        IsActive = false;
        onDialogueEnd?.Invoke(CurrentNpcId, CurrentDialogueId);

        CurrentNpcId = null;
        CurrentDialogueId = null;
        _activeLines.Clear();
        cursor = -1;
    }

    // ─────────────────────────────────────────────────────────────
    // 내부: 조건 평가 & 대화ID 선택
    // ─────────────────────────────────────────────────────────────

    string SelectDialogueId(string npcId)
    {
        if (metaTable == null || metaTable.rows == null) return null;

        // 동일 NPC 행만 모으고 index 오름차순
        var rows = metaTable.rows.FindAll(r =>
            string.Equals(r.npcId, npcId, System.StringComparison.OrdinalIgnoreCase));
        rows.Sort((a, b) => a.index.CompareTo(b.index));

        // 1) 조건 대화(2) 먼저 검사 (조건 None은 건너뜀)
        foreach (var m in rows)
        {
            if (m.dialogueType != DialogueType.Conditional) continue;
            if (m.conditionType == ConditionType.None) continue;

            if (IsConditionPass(m, npcId))
                return m.dialogueId;
        }

        // 2) 일반 대화(1)
        foreach (var m in rows)
            if (m.dialogueType == DialogueType.Normal)
                return m.dialogueId;

        // 3) 폴백: 조건대화인데 None으로 들어온 것이 있으면 첫 행 사용
        foreach (var m in rows)
            if (m.dialogueType == DialogueType.Conditional && m.conditionType == ConditionType.None)
                return m.dialogueId;

        return null;
    }

    bool IsConditionPass(DialogueMeta m, string npcId)
    {
        switch (m.conditionType)
        {
            case ConditionType.None: return false; // 조건 대화에서 None은 무시
            case ConditionType.HaveItem: return Cond.HasItem(m.conditionKey);
            case ConditionType.FirstObjectInteract: return Cond.IsFirstObjectInteract(m.conditionKey);
            case ConditionType.FirstNpcInteract: return Cond.IsFirstNpcInteract(string.IsNullOrEmpty(m.conditionKey) ? npcId : m.conditionKey);
            case ConditionType.KeyInput: return Cond.IsKeySatisfied(m.conditionKey);
            default: return false;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Localization (리플렉션 제거 / 재진입 가드)
    // ─────────────────────────────────────────────────────────────
    string Resolve(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;

        if (_resolving) return key; // 혹시라도 재귀가 생기면 안전하게 키 그대로 반환
        _resolving = true;
        try
        {
            var lm = LocalizationManager.Instance; // 네 프로젝트의 LocalizationManager
            if (lm != null)
                return lm.GetOrKey(key);           // 반드시 GetOrKey 사용
            return key;
        }
        catch { return key; }
        finally { _resolving = false; }
    }
}
