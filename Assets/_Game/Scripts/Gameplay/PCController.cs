using UnityEngine;

public class PCController : MonoBehaviour, IInteractable
{
    [SerializeField] bool starting;
    [SerializeField] int BaseCreditsStorage;
    
    [Header("References")]
    [SerializeField] MeshRenderer screenRenderer;
    [SerializeField] Material screenMaterialTemplate;
    [SerializeField] MinigameManager minigamePrefab;
    [SerializeField] Camera focusCamera;
    [SerializeField] PCUpgrades upgrades;
    [SerializeField] ShopPanelUI shopPanelUI;
    
    public int LocalCredits { get; private set; }
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
        
        _renderTexture = new RenderTexture(128, 128, 16);
        _renderTexture.Create();

        Vector3 offset = GetMinigameOffset();
        
        _minigameInstance = Instantiate(minigamePrefab, offset, Quaternion.identity);
        _minigameInstance.gameObject.SetActive(true);
        _minigameInstance.Setup(this);
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
        
        GameManager.Instance.SetPlayerState(true);
        focusCamera.gameObject.SetActive(true);
        _minigameInstance.SetControlledState(true);
        _isBeingInteracted = true;
        Cursor.lockState = CursorLockMode.Confined;
        
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
        if (LocalCredits + amount >= BaseCreditsStorage + _creditsStorageUpgrade)
            return false;
        
        LocalCredits += amount;
        LocalCredits = Mathf.Clamp(LocalCredits, 0, BaseCreditsStorage + _creditsStorageUpgrade);
        return true;
    }

    public void UpgradeCreditsStorage (int amount)
    {
        _creditsStorageUpgrade += amount;
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
        
        GameManager.Instance.TryModifyCredits(RedeemCredits());
        _minigameInstance.Setup(this);
        GameManager.Instance.SetPlayerState(false);
        focusCamera.gameObject.SetActive(false);
        _minigameInstance.SetControlledState(false);
        _isBeingInteracted = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        shopPanelUI.gameObject.SetActive(false);
    }

    void HandleUpgradeBought ()
    {
        if (_minigameInstance == null)
            return;
        _minigameInstance.UpdatePlayer();
        shopPanelUI.Setup(upgrades);
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