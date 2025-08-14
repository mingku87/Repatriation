using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsUIController : SingletonObject<SettingsUIController>
{
    [SerializeField] private GameObject settingsPanel;
    public void ShowSettings() => Util.SetActive(settingsPanel, true);
    public void SaveSettings()
    {
        AudioManager.Instance.SaveVolume();
        SettingsBridge.SaveSettings();
        // KeyManager.Instance.SaveKeyBindings();
        // HideSettings();
    }
}
