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
    [Tooltip("Enter the exact name of your cutting scene here")]
    public string cuttingSceneName = "CuttingScene"; 

    private StoneBlueprint currentBlueprint;

    // ==========================================
    // 1. For Dummy Stone (Scriptable Object)
    // ==========================================
    public void Setup(StoneDataSO stoneData)
    {
        if (stoneData == null || stoneData.blueprint == null) return;
        
        currentBlueprint = stoneData.blueprint;
        UpdateUI(currentBlueprint);

        // Set the icon
        if (stoneIcon != null)
        {
            if (stoneData.stoneIcon != null) stoneIcon.sprite = stoneData.stoneIcon;
            else SetIconBySize(GetCorrectSize(currentBlueprint)); // 🌟 Updated logic
        }

        SetupButtonListener();
    }

    // ==========================================
    // 2. For live stones (coming from Predictor Mode)
    // ==========================================
    public void SetupLiveStone(StoneBlueprint bp)
    {
        if (bp == null) return;
        
        currentBlueprint = bp;
        UpdateUI(currentBlueprint);
        
        SetIconBySize(GetCorrectSize(currentBlueprint)); // 🌟 Updated logic
        
        SetupButtonListener();
    }

    // ==========================================
    // 🌟 New: Magic function to find the correct size
    // ==========================================
    private string GetCorrectSize(StoneBlueprint bp)
    {
        // If there is Predictor Data, take the exact size from there
        if (bp != null && bp.predictor_challenge_data != null)
        {
            return bp.predictor_challenge_data.targetStoneSize.ToString();
        }
        
        // If not, will take the blueprint's default size
        return string.IsNullOrEmpty(bp.stone_size_label) ? "Medium" : bp.stone_size_label;
    }

    // ==========================================
    // 3. Common UI update function
    // ==========================================
    private void UpdateUI(StoneBlueprint bp)
    {
        if (pointsText != null) pointsText.text = bp.challenge_points.ToString("N0");
        if (weightText != null) weightText.text = "Weight: " + bp.total_weight_kg.ToString("F1") + "kg";
        
        // 🌟 Now the size will come directly from the correct function
        string finalSize = GetCorrectSize(bp);
        if (sizeText != null) sizeText.text = "Size: " + finalSize;
    }

    // ==========================================
    // 4. Button listener setup (sending data to cutting scene)
    // ==========================================
    private void SetupButtonListener()
    {
        // (Your previous code is commented out)
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
    
    // 🌟 We will call this function manually from Unity's inspector
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