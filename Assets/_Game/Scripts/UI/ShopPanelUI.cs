using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanelUI : MonoBehaviour
{
    public event Action OnUpgradeBought;
    
    [SerializeField] Transform entryContainer;
    [SerializeField] ShopUpgradeEntryUI upgradeEntryPrefab;

    List<ShopUpgradeEntryUI> _activeEntries = new();
    PCUpgrades _pcUpgrades;

    public void Setup (PCUpgrades pcUpgrades)
    {
        _pcUpgrades = pcUpgrades;

        CreateEntries();
        UpdateEntries();
    }

    void CreateEntries ()
    {
        int total = _pcUpgrades.AvailableUpgrades.Count;
        int missing = total - _activeEntries.Count;

        for (int i = 0; i < missing; i++)
        {
            ShopUpgradeEntryUI entry = Instantiate(upgradeEntryPrefab, entryContainer);
            _activeEntries.Add(entry);
            entry.OnUpgradeBought += HandleUpgradeBought;
        }
    }

    void UpdateEntries ()
    {
        for (int i = 0; i < _activeEntries.Count; i++)
        {
            _activeEntries[i].gameObject.SetActive(true);
            _activeEntries[i].Setup(_pcUpgrades, _pcUpgrades.AvailableUpgrades[i].UpgradeData.Type);
        }
    }

    void HandleUpgradeBought ()
    {
        OnUpgradeBought?.Invoke();
    }
}
