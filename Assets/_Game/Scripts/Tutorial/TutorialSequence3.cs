using UnityEngine;

public class TutorialSequence3 : MonoBehaviour
{
    public static TutorialSequence3 Instance;
    public bool IsCompleted { get; private set; }

    void Awake ()
    {
        Instance = this;
    }
    
    public void Trigger ()
    {
        DialogueManager.Instance.ShowDialogue($"Use harvested credits to enhance your productivity\nand continue your diligent work.");
        Invoke(nameof(Complete), 1f);
    }
    
    void Complete () => IsCompleted = true;
}