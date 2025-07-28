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
        if (UserSettingsIO.Load(out saveData) == false)
        {
            saveData = CreateDefaultSaveData();
            UserSettingsIO.Save(saveData);
        }

        var runtime = SettingsBridge.ToRuntime(saveData);
        AudioManager.Instance.Initialize();
        SettingsBridge.ApplyToSystems(runtime);

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