using UnityEngine;

// Giving [System.Serializable] means this class can be easily converted to JSON
[System.Serializable]
public class UserProfileData
{
    public string userId;
    public string userName;
    public string avatarUrl;      // The image link will come from the server
    public int totalPoints;
    public int currentTier;       // eg: 5
    public int currentPoints;     // eg: 12897
    public int maxPointsForTier;  // eg: 20000 (used for slider)
    
    public int stonesPlayed;      // eg: 1245
    public int perfectCuts;       // eg: 426
    public int failedCuts;        // eg: 127
    
    public string tierStatus;     // eg: "Master"
}