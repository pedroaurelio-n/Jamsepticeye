using PedroAurelio.AudioSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class MinigameManager : MonoBehaviour
{
    [field: SerializeField] public float PlayerBaseSpeed { get; private set; }
    [field: SerializeField] public float PlayerBaseSpawnDelay { get; private set; }
    [field: SerializeField] public float PlayerBaseDeathValue { get; private set; }
    
    [Header("References")]
    [SerializeField] MinigameController playerPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Spike[] spikes;
    [SerializeField] MinigameHud minigameHud;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] PlayAudioEvent deathAudio;
    [SerializeField] PlayAudioEvent screamAudio;
    [SerializeField] PlayAudioEvent jumpscareAudio;
    [SerializeField] PlayAudioEvent spawnAudio;
    [SerializeField] Animator animator;
    [SerializeField] GameObject jumpscareSprite;
    
    [Header("Shake Settings")]
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeMagnitude = 0.3f;
    [SerializeField] float dampingSpeed = 1.0f;
    [SerializeField] Camera minigameCamera;

    Vector3 _initialPosition;
    float _currentShakeDuration;
    float _shakeMagnitude;

    public bool IsControlled { get; private set; }
    public bool AutoMove { get; private set; }
    public int PCIndex => _currentPC.Index;

    MinigameController _currentPlayer;
    PCController _currentPC;

    float _speedAddUpgrade;
    float _spawnDelayDecreaseUpgrade;
    float _deathValueAddUpgrade;

    int _counter;

    void Start ()
    {
        SpawnPlayer();
        _initialPosition = minigameCamera.transform.localPosition;
    }

    public void Setup (PCController pcController, bool initialize = false)
    {
        if (GameManager.Instance.Finished)
            return;
        _currentPC = pcController;
        UpdateHud();
    }

    void TryTriggerJumpscare ()
    {
        if (!IsControlled)
            return;
        
        if (ParanoiaManager.Instance.TryTriggerParanoia())
        {
            jumpscareSprite.transform.position = _currentPlayer.transform.position;
            jumpscareAudio.PlayAudio();
            animator.SetTrigger("Jumpscare");
        }
    }

    public void SetControlledState (bool value)
    {
        IsControlled = value;
        UpdateHud();
        TryTriggerJumpscare();
    }

    public void UpdateHud ()
    {
        minigameHud.UpdateLocalCredits(_currentPC.LocalCredits, _currentPC.Storage, !IsControlled && AutoMove);
    }
    
    void Update()
    {
        if (_currentShakeDuration > 0)
        {
            minigameCamera.transform.localPosition = _initialPosition + Random.insideUnitSphere * _shakeMagnitude;
            _currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            _currentShakeDuration = 0f;
            minigameCamera.transform.localPosition = _initialPosition;
        }
    }

    public void UpdatePlayer ()
    {
        if (_currentPlayer == null)
            return;
        _currentPlayer.Init(this, PlayerBaseSpeed + _speedAddUpgrade, AutoMove);
    }

    void SpawnPlayer ()
    {
        if (GameManager.Instance.Finished)
            return;
        
        _currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity, transform);
        _currentPlayer.Init(this, PlayerBaseSpeed + _speedAddUpgrade, AutoMove);
        
        if (IsControlled)
        {
            spawnAudio.PlayAudio();
            
            if (Random.value > 0.5f)
                TryTriggerJumpscare();
        }
    }
    
    public void OnPlayerDeath (float spikeValue)
    {
        GameManager.Instance.IncreaseDeaths();
        screamAudio.SetObjectToFollow(_currentPC.transform);
        deathAudio.SetObjectToFollow(_currentPC.transform);
        
        if (_currentPC.Starting && _counter == 0)
            screamAudio.PlayAudio();
        else if (_currentPC.Starting && _counter < 5)
        {
            screamAudio.PlayAudio();
            deathAudio.PlayAudio();
        }
        else
        {
            if (ParanoiaManager.Instance.TryTriggerParanoia())
                screamAudio.PlayAudio();
            else
                deathAudio.PlayAudio();
        }

        _counter++;
        
        deathParticles.transform.position = _currentPlayer.transform.position;
        deathParticles.Play();

        float value = (PlayerBaseDeathValue + _deathValueAddUpgrade) * spikeValue;
        if (IsControlled && GameManager.Instance.GlobalUpgradeManager.ManualPlayMultiplier > 1f)
            value *= GameManager.Instance.GlobalUpgradeManager.ManualPlayMultiplier;
        _currentPC.AddCredits(Mathf.RoundToInt(value));
        
        Destroy(_currentPlayer.gameObject);
        _currentPlayer = null;
        
        UpdateHud();
        minigameHud.StartCountdown(PlayerBaseSpawnDelay - _spawnDelayDecreaseUpgrade, SpawnPlayer);
        
        if (IsControlled)
            ShakeScreen();
    }
    
    public void UpgradeSpeed (float amount)
    {
        _speedAddUpgrade += amount;
    }
    
    public void UpgradeSpawnDelay (float amount)
    {
        _spawnDelayDecreaseUpgrade += amount;
    }

    public void EnableAutoMove ()
    {
        AutoMove = true;
    }
    
    public void UpgradeDeathValue (float amount)
    {
        _deathValueAddUpgrade += amount;
    }

    public void UpgradeCreditsStorage (float amount)
    {
        _currentPC.UpgradeCreditsStorage((int)amount);
    }

    void ShakeScreen ()
    {
        _currentShakeDuration = 0.25f;
        _shakeMagnitude = 0.15f;
    }
}