using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PCUpgrades : MonoBehaviour
{
    [field: SerializeField] public List<PCUpgradeEntry> AvailableUpgrades { get; private set; }

    public int PCIndex => _minigameManager.PCIndex;
    
    MinigameManager _minigameManager;

    public void Setup (MinigameManager minigameManager)
    {
        _minigameManager = minigameManager;
    }

    public PCUpgradeEntry GetEntryByType (PCUpgradeType type)
    {
        return AvailableUpgrades.FirstOrDefault(x => x.PCUpgradeData.Type == type);
    }

    public bool BuyNextUpgrade(PCUpgradeType type)
    {
        PCUpgradeEntry entry = GetEntryByType(type);
        if (entry == null)
            return false;
        
        PCUpgradeData data = entry.PCUpgradeData;
        int index = entry.CurrentIndex;
        
        float currentValue = GetCurrentValue(entry);
        if (currentValue >= data.MaxLimit)
            return false;
        
        int cost = GetNextCost(type, _minigameManager.PCIndex);
        if (!GameManager.Instance.TryModifyCredits(-cost))
            return false;
        
        ApplyUpgrade(data);
        entry.CurrentIndex++;
        return true;
    }
    
    void ApplyUpgrade(PCUpgradeData data)
    {
        switch (data.Type)
        {
            case PCUpgradeType.Speed:
                _minigameManager.UpgradeSpeed(data.Value);
                break;
            case PCUpgradeType.SpawnDelay:
                _minigameManager.UpgradeSpawnDelay(data.Value);
                break;
            case PCUpgradeType.AutoMove:
                _minigameManager.EnableAutoMove();
                break;
            case PCUpgradeType.DeathValue:
                _minigameManager.UpgradeDeathValue(data.Value);
                break;
            case PCUpgradeType.CreditsStorage:
                _minigameManager.UpgradeCreditsStorage(data.Value);
                break;
        }
    }
    
    float GetCurrentValue(PCUpgradeEntry entry)
    {
        PCUpgradeData data = entry.PCUpgradeData;
        return data.Value * entry.CurrentIndex;
    }
    
    public int GetNextCost (PCUpgradeType type, int pcIndex)
    {
        PCUpgradeEntry entry = GetEntryByType(type);
        if (entry == null)
            return -1;
        
        PCUpgradeData data = entry.PCUpgradeData;
        
        float localCost = data.BaseCost * Mathf.Pow(data.LocalMultiplier, entry.CurrentIndex);
        float pcScaling = Mathf.Pow(data.PCMultiplier, pcIndex);
        float discount = 1f - GameManager.Instance.GlobalUpgradeManager.CostReducerPercent;
        
        return Mathf.RoundToInt(localCost * pcScaling * discount);
    }
}