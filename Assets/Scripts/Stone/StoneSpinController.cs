using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StoneSpinController : MonoBehaviour
{
    private Transform targetStone; 
    private StoneChallengeData predictorData; 
    private bool isPredictorMode = false; 

    [Header("Spin Settings (GDD Rules)")]
    public float currentSpeed = 0f;
    public float currentAngle = 0f;
    public Vector3 spinAxis = Vector3.zero;
    
    [Header("Advanced Torch Settings")]
    public Material xRayMaterial; 
    public int maxTorchUses = 3; 
    
    private Material originalMaterial;
    private MeshRenderer stoneRenderer;
    private int currentTorchUses = 0;

    [Header("State")]
    public bool isSpinning = true;
    public bool isCommitFrozen = false; 
    private bool isTorchActive = false;
    public static bool GlobalTorchActive = false;   

    [Header("Mobile UI Buttons")]
    public Button torchButton;  
    public Button commitButton; 

    private Camera mainCam;
    private Vector3 originalPosition;
    
    // ==========================================
    // 🌟 NEW: Sequence Timer Variables
    // ==========================================
    private float globalTimer = 0f;          // পুরো লুপের জন্য টাইমার
    private float stepTimer = 0f;            // বর্তমান স্টেপের জন্য টাইমার
    private int currentStepIndex = 0;        // এখন কত নাম্বার স্টেপে আছে
    private string activePattern = "Static"; // বর্তমানে কোন মুভমেন্ট চলছে

    void Start()
    {
        if (currentSpeed == 0f)
        {
            float[] speeds = { 40f, 80f, 120f }; 
            currentSpeed = speeds[Random.Range(0, speeds.Length)];
        }

        if (spinAxis == Vector3.zero)
        {
            spinAxis = new Vector3(Random.Range(-0.3f, 0.3f), 1f, Random.Range(-0.3f, 0.3f)).normalized;
        }

        mainCam = Camera.main;

        isCommitFrozen = false;
        isSpinning = true;
        isTorchActive = false;
        GlobalTorchActive = false;
    }

    // 🌟 THE MVC RECEIVER
    public void ReceiveStoneData(Transform stoneTransform, StoneChallengeData data, bool isPredictor, float blueprintAngle = 0f, float blueprintSpeed = 0f)
    {
        targetStone = stoneTransform;
        originalPosition = targetStone.position;
        
        Rigidbody rb = targetStone.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;  
            rb.constraints = RigidbodyConstraints.None; 
        }
        
        stoneRenderer = targetStone.GetComponent<MeshRenderer>();
        if (stoneRenderer != null) 
        {
            originalMaterial = stoneRenderer.material;
        }

        predictorData = data;
        isPredictorMode = isPredictor;
        
        if (isPredictorMode && predictorData != null)
        {
            currentSpeed = predictorData.manualSpeedSlider;
            
            // 🌟 NEW: যদি নতুন Sequence List থাকে, তবে প্রথম স্টেপটা লোড করো
            if (predictorData.movementSequence != null && predictorData.movementSequence.Count > 0)
            {
                currentStepIndex = 0;
                stepTimer = predictorData.movementSequence[0].duration;
                activePattern = predictorData.movementSequence[0].movementPattern;
                Debug.Log($"<color=yellow>⏳ Sequence Started! Step 1: {stepTimer}s | {activePattern}</color>");
            }
            else
            {
                // পুরানো সিঙ্গেল মুভমেন্ট সিস্টেম (ব্যাকআপ)
                activePattern = GetLegacyPatternString(predictorData);
            }
        }
        else
        {
            currentSpeed = blueprintSpeed > 0 ? blueprintSpeed : 20f;
        }
        
        currentAngle = blueprintAngle;
        
        if (targetStone != null)
        {
            targetStone.localRotation = Quaternion.Euler(currentAngle, 0, 0);
        }

        Debug.Log($"<color=cyan>🎯 MVC CONNECTED:</color> Spin Controller received data! Speed: {currentSpeed}, Angle: {currentAngle}");
    }

    void Update()
    {
        if (targetStone == null) return;

        if (isSpinning && !isCommitFrozen && !isTorchActive)
        {
            if (isPredictorMode && predictorData != null) 
            {
                HandleSequenceTimer(); // 🌟 NEW: টাইমার চেক করবে
                ApplyPredictorMovement(); // মুভমেন্ট চালাবে
            }
            else 
            {
                targetStone.Rotate(spinAxis * currentSpeed * Time.deltaTime, Space.World);
            }
        }

        if (isTorchActive && !isCommitFrozen) UpdateTorchPosition();

        if (Input.GetKeyDown(KeyCode.Space) && !isCommitFrozen)
        {
            if (!isTorchActive && currentTorchUses < maxTorchUses) ToggleTorch();
            else if (isTorchActive) ToggleTorch();
        }
        if (Input.GetKeyDown(KeyCode.Return) && !isCommitFrozen) CommitFreeze();
    }

    // ==========================================
    // 🌟 NEW: THE SEQUENCE LOGIC
    // ==========================================
    private void HandleSequenceTimer()
    {
        // যদি লিস্ট না থাকে, তবে কিছুই করার দরকার নেই (পুরানো সিস্টেম চলবে)
        if (predictorData.movementSequence == null || predictorData.movementSequence.Count == 0) return;

        stepTimer -= Time.deltaTime; // টাইমার কমানো হচ্ছে

        // বর্তমান স্টেপের সময় শেষ হলে
        if (stepTimer <= 0)
        {
            currentStepIndex++; // পরের স্টেপে যাও

            // যদি লিস্ট শেষ হয়ে যায়, তাহলে আবার প্রথম থেকে শুরু করো (লুপ)
            if (currentStepIndex >= predictorData.movementSequence.Count)
            {
                currentStepIndex = 0;
            }

            // নতুন স্টেপের ডাটা সেট করা
            stepTimer = predictorData.movementSequence[currentStepIndex].duration;
            activePattern = predictorData.movementSequence[currentStepIndex].movementPattern;
            
            Debug.Log($"<color=yellow>🔄 Sequence Changed! Now running Step {currentStepIndex + 1}: {stepTimer}s | {activePattern}</color>");
        }
    }

    void ApplyPredictorMovement()
    {
        globalTimer += Time.deltaTime; // মুভমেন্ট স্মুথ রাখার জন্য গ্লোবাল টাইমার
        Vector3 currentTargetPosition = originalPosition;
        float moveSpeed = predictorData.manualSpeedSlider > 0 ? predictorData.manualSpeedSlider * 0.1f : 1f;
        float rotSpeed = predictorData.manualSpeedSlider > 0 ? predictorData.manualSpeedSlider : 20f;
        
        bool shouldUpdatePosition = false; 

        // 🌟 NEW: এখন activePattern অনুযায়ী মুভমেন্ট হবে (Enum এর বদলে String দিয়ে চেক)
        if (activePattern == "Oscillation")
        {
            float offset = Mathf.Sin(globalTimer * moveSpeed) * 2f; 
            currentTargetPosition += new Vector3(offset, 0, 0);
            shouldUpdatePosition = true; 
        }
        else if (activePattern == "Linear")
        {
            float offset = Mathf.PingPong(globalTimer * (currentSpeed * 0.05f), 2f) - 1f; 
            currentTargetPosition = originalPosition + new Vector3(offset, 0, 0);
            shouldUpdatePosition = true; 
        }
        else if (activePattern == "Circular")
        {
            targetStone.Rotate(Vector3.up, rotSpeed * Time.deltaTime);
        }
        else if (activePattern == "Chaotic")
        {
            float chaoticX = (Mathf.PerlinNoise(globalTimer, 0) - 0.5f) * rotSpeed;
            float chaoticY = (Mathf.PerlinNoise(0, globalTimer) - 0.5f) * rotSpeed;
            targetStone.Rotate(new Vector3(chaoticX, chaoticY, 0) * Time.deltaTime);
        }
        // "Static" হলে পাথর এক জায়গায় দাঁড়িয়ে থাকবে

        // 🌟 স্মুথ জিটার লজিক (Adversity)
        if (predictorData.jitterAmount > 0)
        {
            float jitterStr = predictorData.jitterAmount * 0.015f; 
            float shakeX = (Mathf.PerlinNoise(globalTimer * 15f, 0f) - 0.5f) * jitterStr;
            float shakeY = (Mathf.PerlinNoise(0f, globalTimer * 15f) - 0.5f) * jitterStr;
            
            currentTargetPosition += new Vector3(shakeX, shakeY, 0);
            shouldUpdatePosition = true; 
        }

        if (shouldUpdatePosition)
        {
            targetStone.position = currentTargetPosition;
        }
    }

    // ব্যাকআপ মেথড: যদি প্লেয়ার পুরনো UI দিয়ে মুভমেন্ট সেট করে থাকে
    private string GetLegacyPatternString(StoneChallengeData data)
    {
        if (data.coreMovement == StoneChallengeData.MovementType.Oscillation) return "Oscillation";
        if (data.coreMovement == StoneChallengeData.MovementType.Linear) return "Linear";
        if (data.rotationPattern == StoneChallengeData.RotationalPattern.Circular) return "Circular";
        if (data.rotationPattern == StoneChallengeData.RotationalPattern.Chaotic) return "Chaotic";
        return "Static";
    }

    void UpdateTorchPosition()
    {
        if (mainCam == null || stoneRenderer == null || targetStone == null) return;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.transform == targetStone || hit.collider.transform.IsChildOf(targetStone))
            {
                stoneRenderer.material.SetVector("_TorchPosition", hit.point);
            }
        }
    }

    public void ToggleTorch()
    {
        if (isCommitFrozen || targetStone == null) return; 

        isTorchActive = !isTorchActive;
        GlobalTorchActive = isTorchActive; 
        
        if (isTorchActive)
        {
            currentTorchUses++;
            isSpinning = false; 

            if (TorchInspectionManager.Instance != null)
            {
                TorchInspectionManager.Instance.TurnOnTorch();
            }
            else if (stoneRenderer != null && xRayMaterial != null) 
            {
                stoneRenderer.material = xRayMaterial; 
                UpdateTorchPosition(); 
            }

            if (ToolCameraManager.Instance != null)
            {
                ToolCameraManager.Instance.ZoomInOnTorch();
            }
        }
        else
        {
            isSpinning = true; 

            if (TorchInspectionManager.Instance != null)
            {
                TorchInspectionManager.Instance.TurnOffTorch();
            }
            else if (stoneRenderer != null && originalMaterial != null) 
            {
                stoneRenderer.material = originalMaterial; 
            }

            if (currentTorchUses >= maxTorchUses && torchButton != null) torchButton.interactable = false;

            if (ToolCameraManager.Instance != null)
            {
                ToolCameraManager.Instance.ZoomOutToDefault();
            }
        }
    }

    public void CommitFreeze()
    {
        if (isCommitFrozen || targetStone == null) return; 

        if(isTorchActive) 
        {
            isTorchActive = false;
            if(stoneRenderer != null && originalMaterial != null) stoneRenderer.material = originalMaterial; 
        }
        
        isSpinning = false;
        isCommitFrozen = true;

        if(torchButton != null) torchButton.gameObject.SetActive(false);
        if(commitButton != null) commitButton.gameObject.SetActive(false);
    }
}