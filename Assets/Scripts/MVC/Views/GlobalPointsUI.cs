using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GlobalPointsUI : MonoBehaviour
{
    private TextMeshProUGUI myText;

    private void Awake()
    {
        // নিজে থেকেই TextMeshPro কম্পোনেন্টটা খুঁজে নেবে
        myText = GetComponent<TextMeshProUGUI>(); 
    }

    private void OnEnable()
    {
        UpdatePointDisplay();
        
        // DataManager এর সাথে কানেকশন (পয়েন্ট চেঞ্জ হলেই আপডেট হবে)
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