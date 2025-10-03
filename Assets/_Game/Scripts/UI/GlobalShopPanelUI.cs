using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalShopPanelUI : MonoBehaviour
{
    public event Action OnUpgradeBought;
    
    [SerializeField] Transform entryContainer;
    [SerializeField] GlobalUpgradeEntryUI upgradeEntryPrefab;

    List<GlobalUpgradeEntryUI> _activeEntries = new();

    public void Setup ()
    {
        CreateEntries();
        UpdateEntries();
    }

    void CreateEntries ()
    {
        int total = GameManager.Instance.GlobalUpgradeManager.AvailableUpgrades.Count;
        int missing = total - _activeEntries.Count;

        for (int i = 0; i < missing; i++)
        {
            GlobalUpgradeEntryUI entry = Instantiate(upgradeEntryPrefab, entryContainer);
            _activeEntries.Add(entry);
            entry.OnUpgradeBought += HandleUpgradeBought;
        }
    }

    void UpdateEntries ()
    {
        for (int i = 0; i < _activeEntries.Count; i++)
        {
            _activeEntries[i].gameObject.SetActive(true);
            _activeEntries[i].Setup(GameManager.Instance.GlobalUpgradeManager.AvailableUpgrades[i].UpgradeData.Type);
        }
    }

    void HandleUpgradeBought ()
    {
        OnUpgradeBought?.Invoke();
    }
}
