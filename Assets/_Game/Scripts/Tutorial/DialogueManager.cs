using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PedroAurelio.AudioSystem;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI dialogueCenterText;
    [SerializeField] float typingSpeed = 0.03f;
    [SerializeField] PlayAudioEvent typingAudio;

    Coroutine typingCoroutine;

    void Awake ()
    {
        Instance = this;
    }

    public void ShowDialogue (string message)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(message, dialogueText));
    }
    
    public void ShowDialogueCenter (string message)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(message, dialogueCenterText));
    }

    IEnumerator TypeText (string message, TextMeshProUGUI text)
    {
        text.gameObject.SetActive(true);
        
        text.text = message.ToUpper();
        text.maxVisibleCharacters = 0;
        foreach (char c in message)
        {
            text.maxVisibleCharacters++;
            typingAudio.PlayAudio();
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(3.5f);
        dialogueText.gameObject.SetActive(false);
    }
}