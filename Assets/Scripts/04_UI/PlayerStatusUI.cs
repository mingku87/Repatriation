using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerStatusUIElement
{
    public PlayerStatusType statusType;
    public Image statusBar;
}

public class PlayerStatusUI : MonoBehaviour
{
    [SerializeField] private PlayerStatusUIElement[] playerStatusUIElements;

    public void SetPlayerStatus(PlayerStatusType statusType, float currentValue, float maxValue)
    {
        foreach (var element in playerStatusUIElements)
        {
            if (element.statusType != statusType) continue;

            element.statusBar.fillAmount = currentValue / maxValue;
            break;
        }
    }
}