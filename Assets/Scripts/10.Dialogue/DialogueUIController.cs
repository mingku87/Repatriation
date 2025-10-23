using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Dialogue; // SpeakerKind

public class DialogueUIController : MonoBehaviour
{
    [Header("Root / BG")]
    [SerializeField] GameObject root;       // InGameUI/Dialogue
    [SerializeField] GameObject bgObject;   // InGameUI/Dialogue/BG (Image Ǵ CanvasGroup)

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

    [Header("Portraits")]
    [SerializeField] DialogueManager dialogueManager;
    [SerializeField] Image playerPortrait;
    [SerializeField] Image npcPortrait;

    [Header("Config")]
    [SerializeField] string defaultPlayerName = "Player";

    void Awake()
    {
        if (!root) root = gameObject; //  ũƮ Dialogue Ʈ ٿٸ ڵ Ҵ
        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();
        if (bgObject)
        {
            _bgCg = bgObject.GetComponent<CanvasGroup>();
            _bgImg = bgObject.GetComponent<Image>();
        }

        if (!playerPortrait)
            playerPortrait = FindPortraitImage(playerPanel, "Image", "PlayerImage");
        if (!npcPortrait)
            npcPortrait = FindPortraitImage(npcPanel, "Image", "NPCImage");

        SetActive(false);
        SetPortrait(playerPortrait, null);
        SetPortrait(npcPortrait, null);
    }

    // ----- DialogueManager ̺Ʈ  -----

    // ȭ : â ѱ + ʱȭ
    public void OnDialogueStart(string npcId, string dialogueId)
    {
        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();
        SetActive(true);
        ShowPanel(SpeakerKind.Player, false);
        ShowPanel(SpeakerKind.Npc, false);
        if (playerText) playerText.text = "";
        if (npcText) npcText.text = "";
        SetPortrait(playerPortrait, null);
        SetPortrait(npcPortrait, null);
    }

    //   ǥ: ȭ г  + ؽƮ
    public void OnShowLine(string speakerName, string text, SpeakerKind who)
    {
        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();
        Sprite portrait = dialogueManager ? dialogueManager.CurrentSpeakerPortrait : null;
        if (who == SpeakerKind.Player)
        {
            ShowPanel(SpeakerKind.Npc, false);
            ShowPanel(SpeakerKind.Player, true);
            if (playerName) playerName.text = string.IsNullOrEmpty(speakerName) ? defaultPlayerName : speakerName;
            if (playerText) playerText.text = text;
            SetPortrait(playerPortrait, portrait);
            SetPortrait(npcPortrait, null);
        }
        else
        {
            ShowPanel(SpeakerKind.Player, false);
            ShowPanel(SpeakerKind.Npc, true);
            if (npcName) npcName.text = string.IsNullOrEmpty(speakerName) ? "NPC" : speakerName;
            if (npcText) npcText.text = text;
            SetPortrait(npcPortrait, portrait);
            SetPortrait(playerPortrait, null);
        }
    }

    // ȭ : â
    public void OnDialogueEnd(string npcId, string dialogueId)
    {
        SetActive(false);
        SetPortrait(playerPortrait, null);
        SetPortrait(npcPortrait, null);
    }

    // -----  ƿ -----
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

    Image FindPortraitImage(GameObject panel, params string[] candidateNames)
    {
        if (!panel) return null;

        if (candidateNames != null)
        {
            foreach (var name in candidateNames)
            {
                if (string.IsNullOrEmpty(name)) continue;
                var t = panel.transform.Find(name);
                if (t)
                {
                    var img = t.GetComponent<Image>();
                    if (img) return img;
                }
            }
        }

        foreach (var img in panel.GetComponentsInChildren<Image>(true))
        {
            if (img == null) continue;
            return img;
        }

        return null;
    }

    void SetPortrait(Image target, Sprite sprite)
    {
        if (!target) return;

        target.sprite = sprite;
        target.enabled = sprite != null;
    }

#if UNITY_EDITOR
    // νͿ root ص BG ڵ Ž (ڽ ̸ "BG")
    void OnValidate()
    {
        if (!root) root = gameObject;
        if (!bgObject && root)
        {
            var t = root.transform.Find("BG");
            if (t) bgObject = t.gameObject;
        }
        if (!playerPortrait && playerPanel)
        {
            var tPlayer = playerPanel.transform.Find("Image");
            if (tPlayer) playerPortrait = tPlayer.GetComponent<Image>();
        }
        if (!npcPortrait && npcPanel)
        {
            var tNpc = npcPanel.transform.Find("Image");
            if (tNpc) npcPortrait = tNpc.GetComponent<Image>();
        }
    }
#endif
}
