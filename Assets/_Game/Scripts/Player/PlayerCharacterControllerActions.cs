using UnityEngine;

public partial class PlayerCharacterController
{
    void EvaluateGroundMovement (ref Vector3 currentVelocity, float deltaTime)
    {
        // Adjust velocity to match the ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) *
                          currentVelocity.magnitude;

        // Calculate target velocity based on input
        Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
        Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized *
                                  _moveInputVector.magnitude;
            
        Vector3 targetMovementVelocity = reorientedInput * maxGroundMoveSpeed;

        // Smoothly interpolate the current velocity toward the target velocity
        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetMovementVelocity,
            1 - Mathf.Exp(-GroundMovementSharpness * deltaTime)
        );
    }

    void EvaluateAirMovement (ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 targetMovementVelocity;
        
        // Assign velocity based on air input
        if (_moveInputVector.sqrMagnitude > 0f)
        {
            targetMovementVelocity = _moveInputVector * maxAirMoveSpeed;

            // Prevent climbing on steep slopes with air movement
            if (Motor.GroundingStatus.FoundAnyGround)
            {
                Vector3 perpendicularObstructionNormal = Vector3
                    .Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp)
                    .normalized;
                
                targetMovementVelocity =
                    Vector3.ProjectOnPlane(targetMovementVelocity, perpendicularObstructionNormal);
            }

            Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, _gravity);

            if (IsOnClimbableWall && Mathf.Approximately(wallClimbGravity.y, 0f))
            {
                Vector3 horizontalMovement = new(velocityDiff.x, 0f, velocityDiff.z);
                Vector3 movementAlongWall = Vector3.ProjectOnPlane(horizontalMovement, _wallClimbNormal);
                Vector3 movementIntoWall = velocityDiff - movementAlongWall;
                
                if (movementIntoWall.sqrMagnitude > wallClimbPerpThreshold)
                {
                    velocityDiff.x = movementAlongWall.sqrMagnitude > wallClimbAlongThreshold ? movementAlongWall.x : -currentVelocity.x;
                    velocityDiff.z = movementAlongWall.sqrMagnitude > wallClimbAlongThreshold ? movementAlongWall.z : -currentVelocity.z;
                }
            }
            currentVelocity += airAccelerationSpeed * deltaTime * velocityDiff;
        }
        // Assign velocity based on air deceleration
        else
        {
            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(Motor.CharacterUp, inputRight).normalized *
                                      _moveInputVector.magnitude;

            targetMovementVelocity = reorientedInput * maxAirMoveSpeed;
            targetMovementVelocity.y = currentVelocity.y;

            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetMovementVelocity,
                1 - Mathf.Exp(-airDecelerationSpeed * deltaTime)
            );
        }

        // Apply gravity and drag
        currentVelocity += _gravity * deltaTime;
        currentVelocity *= 1f / (1f + AirDrag * deltaTime);
    }

    void EvaluateJump (ref Vector3 currentVelocity, float deltaTime)
    {
        // Handle jumping
        _jumpedThisFrame = false;
        _timeSinceJumpRequested += deltaTime;
        if (!_jumpRequested || !CanJump)
            return;
        
        bool evaluateWallClimb = CanWallClimb && _isOnClimbableWall;
        bool evaluateGroundStatus = allowJumpingWhenSliding
            ? Motor.GroundingStatus.FoundAnyGround
            : Motor.GroundingStatus.IsStableOnGround;

        // Handle first jump & wall jump
        if (evaluateWallClimb || (_currentJumpCount == 0 && (evaluateGroundStatus || _canCoyoteJump)))
        {
            if (_canCoyoteJump && !evaluateGroundStatus)
                _coyoteJumped = true;

            // Calculate jump direction before un-grounding
            Vector3 jumpDirection = Motor.CharacterUp;
            if (evaluateWallClimb)
                jumpDirection = _wallClimbNormal * wallJumpNormalWeight + jumpDirection * wallJumpUpWeight;
            else if (Motor.GroundingStatus is { FoundAnyGround: true, IsStableOnGround: false })
                jumpDirection = Motor.GroundingStatus.GroundNormal;
                
            // Makes the character skip ground probing/snapping on its next update.
            // If this line weren't here, the character would remain snapped to the ground when trying to jump
            Motor.ForceUnground();

            // Add to the return velocity and reset jump state
            currentVelocity += jumpDirection * _jumpForce - Vector3.Project(currentVelocity, Motor.CharacterUp);
            _jumpRequested = false;
            _currentJumpCount++;
            _jumpedThisFrame = true;
        }

        bool hasExtraJumps = _currentJumpCount > 0 && _currentJumpCount < maxJumpCount;
        if (_isOnClimbableWall || _coyoteJumped || !hasExtraJumps || evaluateGroundStatus)
            return;
        
        Motor.ForceUnground();

        // Add to the return velocity and reset jump state
        currentVelocity += Motor.CharacterUp * _jumpForce - Vector3.Project(currentVelocity, Motor.CharacterUp);
        _jumpRequested = false;
        _currentJumpCount++;
        _jumpedThisFrame = true;
    }
    
    void EvaluateAfterJumpStatus (float deltaTime)
    {
        // Handle jump-related values
        if (_coyoteJumped && _currentJumpCount > 0)
            _coyoteJumped = false;

        if (!_canCoyoteJump && _currentJumpCount == 0 && !_isOnClimbableWall)
            _currentJumpCount++;

        // Handle jumping pre-ground grace period
        if (_jumpRequested && _timeSinceJumpRequested > jumpPreGroundingGraceTime)
            _jumpRequested = false;

        // Handle jumping while sliding
        if (allowJumpingWhenSliding ? !Motor.GroundingStatus.FoundAnyGround : !Motor.GroundingStatus.IsStableOnGround)
        {
            // Keep track of time since we were last able to jump (for grace period)
            _timeSinceLastAbleToJump += deltaTime;
            return;
        }
        
        // If we're on a ground surface, reset jumping values
        if (!_jumpedThisFrame)
            _currentJumpCount = 0;

        _timeSinceLastAbleToJump = 0f;
    }
    
    void EvaluateAfterWallClimbStatus (float deltaTime)
    {
        // Collider[] wallOverlap = Physics.OverlapSphere(
        //     wallOverlapTransform.position,
        //     wallOverlapRadius,
        //     wallOverlapLayer,
        //     QueryTriggerInteraction.Ignore
        // );
        //
        // if (wallOverlap.Length > 0)
        // {
        //     if (CanWallClimb && allowWallJump && !Motor.GroundingStatus.IsStableOnGround)
        //     {
        //         _lastClimbableWallCollider = wallOverlap[0];
        //         Vector3 closestPoint = _lastClimbableWallCollider.ClosestPoint(transform.position);
        //         Vector3 normal = (transform.position - closestPoint).normalized;
        //
        //         if (_lastClimbableWall == null || _lastClimbableWall.gameObject != _lastClimbableWallCollider.gameObject)
        //             _lastClimbableWall = _lastClimbableWallCollider.GetComponent<ClimbableWall>();
        //         
        //         if (!_lastClimbableWall.IsSurfaceClimbable(normal))
        //         {
        //             _isOnClimbableWall = false;
        //             return;
        //         }
        //
        //         if (wallClimbBasedOnMovement && _moveInputVector.sqrMagnitude <= 0f)
        //         {
        //             _isOnClimbableWall = false;
        //             return;
        //         }
        //
        //         _isOnClimbableWall = true;
        //         _wallClimbNormal = normal;
        //         _currentJumpCount = 0;
        //         _timeSinceLastAbleToJump = 0f;
        //
        //         if (Motor.Velocity.y < 0)
        //             _gravity = wallClimbGravity;
        //     }
        //     else
        //         _isOnClimbableWall = false;
        // }
        // else
        //     _isOnClimbableWall = false;
    }
    
    void EvaluateNewGroundingStatus (float deltaTime)
    {
        if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
            HandleLanded();
        else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
            HandleLeftStableGround();
        
        void HandleLanded ()
        {
        }

        void HandleLeftStableGround ()
        {
        }
    }
}