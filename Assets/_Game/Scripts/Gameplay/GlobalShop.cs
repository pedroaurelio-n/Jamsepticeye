using PedroAurelio.AudioSystem;
using UnityEngine;

public class GlobalShop : MonoBehaviour, IInteractable
{
    [SerializeField] GlobalShopPanelUI shopPanelUI;
    [SerializeField] Camera focusCamera;
    [SerializeField] PlayAudioEvent turnOnAudio;
    [SerializeField] PlayAudioEvent turnOffAudio;
    
    bool _isBeingInteracted;

    void Start ()
    {
        shopPanelUI.OnUpgradeBought += HandleUpgradeBought;
    }

    public void Interact ()
    {
        if (_isBeingInteracted)
            return;
        
        turnOnAudio.PlayAudio();
        GameManager.Instance.SetPlayerState(true);
        focusCamera.gameObject.SetActive(true);
        _isBeingInteracted = true;
        Cursor.lockState = CursorLockMode.Confined;
        
        shopPanelUI.gameObject.SetActive(true);
        shopPanelUI.Setup();
        Invoke(nameof(LateSetup), 0.1f);
    }

    void LateSetup ()
    {
        shopPanelUI.gameObject.SetActive(false);
        shopPanelUI.gameObject.SetActive(true);
    }

    public void Exit ()
    {
        if (!_isBeingInteracted)
            return;
        
        turnOffAudio.PlayAudio();
        GameManager.Instance.SetPlayerState(false);
        focusCamera.gameObject.SetActive(false);
        _isBeingInteracted = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        shopPanelUI.gameObject.SetActive(false);
    }
    
    void HandleUpgradeBought ()
    {
        shopPanelUI.Setup();
    }
}