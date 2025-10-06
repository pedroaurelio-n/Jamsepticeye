using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PedroAurelio.AudioSystem;
using UnityEngine;
using Random = UnityEngine.Random;

public class CeilingFanController : MonoBehaviour
{
    [SerializeField] Vector2 speedRange;
    [SerializeField] Transform fanTransform;
    [SerializeField] GameObject[] lightObjects;
    [SerializeField] PlayAudioEvent paranoiaAudio;
    
    [Header("Flicker Settings")]
    [SerializeField] Vector2 flickerIntervalRange = new(0.05f, 0.3f);
    [SerializeField] Vector2 flickerDurationRange = new (2f, 5f);

    Vector3 _initialRotation;
    TweenerCore<Quaternion, Vector3, QuaternionOptions> _rotate;

    void Awake ()
    {
        _initialRotation = fanTransform.localEulerAngles;
        RandomRotate();
    }

    void RandomRotate ()
    {
        if (_rotate.IsActive())
            _rotate.Kill();
        
        float randomSpeed = Random.Range(speedRange.x, speedRange.y);
        _rotate = fanTransform.DOLocalRotate(new Vector3(_initialRotation.x, 359f, _initialRotation.z), randomSpeed)
            .SetLoops(-1, LoopType.Incremental).SetSpeedBased();
    }

    public void TriggerParanoia ()
    {
        StartCoroutine(ParanoiaRoutine());
    }

    IEnumerator ParanoiaRoutine ()
    {
        paranoiaAudio.PlayAudio();
        RandomRotate();
        
        float paranoiaDuration = Random.Range(flickerDurationRange.x, flickerDurationRange.y);
        float elapsed = 0f;

        while (elapsed < paranoiaDuration)
        {
            bool enable = Random.value > 0.5f;

            foreach (var lightObj in lightObjects)
            {
                if (lightObj != null)
                    lightObj.SetActive(enable);
            }

            float waitTime = Random.Range(flickerIntervalRange.x, flickerIntervalRange.y);
            yield return new WaitForSeconds(waitTime);

            elapsed += waitTime;
            
            if (elapsed >= paranoiaDuration * 0.7f)
                paranoiaAudio.StopAudio();
        }
        
        foreach (var lightObj in lightObjects)
        {
            if (lightObj != null)
                lightObj.SetActive(true);
        }
    }
}
