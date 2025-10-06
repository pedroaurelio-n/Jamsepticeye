using System.Collections.Generic;
using System.Linq;
using PedroAurelio.AudioSystem;
using UnityEngine;

public class GlobalUpgradeManager : MonoBehaviour
{
    [field: SerializeField] public List<GlobalUpgradeEntry> AvailableUpgrades { get; private set; }
    [SerializeField] PlayAudioEvent buyAudio;
    [SerializeField] PlayAudioEvent failureAudio;
    [SerializeField] PlayAudioEvent newPcAudio;
    [SerializeField] EndingSequence endingSequence;

    public float CostReducerPercent { get; private set; }
    public bool AutoClaimEnabled { get; private set; }
    public float ManualPlayMultiplier { get; private set; } = 1f;
    public float GlobalSpeedBoost { get; private set; }
    public float GlobalSpawnDelayBoost { get; private set; }
    
    public GlobalUpgradeEntry GetEntryByType (GlobalUpgradeType type)
    {
        return AvailableUpgrades.FirstOrDefault(x => x.UpgradeData.Type == type);
    }
    
    public bool BuyNextUpgrade(GlobalUpgradeType type)
    {
        GlobalUpgradeEntry entry = GetEntryByType(type);
        if (entry == null)
            return false;

        GlobalUpgradeData data = entry.UpgradeData;
        if (entry.CurrentIndex >= data.MaxLimit)
            return false;

        int cost = GetNextCost(data.Type);

        if (!GameManager.Instance.TryModifyCredits(-cost))
        {
            failureAudio.PlayAudio();
            return false;
        }

        ApplyUpgrade(data);
        entry.CurrentIndex++;
        return true;
    }
    
    void ApplyUpgrade(GlobalUpgradeData data)
    {
        switch (data.Type)
        {
            case GlobalUpgradeType.CostReducer:
                CostReducerPercent += data.Value;
                buyAudio.PlayAudio();
                break;
            case GlobalUpgradeType.AutoClaim:
                AutoClaimEnabled = true;
                GameManager.Instance.StartAutoClaimRoutine();
                break;
            case GlobalUpgradeType.ManualPlayBoost:
                ManualPlayMultiplier += data.Value;
                buyAudio.PlayAudio();
                break;
            case GlobalUpgradeType.GlobalSpeed:
                GlobalSpeedBoost += data.Value;
                buyAudio.PlayAudio();
                break;
            case GlobalUpgradeType.GlobalSpawnDelay:
                GlobalSpawnDelayBoost += data.Value;
                buyAudio.PlayAudio();
                break;
            case GlobalUpgradeType.NewPC:
                GameManager.Instance.BuyNewPC();
                newPcAudio.PlayAudio();
                break;
            case GlobalUpgradeType.Salvation:
                GameManager.Instance.Finished = true;
                endingSequence.Trigger();
                break;
        }
    }
    
    public int GetNextCost (GlobalUpgradeType type)
    {
        GlobalUpgradeEntry entry = GetEntryByType(type);
        if (entry == null)
            return -1;

        int cost = Mathf.RoundToInt(entry.UpgradeData.BaseCost *
                                    Mathf.Pow(entry.UpgradeData.Multiplier, entry.CurrentIndex));
        cost = Mathf.RoundToInt(cost * (1f - CostReducerPercent));
        return cost;
    }
}