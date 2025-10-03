using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] GameObject interactIndicator;

    public void UpdateCredits (int value) => creditsText.text = value.ToString();

    public void SetInteractIndicator (bool value) => interactIndicator.SetActive(value);
}
