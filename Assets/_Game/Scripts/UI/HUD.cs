using System.Collections;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI creditsLabel;
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] TextMeshProUGUI interactText;

    Coroutine _waitCoroutine;
    bool _paranoid;

    public void UpdateCredits (int value)
    {
        if (_waitCoroutine != null)
        {
            if (_paranoid)
            {
                creditsLabel.text = "DEATHS:";
                creditsLabel.color = Color.red;
                creditsText.text = GameManager.Instance.GlobalDeaths.ToString();
                creditsText.color = Color.red;
            }
            else
            {
                creditsLabel.text = "CREDITS:";
                creditsLabel.color = Color.white;
                creditsText.text = value.ToString();
                creditsText.color = Color.white;
            }
            return;
        }
        
        bool paranoid = ParanoiaManager.Instance.TryTriggerParanoia();
        if (!paranoid)
        {
            creditsLabel.text = "CREDITS:";
            creditsLabel.color = Color.white;
            creditsText.text = value.ToString();
            creditsText.color = Color.white;
            return;
        }
        
        creditsLabel.text = "DEATHS:";
        creditsLabel.color = Color.red;
        creditsText.text = GameManager.Instance.GlobalDeaths.ToString();
        creditsText.color = Color.red;
        _waitCoroutine = StartCoroutine(WaitForNewParanoia());
    }

    public void SetInteractIndicator (bool value)
    {
        string text = ParanoiaManager.Instance.TryTriggerParanoia() ? "PRESS E TO CONTINUE YOUR JOB" : "PRESS E TO INTERACT";
        interactText.text = text;
        interactText.gameObject.SetActive(value);
    }

    IEnumerator WaitForNewParanoia ()
    {
        _paranoid = true;
        float timer = 0f;
        while (timer < 5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _paranoid = false;
        timer = 0f;
        while (timer < 40f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _waitCoroutine = null;
    }
}
