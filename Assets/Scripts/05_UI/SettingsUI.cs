using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsUI : SingletonObject<SettingsUI>
{
    [SerializeField] private GameObject settingsPanel;
    public void ShowSettings() => Util.SetActive(settingsPanel, true);
}
