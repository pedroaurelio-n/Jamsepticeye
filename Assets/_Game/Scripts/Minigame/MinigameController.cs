using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MinigameController : MonoBehaviour
{
    [SerializeField] float jumpForce = 5f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    
    MinigameManager _manager;
    Rigidbody2D _rb;

    float _speed;
    bool _autoMove;
    Vector2 _input;
    Vector2 _moveInput;
    bool _jumpInput;

    void Awake ()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Init (MinigameManager manager, float speed, bool auto)
    {
        _manager = manager;
        _speed = speed;
        _autoMove = auto;
    }

    void Update ()
    {
        _moveInput = Vector2.zero;
        
        if (_manager.IsControlled)
        {
            _input = Vector2.zero;
            
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                _input.x = -1;
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                _input.x = 1;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                _jumpInput = true;

            if (_input == Vector2.zero && _autoMove)
                _moveInput = Vector2.right;
            else
                _moveInput = _input;
        }
        else if (_autoMove)
        {
            _moveInput = Vector2.right;
        }
        else
        {
            _moveInput = Vector2.zero;
        }
    }

    void FixedUpdate ()
    {
        Vector2 velocity = _rb.linearVelocity;
        velocity.x = _moveInput.x * _speed;
        _rb.linearVelocity = velocity;
        
        if (_jumpInput && IsGrounded())
        {
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        _jumpInput = false;
    }

    bool IsGrounded ()
    {
        if (groundCheck == null)
            return true;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        if (other.TryGetComponent(out Spike spike))
        {
            _manager.OnPlayerDeath(spike.Multiplier);
        }
    }
    
    void OnDrawGizmosSelected ()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
