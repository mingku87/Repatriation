using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Dialogue; // SpeakerKind

public class DialogueUIController : MonoBehaviour
{
    [Header("Root / BG")]
    [SerializeField] GameObject root;       // InGameUI/Dialogue
    [SerializeField] GameObject bgObject;   // InGameUI/Dialogue/BG (Image 또는 CanvasGroup)

    CanvasGroup _bgCg;
    Image _bgImg;

    [Header("Player Panel")]
    [SerializeField] GameObject playerPanel;    // InGameUI/Dialogue/PlayerDialogue
    [SerializeField] TMP_Text playerName;       // PlayerDialogue/Name/Text (TMP)
    [SerializeField] TMP_Text playerText;       // PlayerDialogue/Dialogue/Text (TMP)

    [Header("NPC Panel")]
    [SerializeField] GameObject npcPanel;       // InGameUI/Dialogue/NPCDialogue
    [SerializeField] TMP_Text npcName;          // NPCDialogue/Name/Text (TMP)
    [SerializeField] TMP_Text npcText;          // NPCDialogue/Dialogue/Text (TMP)

    [Header("Config")]
    [SerializeField] string defaultPlayerName = "Player";

    void Awake()
    {
        if (!root) root = gameObject; // 이 스크립트를 Dialogue 루트에 붙였다면 자동 할당
        if (bgObject)
        {
            _bgCg = bgObject.GetComponent<CanvasGroup>();
            _bgImg = bgObject.GetComponent<Image>();
        }
        SetActive(false);
    }

    // ----- DialogueManager 이벤트에 연결 -----

    // 대화 시작: 창 켜기 + 초기화
    public void OnDialogueStart(string npcId, string dialogueId)
    {
        SetActive(true);
        ShowPanel(SpeakerKind.Player, false);
        ShowPanel(SpeakerKind.Npc, false);
        if (playerText) playerText.text = "";
        if (npcText) npcText.text = "";
    }

    // 한 줄 표시: 화자 패널 토글 + 텍스트 갱신
    public void OnShowLine(string speakerName, string text, SpeakerKind who)
    {
        if (who == SpeakerKind.Player)
        {
            ShowPanel(SpeakerKind.Npc, false);
            ShowPanel(SpeakerKind.Player, true);
            if (playerName) playerName.text = string.IsNullOrEmpty(speakerName) ? defaultPlayerName : speakerName;
            if (playerText) playerText.text = text;
        }
        else
        {
            ShowPanel(SpeakerKind.Player, false);
            ShowPanel(SpeakerKind.Npc, true);
            if (npcName) npcName.text = string.IsNullOrEmpty(speakerName) ? "NPC" : speakerName;
            if (npcText) npcText.text = text;
        }
    }

    // 대화 종료: 창 끄기
    public void OnDialogueEnd(string npcId, string dialogueId)
    {
        SetActive(false);
    }

    // ----- 내부 유틸 -----
    void SetActive(bool on)
    {
        if (root) root.SetActive(on);

        if (_bgCg)
        {
            _bgCg.alpha = on ? 1f : 0f;
            _bgCg.blocksRaycasts = on;
            _bgCg.interactable = on;
        }
        else if (_bgImg)
        {
            _bgImg.enabled = on;
            _bgImg.raycastTarget = on;
        }
        else if (bgObject)
        {
            bgObject.SetActive(on);
        }
    }

    void ShowPanel(SpeakerKind who, bool on)
    {
        if (who == SpeakerKind.Player)
        {
            if (playerPanel) playerPanel.SetActive(on);
        }
        else
        {
            if (npcPanel) npcPanel.SetActive(on);
        }
    }

#if UNITY_EDITOR
    // 인스펙터에서 root만 지정해도 BG 자동 탐색 (자식 이름 "BG")
    void OnValidate()
    {
        if (!root) root = gameObject;
        if (!bgObject && root)
        {
            var t = root.transform.Find("BG");
            if (t) bgObject = t.gameObject;
        }
    }
#endif
}
