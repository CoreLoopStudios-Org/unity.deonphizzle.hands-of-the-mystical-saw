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

    // এই ফাংশনটি দিয়ে আমরা শপ থেকে ডাটা পাঠাবো
    public void SetupTool(string name, Sprite icon, int price)
    {
        toolNameText.text = name;
        toolIcon.sprite = icon;
        priceText.text = price.ToString("N0"); // কমা সহ দাম দেখাবে (যেমন 12,500)
    }

    public void OnUpgradeClicked()
    {
        Debug.Log($"Upgrade clicked for {toolNameText.text}!");
        // এখানে আপগ্রেডের লজিক বসবে
    }
}