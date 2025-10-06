using System;
using UnityEngine;

public class TutorialSequence2 : MonoBehaviour
{
    public static TutorialSequence2 Instance;
    public bool IsCompleted { get; private set; }

    void Awake ()
    {
        Instance = this;
    }

    public void Trigger ()
    {
        DialogueManager.Instance.ShowDialogue($"Direct the Subject toward its conclusion\nwith the same keys.");
        Invoke(nameof(Complete), 3.5f);
    }
    
    void Complete () => IsCompleted = true;
}