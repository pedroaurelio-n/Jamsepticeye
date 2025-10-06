using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParanoiaManager : MonoBehaviour
{
    public static ParanoiaManager Instance;

    [SerializeField] float startingParanoia = 0f;
    [SerializeField] float paranoiaCap = 0.2f;
    [SerializeField] float paranoiaIncrease = 0.01f;
    [SerializeField] public int paranoiaStep = 150;
    [SerializeField] SpriteRenderer[] bloodSprites;
    [SerializeField] CeilingFanController ceilingFan;

    float _previousParanoia;
    float _currentParanoia;

    void Awake ()
    {
        Instance = this;
    }

    void Start ()
    {
        _currentParanoia = startingParanoia;
    }

    public bool TryTriggerParanoia ()
    {
        if (GameManager.Instance.Finished)
            return false;
        return Random.value < _currentParanoia;
    }

    public void UpdateParanoia (int totalDeaths)
    {
        if (GameManager.Instance.Finished)
            return;
        
        _previousParanoia = _currentParanoia;
        int paranoiaLevel = totalDeaths / paranoiaStep;
        _currentParanoia = Mathf.Min(paranoiaLevel * paranoiaIncrease, paranoiaCap);

        if (_previousParanoia < _currentParanoia)
        {
            ceilingFan.TriggerParanoia();
            foreach (SpriteRenderer sprite in bloodSprites)
            {
                float alpha = sprite.color.a;
                alpha += 0.1f;
                sprite.color = new Color(1, 1, 1, alpha);
            }
        }
    }
}