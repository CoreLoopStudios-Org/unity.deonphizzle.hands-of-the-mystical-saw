using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TorchSize { Small, Medium, Large }

public class TorchInspectionManager : MonoBehaviour
{
    public static TorchInspectionManager Instance;

    [Header("--- Current Settings ---")]
    public TorchSize currentTorch = TorchSize.Small; 
    public bool isStoneWet = false; 
    public bool isTorchActive = false; 

    [Header("--- UI References ---")]
    public TextMeshProUGUI visibilityPercentageText; 
    public TextMeshProUGUI estimatedValueText;       
    public Button sprayWaterButton; 

    [Header("--- Effects & Transforms ---")]
    public ParticleSystem waterParticleEffect; 
    public Transform torchTransform; 

    [Header("--- Materials (4 States) ---")]
    public Material dryMaterial;      // টর্চ অফ, পাথর শুকনো
    public Material wetMaterial;      // টর্চ অফ, পাথর ভেজা
    public Material dryXRayMaterial;  // টর্চ অন, পাথর শুকনো (আপনার আগের X-Ray)
    public Material wetXRayMaterial;  // টর্চ অন, পাথর ভেজা (নতুন X-Ray)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 🌟 স্প্রে বাটন সবসময় অন থাকবে (টর্চের উপর নির্ভরশীল নয়)
        if (sprayWaterButton != null) sprayWaterButton.interactable = true;

        if (visibilityPercentageText == null || estimatedValueText == null)
        {
            FindTextMeshProReferences();
        }

        // Initialize display texts to fix spelling typos
        if (visibilityPercentageText != null) visibilityPercentageText.text = "VISIBILITY: 0%";
        if (estimatedValueText != null) estimatedValueText.text = "EST. VALUE: ??? PTS";
    }

    private void FindTextMeshProReferences()
    {
        GameObject visibilityPanel = GameObject.Find("Visibility (1)");
        if (visibilityPanel != null)
        {
            TextMeshProUGUI[] texts = visibilityPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                if (t.gameObject.name == "VisibilityText" && visibilityPercentageText == null)
                {
                    visibilityPercentageText = t;
                }
                else if (t.gameObject.name == "EstimatedValueText" && estimatedValueText == null)
                {
                    estimatedValueText = t;
                }
            }
        }
    }

    private void Update()
    {
        // টর্চ অন থাকলে বর্তমান ম্যাটেরিয়ালে টর্চের পজিশন পাঠাবে
        if (isTorchActive && torchTransform != null)
        {
            GameObject liveStone = GameObject.FindGameObjectWithTag("Stone");
            if (liveStone != null)
            {
                MeshRenderer renderer = liveStone.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.SetVector("_TorchPosition", torchTransform.position);
                }
            }
        }
    }

    // ==========================================
    // 🔦 টর্চ লজিক
    private void ToggleJadeGlow(bool active)
    {
        GameObject liveStone = GameObject.FindGameObjectWithTag("Stone");
        if (liveStone != null)
        {
            StoneGenerator sg = liveStone.GetComponent<StoneGenerator>();
            if (sg != null)
            {
                sg.SetJadeGlow(active);
            }
        }
    }

    public void TurnOnTorch()
    {
        isTorchActive = true;
        UpdateStoneMaterial();
        InspectStone();
        ToggleJadeGlow(true);

        if (ToolCameraManager.Instance != null)
        {
            ToolCameraManager.Instance.ZoomInOnTorch();
        }
    }

    public void TurnOffTorch()
    {
        isTorchActive = false;
        UpdateStoneMaterial();
        ToggleJadeGlow(false);

        // টর্চ অফ করার পর টর্চের পজিশন দূরে সরিয়ে দেওয়া (যাতে এক্স-রে গ্লিচ না হয়)
        GameObject liveStone = GameObject.FindGameObjectWithTag("Stone");
        if (liveStone != null)
        {
            MeshRenderer renderer = liveStone.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.SetVector("_TorchPosition", new Vector3(0, 10000, 0));
            }
        }

        if (ToolCameraManager.Instance != null)
        {
            ToolCameraManager.Instance.ZoomOutToDefault();
        }

        if (visibilityPercentageText != null) visibilityPercentageText.text = "VISIBILITY: 0%";
        if (estimatedValueText != null) estimatedValueText.text = "EST. VALUE: ??? PTS";
    }

    // ==========================================
    // 💦 স্প্রে লজিক (টর্চ থেকে সম্পূর্ণ আলাদা)
    // ==========================================
    public void ApplyWaterFromButton()
    {
        isStoneWet = true; 
        Debug.Log("<color=blue>💦 Water Applied!</color>");

        if (waterParticleEffect != null) waterParticleEffect.Play();

        // একবার পানি দিলে বাটন অফ হয়ে যাবে
        if (sprayWaterButton != null) sprayWaterButton.interactable = false;

        UpdateStoneMaterial();
        InspectStone(); 
    }

    // ==========================================
    // 🎨 ম্যাটেরিয়াল চেঞ্জ লজিক
    // ==========================================
    private void UpdateStoneMaterial()
    {
        GameObject liveStone = GameObject.FindGameObjectWithTag("Stone");
        if (liveStone == null) return;

        MeshRenderer renderer = liveStone.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        Material targetMaterial = null;

        if (isTorchActive)
        {
            // 🌟 টর্চ অন থাকলে: ভেজা হলে নতুন X-Ray, শুকনো হলে আগের X-Ray
            targetMaterial = isStoneWet ? wetXRayMaterial : dryXRayMaterial;
        }
        else
        {
            // 🌟 টর্চ অফ থাকলে: ভেজা হলে Wet ম্যাটেরিয়াল, শুকনো হলে Dry ম্যাটেরিয়াল
            targetMaterial = isStoneWet ? wetMaterial : dryMaterial;
        }

        if (targetMaterial != null)
        {
            renderer.material = targetMaterial;
        }
    }

    // ==========================================
    // 📊 ইন্সপেকশন লজিক
    // ==========================================
    public void InspectStone()
    {
        int sizeIndex = 1; // Default to Medium (1)
        if (GlobalStoneData.CurrentStone != null)
        {
            sizeIndex = (int)GlobalStoneData.CurrentStone.StoneSize;
        }
        if (sizeIndex < 0 || sizeIndex > 2) sizeIndex = 1; // Safeguard

        int torchIndex = (int)currentTorch;

        int[,] dryVisibility = { { 9, 11, 13 }, { 6, 8, 10 }, { 4, 6, 8 } };
        int[,] wetVisibility = { { 29, 32, 36 }, { 25, 28, 32 }, { 21, 25, 29 } };

        int visibilityPercent = isStoneWet ? wetVisibility[sizeIndex, torchIndex] : dryVisibility[sizeIndex, torchIndex];

        if (visibilityPercentageText != null) visibilityPercentageText.text = $"VISIBILITY: {visibilityPercent}%";

        CalculateEstimatedValue(visibilityPercent);
    }

    private int GetActualStonePoints()
    {
        if (GlobalStoneData.CurrentBlueprint != null)
        {
            return GlobalStoneData.CurrentBlueprint.challenge_points;
        }

        // fallback: check if StoneGenerator exists in scene and try to get the blueprint/points
        GameObject liveStone = GameObject.FindGameObjectWithTag("Stone");
        if (liveStone != null)
        {
            StoneGenerator sg = liveStone.GetComponent<StoneGenerator>();
            if (sg != null)
            {
                if (sg.currentStoneModel != null && sg.currentStoneModel.parsedBlueprint != null)
                {
                    return sg.currentStoneModel.parsedBlueprint.challenge_points;
                }
            }
        }

        return 1000; // default backup points
    }

    private void CalculateEstimatedValue(int visibilityPercent)
    {
        int actualPoints = GetActualStonePoints(); 
        float inaccuracyMargin = 1f - (visibilityPercent / 100f); 

        int minEstimate = Mathf.RoundToInt(actualPoints * (1f - inaccuracyMargin));
        int maxEstimate = Mathf.RoundToInt(actualPoints * (1f + inaccuracyMargin));

        if (estimatedValueText != null) 
            estimatedValueText.text = $"EST. VALUE: {minEstimate.ToString("N0")} - {maxEstimate.ToString("N0")} PTS";
    }
}