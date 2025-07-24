using System.Collections;
using UnityEngine;

public class BootstrapController : SingletonObject<BootstrapController>
{
    private float delay = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        UserSettingsSaveData saveData;
        if (!UserSettingsIO.Load(out saveData))
        {
            saveData = CreateDefaultSaveData();
            UserSettingsIO.Save(saveData);
        }

        yield return new WaitForSeconds(delay);
        SceneController.Instance.ChangeScene(SceneName.Title);
    }

    private UserSettingsSaveData CreateDefaultSaveData()
    {
        UserSettingsRuntimeData runtimeData = new UserSettingsRuntimeData
        {
            volumes = DefaultSettings.defaultVolumes,
            actionKeys = DefaultSettings.defaultActionKeys,
            quickSlotKeys = DefaultSettings.defaultQuickSlotKeys
        };
        return SettingsBridge.ToSave(runtimeData);
    }
}