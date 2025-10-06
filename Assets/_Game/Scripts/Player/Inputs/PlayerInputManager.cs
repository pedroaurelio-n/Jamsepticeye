using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    public string ActiveControlScheme => _input.currentControlScheme;

    Player _player;
    PlayerCharacterController _characterController;
    PlayerCameraController _cameraController;
    PlayerInput _input;
    
    PlayerControls _controls;
    
    void Awake ()
    {
        _player = GetComponentInParent<Player>();
        _characterController = _player.CharacterController;
        _cameraController = _player.CameraController;
        _input = GetComponent<PlayerInput>();
        
        _controls = new PlayerControls();
        _controls.Enable();
    }

    void OnEnable ()
    {
        _controls.Enable();
    }

    void OnDisable ()
    {
        _controls.Disable();
    }

    void Update ()
    {
        Move();
        Act();
        
        //TODO pedro: temporary
        if (Keyboard.current.pKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Move ()
    {
        _cameraController.SetCameraMovementActive(_player.CanMove);
        PlayerCharacterInputs inputs;

        if (!_player.CanMove)
        {
            inputs = new PlayerCharacterInputs();
            _characterController.SetInputs(ref inputs);
            return;
        }
        
        inputs = new PlayerCharacterInputs();

        Vector2 movement = _controls.Gameplay.Move.ReadValue<Vector2>();
        inputs.MoveForwardAxis = movement.y;
        inputs.MoveRightAxis = movement.x;
        
        inputs.LookRotation = _cameraController.LookRotation;
        inputs.JumpPressed = _controls.Gameplay.Jump.triggered;
        
        _characterController.SetInputs(ref inputs);
    }

    void Act ()
    {
        if (_controls.Gameplay.Interact.triggered)
            _player.InteractionController.TryInteract(false);
        
        if (!_player.CanAct)
            return;
    }
}
