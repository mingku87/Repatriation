using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeySettingButton : MonoBehaviour
{
    [SerializeField] private PlayerAction action;
    [SerializeField] private TextMeshProUGUI ActionText;
    [SerializeField] private TextMeshProUGUI KeyText;
    [SerializeField] private Image KeyImage; // It used for Mouse Key Input

    private bool isListeningForInput = false;

    void Start()
    {
        UpdateKeyText();
    }

    void Update()
    {
        if (isListeningForInput)
        {
            ListenForInput();
        }
    }

    private void ListenForInput()
    {
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                SetKeyCode(keyCode);
                return;
            }
        }
    }

    void SetKeyCode(KeyCode keyCode)
    {
        isListeningForInput = false;
        KeySetting.SetKey(action, keyCode);
        UpdateKeyText();
        EventSystem.current.SetSelectedGameObject(null);
        //if (GameManager.Instance.isInGame) PlayerUIManager.Instance.UpdateHotKeyText(action);
    }

    public void OnClick()
    {
        isListeningForInput = true;
    }

    public void UpdateKeyText()
    {
        //ActionText.text = action.ToString();
        KeyText.text = KeySetting.GetKey(action).ToString();
    }
}