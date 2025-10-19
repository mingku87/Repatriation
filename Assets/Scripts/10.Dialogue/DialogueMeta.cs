using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogue
{
    public enum DialogueType { Normal = 1, Conditional = 2 }
    public enum ConditionType { None = 0, HaveItem = 1, FirstObjectInteract = 2, FirstNpcInteract = 3, KeyInput = 4 }
    public enum SpeakerKind { Player, Npc }

    // ���� ��Ÿ: NPC �� ��ȭID ���� + ����
    [Serializable]
    public class DialogueMeta
    {
        public int index;
        public string npcId;
        public DialogueType dialogueType;     // 1/2
        public ConditionType conditionType;   // 0~4
        public string conditionKey;           // none -> ""
        public string dialogueId;             // ��: "NPC001_01"
    }

    [CreateAssetMenu(fileName = "DialogueMetaTable", menuName = "Game/Dialogue/Meta Table")]
    public class DialogueMetaTable : ScriptableObject
    {
        public List<DialogueMeta> rows = new();
        public List<DialogueMeta> GetByNpc(string npcId)
        {
            var list = rows.FindAll(r => string.Equals(r.npcId, npcId, StringComparison.OrdinalIgnoreCase));
            list.Sort((a, b) =>
            {
                int pa = (a.dialogueType == DialogueType.Conditional && a.conditionType != ConditionType.None) ? 0 : 1;
                int pb = (b.dialogueType == DialogueType.Conditional && b.conditionType != ConditionType.None) ? 0 : 1;
                int c = pa.CompareTo(pb);
                if (c != 0) return c;
                return a.index.CompareTo(b.index);
            });
            return list;
        }
    }

    // ���� ����: ��ȭID ���� ����
    [Serializable]
    public class DialogueLine
    {
        public string dialogueId;   // Ű
        public int dialogueIndex;   // 1..N (����)
        public int nextDialogue;    // ���� �ε���, �������̸� 0
        public SpeakerKind speakerKind;
        public string speakerNpcId; // Player�� "", NPC�� NPCID (�ݵ�� ����)
        public string textKey;      // Localize key
    }

    [CreateAssetMenu(fileName = "DialogueLinesTable", menuName = "Game/Dialogue/Lines Table")]
    public class DialogueLinesTable : ScriptableObject
    {
        public List<DialogueLine> lines = new();
        public List<DialogueLine> GetLines(string dialogueId)
        {
            var list = lines.FindAll(l => string.Equals(l.dialogueId, dialogueId, StringComparison.OrdinalIgnoreCase));
            list.Sort((a, b) => a.dialogueIndex.CompareTo(b.dialogueIndex));
            return list;
        }
    }
}
