using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] HUD hud;
    [SerializeField] Player player;
    
    public int CurrentCredits { get; private set; }

    void Awake ()
    {
        Instance = this;
    }

    void Start ()
    {
        hud.UpdateCredits(CurrentCredits);
    }

    public bool TryModifyCredits (int value)
    {
        if (value < 0 && Mathf.Abs(value) > CurrentCredits)
            return false;
        
        CurrentCredits += value;
        hud.UpdateCredits(CurrentCredits);
        return true;
    }

    public void SetInteractHudActive (bool active)
    {
        hud.SetInteractIndicator(active);
    }
    
    public void SetExitHudActive (bool active)
    {
        hud.SetExitIndicator(active);
    }

    public void SetPlayerState (bool interacting)
    {
        player.CanAct = !interacting;
        player.CanMove = !interacting;
    }
}