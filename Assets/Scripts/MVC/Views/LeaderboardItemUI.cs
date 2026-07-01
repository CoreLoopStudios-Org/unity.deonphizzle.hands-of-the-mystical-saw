using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking; // 🌟 This is required for downloading images from the internet

public class LeaderboardItemUI : MonoBehaviour
{
    [Header("Item References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI userNameText;
    // levelText is removed because the design doesn't have it
    public TextMeshProUGUI tierText;   
    public TextMeshProUGUI pointText;  
    public Image avatarImage;

    /// <summary>
    /// Setup the UI item with dynamic player data.
    /// </summary>
    public void Setup(LeaderboardPlayer player)
    {
        if (rankText != null) rankText.text = $"#{player.rank}";
        if (userNameText != null) userNameText.text = player.playerName;
        if (tierText != null) tierText.text = player.tier;
        if (pointText != null) pointText.text = player.points.ToString();

        // 🌟 Calling coroutine to load image from internet
        if (avatarImage != null && !string.IsNullOrEmpty(player.avatarUrl))
        {
            StartCoroutine(DownloadAvatarImage(player.avatarUrl));
        }
    }

    // ==========================================
    // 🌟 Server Image Download Logic
    // ==========================================
    private IEnumerator DownloadAvatarImage(string mediaUrl)
    {
        // Request image from URL
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl);
        
        // Wait for the image to download
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning($"Error downloading avatar: {request.error}");
        }
        else
        {
            // If the download is successful, convert the texture to a sprite and place it in the UI
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            
            avatarImage.sprite = sprite;
            Debug.Log($"Avatar downloaded successfully for {userNameText.text}");
        }
    }
}