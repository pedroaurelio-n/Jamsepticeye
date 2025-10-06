using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [field: SerializeField] public GlobalUpgradeManager GlobalUpgradeManager { get; private set; }

    [SerializeField] HUD hud;
    [SerializeField] Player player;
    [SerializeField] PCList pcList;
    
    public bool Finished { get; set; }
    public int GlobalCredits { get; private set; }
    public int GlobalDeaths { get; private set; }
    public int CurrentPCCount { get; private set; } = 1;

    void Awake ()
    {
        Instance = this;
    }

    void Start ()
    {
        hud.UpdateCredits(GlobalCredits);
    }

    void Update ()
    {
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
            TryModifyCredits(+1000);
    }

    public bool TryModifyCredits (int value, bool passive = false)
    {
        if (value < 0 && Mathf.Abs(value) > GlobalCredits)
            return false;
        
        GlobalCredits += value;
        hud.UpdateCredits(GlobalCredits);
        return true;
    }

    public void IncreaseDeaths ()
    {
        GlobalDeaths++;
        ParanoiaManager.Instance.UpdateParanoia(GlobalDeaths);
    }

    public void BuyNewPC ()
    {
        CurrentPCCount++;
        pcList.ActivateNewPC();
    }

    public void SetInteractHudActive (bool active)
    {
        hud.SetInteractIndicator(active);
    }

    public void SetPlayerState (bool interacting)
    {
        player.SetInteractionMode(interacting);
    }

    public void StartAutoClaimRoutine ()
    {
        StartCoroutine(AutoClaimRoutine());
    }
    
    IEnumerator AutoClaimRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            
            if (!GlobalUpgradeManager.AutoClaimEnabled)
                continue;
            
            foreach (PCController pc in pcList.ActivePcs)
            {
                int claimed = pc.RedeemCredits();
                if (claimed > 0)
                    TryModifyCredits(claimed, passive: true);
            }
        }
    }
}