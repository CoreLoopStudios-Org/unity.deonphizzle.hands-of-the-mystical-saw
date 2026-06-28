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
    public int maxSteps = 5; // আমাদের মোট প্যাটার্ন ৫টি, তাই ৫-ই হবে সর্বোচ্চ
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
            timeStepSlider.minValue = 1; // 🌟 0 সেভ করা যাবে না, সর্বনিম্ন 1s
            
            // 🌟 ফিক্স: স্লাইডারের সর্বোচ্চ ভ্যালু একদম হুবহু রিমেইনিং টাইমের সমান হবে
            timeStepSlider.maxValue = remainingTime; 
            
            // 🌟 ফিক্স: ডিফল্টভাবে স্লাইডারটি টেনে একদম ফুল (Full) টাইমে রাখা থাকবে
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
        timeStepSlider.value = snappedVal;
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
        
        // যদি এটি শেষ প্যাটার্ন হয় (৫ম প্যাটার্ন), তবে এটি জোর করে বাকি সবটুকু সময় নিয়ে নেবে!
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

        // বাকি সময় কমানো হলো
        remainingTime -= timeToSave;
        
        // 🌟 স্লাইডার আপডেট ও লক করার লজিক
        if (remainingTime > 0 && savedSteps.Count < maxSteps)
        {
            // যদি ৪টি প্যাটার্ন সেভ হয়ে গিয়ে থাকে (মানে ৫ম বা শেষ প্যাটার্নটি বাকি)
            if (savedSteps.Count == maxSteps - 1)
            {
                timeStepSlider.minValue = remainingTime;
                timeStepSlider.maxValue = remainingTime;
                timeStepSlider.value = remainingTime;
                timeStepSlider.interactable = false; // 🌟 শেষ প্যাটার্নের জন্য স্লাইডার লক!
            }
            else
            {
                timeStepSlider.minValue = 1;
                // 🌟 ফিক্স: স্লাইডারের সর্বোচ্চ লিমিট এখন বাকি থাকা সময়ের সমান!
                timeStepSlider.maxValue = remainingTime;
                // 🌟 ফিক্স: স্লাইডারের হ্যান্ডেলটি অটোমেটিক বাকি থাকা সর্বোচ্চ সময়ে গিয়ে বসে থাকবে!
                timeStepSlider.value = remainingTime; 
                timeStepSlider.interactable = true;
            }
        }
        else
        {
            // সময় একদম ০ হয়ে গেলে স্লাইডার অফ
            timeStepSlider.minValue = 0;
            timeStepSlider.maxValue = 0;
            timeStepSlider.value = 0;
            timeStepSlider.interactable = false;
        }

        UpdateAllStepUI();
        RefreshCarousel();
        UpdateSliderText();

        // ৫টি মুভমেন্ট শেষ হলে বা সময় শেষ হলে সেভ বাটন লক
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