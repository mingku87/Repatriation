using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 조건 전담 서비스:
/// - HaveItem: 인벤토리/장비 등에서 아이템 키 보유 여부
/// - FirstObjectInteract / FirstNpcInteract: 최초 상호작용 "이후" true (==1)
/// - Key 플래그: 임의 키 저장/조회
/// </summary>
public class GameConditionService : MonoBehaviour, Game.Dialogue.IDialogueConditionService, Game.Dialogue.IConditionWriteback
{
    // PlayerPrefs 키 접두사
    const string PREF_OBJ = "FirstObject_";
    const string PREF_NPC = "FirstNpc_";
    const string PREF_KEY = "Key_";

    [Header("Optional direct refs")]
    public MonoBehaviour inventoryProvider; // InventoryManager 등
    public MonoBehaviour equipmentProvider; // EquipmentManager 등

    // 씬 스캔 캐시
    static readonly List<object> _cachedSources = new List<object>();
    bool _scannedOnce;

    // ───────── 아이템 보유 ─────────
    public bool HasItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        itemId = itemId.Trim();

        // 1) 명시 참조 우선 (자기 자신/조건 서비스 제외)
        if (IsValidSource(inventoryProvider) && TryHas(inventoryProvider, itemId)) return true;
        if (IsValidSource(equipmentProvider) && TryHas(equipmentProvider, itemId)) return true;

        // 2) 최초 1회만 씬 스캔하여 캐시
        if (!_scannedOnce)
        {
            _cachedSources.Clear();
            foreach (var mb in FindObjectsOfType<MonoBehaviour>(true))
            {
                if (!IsValidSource(mb)) continue;
                if (HasAnyItemApi(mb.GetType()))
                    _cachedSources.Add(mb);
            }
            _scannedOnce = true;
        }

        // 3) 캐시 검사
        foreach (var s in _cachedSources)
            if (TryHas(s, itemId)) return true;

        return false;
    }

    bool IsValidSource(object obj)
    {
        if (obj == null) return false;
        if (ReferenceEquals(obj, this)) return false;
        if (obj is Game.Dialogue.IDialogueConditionService) return false; // 다른 조건 서비스도 제외(순환 방지)
        if (obj is Component c && c == null) return false;
        return true;
    }

    static bool HasAnyItemApi(System.Type t)
    {
        return t.GetMethod("HasItem", new[] { typeof(string) })?.ReturnType == typeof(bool)
            || t.GetMethod("Has", new[] { typeof(string) })?.ReturnType == typeof(bool)
            || t.GetMethod("Contains", new[] { typeof(string) })?.ReturnType == typeof(bool)
            || t.GetMethod("GetCount", new[] { typeof(string) })?.ReturnType == typeof(int);
    }

    bool TryHas(object obj, string key)
    {
        if (!IsValidSource(obj)) return false;
        var t = obj.GetType();

        var m = t.GetMethod("HasItem", new[] { typeof(string) });
        if (m != null && m.ReturnType == typeof(bool)) return (bool)m.Invoke(obj, new object[] { key });

        m = t.GetMethod("Has", new[] { typeof(string) });
        if (m != null && m.ReturnType == typeof(bool)) return (bool)m.Invoke(obj, new object[] { key });

        m = t.GetMethod("Contains", new[] { typeof(string) });
        if (m != null && m.ReturnType == typeof(bool)) return (bool)m.Invoke(obj, new object[] { key });

        m = t.GetMethod("GetCount", new[] { typeof(string) });
        if (m != null && m.ReturnType == typeof(int)) return ((int)m.Invoke(obj, new object[] { key })) > 0;

        return false;
    }

    // ───────── 최초 상호작용 (처음 false, 이후 true) ─────────
    // 의도: 첫 대화는 1번(기본), 이후부터 2번(조건)이 선택되도록
    public bool IsFirstObjectInteract(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey)) return false;
        return PlayerPrefs.GetInt(PREF_OBJ + objectKey, 0) == 1; // ← 이후 true
    }

    public bool IsFirstNpcInteract(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId)) return false;
        return PlayerPrefs.GetInt(PREF_NPC + npcId, 0) == 1; // ← 이후 true
    }

    // 기록 업데이트(상호작용 완료 시 호출) — DialogueManager가 대화 시작 성공 시 자동 호출
    public void MarkObjectInteracted(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey)) return;
        PlayerPrefs.SetInt(PREF_OBJ + objectKey, 1);
        PlayerPrefs.Save();
    }

    public void MarkNpcInteracted(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId)) return;
        PlayerPrefs.SetInt(PREF_NPC + npcId, 1);
        PlayerPrefs.Save();
    }

    // ───────── 임의 키 플래그 ─────────
    public bool IsKeySatisfied(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName)) return false;
        return PlayerPrefs.GetInt(PREF_KEY + keyName, 0) == 1;
    }

    public void SetKey(string keyName, bool value = true)
    {
        if (string.IsNullOrWhiteSpace(keyName)) return;
        PlayerPrefs.SetInt(PREF_KEY + keyName, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
