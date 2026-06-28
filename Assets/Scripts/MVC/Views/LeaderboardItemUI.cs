using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking; // 🌟 ইন্টারনেট থেকে ছবি ডাউনলোডের জন্য এটি লাগবে

public class LeaderboardItemUI : MonoBehaviour
{
    [Header("Item References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI userNameText;
    // levelText রিমুভ করা হয়েছে কারণ ডিজাইনে এটি নেই
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

        // 🌟 ইন্টারনেট থেকে ছবি লোড করার Coroutine কল করা হচ্ছে
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
        // URL থেকে ছবি রিকোয়েস্ট করা
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl);
        
        // ছবি ডাউনলোড হওয়া পর্যন্ত অপেক্ষা করা
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning($"Error downloading avatar: {request.error}");
        }
        else
        {
            // ডাউনলোড সফল হলে টেক্সচারটিকে স্প্রাইটে (Sprite) কনভার্ট করে UI-তে বসানো
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            
            avatarImage.sprite = sprite;
            Debug.Log($"Avatar downloaded successfully for {userNameText.text}");
        }
    }
}