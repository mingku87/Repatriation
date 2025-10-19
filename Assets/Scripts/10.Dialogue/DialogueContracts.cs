// ────────────────────────────────────────────────
// 모든 대화 관련 시스템(GameConditionService, DialogueManager 등)의 공통 인터페이스 정의
// ────────────────────────────────────────────────

using UnityEngine;

namespace Game.Dialogue
{
    /// <summary>
    /// 조건 판정을 위한 서비스 (읽기 전용)
    /// DialogueManager가 참조하여 조건 통과 여부를 판단함.
    /// </summary>
    public interface IDialogueConditionService
    {
        /// <summary>
        /// 플레이어가 특정 아이템을 소지하고 있는가?
        /// </summary>
        bool HasItem(string itemId);

        /// <summary>
        /// 특정 오브젝트와 최초로 상호작용했는가?
        /// true = 이미 상호작용 완료 (한 번 이상)
        /// false = 아직 처음
        /// </summary>
        bool IsFirstObjectInteract(string objectKey);

        /// <summary>
        /// 특정 NPC와 최초로 상호작용했는가?
        /// true = 이미 대화한 적 있음
        /// false = 처음 대화
        /// </summary>
        bool IsFirstNpcInteract(string npcId);

        /// <summary>
        /// PlayerPrefs 기반의 임의 키 플래그(예: 퀘스트 시작 여부)
        /// </summary>
        bool IsKeySatisfied(string keyName);
    }

    /// <summary>
    /// 조건 상태를 기록/쓰기 위한 서비스
    /// DialogueManager가 최초 대화나 상호작용 완료 시 호출함.
    /// </summary>
    public interface IConditionWriteback
    {
        /// <summary>
        /// 특정 오브젝트를 상호작용 완료로 기록
        /// </summary>
        void MarkObjectInteracted(string objectKey);

        /// <summary>
        /// 특정 NPC를 상호작용 완료로 기록
        /// </summary>
        void MarkNpcInteracted(string npcId);

        /// <summary>
        /// PlayerPrefs 기반으로 임의 키 플래그를 저장
        /// (ex: 퀘스트 완료 여부, 컷씬 재생 여부 등)
        /// </summary>
        void SetKey(string keyName, bool value = true);
    }
}
