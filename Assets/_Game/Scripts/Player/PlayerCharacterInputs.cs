using UnityEngine;

public struct PlayerCharacterInputs
{
    public float MoveForwardAxis { get; set; }
    public float MoveRightAxis { get; set; }
    public bool JumpPressed { get; set; }
    public Quaternion LookRotation { get; set; }
}