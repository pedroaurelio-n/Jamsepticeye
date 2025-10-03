using System;
using UnityEngine;

public enum GlobalUpgradeType
{
    CostReducer = 0,
    AutoClaim = 1,
    ManualPlayBoost = 2,
    GlobalSpeed = 3,
    GlobalSpawnDelay = 4,
    NewPC = 5
}

[Serializable]
public class GlobalUpgradeEntry
{
    [field: SerializeField] public GlobalUpgradeData UpgradeData { get; private set; }
    [field: SerializeField] public int CurrentIndex { get; set; }
}

[CreateAssetMenu(menuName = "Game/GlobalUpgradeData")]
public class GlobalUpgradeData : ScriptableObject
{
    [field: SerializeField] public string UpgradeName { get; private set; }
    [field: SerializeField] public GlobalUpgradeType Type { get; private set; }
    [field: SerializeField] public int BaseCost { get; private set; }
    [field: SerializeField] public float Value { get; private set; }
    [field: SerializeField] public float Multiplier { get; private set; } = 1.5f;
    [field: SerializeField] public float MaxLimit { get; private set; }
}