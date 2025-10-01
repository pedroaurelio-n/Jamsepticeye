using KinematicCharacterController;
using UnityEngine;

public partial class PlayerCharacterController : MonoBehaviour, ICharacterController
{
    [field: SerializeField] public KinematicCharacterMotor Motor { get; private set; }
    [field: SerializeField] public bool CanJump { get; private set; }
    [field: SerializeField] public bool CanWallClimb { get; private set; }
    
    [Header("Ground Movement")]
    [SerializeField] float maxGroundMoveSpeed = 9f;
    [SerializeField] float groundMovementSharpness = 15f;
    [SerializeField] float orientationSharpness = 10f;

    [Header("Air Movement")]
    [SerializeField] float maxAirMoveSpeed = 8f;
    [SerializeField] float airAccelerationSpeed = 5f;
    [SerializeField] float airDecelerationSpeed = 5f;
    [SerializeField] float airDrag = 0.1f;

    [Header("Jumping")]
    [SerializeField] bool allowJumpingWhenSliding;
    [SerializeField] int maxJumpCount = 2;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float jumpPreGroundingGraceTime = 0.2f;
    [SerializeField] float jumpPostGroundingGraceTime = 0.2f;

    [Header("Wall")]
    [SerializeField] bool allowWallJump = true;
    [SerializeField] bool wallClimbBasedOnMovement;
    [SerializeField, Range(0f, 1.5f)] float wallJumpUpWeight = 1.25f;
    [SerializeField, Range(1f, 3f)] float wallJumpNormalWeight = 1f;
    [SerializeField] Transform wallOverlapTransform;
    [SerializeField] float wallOverlapRadius;
    [SerializeField] LayerMask wallOverlapLayer;
    [SerializeField] float wallClimbPerpThreshold = 80f;
    [SerializeField] float wallClimbAlongThreshold = 2f;
    [SerializeField, Range(-10, -1)] float maxWallClimbFallSpeed = -4f;

    [Header("Misc")]
    [SerializeField] bool orientTowardsGravity;
    [SerializeField] Vector3 defaultGravity = new(0, -30f, 0);
    [SerializeField] Vector3 wallClimbGravity = new(0, -12f, 0);
    [SerializeField] Transform meshRoot;
    
    public float GroundMovementSharpness { get; set; }
    public float StartGroundMovementSharpness => groundMovementSharpness;
    
    public float AirDrag { get; set; }
    public float StartAirDrag => airDrag;

    public Vector2 RawInputVector => _rawInputVector;
    public Vector3 MoveInputVector => _moveInputVector;
    public Vector3 FullLookDirection => _fullLookInputVector;
    public bool IsGrounded => Motor.GroundingStatus.IsStableOnGround;
    public bool IsOnClimbableWall => _isOnClimbableWall;
    public Vector3 WallClimbNormal => _wallClimbNormal;
    
    Vector2 _rawInputVector;
    Vector3 _moveInputVector;
    Vector3 _rawLookInputVector;
    Vector3 _fullLookInputVector;
    Vector3 _lookInputVector;

    Vector3 _gravity;
    Vector3 _velocity;
    Vector3 _internalVelocityAdd = Vector3.zero;
    
    bool _resetSpeedRequested;

    int _currentJumpCount;
    float _jumpForce;
    float _timeSinceJumpRequested = Mathf.Infinity;
    float _timeSinceLastAbleToJump;
    bool _jumpRequested;
    bool _jumpedThisFrame;
    bool _canCoyoteJump;
    bool _coyoteJumped;
    
    bool _isOnClimbableWall;
    Vector3 _wallClimbNormal;

    Collider _lastClimbableWallCollider;
    // ClimbableWall _lastClimbableWall;

    void Start ()
    {
        Motor.CharacterController = this;

        GroundMovementSharpness = StartGroundMovementSharpness;
        AirDrag = StartAirDrag;

        _gravity = defaultGravity;
        _jumpForce = Mathf.Sqrt(-2f * jumpHeight * _gravity.y);
    }

    public void SetInputs (ref PlayerCharacterInputs inputs)
    {
        _rawInputVector = new Vector2(inputs.MoveRightAxis, inputs.MoveForwardAxis);

        // Clamp raw input for movement
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(_rawInputVector.x, 0f, _rawInputVector.y), 1f);

        // Calculate camera-aligned movement direction
        Vector3 cameraPlanarDirection =
            Vector3.ProjectOnPlane(inputs.LookRotation * Vector3.forward, Motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
            cameraPlanarDirection =
                Vector3.ProjectOnPlane(inputs.LookRotation * Vector3.up, Motor.CharacterUp).normalized;

        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        _moveInputVector = cameraPlanarRotation * moveInputVector;
        _lookInputVector = cameraPlanarDirection;
        
        _rawLookInputVector = inputs.LookRotation * Vector3.forward;
        _fullLookInputVector = new Vector3(_lookInputVector.x, _rawLookInputVector.y, _lookInputVector.z);

        if (inputs.JumpPressed)
        {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
        }
    }
    
    public void BeforeCharacterUpdate (float deltaTime)
    {
        if (!_isOnClimbableWall)
            _gravity = defaultGravity;

        // Check if coyote time jump is possible
        _canCoyoteJump = _timeSinceLastAbleToJump <= jumpPostGroundingGraceTime;
    }

    public void UpdateRotation (ref Quaternion currentRotation, float deltaTime)
    {
        if (_lookInputVector != Vector3.zero && orientationSharpness > 0f)
        {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(
                Motor.CharacterForward,
                _lookInputVector,
                1 - Mathf.Exp(-orientationSharpness * deltaTime)
            ).normalized;

            // Update the current rotation to face the smoothed direction (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
        }

        if (orientTowardsGravity)
        {
            // Align the character's up direction to gravity
            currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -_gravity) * currentRotation;
        }
    }

    public void UpdateVelocity (ref Vector3 currentVelocity, float deltaTime)
    {
        if (Motor.GroundingStatus.IsStableOnGround)
            EvaluateGroundMovement(ref currentVelocity, deltaTime);
        else
            EvaluateAirMovement(ref currentVelocity, deltaTime);

        EvaluateJump(ref currentVelocity, deltaTime);
        
        if (CanWallClimb && _isOnClimbableWall && currentVelocity.y < 0f)
            currentVelocity.y = Mathf.Max(currentVelocity.y, maxWallClimbFallSpeed);

        // Reset wall jump
        _isOnClimbableWall = false;
        
        if (_resetSpeedRequested)
        {
            currentVelocity = Vector3.zero;
            _resetSpeedRequested = false;
        }

        // Apply additive velocity if necessary
        if (_internalVelocityAdd.sqrMagnitude > 0f)
        {
            currentVelocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }

        _velocity = Motor.Velocity;
    }
    
    public void AfterCharacterUpdate (float deltaTime)
    {
        EvaluateAfterJumpStatus(deltaTime);
        EvaluateAfterWallClimbStatus(deltaTime);
    }
    
    
    public bool IsColliderValidForCollisions (Collider coll)
    {
        return true;
    }
    
    public void OnGroundHit (
        Collider hitCollider,
        Vector3 hitNormal,
        Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport
    )
    {
    }

    public void OnMovementHit (
        Collider hitCollider,
        Vector3 hitNormal,
        Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport
    )
    {
    }
    
    public void ProcessHitStabilityReport (
        Collider hitCollider,
        Vector3 hitNormal,
        Vector3 hitPoint,
        Vector3 atCharacterPosition,
        Quaternion atCharacterRotation,
        ref HitStabilityReport hitStabilityReport
    )
    {
    }

    public void PostGroundingUpdate (float deltaTime)
    {
        EvaluateNewGroundingStatus(deltaTime);
    }

    public void OnDiscreteCollisionDetected (Collider hitCollider)
    {
    }

    public void AddVelocity (Vector3 velocity) => _internalVelocityAdd += velocity;

    public void ResetSpeed () => _resetSpeedRequested = true;
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(wallOverlapTransform.position, wallOverlapRadius);
    }
}
