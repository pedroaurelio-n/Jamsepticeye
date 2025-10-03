using System;
using UnityEngine;

public enum UpgradeType
{
    Speed = 0,
    SpawnDelay = 1,
    AutoMove = 2,
    DeathValue = 3
    //TODO pedro: BetterSpikesChance
}

[Serializable]
public class UpgradeEntry
{
    [field: SerializeField] public UpgradeData UpgradeData { get; private set; }
    [field: SerializeField] public int CurrentIndex { get; set; }
}

[CreateAssetMenu(menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [field: SerializeField] public string UpgradeName { get; private set; }
    [field: SerializeField] public UpgradeType Type { get; private set; }
    [field: SerializeField] public int BaseCost { get; private set; }
    [field: SerializeField] public float Value { get; private set; }
    [field: SerializeField] public float Multiplier { get; private set; }
    [field: SerializeField] public float MaxLimit { get; private set; }
}