using UnityEngine;
using UnityEngine.Serialization;

public class MinigameManager : MonoBehaviour
{
    [field: SerializeField] public float PlayerBaseSpeed { get; private set; }
    [field: SerializeField] public float PlayerBaseSpawnDelay { get; private set; }
    
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

    void SpawnPlayer ()
    {
        _currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity, transform);
        _currentPlayer.Init(this, PlayerBaseSpeed + _speedAddUpgrade, AutoMove);
    }
    
    public void OnPlayerDeath (int spikeValue)
    {
        deathParticles.transform.position = _currentPlayer.transform.position;
        deathParticles.Play();
        _currentPC.AddCredits(spikeValue);
        Destroy(_currentPlayer.gameObject);
        
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
}