using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StrikeSystem : MonoBehaviour
{
    [System.Serializable]
    public class StrikeVisual
    {
        public Image barImage;    // UI image
        public Color activeColor; // The color that this bar will be when full
    }

    [Header("Strike Settings")]
    public List<StrikeVisual> strikes = new List<StrikeVisual>();
    public Color normalColor = Color.white; // The color that will be in the initialization or reset state
    
    private int currentStrikes = 0;

    // 1. Adding Strike (with Dynamic Color)
    public bool AddStrike()
    {
        if (currentStrikes < strikes.Count)
        {
            // Apply the specified color set in the inspector
            strikes[currentStrikes].barImage.color = strikes[currentStrikes].activeColor;
            currentStrikes++;
            
            // Returns True if all strikes are complete (Game Over)
            return currentStrikes >= strikes.Count;
        }
        return false;
    }

    // 2. Dynamically setup for new canvas
    public void SetupNewStrikes(List<Image> newImages, List<Color> newColors)
    {
        strikes.Clear();
        for (int i = 0; i < newImages.Count; i++)
        {
            StrikeVisual newVisual = new StrikeVisual();
            newVisual.barImage = newImages[i];
            // If the color list is small, default will be red
            newVisual.activeColor = (i < newColors.Count) ? newColors[i] : Color.red;
            strikes.Add(newVisual);
        }
        ResetStrikes();
    }

    // 3. to reset
    public void ResetStrikes()
    {
        currentStrikes = 0;
        foreach (var strike in strikes)
        {
            if (strike.barImage != null)
                strike.barImage.color = normalColor;
        }
    }
}