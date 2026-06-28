using UnityEngine;

public class StonePropertySelector : MonoBehaviour
{
    // 🌟 1. নতুন ইনপুটগুলোর জন্য Enum লিস্ট আপডেট করা হলো
    public enum PropertyType 
    { 
        RotationAngle, 
        RotationSpeed, 
        AnchorPoints,
        StoneSize,           // নতুন
        DifficultyTier,      // নতুন
        MovementPattern,     // নতুন
        LoopDuration         // নতুন
    }
    
    [Header("Data to Send")]
    public PropertyType propertyType;
    public string propertyValue; 

    // (হাইলাইটের কোডগুলো রিমুভ করা হলো, কারণ ButtonGroupManager সেটা কন্ট্রোল করবে)

    public void OnClick()
    {
        // গেম জেনারেট হওয়ার সময় বাটনে ক্লিক ব্লক করা
        if (PredictorUIManager.Instance != null && PredictorUIManager.Instance.isGenerating) return;

        // 🌟 2. PredictorUIManager-এ শুধু ডেটা পাঠানো
        PredictorUIManager.Instance.UpdateManualSelection(propertyType.ToString(), propertyValue);
    }
}