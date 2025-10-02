using UnityEngine;
using UnityEngine.Serialization;

public class MinigameManager : MonoBehaviour
{
    [field: SerializeField] public float PlayerBaseSpeed { get; private set; }
    
    [Header("References")]
    [SerializeField] MinigameController playerPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Spike[] spikes;
    [SerializeField] ParticleSystem deathParticles;

    public bool IsControlled { get; private set; }
    // public int Credits { get; private set; }
    public bool AutoMove { get; private set; }

    MinigameController _currentPlayer;

    void Start ()
    {
        SpawnPlayer();
    }

    public void SetControlledState (bool value)
    {
        IsControlled = value;
    }

    void SpawnPlayer ()
    {
        _currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity, transform);
        _currentPlayer.Init(this, PlayerBaseSpeed, AutoMove);
    }
    
    public void OnPlayerDeath (int spikeValue)
    {
        // Credits += spikeValue;
        deathParticles.transform.position = _currentPlayer.transform.position;
        deathParticles.Play();
        GameManager.Instance.TryModifyCredits(spikeValue);
        Destroy(_currentPlayer.gameObject);
        SpawnPlayer();
    }
    
    public void UpgradeSpeed (float amount)
    {
        PlayerBaseSpeed += amount;
    }

    public void EnableAutoMove ()
    {
        AutoMove = true;
    }
}