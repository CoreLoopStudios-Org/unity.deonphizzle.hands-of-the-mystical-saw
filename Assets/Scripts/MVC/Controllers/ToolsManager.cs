using UnityEngine;
using TMPro;

public class ToolsManager : MonoBehaviour
{
    [Header("Top Bar UI")]
    public TextMeshProUGUI pointsText; // 🌟 DataManager থেকে পাওয়া মোট পয়েন্ট এখানে বসবে

    private void OnEnable()
    {
        // প্যানেলটি ওপেন হলেই পয়েন্ট আপডেট হয়ে যাবে
        UpdatePointsUI();
    }

    public void UpdatePointsUI()
    {
        // DataManager আছে কি না তা চেক করে নেওয়া
        if (DataManager.Instance == null) return;

        // 🌟 DataManager থেকে পয়েন্ট এনে Tools Panel-এর টপ বারে বসানো
        if (pointsText != null) 
        {
            // তুমি চাইলে কমা সহ দেখাতে চাইলে ToString("N0") দিতে পারো, অথবা শুধু ToString()
            pointsText.text = DataManager.Instance.totalPoints.ToString(); 
        }
    }
}