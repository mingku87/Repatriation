using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeySettingButton : MonoBehaviour
{
    [SerializeField] private PlayerAction action;
    [SerializeField] private TextMeshProUGUI ActionText;
    [SerializeField] private TextMeshProUGUI KeyText;
    [SerializeField] private Button button;
    private bool isListeningForInput = false;

    public void Initialize()
    {
        UpdateKeyText();
        button.onClick.AddListener(() => isListeningForInput = true);
    }

    void Update()
    {
        if (isListeningForInput) ListenForInput();
    }

    private void ListenForInput()
    {
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode) == false) continue;

            SetKeyCode(keyCode);
            return;
        }
    }

    private void SetKeyCode(KeyCode keyCode)
    {
        isListeningForInput = false;
        KeyManager.SetKey(action, keyCode);
        UpdateKeyText();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void UpdateKeyText() => KeyText.text = KeyManager.GetKey(action).ToString();
}