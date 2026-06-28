using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI userNameText;
    
    // 🌟 প্রোগ্রেস বারের পয়েন্ট (যেমন: 12897 / 20000)
    public TextMeshProUGUI pointsText; 
    
    // 🌟 মোট পয়েন্ট (যেমন: 45000) দেখানোর জন্য
    public TextMeshProUGUI totalPointsText; 
    
    public Image tierProgressFill; 
    
    public TextMeshProUGUI stonesPlayedText;
    public TextMeshProUGUI perfectText;
    public TextMeshProUGUI failureText;
    
    [Header("Tier Texts (Multiple Places)")]
    public TextMeshProUGUI[] allTierTexts; 
    
    [Header("Tier Badge Settings")]
    public Image tierBadgeImage; 
    public List<TierBadgeData> tierBadges; 

    private void OnEnable()
    {
        FetchProfileDataFromServer();
    }

    private void FetchProfileDataFromServer()
    {
        // 🌟 জাদুকরী লাইন: 
        // কাটিং সিন থেকে জেতা পয়েন্টটা মেমোরি থেকে টেনে আনবে। 
        // যদি মেমোরিতে কিছু না থাকে (ফার্স্ট টাইম প্লে), তাহলে তোমার DataManager এর ডিফল্ট 45000 নিয়ে নেবে।
        int latestTotalPoints = PlayerPrefs.GetInt("PlayerTotalPoints", DataManager.Instance.totalPoints);

        UserProfileData mockData = new UserProfileData
        {
            userId = "USR-1001",
            userName = DataManager.Instance.userName,                
            currentTier = 5,
            
            // টিয়ার প্রোগ্রেস ডাটা
            currentPoints = DataManager.Instance.tierProgressPoints, 
            maxPointsForTier = DataManager.Instance.tierMaxPoints,   
            
            // 🌟 আপডেট: এখন মেমোরি থেকে লেটেস্ট পয়েন্ট নেবে
            totalPoints = latestTotalPoints,          
            
            stonesPlayed = 1245,
            perfectCuts = 426,
            failedCuts = 127,
            tierStatus = DataManager.Instance.tier                   
        };

        UpdateProfileUI(mockData);
    }

    private void UpdateProfileUI(UserProfileData data)
    {
        // Null চেক করে সাধারণ টেক্সট আপডেট
        if (userNameText != null) userNameText.text = data.userName;
        
        // প্রোগ্রেস বারের টেক্সট
        if (pointsText != null) pointsText.text = $"{data.currentPoints:N0} / {data.maxPointsForTier:N0}";
        
        // 🌟 আপডেট: N0 দেওয়ার কারণে ৪৫,০০০ কমা সহ সুন্দর করে আসবে
        if (totalPointsText != null) totalPointsText.text = data.totalPoints.ToString("N0");

        // স্লাইডার আপডেট
        if (tierProgressFill != null && data.maxPointsForTier > 0)
        {
            tierProgressFill.fillAmount = (float)data.currentPoints / data.maxPointsForTier;
        }
        
        if (stonesPlayedText != null) stonesPlayedText.text = data.stonesPlayed.ToString("N0");
        if (perfectText != null) perfectText.text = data.perfectCuts.ToString("N0");
        if (failureText != null) failureText.text = data.failedCuts.ToString("N0");

        // লুপ চালিয়ে ৩টি জায়গাতেই একই Tier টেক্সট বসিয়ে দেওয়া
        foreach (var tierText in allTierTexts)
        {
            if (tierText != null)
            {
                tierText.text = data.tierStatus; 
            }
        }

        // Tier অনুযায়ী ব্যাজ (Image) আপডেট করা
        UpdateTierBadge(data.tierStatus);
    }

    private void UpdateTierBadge(string currentTierName)
    {
        if (tierBadgeImage == null || tierBadges == null) return;

        foreach (var badge in tierBadges)
        {
            if (badge.tierName.ToLower() == currentTierName.ToLower())
            {
                tierBadgeImage.sprite = badge.badgeSprite;
                break;
            }
        }
    }
}

// 🌟 Inspector-এ ব্যাজ সেট করার জন্য কাস্টম ক্লাস
[System.Serializable]
public class TierBadgeData
{
    public string tierName; 
    public Sprite badgeSprite; 
}