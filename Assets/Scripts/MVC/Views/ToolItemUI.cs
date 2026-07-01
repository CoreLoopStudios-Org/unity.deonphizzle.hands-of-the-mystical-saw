using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image toolIcon;
    public TextMeshProUGUI toolNameText;
    public TextMeshProUGUI priceText;
    public Slider upgradeSlider;
    public Button upgradeButton;

    // With this function we will send data from shop
    public void SetupTool(string name, Sprite icon, int price)
    {
        toolNameText.text = name;
        toolIcon.sprite = icon;
        priceText.text = price.ToString("N0"); // will show the price with commas (eg 12,500)
    }

    public void OnUpgradeClicked()
    {
        Debug.Log($"Upgrade clicked for {toolNameText.text}!");
        // This is where the upgrade logic sits
    }
}