using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

    void Update ()
    {
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
            TryModifyCredits(+100);
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

    public void SetPlayerState (bool interacting)
    {
        player.SetInteractionMode(interacting);
    }
}