using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GlobalPointsUI : MonoBehaviour
{
    private TextMeshProUGUI myText;

    private void Awake()
    {
        // Automatically find the TextMeshPro component
        myText = GetComponent<TextMeshProUGUI>(); 
    }

    private void OnEnable()
    {
        UpdatePointDisplay();
        
        // Connection to DataManager (updates when point changes)
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnPointsUpdated += UpdatePointDisplay;
        }
    }

    private void OnDisable()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnPointsUpdated -= UpdatePointDisplay;
        }
    }

    private void UpdatePointDisplay()
    {
        if (DataManager.Instance != null && myText != null)
        {
            myText.text = DataManager.Instance.totalPoints.ToString("N0");
        }
    }
}