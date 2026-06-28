using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JadeCuttingGame : MonoBehaviour
{
    [Header("Stone & Spin Settings")]
    public GameObject stoneObject;      
    public float spinSpeed = 50f;       
    private bool isSpinning = true;     

    [Header("External References (Torch Manager)")]
    public GameObject innerJadeGroup;   
    
    [Header("External References")]
    public SimpleTorch simpleTorch;

    [Header("Game State Values")]
    public float totalPrize = 3200f;    
    public float wageredAmount = 500f;  
    public float timeRemaining = 150f;  
    public float scoreDecayRate = 5f;   
    public int maxStrikes = 3;          
    private int currentStrikes = 0;
    private bool isTorchActive = false;
    private bool isGameOver = false;

    [Header("UI Text Arrays")]
    public TextMeshProUGUI[] timerTexts;   
    public TextMeshProUGUI[] prizeTexts;   
    public TextMeshProUGUI[] wagerTexts;   
    public TextMeshProUGUI[] statusTexts;  

    [Header("UI Visual References")]
    public GameObject torchViewOverlay; 
    public Image[] strikeDots;          
    public Button[] toolButtons;   
    
    [Header("External Managers")]
    public ToolManager toolManager;      
    public StrikeSystem strikeSystem;    

    void Awake()
    {
        timeRemaining = 150f; 
    }

    void Start()
    {
        if (innerJadeGroup != null) innerJadeGroup.SetActive(false);
        if (torchViewOverlay != null) torchViewOverlay.SetActive(false);

        UpdateWagerTexts();
        UpdatePrizeUI();
        UpdateTimerUI();
        SetStatusText("পছন্দ করুন: সরাসরি আঘাত, ড্রেমেল নাকি করাত?");
        
        Canvas.ForceUpdateCanvases();
    }

    void Update()
    {
        if (isGameOver) return;

        if (isSpinning && stoneObject != null)
        {
            stoneObject.transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
        }

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            GameOver("সময় শেষ!");
        }

        if (isTorchActive)
        {
            totalPrize -= scoreDecayRate * Time.deltaTime; 
            UpdatePrizeUI();
        }
    }

    public void ToggleTorch()
    {
        if (isGameOver) return;
    
        isTorchActive = !isTorchActive;
        isSpinning = !isTorchActive; 

        // 🌟 জাদুকরী লক: গ্লোবাল ভেরিয়েবল আপডেট করে পুরো গেমকে জানিয়ে দেওয়া
        StoneSpinController.GlobalTorchActive = isTorchActive;

        if (simpleTorch != null)
        {
            simpleTorch.ToggleTorch(isTorchActive);
        }

        if (innerJadeGroup != null) innerJadeGroup.SetActive(isTorchActive); 
    
        SetStatusText(isTorchActive ? "TORCH INSPECTION ON" : "TORCH INSPECTION OFF");

        if (TorchInspectionManager.Instance != null)
        {
            if (isTorchActive) TorchInspectionManager.Instance.TurnOnTorch();
            else TorchInspectionManager.Instance.TurnOffTorch();
        }

        if (ToolCameraManager.Instance != null)
        {
            if (isTorchActive)
            {
                ToolCameraManager.Instance.ZoomInOnTorch();
            }
            else
            {
                ToolCameraManager.Instance.ZoomOutToDefault();
            }
        }
    }

    public void CalibrateStone()
    {
        if (isGameOver || stoneObject == null) return;
        
        isSpinning = false;
        stoneObject.transform.rotation = Quaternion.identity; 
        SetStatusText("STONE CALIBRATED & FROZEN");
    }

    public void OnActionButtonClick()
    {
        // 🌟 জাদুকরী লক: টর্চ অন থাকলে কোনো টুলস অ্যাক্টিভ হবে না!
        if (isGameOver || toolManager == null || StoneSpinController.GlobalTorchActive) return;

        if (toolManager.activeToolController != null)
        {
            toolManager.activeToolController.ToggleWorkingState();
        
            bool isActive = toolManager.activeToolController.isWorking;
            SetStatusText(isActive ? "TOOL ACTIVE - START CUTTING" : "TOOL IDLE");
        }

        isSpinning = false; 

        float successChance = toolManager.GetSuccessChance(isTorchActive); 
        float roll = Random.Range(0f, 100f);
    
        if (roll <= successChance) 
        {
            // Victory();
        }
    }
    
    public void ProcessToolHit(string toolTag) 
    {
        // 🌟 জাদুকরী লক: টর্চ অন থাকলে কোনো আঘাত কাজ করবে না!
        if (isGameOver || StoneSpinController.GlobalTorchActive) return;

        Debug.Log("Hit detected by: " + toolTag);

        float successChance = toolManager.GetSuccessChance(isTorchActive); 
        float roll = Random.Range(0f, 100f);

        if (roll <= successChance) 
        {
            SetStatusText(toolTag + " দিয়ে চমৎকার কাট!"); 
        }
        else
        {
            bool isShattered = strikeSystem.AddStrike(); 
            SetStatusText(toolTag + " আঘাত ব্যর্থ!");
        
            if (isShattered) GameOver("পাথরটি পুরোপুরি ভেঙে গেছে!");
        }
        Debug.Log(toolTag + " দিয়ে পাথর কাটা হচ্ছে!");
    }

    // --- UI আপডেট মেথডসমূহ ---
    void UpdateTimerUI() {
        int min = Mathf.FloorToInt(timeRemaining / 60);
        int sec = Mathf.FloorToInt(timeRemaining % 60);
        string timeStr = string.Format("TIME: {0:02}:{1:02}", min, sec);
        foreach (var t in timerTexts) if (t != null) t.text = timeStr;
    }

    void UpdatePrizeUI() {
        string prizeStr = "MAX: " + totalPrize.ToString("F0");
        foreach (var p in prizeTexts) if (p != null) p.text = prizeStr;
    }

    void UpdateWagerTexts() {
        string wagerStr = "WAGERED: " + wageredAmount.ToString() + " PTS";
        foreach (var w in wagerTexts) if (w != null) w.text = wagerStr;
    }

    void SetStatusText(string msg) {
        foreach (var s in statusTexts) if (s != null) s.text = msg;
    }

    void UpdateStrikesUI() {
        if (currentStrikes > 0 && currentStrikes <= strikeDots.Length)
            strikeDots[currentStrikes - 1].color = Color.red;
    }

    void Victory() { isGameOver = true; SetStatusText("SUCCESS! JADE FREED!"); DisableButtons(); }
    void GameOver(string msg) { isGameOver = true; SetStatusText(msg + " - GAME OVER"); DisableButtons(); }
    
    void DisableButtons() 
    { 
        isSpinning = false;
        foreach (var btn in toolButtons) if (btn != null) btn.interactable = false; 
    }
}