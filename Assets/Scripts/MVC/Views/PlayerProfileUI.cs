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

    // যখনই প্রোফাইল সিন ওপেন হবে, এই ফাংশন লেটেস্ট পয়েন্ট টেনে আনবে
    public void UpdateProfileUI()
    {
        // মেমোরি থেকে সেভ করা পয়েন্ট টেনে আনা (আগে না থাকলে 0 দেখাবে)
        int totalPoints = PlayerPrefs.GetInt("PlayerTotalPoints", 0);

        if (totalPointsText != null)
        {
            totalPointsText.text = totalPoints.ToString("N0"); // কমা দিয়ে সুন্দর করে দেখাবে, যেমন: 10,500
        }
    }
}