using UnityEngine;
using TMPro;

public class PlayerProfileUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI totalPointsText;

    void Start()
    {
        UpdateProfileUI();
    }

    // Whenever the profile scene is opened, this function will fetch the latest point
    public void UpdateProfileUI()
    {
        // pull saved point from memory (will show 0 if not already)
        int totalPoints = PlayerPrefs.GetInt("PlayerTotalPoints", 0);

        if (totalPointsText != null)
        {
            totalPointsText.text = totalPoints.ToString("N0"); // will show nicely with commas, eg: 10,500
        }
    }
}