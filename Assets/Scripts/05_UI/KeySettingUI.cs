using TMPro;
using UnityEngine;

public class KeySettingUI : MonoBehaviour
{
    [SerializeField] private KeySettingButton[] keyButtons; 

    private void Start()
    {
        UpdateKeySettingUI();
    }

    private void Update()
    {
        UpdateKeySettingUI();
    }

    public void UpdateKeySettingUI()
    {
        foreach (var button in keyButtons)
        {
            button.UpdateKeyText();
        }
    }
}