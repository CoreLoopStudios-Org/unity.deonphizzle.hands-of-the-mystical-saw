using UnityEngine;

public class StonePropertySelector : MonoBehaviour
{
    // 🌟 1. Enum list updated for new inputs
    public enum PropertyType 
    { 
        RotationAngle, 
        RotationSpeed, 
        AnchorPoints,
        StoneSize , // new
        DifficultyTier, // new
        MovementPattern, // new
        LoopDuration // new
    }
    
    [Header("Data to Send")]
    public PropertyType propertyType;
    public string propertyValue; 

    // (Highlight code removed, as ButtonGroupManager will control it)

    public void OnClick()
    {
        // Block click on button while game is generated
        if (PredictorUIManager.Instance != null && PredictorUIManager.Instance.isGenerating) return;

        // 🌟 2. Just sending data to PredictorUIManager
        PredictorUIManager.Instance.UpdateManualSelection(propertyType.ToString(), propertyValue);
    }
}