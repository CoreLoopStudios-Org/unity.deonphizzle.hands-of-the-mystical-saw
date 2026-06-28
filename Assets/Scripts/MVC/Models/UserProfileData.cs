using UnityEngine;

// [System.Serializable] দেওয়ার মানে হলো এই ক্লাসটিকে খুব সহজেই JSON-এ কনভার্ট করা যাবে
[System.Serializable]
public class UserProfileData
{
    public string userId;
    public string userName;
    public string avatarUrl;      // সার্ভার থেকে ছবির লিংক আসবে
    public int totalPoints;
    public int currentTier;       // যেমন: 5
    public int currentPoints;     // যেমন: 12897
    public int maxPointsForTier;  // যেমন: 20000 (স্লাইডারের জন্য লাগবে)
    
    public int stonesPlayed;      // যেমন: 1245
    public int perfectCuts;       // যেমন: 426
    public int failedCuts;        // যেমন: 127
    
    public string tierStatus;     // যেমন: "Master"
}