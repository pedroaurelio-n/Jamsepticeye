using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] HUD hud;
    [SerializeField] Player player;
    
    public int GlobalCredits { get; private set; }

    void Awake ()
    {
        Instance = this;
    }

    void Start ()
    {
        hud.UpdateCredits(GlobalCredits);
    }

    public bool TryModifyCredits (int value, bool passive = false)
    {
        if (value < 0 && Mathf.Abs(value) > GlobalCredits)
            return false;
        
        GlobalCredits += value;
        hud.UpdateCredits(GlobalCredits);
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