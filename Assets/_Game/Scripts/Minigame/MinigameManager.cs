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

    public bool IsControlled { get; private set; }
    public bool AutoMove { get; private set; }

    MinigameController _currentPlayer;
    PCController _currentPC;

    float _speedAddUpgrade;
    float _spawnDelayDecreaseUpgrade;
    float _deathValueAddUpgrade;

    void Start ()
    {
        SpawnPlayer();
    }

    public void Setup (PCController pcController)
    {
        _currentPC = pcController;
        minigameHud.UpdateLocalCredits(_currentPC.LocalCredits);
    }

    public void SetControlledState (bool value)
    {
        IsControlled = value;
    }

    public void UpdatePlayer ()
    {
        if (_currentPlayer == null)
            return;
        _currentPlayer.Init(this, PlayerBaseSpeed + _speedAddUpgrade, AutoMove);
    }

    void SpawnPlayer ()
    {
        _currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity, transform);
        _currentPlayer.Init(this, PlayerBaseSpeed + _speedAddUpgrade, AutoMove);
    }
    
    public void OnPlayerDeath (float spikeValue)
    {
        deathParticles.transform.position = _currentPlayer.transform.position;
        deathParticles.Play();
        _currentPC.AddCredits(Mathf.RoundToInt((PlayerBaseDeathValue + _deathValueAddUpgrade) * spikeValue));
        Destroy(_currentPlayer.gameObject);
        _currentPlayer = null;
        
        minigameHud.UpdateLocalCredits(_currentPC.LocalCredits);
        minigameHud.StartCountdown(PlayerBaseSpawnDelay - _spawnDelayDecreaseUpgrade, SpawnPlayer);
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
}