using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public PlayerCharacterController CharacterController { get; private set; }
    [field: SerializeField] public PlayerCameraController CameraController { get; private set; }
    [field: SerializeField] public PlayerInputManager InputManager { get; private set; }
    [field: SerializeField] public PlayerInteractionController InteractionController { get; private set; }
    
    public bool CanMove { get; set; }
    public bool CanAct { get; set; }

    void Start ()
    {
        CanMove = true;
        CanAct = true;
        CameraController.Initialize();
        InteractionController.Initialize(this);
        
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void SetInteractionMode(bool active)
    {
        CanMove = !active;
        CanAct = !active;
        CameraController.SetCameraMovementActive(!active);
    }
}
