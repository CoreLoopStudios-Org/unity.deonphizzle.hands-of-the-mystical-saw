using UnityEngine;
using System.Threading.Tasks;
using TMPro;

// --- Your previous ENUMS (left intact) ---
public enum StoneSize { Small, Medium, Large }
public enum StoneDensity { Light, Normal, Heavy }
public enum StoneStress { Low, Medium, High }
public enum FractureTolerance { Fragile, Normal, Strong }
public enum JadeColor { PaleGreen, DeepGreen, Emerald, Imperial }
public enum JadeQuantity { Single, Few, Many }
public enum StoneAnchor { Free, Grounded, WallAttached }
public enum AdversityLevel { Low, Medium, High }
public enum RotationPattern { LeftToRight, RightToLeft }
public enum SpinSpeed { Slow, Fast }

public class PredictorUIManager : MonoBehaviour
{
    public static PredictorUIManager Instance;

    [Header("--- User Manual Selections (Old) ---")]
    public float selectedSpeed = 10f;
    public float selectedAngle = 45f;
    public int selectedAnchors = 2;

    [Header("--- New Predictor Settings (Button Based) ---")]
    public StoneChallengeData.StoneSize selectedSize = StoneChallengeData.StoneSize.Small;
    public StoneChallengeData.SkillTier selectedDifficulty = StoneChallengeData.SkillTier.Initiate;
    public StoneChallengeData.LoopDuration selectedDuration = StoneChallengeData.LoopDuration.Beginner_10s;
    public string selectedPatternString = "Static"; 
    
    [Header("--- Adversity UI (+ / -) ---")]
    public int currentAdversity = 0;
    public TextMeshProUGUI adversityText; 

    [Header("--- Wager UI (Numpad) ---")]
    public TextMeshProUGUI wagerDisplayText; // to show on the display
    private string currentWagerString = "";  // To save numbers in the background

    [Header("--- System Status ---")]
    public bool isGenerating = false;
    public GameObject generatingStatus; 
    public PredictorController predictorController; 
    
    [Header("--- Live Preview ---")]
    public StonePreviewManager previewManager; 

    [Header("--- Separate Status Text Control ---")]
    public TextMeshProUGUI statusText; 

    [Header("--- New Slider UI ---")]
    public TextMeshProUGUI rotationSpeedNumText; 
    
    [Header("--- New Anchor UI (+ / -) ---")]
    public TextMeshProUGUI anchorPointsText; 
    
    
    private void Awake() => Instance = this;

    // 🌟 The separate text will read "Generating..." by default when starting the game
    private void Start()
    {
        if (statusText != null) statusText.text = "Generating...";
        if (rotationSpeedNumText != null) rotationSpeedNumText.text = Mathf.RoundToInt(selectedSpeed).ToString();
        
        // 🌟 Initially anchor's value to show in UI
        if (anchorPointsText != null) anchorPointsText.text = selectedAnchors.ToString();

        // 🌟 to initially show 0 on the wager display
        if (wagerDisplayText != null) wagerDisplayText.text = "0";
    }

    // ==========================================
    // 🌟 NUMPAD LOGIC (new)
    // ==========================================
    
    public void OnNumpadNumberPressed(string number)
    {
        if (currentWagerString.Length < 6)
        {
            currentWagerString += number;
            if (wagerDisplayText != null) wagerDisplayText.text = currentWagerString;
            if (statusText != null) statusText.text = "Generating...";
        }
    }

    public void OnNumpadBackspacePressed()
    {
        if (currentWagerString.Length > 0)
        {
            currentWagerString = currentWagerString.Substring(0, currentWagerString.Length - 1);
            if (wagerDisplayText != null) 
            {
                wagerDisplayText.text = currentWagerString == "" ? "0" : currentWagerString;
            }
            if (statusText != null) statusText.text = "Generating...";
        }
    }

    // ==========================================
    // 🌟 OTHER UI LOGIC
    // ==========================================

    public void OnRotationSpeedSliderChanged(float sliderValue)
    {
        float mappedSpeed = Mathf.Lerp(10f, 100f, sliderValue);
        selectedSpeed = mappedSpeed;
        
        if (rotationSpeedNumText != null)
        {
            rotationSpeedNumText.text = Mathf.RoundToInt(mappedSpeed).ToString();
        }
        
        if (previewManager != null) 
        {
            previewManager.UpdateSpeed(selectedSpeed);
        }
        
        if (statusText != null) statusText.text = "Generating...";
    }
    
    public void ChangeAnchorPoints(int amount)
    {
        selectedAnchors += amount;
        if (selectedAnchors < 1) selectedAnchors = 1;
        if (selectedAnchors > 5) selectedAnchors = 5; 
        
        if (anchorPointsText != null) anchorPointsText.text = selectedAnchors.ToString();
        if (statusText != null) statusText.text = "Generating...";
    }

    public void ChangeAdversity(int amount)
    {
        currentAdversity += amount;
        
        if (currentAdversity < 0) currentAdversity = 0;
        if (currentAdversity > 10) currentAdversity = 10; 
        
        if (adversityText != null) adversityText.text = currentAdversity.ToString();
        if (statusText != null) statusText.text = "Generating...";
    }
    
    // old manual update method
    public void UpdateManualSelection(string category, string value)
    {
        try
        {
            switch (category)
            {
                case "RotationAngle": 
                    if(float.TryParse(value, out float angle)) 
                    {
                        selectedAngle = angle; 
                        if (previewManager != null) previewManager.UpdateAngle(angle); 
                    }
                    break;
                case "RotationSpeed": 
                    if(float.TryParse(value, out float speed)) 
                    {
                        selectedSpeed = speed; 
                        if (previewManager != null) previewManager.UpdateSpeed(speed); 
                        if (rotationSpeedNumText != null) rotationSpeedNumText.text = speed.ToString();
                    }
                    break;
                case "AnchorPoints": 
                    if(int.TryParse(value, out int anchors)) selectedAnchors = anchors; 
                    break;
                case "StoneSize":
                    if (value == "Small") selectedSize = StoneChallengeData.StoneSize.Small;
                    else if (value == "Medium") selectedSize = StoneChallengeData.StoneSize.Medium;
                    else if (value == "Large") selectedSize = StoneChallengeData.StoneSize.Large;
                    
                    if (previewManager != null) previewManager.UpdateSize(value); 
                    break;
                case "DifficultyTier":
                    if (value == "Initiate") selectedDifficulty = StoneChallengeData.SkillTier.Initiate;
                    else if (value == "Cutter") selectedDifficulty = StoneChallengeData.SkillTier.Cutter;
                    else if (value == "Carver") selectedDifficulty = StoneChallengeData.SkillTier.Carver;
                    else if (value == "Master") selectedDifficulty = StoneChallengeData.SkillTier.MasterCutter;
                    else if (value == "Mythic") selectedDifficulty = StoneChallengeData.SkillTier.Mythic;
                    break;
                case "MovementPattern":
                    selectedPatternString = value; 
                    if (previewManager != null) previewManager.UpdatePattern(value); 
                    break;
                case "LoopDuration":
                    if (value == "10s") selectedDuration = StoneChallengeData.LoopDuration.Beginner_10s;
                    else if (value == "30s") selectedDuration = StoneChallengeData.LoopDuration.Competitive_30s;
                    else if (value == "60s") selectedDuration = StoneChallengeData.LoopDuration.Advanced_60s;
                    else if (value == "120s") selectedDuration = StoneChallengeData.LoopDuration.Expert_120s;
                    break;
            }
            if (statusText != null) statusText.text = "Generating...";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Parse Error for {category}: {ex.Message}");
        }
    }

    public async void OnGenerateButtonPressed()
    {
        if (isGenerating) return;
        isGenerating = true;
        
        if (generatingStatus != null) generatingStatus.SetActive(true);
        if (statusText != null) statusText.text = "Generating..."; 

        StoneChallengeData newChallenge = new StoneChallengeData();

        newChallenge.targetStoneSize = selectedSize;
        newChallenge.minimumSkillRequired = selectedDifficulty;
        newChallenge.challengeDuration = selectedDuration;
        newChallenge.manualSpeedSlider = selectedSpeed; 
        newChallenge.jitterAmount = currentAdversity; 

        if (selectedPatternString == "Static") { newChallenge.coreMovement = StoneChallengeData.MovementType.Static; newChallenge.rotationPattern = StoneChallengeData.RotationalPattern.None; }
        else if (selectedPatternString == "Linear") { newChallenge.coreMovement = StoneChallengeData.MovementType.Linear; newChallenge.rotationPattern = StoneChallengeData.RotationalPattern.None; }
        else if (selectedPatternString == "Oscillation") { newChallenge.coreMovement = StoneChallengeData.MovementType.Oscillation; newChallenge.rotationPattern = StoneChallengeData.RotationalPattern.None; }
        else if (selectedPatternString == "Circular") { newChallenge.coreMovement = StoneChallengeData.MovementType.Static; newChallenge.rotationPattern = StoneChallengeData.RotationalPattern.Circular; }
        else if (selectedPatternString == "Chaotic") { newChallenge.coreMovement = StoneChallengeData.MovementType.Static; newChallenge.rotationPattern = StoneChallengeData.RotationalPattern.Chaotic; }
        
        int wager = 0;
        if (!string.IsNullOrEmpty(currentWagerString) && int.TryParse(currentWagerString, out int w)) wager = w;
        newChallenge.wagerAmount = wager;

        newChallenge.maxStrikesAllowed = 3;             
        
        // ==========================================
        // 🌟 NEW: Time Step Sequence Data Injection
        // ==========================================
        if (TimeStepSequenceManager.Instance != null && TimeStepSequenceManager.Instance.savedSteps.Count > 0)
        {
            // The complete sequence list saved from the UI is inserted into the main database
            newChallenge.movementSequence = new System.Collections.Generic.List<TimeStepData>(TimeStepSequenceManager.Instance.savedSteps);
            Debug.Log($"<color=green>✅ Packed {newChallenge.movementSequence.Count} steps into StoneChallengeData!</color>");
        }
        else
        {
            Debug.LogWarning("⚠️ No Time Steps were saved! Playing default movement.");
        }
        // ==========================================
        
        string jsonData = JsonUtility.ToJson(newChallenge);
        PlayerPrefs.SetString("PendingStoneChallenge", jsonData);
        PlayerPrefs.Save();
        Debug.Log("✅ [Predictor Mode]: GDD Challenge Data Saved Successfully!\n" + jsonData);
        
        StoneSize randomSize = (StoneSize)((int)selectedSize); 
        StoneDensity randomDensity = (StoneDensity)Random.Range(0, 3);
        StoneStress randomStress = (StoneStress)Random.Range(0, 3);
        FractureTolerance randomFracture = (FractureTolerance)Random.Range(0, 3);
        JadeColor randomColor = (JadeColor)Random.Range(0, 4);
        JadeQuantity randomQuantity = (JadeQuantity)Random.Range(0, 3);
        StoneAnchor randomAnchor = (StoneAnchor)Random.Range(0, 3);
        AdversityLevel randomAdversity = (AdversityLevel)Random.Range(0, 3);
        RotationPattern randomPattern = (RotationPattern)Random.Range(0, 2);
        SpinSpeed randomSpin = (SpinSpeed)Random.Range(0, 2);

        try 
        {
            await predictorController.OnGenerateButtonClick(
                randomSize, randomDensity, randomStress, randomFracture, 
                randomColor, randomQuantity, randomAnchor, randomAdversity, 
                selectedSpeed, selectedAngle, randomPattern, randomSpin, selectedAnchors
            );

            await Task.Delay(500); 

            if (StoneServer.Instance != null && StoneServer.Instance.liveStonesList.Count > 0)
            {
                var latestStone = StoneServer.Instance.liveStonesList[StoneServer.Instance.liveStonesList.Count - 1];
                latestStone.predictor_challenge_data = newChallenge; 
                
                if (GlobalStoneData.CurrentBlueprint != null)
                {
                    GlobalStoneData.CurrentBlueprint.predictor_challenge_data = newChallenge;
                }
                Debug.Log("<color=cyan>🌐 [MVC Bridge]:</color> Predictor Data successfully injected into Market Server!");
            }
            else
            {
                Debug.LogWarning("⚠️ StoneServer is missing or no stone was generated yet!");
            }
        }
        catch (System.Exception ex) { Debug.LogError($"❌ UI Error: {ex.Message}"); }
        finally
        {
            isGenerating = false;
            if (statusText != null) statusText.text = "Generated";
        }
    }
    
    public void SetLoopDurationFromDial(float durationSeconds)
    {
        if (durationSeconds <= 10f) selectedDuration = StoneChallengeData.LoopDuration.Beginner_10s;
        else if (durationSeconds <= 30f) selectedDuration = StoneChallengeData.LoopDuration.Competitive_30s;
        else if (durationSeconds <= 60f) selectedDuration = StoneChallengeData.LoopDuration.Advanced_60s;
        else selectedDuration = StoneChallengeData.LoopDuration.Expert_120s;

        // 🌟 NEW: Dial seconds sent to new mechanism!
        if (TimeStepSequenceManager.Instance != null)
        {
            TimeStepSequenceManager.Instance.ResetSequence(durationSeconds);
        }

        if (statusText != null) statusText.text = "Live Preview...";
    }
    
    // ==========================================
    // 🌟 THE MAGIC UPDATE (the original logic for the carousel)
    // ==========================================
    public void SetMovementPatternFromCarousel(string patternName)
    {
        Debug.Log($"[Predictor] Movement Pattern Set to: {patternName}");
        selectedPatternString = patternName; 

        if (previewManager != null) 
        {
            previewManager.UpdatePattern(patternName); 
        }
        
        if (statusText != null) statusText.text = "Live Preview...";
    }
    public void SetDifficultyTierFromCarousel(string tierName)
    {
        Debug.Log($"[Predictor] Difficulty Tier Set to: {tierName}");

        if (tierName == "Initiate") selectedDifficulty = StoneChallengeData.SkillTier.Initiate;
        else if (tierName == "Cutter") selectedDifficulty = StoneChallengeData.SkillTier.Cutter;
        else if (tierName == "Carver") selectedDifficulty = StoneChallengeData.SkillTier.Carver;
        else if (tierName == "Master") selectedDifficulty = StoneChallengeData.SkillTier.MasterCutter;
        else if (tierName == "Mythic") selectedDifficulty = StoneChallengeData.SkillTier.Mythic;

        if (statusText != null) statusText.text = "Live Preview...";
    }
    public void OnAdversitySliderChanged(float sliderValue)
    {
        float clampedValue = Mathf.Clamp(sliderValue, 0f, 1f);

        currentAdversity = Mathf.RoundToInt(clampedValue * 10f);

        if (adversityText != null) 
        {
            adversityText.text = currentAdversity.ToString();
        }
        if (statusText != null) 
        {
            statusText.text = "Generating...";
        }
    
        Debug.Log($"[Predictor] Slider Input: {sliderValue}, Clamped: {clampedValue}, Final Adversity: {currentAdversity}");
    }
}