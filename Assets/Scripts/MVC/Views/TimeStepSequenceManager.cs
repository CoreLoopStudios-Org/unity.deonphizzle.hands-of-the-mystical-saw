using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class TimeStepData
{
    public float duration;
    public string movementPattern;
}

public class TimeStepSequenceManager : MonoBehaviour
{
    public static TimeStepSequenceManager Instance;

    [Header("--- Setup & Limits ---")]
    public int maxSteps = 5; // We have 5 patterns in total, so 5th will be max
    private float totalLoopDuration = 0f;
    private float remainingTime = 0f;

    [Header("--- UI References ---")]
    public Slider timeStepSlider;
    public TextMeshProUGUI timeStepValueText; 
    public Button saveButton;
    public TextMeshProUGUI[] stepDisplayTexts; 

    [Header("--- Data (Do Not Touch) ---")]
    public List<TimeStepData> savedSteps = new List<TimeStepData>();

    private void Awake() { Instance = this; }

    private void Start()
    {
        if (timeStepSlider != null) timeStepSlider.onValueChanged.AddListener(OnSliderValueChanged);
        if (saveButton != null) saveButton.onClick.AddListener(SaveCurrentStep);
        
        ResetSequence(0f); 
    }

    public void ResetSequence(float newTotalDuration)
    {
        totalLoopDuration = newTotalDuration;
        remainingTime = newTotalDuration;
        savedSteps.Clear(); 

        if (timeStepSlider != null)
        {
            timeStepSlider.interactable = true;
            timeStepSlider.minValue = 1; // 🌟 0 cannot be saved, minimum 1s
            
            // 🌟 Fix: Slider's maximum value will be exactly equal to remaining time
            timeStepSlider.maxValue = remainingTime; 
            
            // 🌟 Fix: By default the slider will be kept at Full time
            timeStepSlider.value = remainingTime; 
        }

        if (saveButton != null) saveButton.interactable = true;

        UpdateAllStepUI();
        UpdateSliderText();
        RefreshCarousel();
    }

    private void OnSliderValueChanged(float val)
    {
        float snappedVal = Mathf.Round(val);
        timeStepSlider.SetValueWithoutNotify(snappedVal);
        UpdateSliderText();
    }

    private void UpdateSliderText()
    {
        if (timeStepValueText != null)
        {
            timeStepValueText.text = timeStepSlider.value.ToString("0") + "s";
        }
    }

    public void SaveCurrentStep()
    {
        if (remainingTime <= 0 || savedSteps.Count >= maxSteps) return;

        string currentPattern = PredictorUIManager.Instance.selectedPatternString;
        if (IsPatternUsed(currentPattern)) return;

        float timeToSave = timeStepSlider.value;
        
        // If this is the last pattern (the 5th pattern), it will force all the remaining time!
        if (savedSteps.Count == maxSteps - 1)
        {
            timeToSave = remainingTime; 
        }
        else if (timeToSave > remainingTime) 
        {
            timeToSave = remainingTime;
        }

        if (timeToSave <= 0) return;

        TimeStepData newStep = new TimeStepData();
        newStep.duration = timeToSave;
        newStep.movementPattern = currentPattern;
        savedSteps.Add(newStep);

        // Remaining time is reduced
        remainingTime -= timeToSave;
        
        // 🌟 Logic to update and lock the slider
        if (remainingTime > 0 && savedSteps.Count < maxSteps)
        {
            // if 4 patterns have been saved (ie 5th or last pattern left)
            if (savedSteps.Count == maxSteps - 1)
            {
                timeStepSlider.minValue = remainingTime;
                timeStepSlider.maxValue = remainingTime;
                timeStepSlider.value = remainingTime;
                timeStepSlider.interactable = false; // 🌟 Slider lock for last pattern!
            }
            else
            {
                timeStepSlider.minValue = 1;
                // 🌟 FIX: Slider max limit is now equal to remaining time!
                timeStepSlider.maxValue = remainingTime;
                // 🌟 FIX: Slider handle would automatically go to max remaining time!
                timeStepSlider.value = remainingTime; 
                timeStepSlider.interactable = true;
            }
        }
        else
        {
            //Slider off when time is 0
            timeStepSlider.minValue = 0;
            timeStepSlider.maxValue = 0;
            timeStepSlider.value = 0;
            timeStepSlider.interactable = false;
        }

        UpdateAllStepUI();
        RefreshCarousel();
        UpdateSliderText();

        // Save button lock after 5 movements or timeout
        if (remainingTime <= 0 || savedSteps.Count >= maxSteps)
        {
            if (saveButton != null) saveButton.interactable = false;
            if (timeStepSlider != null) timeStepSlider.interactable = false;
        }
    }

    private void UpdateAllStepUI()
    {
        for (int i = 0; i < stepDisplayTexts.Length; i++)
        {
            if (stepDisplayTexts[i] == null) continue;

            if (i < savedSteps.Count)
            {
                stepDisplayTexts[i].text = $"<color=#00FFFF>{savedSteps[i].duration}s | {savedSteps[i].movementPattern}</color>";
            }
            else
            {
                stepDisplayTexts[i].text = $"<color=#888888>---</color>";
            }
        }
    }

    public bool IsPatternUsed(string patternName)
    {
        foreach (var step in savedSteps)
        {
            if (step.movementPattern == patternName) return true;
        }
        return false;
    }

    private void RefreshCarousel()
    {
        OptionCarouselController[] carousels = Object.FindObjectsByType<OptionCarouselController>(FindObjectsSortMode.None);
        foreach (var c in carousels)
        {
            if (c.carouselType == OptionCarouselController.CarouselType.MovementPattern)
            {
                c.RefreshIfLocked();
            }
        }
    }
}