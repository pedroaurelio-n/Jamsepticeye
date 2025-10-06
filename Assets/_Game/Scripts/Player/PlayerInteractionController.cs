using System;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    Player _player;
    IInteractable _currentInteractable;

    public void Initialize (Player player)
    {
        _player = player;
    }

    public void TryInteract (bool exit)
    {
        if (_currentInteractable == null)
            return;
        
        if (exit)
        {
            _currentInteractable.Exit();
            return;
        }
        
        if (_player.CanAct)
        {
            _currentInteractable.Interact();
            _player.CharacterController.ResetSpeed();
            GameManager.Instance.SetInteractHudActive(false);
        }
        else
            _currentInteractable.Exit();
    }

    void OnTriggerEnter (Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable))
            return;
        
        _currentInteractable = interactable;
        GameManager.Instance.SetInteractHudActive(true);
    }

    void OnTriggerExit (Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable))
            return;

        if (_currentInteractable != interactable)
            return;
        
        _currentInteractable = null;
        GameManager.Instance.SetInteractHudActive(false);
    }
}
