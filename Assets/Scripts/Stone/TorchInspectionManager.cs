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
    public Material dryMaterial;      // Torch off, stone dry
    public Material wetMaterial;      // Torch off, stone wet
    public Material dryXRayMaterial;  // Torch on, stone dry (your previous X-Ray)
    public Material wetXRayMaterial;  // torch on, stone wet (new X-Ray)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 🌟 Spray button will always be on (not dependent on torch)
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
        // Send torch position to current material if torch is on
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
    // 🔦 Torch logic
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

        // offset torch position after torch off (to avoid x-ray glitches)
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
    // 💦 spray logic (completely separate from torch)
    // ==========================================
    public void ApplyWaterFromButton()
    {
        isStoneWet = true; 
        Debug.Log("<color=blue>💦 Water Applied!</color>");

        if (waterParticleEffect != null) waterParticleEffect.Play();

        // Once the water is applied, the button will turn off
        if (sprayWaterButton != null) sprayWaterButton.interactable = false;

        UpdateStoneMaterial();
        InspectStone(); 
    }

    // ==========================================
    // 🎨 material change logic
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
            // 🌟 If torch is on: new X-Ray if wet, previous X-Ray if dry
            targetMaterial = isStoneWet ? wetXRayMaterial : dryXRayMaterial;
        }
        else
        {
            // 🌟 With torch off: Wet material if wet, Dry material if dry
            targetMaterial = isStoneWet ? wetMaterial : dryMaterial;
        }

        if (targetMaterial != null)
        {
            renderer.material = targetMaterial;
        }
    }

    // ==========================================
    // 📊 Inspection logic
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