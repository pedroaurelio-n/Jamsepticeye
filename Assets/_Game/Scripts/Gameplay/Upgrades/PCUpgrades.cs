using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PCUpgrades : MonoBehaviour
{
    [field: SerializeField] public List<UpgradeEntry> AvailableUpgrades { get; private set; }
    
    MinigameManager _minigameManager;

    public void Setup (MinigameManager minigameManager)
    {
        _minigameManager = minigameManager;
    }

    public UpgradeEntry GetEntryByType (UpgradeType type)
    {
        return AvailableUpgrades.FirstOrDefault(x => x.UpgradeData.Type == type);
    }

    public bool BuyNextUpgrade(UpgradeType type)
    {
        UpgradeEntry entry = GetEntryByType(type);
        if (entry == null)
            return false;
        
        UpgradeData data = entry.UpgradeData;
        int index = entry.CurrentIndex;
        
        float currentValue = GetCurrentValue(entry);
        if (currentValue >= data.MaxLimit)
            return false;
        
        int cost = Mathf.RoundToInt(data.BaseCost * Mathf.Pow(data.Multiplier, index));
        if (!GameManager.Instance.TryModifyCredits(-cost))
            return false;
        
        ApplyUpgrade(data);
        entry.CurrentIndex++;
        return true;
    }
    
    void ApplyUpgrade(UpgradeData data)
    {
        switch (data.Type)
        {
            case UpgradeType.Speed:
                _minigameManager.UpgradeSpeed(data.Value);
                break;
            case UpgradeType.SpawnDelay:
                _minigameManager.UpgradeSpawnDelay(data.Value);
                break;
            case UpgradeType.AutoMove:
                _minigameManager.EnableAutoMove();
                break;
            case UpgradeType.DeathValue:
                _minigameManager.UpgradeDeathValue(data.Value);
                break;
        }
    }
    
    float GetCurrentValue(UpgradeEntry entry)
    {
        UpgradeData data = entry.UpgradeData;
        return data.Value * entry.CurrentIndex;
    }
    
    public int GetNextCost(UpgradeType type)
    {
        UpgradeEntry entry = GetEntryByType(type);
        if (entry == null)
            return -1;
        
        return Mathf.RoundToInt(entry.UpgradeData.BaseCost * Mathf.Pow(entry.UpgradeData.Multiplier, entry.CurrentIndex));
    }
}