using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerStatusUIElement
{
    public Status statusType;
    public Image statusBar;
}

public class PlayerStatusUI : MonoBehaviour
{
    [SerializeField] private PlayerStatusUIElement[] playerStatusUIElements;

    void Start()
    {
        Player.AddOnStatChangedEvent(OnStatChanged);
    }

    private void OnStatChanged(Status statusType, float currentValue, float maxValue)
    {
        if (InGameManager.difficulty == Difficulty.Hard) return;

        foreach (var element in playerStatusUIElements)
        {
            if (element.statusType != statusType) continue;

            element.statusBar.fillAmount = currentValue / maxValue;
            break;
        }
    }
}