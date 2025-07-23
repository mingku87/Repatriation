using UnityEngine;

public class SettingsBootstrap : MonoBehaviour
{
    [SerializeField] AudioManager audioMgr; // 니가 만든 Mixer 적용 코드
    [SerializeField] KeyManager keyMgr;     // 기본 키 세팅 들어있는 애 (Awake에서 기본 세팅됨) :contentReference[oaicite:4]{index=4}

    void Awake()
    {
        // 먼저 KeyManager가 기본값 셋업 (Awake) 되도록 Script Execution Order 조정 or 이쪽에서 수동 호출

        if (UserSettingsIO.Load(out var data))
        {
            // // 불러온 값이 있으면 덮어쓰기
            // SettingsBridge.ApplyToSystems(data, (m, b, s) =>
            // {
            //     // audioMgr.SetMaster(m);
            //     // audioMgr.SetBgm(b);
            //     // audioMgr.SetSfx(s);
            // });
        }
        else
        {
            // 저장 없음: 기본값 유지 → 캡처해서 곧바로 저장해도 됨
            // var captured = SettingsBridge.Capture(
            //     audioMgr.master, audioMgr.bgm, audioMgr.sfx,
            //     keyMgr.GetActionKeyCodes(), keyMgr.GetQuickSlotKeyCodes()); // :contentReference[oaicite:5]{index=5}
            // UserSettingsIO.Save(captured);
        }
    }
}
