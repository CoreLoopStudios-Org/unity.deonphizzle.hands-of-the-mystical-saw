using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class LeaderboardPlayer
{
    public int rank;           
    public string playerName;
    public string tier;        
    public int points;         
    public string avatarUrl;   
}

public class LeaderboardManager : MonoBehaviour
{
    [Header("List Setup (Right Side)")]
    public Transform contentPanel; 
    public GameObject leaderboardItemPrefab; 
    
    [Header("Current User Setup (Left Side)")]
    public TextMeshProUGUI currentUserRankText;
    public TextMeshProUGUI currentUserNameText;
    public TextMeshProUGUI currentUserTierText;
    public TextMeshProUGUI currentUserPointText;
    public Image currentUserAvatarImage;

    private void OnEnable()
    {
        FetchLeaderboardData();
    }

    private void FetchLeaderboardData()
    {
        Debug.Log("Fetching Leaderboard Data...");
        List<LeaderboardPlayer> mockList = new List<LeaderboardPlayer>()
        {
            new LeaderboardPlayer { rank = 1, playerName = "Monowar Hossain", tier = "Diamond", points = 154200, avatarUrl = "url1" },
            new LeaderboardPlayer { rank = 2, playerName = "Tanmoy Komer", tier = "Diamond", points = 130500, avatarUrl = "url2" },
            new LeaderboardPlayer { rank = 3, playerName = "Md. Gazi Fahim", tier = "Gold", points = 117500, avatarUrl = "url3" },
            new LeaderboardPlayer { rank = 4, playerName = "Alex Hunter", tier = "Silver", points = 85000, avatarUrl = "url4" }
        };
        LeaderboardPlayer currentUserMockData = new LeaderboardPlayer 
        { 
            rank = 42, 
            playerName = DataManager.Instance.userName,  
            tier = DataManager.Instance.tier,            
            points = DataManager.Instance.totalPoints,   
            avatarUrl = "my_avatar_url" 
        };
        
        UpdateLeaderboardUI(mockList);
        UpdateCurrentUserUI(currentUserMockData);
    }

    private void UpdateLeaderboardUI(List<LeaderboardPlayer> players)
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var player in players)
        {
            GameObject newItem = Instantiate(leaderboardItemPrefab, contentPanel);
            LeaderboardItemUI itemUI = newItem.GetComponent<LeaderboardItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(player);
            }
        }
    }
    private void UpdateCurrentUserUI(LeaderboardPlayer currentUser)
    {
        if (currentUserRankText != null) currentUserRankText.text = $"#{currentUser.rank}";
        if (currentUserNameText != null) currentUserNameText.text = currentUser.playerName;
        if (currentUserTierText != null) currentUserTierText.text = currentUser.tier;
        if (currentUserPointText != null) currentUserPointText.text = currentUser.points.ToString();
    }
}