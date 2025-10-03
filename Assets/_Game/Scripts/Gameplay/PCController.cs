using UnityEngine;

public class PCController : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] MeshRenderer screenRenderer;
    [SerializeField] Material screenMaterialTemplate;
    [SerializeField] MinigameManager minigamePrefab;
    [SerializeField] Camera focusCamera;
    [SerializeField] PCUpgrades upgrades;
    [SerializeField] ShopPanelUI shopPanelUI;
    
    public int LocalCredits { get; private set; }

    MinigameManager _minigameInstance;
    RenderTexture _renderTexture;

    bool _isBeingInteracted;

    void Start ()
    {
        _renderTexture = new RenderTexture(128, 128, 16);
        _renderTexture.Create();
        
        _minigameInstance = Instantiate(minigamePrefab, Vector3.zero, Quaternion.identity);
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
        
        shopPanelUI.Setup(upgrades);
        shopPanelUI.gameObject.SetActive(true);
        shopPanelUI.Setup(upgrades);
    }

    public void AddCredits (int amount)
    {
        LocalCredits += amount;
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
}