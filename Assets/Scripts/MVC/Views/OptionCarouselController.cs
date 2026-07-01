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

    void Awake()
    {
        if (carouselType == CarouselType.MovementPattern)
            options = new string[] { "Static", "Linear", "Oscillation", "Circular", "Chaotic" };
        else if (carouselType == CarouselType.DifficultyTier)
            options = new string[] { "Initiate", "Cutter", "Carver", "Master", "Mythic" };
    }

    void Start()
    {
        if (leftButton != null) leftButton.onClick.AddListener(OnLeftButtonClicked);
        if (rightButton != null) rightButton.onClick.AddListener(OnRightButtonClicked);
        
        RefreshIfLocked(); // to load everything properly at the beginning
    }

    public void OnLeftButtonClicked()
    {
        int startIndex = currentIndex;
        do
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = options.Length - 1; 
            if (currentIndex == startIndex) break; // If all are locked, the loop will break
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
            if (currentIndex == startIndex) break; // If all are locked, the loop will break
        } while (IsLocked(options[currentIndex]));
        
        UpdateDisplay();
    }

    // 🌟 New: Automatically go to next unlock option after saving
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

    // 🌟 Check if this option is in already saved list or not
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
            // if all 5 options are used
            if (IsLocked(selectedOption)) 
            {
                displayText.text = "All Used";
                displayText.color = Color.red;
            }
            else
            {
                displayText.text = selectedOption;
                displayText.color = Color.cyan; // Shows cyan color if unlocked
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