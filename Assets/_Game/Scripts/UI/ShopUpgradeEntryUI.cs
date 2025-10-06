using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUpgradeEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action OnUpgradeBought;
    
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Button buyButton;
    [SerializeField] Color availableColor;
    [SerializeField] Color unavailableColor;
    [SerializeField] GameObject tooltipObj;
    [SerializeField] TextMeshProUGUI tooltipText;
    
    PCUpgrades _pcUpgrades;
    PCUpgradeType _type;
    bool _paranoid;

    public void Setup (PCUpgrades pcUpgrades, PCUpgradeType type, bool paranoid)
    {
        _pcUpgrades = pcUpgrades;
        _type = type;
        
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);

        _paranoid = paranoid;
        SyncEntryUI();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        PCUpgradeEntry entry = _pcUpgrades.GetEntryByType(_type);
        if (entry == null)
            return;
        
        PCUpgradeData data = entry.PCUpgradeData;

        string description = ParanoiaManager.Instance.TryTriggerParanoia()
            ? data.ParanoiaDescription
            : data.UpgradeDescription;
        tooltipText.text = description;
        tooltipObj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipObj.SetActive(false);
    }

    void SyncEntryUI ()
    {
        PCUpgradeEntry entry = _pcUpgrades.GetEntryByType(_type);
        if (entry == null)
        {
            nameText.text = "N/A";
            levelText.text = "---";
            costText.text = "---";
            buyButton.interactable = false;
            return;
        }
        
        PCUpgradeData data = entry.PCUpgradeData;
        int level = entry.CurrentIndex;

        string name = _paranoid ? data.ParanoiaDescription : data.UpgradeName;
        nameText.text = name.ToUpper();
        
        levelText.text = $"LVL.{level}";
        
        int nextCost = _pcUpgrades.GetNextCost(_type, _pcUpgrades.PCIndex);

        if (nextCost < 0 || level + 1 > data.MaxLimit)
        {
            costText.text = "MAX";
            buyButton.interactable = false;
            return;
        }
        
        costText.text = $"${nextCost}";
        // buyButton.interactable = GameManager.Instance.GlobalCredits >= nextCost;
        buyButton.image.color = GameManager.Instance.GlobalCredits >= nextCost ? availableColor : unavailableColor;
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
