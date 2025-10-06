using UnityEngine;

public class TutorialSequence4 : MonoBehaviour
{
    public static TutorialSequence4 Instance;
    public bool IsCompleted { get; private set; }
    
    [SerializeField] BoxCollider interactableCollider;
    [SerializeField] BoxCollider globalShopCollider;

    void Awake ()
    {
        Instance = this;
    }
    
    public void Trigger ()
    {
        interactableCollider.enabled = true;
        globalShopCollider.enabled = true;
        DialogueManager.Instance.ShowDialogue($"Exit the station and move towards the opposite side.\nCollect enough credits to achieve global optimization.");
        Invoke(nameof(Complete), 1f);
    }
    
    void Complete () => IsCompleted = true;
}