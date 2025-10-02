using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] GameObject interactIndicator;
    [SerializeField] GameObject exitIndicator;

    public void UpdateCredits (int value) => creditsText.text = value.ToString();

    public void SetInteractIndicator (bool value) => interactIndicator.SetActive(value);
    
    public void SetExitIndicator (bool value) => exitIndicator.SetActive(value);
}
