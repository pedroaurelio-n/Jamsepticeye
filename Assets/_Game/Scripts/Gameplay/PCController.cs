using PedroAurelio.AudioSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class PCController : MonoBehaviour, IInteractable
{
    [SerializeField] bool starting;
    [SerializeField] int baseCreditsStorage;
    
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] MeshRenderer screenRenderer;
    [SerializeField] Material screenMaterialTemplate;
    [SerializeField] MinigameManager minigamePrefab;
    [SerializeField] Camera focusCamera;
    [SerializeField] PCUpgrades upgrades;
    [SerializeField] ShopPanelUI shopPanelUI;
    [SerializeField] PlayAudioEvent turnOffAudio;
    [SerializeField] PlayAudioEvent turnOnAudio;
    
    public bool Starting => starting;
    public int LocalCredits { get; private set; }
    public int Storage => baseCreditsStorage + _creditsStorageUpgrade;
    public bool Initialized { get; private set; }
    public int Index => _pcIndex;

    MinigameManager _minigameInstance;
    RenderTexture _renderTexture;

    bool _isBeingInteracted;
    int _pcIndex = 0;
    int _creditsStorageUpgrade;

    public void Initialize (int index = 0)
    {
        _pcIndex = index;
        
        _renderTexture = new RenderTexture(192, 192, 16);
        _renderTexture.Create();

        Vector3 offset = GetMinigameOffset();
        
        _minigameInstance = Instantiate(minigamePrefab, offset, Quaternion.identity);
        _minigameInstance.gameObject.SetActive(true);
        _minigameInstance.Setup(this, true);
        upgrades.Setup(_minigameInstance);
        shopPanelUI.OnUpgradeBought += HandleUpgradeBought;
        
        Camera miniCam = _minigameInstance.GetComponentInChildren<Camera>();
        if (miniCam != null)
        {
            miniCam.targetTexture = _renderTexture;
        }
        
        Material screenMat = new(screenMaterialTemplate)
        {
            mainTexture = _renderTexture
        };
        screenRenderer.material = screenMat;
        
        Initialized = true;
    }

    void Start ()
    {
        if (starting)
            Initialize();
    }

    public void Interact ()
    {
        if (_isBeingInteracted)
            return;
        
        if (!TutorialSequence2.Instance.IsCompleted)
            TutorialSequence2.Instance.Trigger();
        
        turnOnAudio.PlayAudio();
        GameManager.Instance.TryModifyCredits(RedeemCredits());
        animator.SetBool("IsOpen", true);
        GameManager.Instance.SetPlayerState(true);
        focusCamera.gameObject.SetActive(true);
        _minigameInstance.SetControlledState(true);
        _isBeingInteracted = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void ActivateShopCanvas ()
    {
        if (!TutorialSequence2.Instance.IsCompleted)
            return;
        shopPanelUI.gameObject.SetActive(true);
        shopPanelUI.Setup(upgrades);
        Invoke(nameof(LateSetup), 0.1f);
    }

    void LateSetup ()
    {
        shopPanelUI.gameObject.SetActive(false);
        shopPanelUI.gameObject.SetActive(true);
    }

    public bool AddCredits (int amount)
    {
        if (_minigameInstance.IsControlled)
        {
            if (!TutorialSequence3.Instance.IsCompleted)
            {
                TutorialSequence3.Instance.Trigger();
                ActivateShopCanvas();
            }
            GameManager.Instance.TryModifyCredits(amount);
            shopPanelUI.Setup(upgrades);
            return true;
        }
        
        if (LocalCredits + amount >= baseCreditsStorage + _creditsStorageUpgrade)
            return false;
        
        LocalCredits += amount;
        LocalCredits = Mathf.Clamp(LocalCredits, 0, baseCreditsStorage + _creditsStorageUpgrade);
        return true;
    }

    public void UpgradeCreditsStorage (int amount)
    {
        _creditsStorageUpgrade += amount;
        _minigameInstance.UpdateHud();
    }

    public int RedeemCredits ()
    {
        int redeemed = LocalCredits;
        LocalCredits = 0;
        return redeemed;
    }

    public void Exit ()
    {
        if (!_isBeingInteracted)
            return;

        if (!TutorialSequence4.Instance.IsCompleted)
            return;
        
        turnOffAudio.PlayAudio();
        
        _minigameInstance.Setup(this);
        GameManager.Instance.SetPlayerState(false);
        focusCamera.gameObject.SetActive(false);
        _minigameInstance.SetControlledState(false);
        _isBeingInteracted = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        animator.SetBool("IsOpen", false);
        shopPanelUI.gameObject.SetActive(false);
    }

    void HandleUpgradeBought ()
    {
        if (_minigameInstance == null)
            return;
        
        if (!TutorialSequence4.Instance.IsCompleted)
        {
            TutorialSequence4.Instance.Trigger();
            ActivateShopCanvas();
        }
        
        _minigameInstance.UpdatePlayer();
        ActivateShopCanvas();
    }

    void OnDestroy ()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
        if (_minigameInstance != null)
        {
            Destroy(_minigameInstance);
        }
    }
    
    Vector3 GetMinigameOffset ()
    {
        Vector3 offset = new(_pcIndex * 15f, 0, 0);
        return offset;
    }
}