using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUpgradeEntryUI : MonoBehaviour
{
    public event Action OnUpgradeBought;
    
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Button buyButton;
    
    PCUpgrades _pcUpgrades;
    UpgradeType _type;

    public void Setup (PCUpgrades pcUpgrades, UpgradeType type)
    {
        _pcUpgrades = pcUpgrades;
        _type = type;
        
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);

        SyncEntryUI();
    }
    
    public void ForceSync () => SyncEntryUI();

    void SyncEntryUI ()
    {
        UpgradeEntry entry = _pcUpgrades.GetEntryByType(_type);
        if (entry == null)
        {
            nameText.text = "N/A";
            levelText.text = "---";
            costText.text = "---";
            buyButton.interactable = false;
            return;
        }
        
        UpgradeData data = entry.UpgradeData;
        int level = entry.CurrentIndex;
        
        nameText.text = data.UpgradeName;
        levelText.text = $"Lvl. {level}";
        
        int nextCost = _pcUpgrades.GetNextCost(_type);

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
        if (_pcUpgrades.BuyNextUpgrade(_type))
        {
            SyncEntryUI();
            OnUpgradeBought?.Invoke();
        }
    }
}
