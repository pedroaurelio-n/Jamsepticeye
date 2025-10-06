using System.Collections;
using PedroAurelio.AudioSystem;
using UnityEngine;

public class EndingSequence : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] GameObject creditsLabel;
    [SerializeField] GameObject creditsText;
    [SerializeField] Camera emptyCamera;
    [SerializeField] PlayAudioEvent static1;
    [SerializeField] PlayAudioEvent static2;
    [SerializeField] PlayAudioEvent finishAudio1;
    [SerializeField] PlayAudioEvent finishAudio2;
    [SerializeField] PlayAudioEvent finishAudio3;

    public void Trigger ()
    {
        StartCoroutine(EndingCoroutine());
    }

    IEnumerator EndingCoroutine ()
    {
        static1.StopAudio();
        static2.StopAudio();
        player.CanMove = false;
        player.CanAct = false;
        creditsLabel.SetActive(false);
        creditsText.SetActive(false);
        
        emptyCamera.gameObject.SetActive(true);
        finishAudio1.PlayAudio();
        
        DialogueManager.Instance.ShowDialogueCenter($"Thank you for your excellent service.\nYour contribution has been invaluable to the Program.");
        
        yield return new WaitForSeconds(6.5f);
        
        DialogueManager.Instance.ShowDialogueCenter($"Units condemned: {GameManager.Instance.GlobalDeaths}.\nSystem efficiency: above expected.\nReactor reaching high temperatures...");
        yield return new WaitForSeconds(3f);
        
        finishAudio1.StopAudio();
        finishAudio2.PlayAudio();
        yield return new WaitForSeconds(4f);

        DialogueManager.Instance.ShowDialogueCenter($"Salvation achieved.\nYour Happiness is no longer desired.");
        yield return new WaitForSeconds(5.5f);
        
        finishAudio2.StopAudio();
        finishAudio3.PlayAudio();
        yield return new WaitForSeconds(1f);

        DialogueManager.Instance.ShowDialogueCenter($"A game by pedroaurelio_n");
        yield return new WaitForSeconds(5f);
        
        Application.Quit();
    }
}