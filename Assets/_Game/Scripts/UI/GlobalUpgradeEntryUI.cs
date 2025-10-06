using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlobalUpgradeEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    
    GlobalUpgradeType _type;

    public void Setup (GlobalUpgradeType type)
    {
        _type = type;
        
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);

        SyncEntryUI();
    }
    
    public void ForceSync () => SyncEntryUI();
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        GlobalUpgradeEntry entry = GameManager.Instance.GlobalUpgradeManager.GetEntryByType(_type);
        if (entry == null)
            return;
        
        GlobalUpgradeData data = entry.UpgradeData;

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
        
        nameText.text = data.UpgradeName.ToUpper();
        levelText.text = $"LVL.{level}";
        
        int nextCost = GameManager.Instance.GlobalUpgradeManager.GetNextCost(_type);
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
        if (GameManager.Instance.GlobalUpgradeManager.BuyNextUpgrade(_type))
        {
            SyncEntryUI();
            OnUpgradeBought?.Invoke();
        }
    }
}