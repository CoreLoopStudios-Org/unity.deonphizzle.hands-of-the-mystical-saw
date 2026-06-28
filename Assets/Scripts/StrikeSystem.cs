using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StrikeSystem : MonoBehaviour
{
    [System.Serializable]
    public class StrikeVisual
    {
        public Image barImage;    // UI ইমেজ
        public Color activeColor; // এই বারটি পূর্ণ হলে যে রঙ হবে
    }

    [Header("Strike Settings")]
    public List<StrikeVisual> strikes = new List<StrikeVisual>();
    public Color normalColor = Color.white; // শুরুতে বা রিসেট অবস্থায় যে রঙ থাকবে
    
    private int currentStrikes = 0;

    // ১. স্ট্রাইক অ্যাড করা (ডাইনামিক কালারসহ)
    public bool AddStrike()
    {
        if (currentStrikes < strikes.Count)
        {
            // ইন্সপেক্টরে সেট করা নির্দিষ্ট রঙটি অ্যাপ্লাই করা
            strikes[currentStrikes].barImage.color = strikes[currentStrikes].activeColor;
            currentStrikes++;
            
            // সব স্ট্রাইক পূর্ণ হলে True পাঠাবে (Game Over)
            return currentStrikes >= strikes.Count;
        }
        return false;
    }

    // ২. নতুন ক্যানভাসের জন্য ডাইনামিকভাবে সেটআপ করা
    public void SetupNewStrikes(List<Image> newImages, List<Color> newColors)
    {
        strikes.Clear();
        for (int i = 0; i < newImages.Count; i++)
        {
            StrikeVisual newVisual = new StrikeVisual();
            newVisual.barImage = newImages[i];
            // যদি কালার লিস্ট ছোট হয় তবে ডিফল্ট লাল রঙ দেবে
            newVisual.activeColor = (i < newColors.Count) ? newColors[i] : Color.red;
            strikes.Add(newVisual);
        }
        ResetStrikes();
    }

    // ৩. রিসেট করা
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