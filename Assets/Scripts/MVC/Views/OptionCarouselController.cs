using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionCarouselController : MonoBehaviour
{
    public enum CarouselType { MovementPattern, DifficultyTier }
    
    [Header("Settings")]
    [SerializeField] public CarouselType carouselType;

    [Header("UI References")]
    [SerializeField] public TMP_Text displayText;
    [SerializeField] public Button leftButton;
    [SerializeField] public Button rightButton;

    private string[] options;
    private int currentIndex = 0;

    void Start()
    {
        if (carouselType == CarouselType.MovementPattern)
            options = new string[] { "Static", "Linear", "Oscillation", "Circular", "Chaotic" };
        else if (carouselType == CarouselType.DifficultyTier)
            options = new string[] { "Initiate", "Cutter", "Carver", "Master", "Mythic" };

        if (leftButton != null) leftButton.onClick.AddListener(OnLeftButtonClicked);
        if (rightButton != null) rightButton.onClick.AddListener(OnRightButtonClicked);
        
        RefreshIfLocked(); // শুরুতে সব ঠিকঠাক লোড করার জন্য
    }

    public void OnLeftButtonClicked()
    {
        int startIndex = currentIndex;
        do
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = options.Length - 1; 
            if (currentIndex == startIndex) break; // যদি সবগুলো লক থাকে, তাহলে লুপ ভেঙে যাবে
        } while (IsLocked(options[currentIndex]));
        
        UpdateDisplay();
    }

    public void OnRightButtonClicked()
    {
        int startIndex = currentIndex;
        do
        {
            currentIndex++;
            if (currentIndex >= options.Length) currentIndex = 0; 
            if (currentIndex == startIndex) break; // যদি সবগুলো লক থাকে, তাহলে লুপ ভেঙে যাবে
        } while (IsLocked(options[currentIndex]));
        
        UpdateDisplay();
    }

    // 🌟 নতুন: সেভ করার পর অটোমেটিক পরের আনলক অপশনে যাওয়ার জন্য
    public void RefreshIfLocked()
    {
        if (IsLocked(options[currentIndex]))
        {
            OnRightButtonClicked();
        }
        else
        {
            UpdateDisplay();
        }
    }

    // 🌟 চেক করবে এই অপশনটি অলরেডি সেভড লিস্টে আছে কি না
    private bool IsLocked(string opt)
    {
        if (carouselType == CarouselType.MovementPattern && TimeStepSequenceManager.Instance != null)
        {
            return TimeStepSequenceManager.Instance.IsPatternUsed(opt);
        }
        return false;
    }

    private void UpdateDisplay()
    {
        if (options == null || options.Length == 0) return;

        string selectedOption = options[currentIndex];
        
        if (displayText != null)
        {
            // যদি ৫টি অপশনই ব্যবহার হয়ে যায়
            if (IsLocked(selectedOption)) 
            {
                displayText.text = "All Used";
                displayText.color = Color.red;
            }
            else
            {
                displayText.text = selectedOption;
                displayText.color = Color.cyan; // আনলক থাকলে সায়ান কালার দেখাবে
            }
        }

        if (PredictorUIManager.Instance != null && !IsLocked(selectedOption))
        {
            if (carouselType == CarouselType.MovementPattern)
                PredictorUIManager.Instance.SetMovementPatternFromCarousel(selectedOption);
            else if (carouselType == CarouselType.DifficultyTier)
                PredictorUIManager.Instance.SetDifficultyTierFromCarousel(selectedOption);
        }
    }
}