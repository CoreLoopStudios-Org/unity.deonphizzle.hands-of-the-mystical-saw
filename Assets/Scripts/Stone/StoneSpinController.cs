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
    private float globalTimer = 0f;          // Timer for the whole loop
    private float stepTimer = 0f;            // Timer for current step
    private int currentStepIndex = 0;        // How many numbers are in the step now?
    private string activePattern = "Static"; // No movement currently in progress

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
            
            // 🌟 NEW: If there is a new Sequence List, load the first step
            if (predictorData.movementSequence != null && predictorData.movementSequence.Count > 0)
            {
                currentStepIndex = 0;
                stepTimer = predictorData.movementSequence[0].duration;
                activePattern = predictorData.movementSequence[0].movementPattern;
                Debug.Log($"<color=yellow>⏳ Sequence Started! Step 1: {stepTimer}s | {activePattern}</color>");
            }
            else
            {
                // old single movement system (backup)
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
                HandleSequenceTimer(); // 🌟 NEW: will check the timer
                ApplyPredictorMovement(); // will execute the movement
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
        // If there is no list, nothing needs to be done (old system will run)
        if (predictorData.movementSequence == null || predictorData.movementSequence.Count == 0) return;

        stepTimer -= Time.deltaTime; // Decrementing the timer

        // When the current step times out
        if (stepTimer <= 0)
        {
            currentStepIndex++; // Go to next step

            // if the list is finished, start again from the beginning (loop)
            if (currentStepIndex >= predictorData.movementSequence.Count)
            {
                currentStepIndex = 0;
            }

            // Set the new step's data
            stepTimer = predictorData.movementSequence[currentStepIndex].duration;
            activePattern = predictorData.movementSequence[currentStepIndex].movementPattern;
            
            Debug.Log($"<color=yellow>🔄 Sequence Changed! Now running Step {currentStepIndex + 1}: {stepTimer}s | {activePattern}</color>");
        }
    }

    void ApplyPredictorMovement()
    {
        globalTimer += Time.deltaTime; // Global timer to keep movement smooth
        Vector3 currentTargetPosition = originalPosition;
        float moveSpeed = predictorData.manualSpeedSlider > 0 ? predictorData.manualSpeedSlider * 0.1f : 1f;
        float rotSpeed = predictorData.manualSpeedSlider > 0 ? predictorData.manualSpeedSlider : 20f;
        
        bool shouldUpdatePosition = false; 

        // 🌟 NEW: Now movement according to activePattern (check with String instead of Enum)
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
        // If "Static" the stone will stay in one place

        // 🌟 Smooth Jitter Logic (Adversity)
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

    // Backup method: if player has movement set with old UI
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