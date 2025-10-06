using System.Collections;
using PedroAurelio.AudioSystem;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] CinemachineCamera cinemachineCamera;
    [SerializeField] CinemachineCameraOffset cinemachineCameraOffset;
    [SerializeField] CinemachineInputAxisController cinemachineInputAxisController;
    [SerializeField] PlayAudioEvent footstepAudio;

    [Header("Movement Tilt Settings")]
    [SerializeField] float movementTiltAngle = 3f;
    [SerializeField] float movementTiltSpeed = 0.1f;
    [SerializeField] float wallClimbTiltAngle = 6f;
    [SerializeField, Range(0f, 1f)] float wallClimbMinValue = 0.1f;
    [SerializeField] float wallClimbTiltSpeed = 0.3f;

    [Header("Head Bobbing Settings")]
    [SerializeField] bool offsetHorizontalHeadMovement;
    [SerializeField, Range(0, .2f)] float headBobVerticalAmplitude = .05f;
    [SerializeField, Range(0, .2f)] float headBobHorizontalAmplitude = .075f;
    [SerializeField, Range(5f, 25f)] float headBobFrequency = 15f;
    [SerializeField, Range(0f, 2f)] float headBobSpeed = 0.3f;
    [SerializeField, Range(0f, 0.5f)] float headBobResetSpeed = 0.3f;

    [Header("Dash Settings")]
    [SerializeField] float dashFovMultiplier = 2f;
    [SerializeField] float dashFovDuration = 1f;
    [SerializeField, Range(0f, 1f)] float dashFovIncreaseRatio = 0.2f;
    
    public Quaternion LookRotation => transform.rotation;

    Player _player;
    PlayerCharacterController _characterController;

    float _startFov;
    Coroutine _applyFovMultiplierRoutine;

    float _headBobbingTimer;
    bool _hasPlayedFootstep;

    void Awake ()
    {
        cinemachineCamera.enabled = false;
        cinemachineCamera.transform.parent = null;

        _startFov = cinemachineCamera.Lens.FieldOfView;
        
        _player = GetComponentInParent<Player>();
        _characterController = _player.CharacterController;
    }

    void LateUpdate ()
    {
        if (!_player.CanMove)
            return;
        ApplyMovementTilt();
        ApplyHeadBobbing();
    }

    public void Initialize ()
    {
        cinemachineCamera.enabled = true;
    }

    public void SetCameraMovementActive (bool active)
    {
        cinemachineInputAxisController.enabled = active;
    }

    public void ApplyDashFovMultiplier ()
    {
        if (_applyFovMultiplierRoutine != null)
        {
            StopCoroutine(_applyFovMultiplierRoutine);
            _applyFovMultiplierRoutine = null;
        }
        
        _applyFovMultiplierRoutine = StartCoroutine(ApplyFovMultiplierRoutine());
    }

    void ApplyMovementTilt ()
    {
        float targetAngle;
        float speed;

        if (_characterController.IsOnClimbableWall && _characterController.MoveInputVector.sqrMagnitude > 0f)
        {
            float wallSideDot = Vector3.Dot(_characterController.WallClimbNormal, transform.right);
            
            float dotValue = Mathf.Abs(wallSideDot) >= wallClimbMinValue ? -Mathf.Sign(wallSideDot) : 0f;
            targetAngle = dotValue * wallClimbTiltAngle;
            speed = wallClimbTiltSpeed;
        }
        else
        {
            targetAngle = -(_characterController.RawInputVector.x * movementTiltAngle);
            speed = cinemachineCamera.Lens.Dutch > movementTiltAngle ? wallClimbTiltSpeed : movementTiltSpeed;
        }
        
        cinemachineCamera.Lens.Dutch = Mathf.MoveTowards(
            cinemachineCamera.Lens.Dutch,
            targetAngle,
            speed * Time.deltaTime
        );
    }

    void ApplyHeadBobbing ()
    {
        // footstepAudio.PlayAudio();
        if (!_characterController.IsGrounded)
        {
            ResetHeadBobbingPosition();
            return;
        }
        
        if (_characterController.RawInputVector == Vector2.zero)
        {
            ResetHeadBobbingPosition();
            return;
        }

        _headBobbingTimer += Time.deltaTime;
        float horizontalOffset = offsetHorizontalHeadMovement ? headBobFrequency * 0.5f : 0;

        float sinValue = Mathf.Sin(_headBobbingTimer * headBobFrequency);
        float targetYOffset = sinValue * headBobVerticalAmplitude;
        float targetXOffset = Mathf.Sin((_headBobbingTimer + horizontalOffset) * headBobFrequency * 0.5f) *
                              headBobHorizontalAmplitude;
        
        if (sinValue <= -0.98f && !_hasPlayedFootstep)
        {
            footstepAudio.PlayAudio();
            _hasPlayedFootstep = true;
        }
        else if (sinValue > 0f)
            _hasPlayedFootstep = false;
        
        cinemachineCameraOffset.Offset.y = Mathf.MoveTowards(
            cinemachineCameraOffset.Offset.y,
            targetYOffset,
            headBobSpeed * Time.deltaTime
        );
        cinemachineCameraOffset.Offset.x = Mathf.MoveTowards(
            cinemachineCameraOffset.Offset.x,
            targetXOffset,
            headBobSpeed * Time.deltaTime
        );

        void ResetHeadBobbingPosition ()
        {
            cinemachineCameraOffset.Offset.y = Mathf.MoveTowards(
                cinemachineCameraOffset.Offset.y,
                0f,
                headBobResetSpeed * Time.deltaTime
            );
            cinemachineCameraOffset.Offset.x = Mathf.MoveTowards(
                cinemachineCameraOffset.Offset.x,
                0f,
                headBobResetSpeed * Time.deltaTime
            );
            
            if (cinemachineCameraOffset.Offset.y < Mathf.Epsilon && cinemachineCameraOffset.Offset.x < Mathf.Epsilon)
                _headBobbingTimer = 0f;
        }
    }
    
    IEnumerator ApplyFovMultiplierRoutine ()
    {
        float timer = 0f;
        float modifiedFov = _startFov * dashFovMultiplier;
        float startSection = dashFovDuration * dashFovIncreaseRatio;
        
        while (timer < startSection)
        {
            timer += Time.deltaTime;
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(
                _startFov,
                modifiedFov,
                timer / startSection
            );
            yield return null;
        }

        cinemachineCamera.Lens.FieldOfView = modifiedFov;
        timer = startSection;

        while (timer < dashFovDuration)
        {
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(
                modifiedFov,
                _startFov,
                (timer - startSection) / (dashFovDuration - startSection)
            );
            timer += Time.deltaTime;
            yield return null;
        }

        cinemachineCamera.Lens.FieldOfView = _startFov;
        _applyFovMultiplierRoutine = null;
    }
}