using System;
using UnityEngine;

public enum PCUpgradeType
{
    Speed = 0,
    SpawnDelay = 1,
    AutoMove = 2,
    DeathValue = 3,
    CreditsStorage = 4,
    //TODO pedro: BetterSpikesChance
}

[Serializable]
public class PCUpgradeEntry
{
    [field: SerializeField] public PCUpgradeData PCUpgradeData { get; private set; }
    [field: SerializeField] public int CurrentIndex { get; set; }
}

[CreateAssetMenu(menuName = "Game/UpgradeData")]
public class PCUpgradeData : ScriptableObject
{
    [field: SerializeField] public string UpgradeName { get; private set; }
    [field: SerializeField] public PCUpgradeType Type { get; private set; }
    [field: SerializeField] public int BaseCost { get; private set; }
    [field: SerializeField] public float Value { get; private set; }
    [field: SerializeField] public float LocalMultiplier { get; private set; }
    [field: SerializeField] public float PCMultiplier { get; private set; } = 1f;
    [field: SerializeField] public float MaxLimit { get; private set; }
}