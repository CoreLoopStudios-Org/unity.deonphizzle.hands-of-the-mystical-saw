using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI userNameText;
    
    // 🌟 Point of progress bar (ie: 12897 / 20000)
    public TextMeshProUGUI pointsText; 
    
    // 🌟 To show total points (ie: 45000).
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
        // 🌟 magic line: 
        // Retrieves the winning point from the cutting scene from memory. 
        // If there is nothing in memory (first time play), your DataManager will take the default of 45000.
        int latestTotalPoints = PlayerPrefs.GetInt("PlayerTotalPoints", DataManager.Instance.totalPoints);

        UserProfileData mockData = new UserProfileData
        {
            userId = "USR-1001",
            userName = DataManager.Instance.userName,                
            currentTier = 5,
            
            // Tear progress data
            currentPoints = DataManager.Instance.tierProgressPoints, 
            maxPointsForTier = DataManager.Instance.tierMaxPoints,   
            
            // 🌟 Update: Now fetches latest point from memory
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
        // Simple text update by checking for null
        if (userNameText != null) userNameText.text = data.userName;
        
        // Progress bar text
        if (pointsText != null) pointsText.text = $"{data.currentPoints:N0} / {data.maxPointsForTier:N0}";
        
        // 🌟 UPDATE: 45,000 will come nicely with commas due to giving N0
        if (totalPointsText != null) totalPointsText.text = data.totalPoints.ToString("N0");

        // Slider update
        if (tierProgressFill != null && data.maxPointsForTier > 0)
        {
            tierProgressFill.fillAmount = (float)data.currentPoints / data.maxPointsForTier;
        }
        
        if (stonesPlayedText != null) stonesPlayedText.text = data.stonesPlayed.ToString("N0");
        if (perfectText != null) perfectText.text = data.perfectCuts.ToString("N0");
        if (failureText != null) failureText.text = data.failedCuts.ToString("N0");

        // Looping to place the same Tier text in 3 places
        foreach (var tierText in allTierTexts)
        {
            if (tierText != null)
            {
                tierText.text = data.tierStatus; 
            }
        }

        // Update badge (Image) according to Tier
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

// 🌟 Custom class to set badges in Inspector
[System.Serializable]
public class TierBadgeData
{
    public string tierName; 
    public Sprite badgeSprite; 
}