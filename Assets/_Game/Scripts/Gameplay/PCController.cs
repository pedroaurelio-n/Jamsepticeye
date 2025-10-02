using UnityEngine;

public class PCController : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] MeshRenderer screenRenderer;
    [SerializeField] Material screenMaterialTemplate;
    [SerializeField] MinigameManager minigamePrefab;
    [SerializeField] Camera focusCamera;

    MinigameManager _minigameInstance;
    RenderTexture _renderTexture;

    void Start ()
    {
        _renderTexture = new RenderTexture(128, 128, 16);
        _renderTexture.Create();
        
        _minigameInstance = Instantiate(minigamePrefab, Vector3.zero, Quaternion.identity);
        _minigameInstance.gameObject.SetActive(true);
        
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
        GameManager.Instance.SetPlayerState(true);
        focusCamera.gameObject.SetActive(true);
        _minigameInstance.SetControlledState(true);
    }

    public void Exit ()
    {
        GameManager.Instance.SetPlayerState(false);
        focusCamera.gameObject.SetActive(false);
        _minigameInstance.SetControlledState(false);
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