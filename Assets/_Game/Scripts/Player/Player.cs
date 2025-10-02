using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public PlayerCharacterController CharacterController { get; private set; }
    [field: SerializeField] public PlayerCameraController CameraController { get; private set; }
    [field: SerializeField] public PlayerInputManager InputManager { get; private set; }
    [field: SerializeField] public PlayerInteractionController InteractionController { get; private set; }
    
    public bool CanMove { get; set; }
    public bool CanAct { get; set; }

    //TODO pedro: maybe isolate in different class
    [Header("Knockback Settings")]
    [SerializeField] float knockbackForce;
    [SerializeField] float knockbackUpBias;
    [SerializeField] bool resetKnockbackY;

    void Start ()
    {
        CanMove = true;
        CanAct = true;
        CameraController.Initialize();
        InteractionController.Initialize(this);
        
        Cursor.lockState = CursorLockMode.Locked;
    }
}
