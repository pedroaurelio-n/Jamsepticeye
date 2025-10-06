using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MinigameHud : MonoBehaviour
{
    [SerializeField] TMP_Text countdownText;
    [SerializeField] TMP_Text localCreditsText;
    
    Coroutine _coroutine;

    public void UpdateLocalCredits (int value, int total, bool show)
    {
        if (!show)
        {
            localCreditsText.gameObject.SetActive(false);
            return;
        }
        
        localCreditsText.gameObject.SetActive(true);
        localCreditsText.text = $"$ {value}/{total}";
    }

    public void StartCountdown (float duration, Action callback)
    {
        if (_coroutine != null)
            return;

        if (duration <= 0)
            callback.Invoke();
        else
            _coroutine = StartCoroutine(CountdownRoutine(duration, callback));
    }

    IEnumerator CountdownRoutine (float duration, Action callback)
    {
        countdownText.gameObject.SetActive(true);
        
        float timer = duration;
        while (timer >= 0f)
        {
            countdownText.text = timer.ToString("F1");
            timer -= Time.deltaTime;
            yield return null;
        }
        
        countdownText.gameObject.SetActive(false);
        callback.Invoke();
        
        _coroutine = null;
    }
}