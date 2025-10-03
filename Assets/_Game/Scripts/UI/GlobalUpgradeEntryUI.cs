using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalUpgradeEntryUI : MonoBehaviour
{
    public event Action OnUpgradeBought;
    
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Button buyButton;
    
    GlobalUpgradeType _type;

    public void Setup (GlobalUpgradeType type)
    {
        _type = type;
        
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);

        SyncEntryUI();
    }
    
    public void ForceSync () => SyncEntryUI();

    void SyncEntryUI ()
    {
        GlobalUpgradeEntry entry = GameManager.Instance.GlobalUpgradeManager.GetEntryByType(_type);
        if (entry == null)
        {
            nameText.text = "N/A";
            levelText.text = "---";
            costText.text = "---";
            buyButton.interactable = false;
            return;
        }
        
        GlobalUpgradeData data = entry.UpgradeData;
        int level = entry.CurrentIndex;
        
        nameText.text = data.UpgradeName;
        levelText.text = $"Lvl. {level}";
        
        int nextCost = GameManager.Instance.GlobalUpgradeManager.GetNextCost(_type);
        if (nextCost < 0 || level + 1 > data.MaxLimit)
        {
            costText.text = "MAX";
            buyButton.interactable = false;
            return;
        }
        
        costText.text = $"${nextCost}";
        buyButton.interactable = GameManager.Instance.GlobalCredits >= nextCost;
    }

    void OnBuyButtonClicked ()
    {
        if (GameManager.Instance.GlobalUpgradeManager.BuyNextUpgrade(_type))
        {
            SyncEntryUI();
            OnUpgradeBought?.Invoke();
        }
    }
}