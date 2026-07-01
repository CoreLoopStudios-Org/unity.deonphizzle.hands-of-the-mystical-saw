using UnityEngine;
using TMPro;

public class ToolsManager : MonoBehaviour
{
    [Header("Top Bar UI")]
    public TextMeshProUGUI pointsText; // 🌟 Total points from DataManager will sit here

    private void OnEnable()
    {
        // Points will be updated only when the panel is open
        UpdatePointsUI();
    }

    public void UpdatePointsUI()
    {
        // Check if DataManager exists
        if (DataManager.Instance == null) return;

        // 🌟 Bringing points from DataManager and placing them on the top bar of the Tools Panel
        if (pointsText != null) 
        {
            // You can return ToString("N0") if you want to display with commas, or just ToString()
            pointsText.text = DataManager.Instance.totalPoints.ToString(); 
        }
    }
}