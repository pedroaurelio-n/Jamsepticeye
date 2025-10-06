using System;
using UnityEngine;

public class TutorialSequence1 : MonoBehaviour
{
    void Start ()
    {
        DialogueManager.Instance.ShowDialogue($"Use WASD to reach your workstation.\nObedience is key towards clarity.");
    }
}
