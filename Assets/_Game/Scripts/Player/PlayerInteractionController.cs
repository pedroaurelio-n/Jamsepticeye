using System;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    Player _player;
    bool _interact;

    public void Initialize (Player player)
    {
        _player = player;
    }

    public void Interact () => _interact = true;

    void OnTriggerStay (Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable))
            return;

        if (_player.CanAct)
        {
            GameManager.Instance.SetInteractHudActive(true);
            GameManager.Instance.SetExitHudActive(false);
        }
        else
        {
            GameManager.Instance.SetInteractHudActive(false);
            GameManager.Instance.SetExitHudActive(true);
        }

        if (!_interact)
            return;

        _interact = false;
        
        if (_player.CanAct)
            interactable.Interact();
        else
            interactable.Exit();
    }

    void OnTriggerExit (Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable))
            return;

        GameManager.Instance.SetInteractHudActive(false);
        GameManager.Instance.SetExitHudActive(false);
    }
}
