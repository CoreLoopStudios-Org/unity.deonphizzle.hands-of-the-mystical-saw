using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; 

public class StoneItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image stoneIcon;         
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI sizeText;
    public Button acceptButton;

    [Header("Stone Icons (By Size)")]
    public Sprite smallStoneIcon;      
    public Sprite mediumStoneIcon;     
    public Sprite largeStoneIcon;      

    [Header("Scene Settings")]
    [Tooltip("আপনার কাটিং সিনের হুবহু নাম এখানে লিখুন")]
    public string cuttingSceneName = "CuttingScene"; 

    private StoneBlueprint currentBlueprint;

    // ==========================================
    // ১. ডামি পাথরের জন্য (Scriptable Object)
    // ==========================================
    public void Setup(StoneDataSO stoneData)
    {
        if (stoneData == null || stoneData.blueprint == null) return;
        
        currentBlueprint = stoneData.blueprint;
        UpdateUI(currentBlueprint);

        // আইকন সেট করা
        if (stoneIcon != null)
        {
            if (stoneData.stoneIcon != null) stoneIcon.sprite = stoneData.stoneIcon;
            else SetIconBySize(GetCorrectSize(currentBlueprint)); // 🌟 আপডেটেড লজিক
        }

        SetupButtonListener();
    }

    // ==========================================
    // ২. লাইভ পাথরের জন্য (Predictor Mode থেকে আসা)
    // ==========================================
    public void SetupLiveStone(StoneBlueprint bp)
    {
        if (bp == null) return;
        
        currentBlueprint = bp;
        UpdateUI(currentBlueprint);
        
        SetIconBySize(GetCorrectSize(currentBlueprint)); // 🌟 আপডেটেড লজিক
        
        SetupButtonListener();
    }

    // ==========================================
    // 🌟 নতুন: সঠিক সাইজ বের করার ম্যাজিক ফাংশন
    // ==========================================
    private string GetCorrectSize(StoneBlueprint bp)
    {
        // যদি Predictor Data থাকে, তবে সেখান থেকে একদম একুরেট সাইজটা নেবে
        if (bp != null && bp.predictor_challenge_data != null)
        {
            return bp.predictor_challenge_data.targetStoneSize.ToString();
        }
        
        // না থাকলে ব্লুপ্রিন্টের ডিফল্ট সাইজ নেবে
        return string.IsNullOrEmpty(bp.stone_size_label) ? "Medium" : bp.stone_size_label;
    }

    // ==========================================
    // ৩. কমন UI আপডেট ফাংশন
    // ==========================================
    private void UpdateUI(StoneBlueprint bp)
    {
        if (pointsText != null) pointsText.text = bp.challenge_points.ToString("N0");
        if (weightText != null) weightText.text = "Weight: " + bp.total_weight_kg.ToString("F1") + "kg";
        
        // 🌟 এখন সাইজটা সরাসরি সঠিক ফাংশন থেকে আসবে
        string finalSize = GetCorrectSize(bp);
        if (sizeText != null) sizeText.text = "Size: " + finalSize;
    }

    // ==========================================
    // ৪. বাটন লিসেনার সেটআপ (কাটিং সিনে ডাটা পাঠানো)
    // ==========================================
    private void SetupButtonListener()
    {
        // (আপনার আগের কোড অনুযায়ী কমেন্ট করা আছে)
    }

    private void SetIconBySize(string sizeLabel)
    {
        if (stoneIcon == null) return;

        switch (sizeLabel)
        {
            case "Small":
                if (smallStoneIcon != null) stoneIcon.sprite = smallStoneIcon;
                break;
            case "Medium":
                if (mediumStoneIcon != null) stoneIcon.sprite = mediumStoneIcon;
                break;
            case "Large":
                if (largeStoneIcon != null) stoneIcon.sprite = largeStoneIcon;
                break;
        }
    }
    
    // 🌟 এই ফাংশনটা আমরা ইউনিটির ইন্সপেক্টর থেকে ম্যানুয়ালি ধরিয়ে দেব
    public void ManualAcceptClick()
    {
        Debug.Log("<color=cyan>🔥 MANUAL CLICK WORKED!</color>");
        
        if (currentBlueprint != null)
        {
            GlobalStoneData.CurrentBlueprint = currentBlueprint; 

            // Check saved theme
            GameModeManager.GameTheme activeTheme = GameModeManager.GameTheme.Modern;
            if (GameModeManager.Instance != null)
            {
                activeTheme = GameModeManager.Instance.currentTheme;
            }
            else
            {
                activeTheme = (GameModeManager.GameTheme)PlayerPrefs.GetInt("SavedGameTheme", 1);
            }

            // Route dynamically based on setting
            string targetScene = (activeTheme == GameModeManager.GameTheme.Classic) 
                ? "StoneCuttingScene_Classic" 
                : "StoneGenerator Scene";

            Debug.Log($"Loading game mode: {activeTheme} -> Scene: {targetScene}");
            SceneManager.LoadScene(targetScene);
        }
    }
}