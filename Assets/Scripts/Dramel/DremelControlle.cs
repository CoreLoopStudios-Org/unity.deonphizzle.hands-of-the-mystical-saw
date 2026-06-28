using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DremelToolController : MonoBehaviour
{
    [Header("--- Tool State ---")]
    public bool isEquipped = false;

    [Header("--- UI Elements ---")]
    public VirtualJoystick joystick; 

    [Header("--- Input Source (Gyro / Tilt) ---")]
    public bool enableGyro = true;  
    [Range(0.5f, 5f)]
    public float gyroSensitivity = 2.0f; 

    public void SetGyroSensitivityFromSlider(float sliderValue)
    {
        gyroSensitivity = sliderValue;
    }

    [Header("--- Manual Movement UI (Assign in Inspector) ---")]
    public Button forwardButton;
    public Button backwardButton;

    [Header("--- Rig Parts (Assign from Hierarchy) ---")]
    public Transform rootBone;       
    public Transform upDownBone; 
    public Transform extendBone; 
    public Transform dremelTip;  
    [Tooltip("আপনার পুরো ড্রামেল মডেলটি এখানে দিন যাতে সে নিজেকে নিজে কলিশন না করে (Ignore Self)")]
    public GameObject toolRoot; 

    private AudioSource localAudioSource;

    [Header("--- Spinning Settings ---")]
    public float rotationSpeed = 3000f;  
    public Vector3 spinAxis = new Vector3(0, 0, 1); 

    [Header("--- Manual Movement Settings (Buttons) ---")]
    [Tooltip("Define the axis that moves the dremel straight towards the stone (e.g., X:1, Y:0, Z:0)")]
    public Vector3 manualMoveAxis = Vector3.forward; 
    public float manualMoveSpeed = 2f; 
    [Tooltip("ড্রামেল মাথার কতটুকু সামনে কলিশন হবে (0 মানে একদম মাথায়)")]
    public float collisionOffset = 0.05f;
    
    [Tooltip("How far the tool can move FORWARD from its starting position")]
    public float maxForwardDistance = 5f;
    [Tooltip("How far the tool can move BACKWARD from its starting position")]
    public float maxBackwardDistance = 2f;
    
    private int manualMoveDirection = 0; 

    [Header("--- Head Aim Settings (Joystick) ---")]
    public float headAimSpeed = 60f;    
    public float minTiltUp = -90f;     
    public float maxTiltUp = 90f;      
    public float minTiltSide = -90f;
    public float maxTiltSide = 90f;

    [Header("--- Invert & Swap Controls ---")]
    public bool swapJoystickAxes = false;
    public bool invertVertical = false;   
    public bool invertHorizontal = false; 

    [Header("--- Grind Settings (Auto Strike) ---")]
    [Tooltip("Max distance the tool automatically strikes when CUT is pressed")]
    [Range(0.1f, 10f)] public float autoStrikeDistance = 5f;   
    public float approachSpeed = 25f;     
    public float returnSpeed = 10f;  
    public Vector3 strikeAxis = Vector3.forward;

    [Header("--- Grinding Logic & Effects ---")]
    [Tooltip("Interval between dents and strikes while grinding.")]
    public float grindInterval = 0.15f;
    public GameObject dentPrefab;
    [Range(0f, 1f)] public float hitSoundVolume = 1f;
    public GameObject hitEffectPrefab;  
    public Vector3 particleRotationOffset = new Vector3(0, 0, 0);
    public AudioClip primaryHitSound;

    // --- State Variables ---
    private float currentAimUp = 0f;
    private float currentAimSide = 0f;
    private Vector3 initialExtendLocalPos;
    private bool isExtending = false;
    private float grindTimer = 0f;
    private float currentManualExtension = 0f; 
    private Vector3 previousManualTipPos;
    private bool isGrindingThisFrame = false;

    // 🌟 FIX: Using Quaternions instead of raw Euler Angles to prevent Gimbal Lock & Snapping
    private Quaternion initialRootRotation;
    private Quaternion initialUpDownRotation;

    void Start()
    {
        if (extendBone != null)
        {
            initialExtendLocalPos = extendBone.localPosition;
        }
            
        // 🌟 FIX: Storing initial Quaternions directly
        if (rootBone != null)
        {
            initialRootRotation = rootBone.localRotation;
            currentAimSide = 0f; 
        }
        
        if (upDownBone != null)
        {
            initialUpDownRotation = upDownBone.localRotation;
            currentAimUp = 0f; 
        }

        if (dremelTip != null)
        {
            previousManualTipPos = dremelTip.position;
        }

        localAudioSource = GetComponent<AudioSource>();
        if (localAudioSource == null) localAudioSource = gameObject.AddComponent<AudioSource>();

        SetupButtonListeners();
    }

    void SetupButtonListeners()
    {
        // Removed manual button listeners to support click-to-strike
    }

    private void AddEventTrigger(Button btn, EventTriggerType type, System.Action action)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => { action(); });
        trigger.triggers.Add(entry);
    }

    void Update()
    {
        isGrindingThisFrame = false;

        if (dremelTip != null && isEquipped)
        {
            dremelTip.Rotate(spinAxis.normalized * rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (!isEquipped) 
        {
            if (manualMoveDirection != 0) Debug.LogWarning("Dremel: Cannot move - Tool is NOT equipped!");
            return;
        }

        // Click-to-Strike Logic...
        if (isEquipped && !isExtending)
        {
            if (Input.GetMouseButtonDown(0) && (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
                RaycastHit bestHit = default;
                bool hitFound = false;
                float minDistance = float.MaxValue;

                foreach (RaycastHit candidateHit in hits)
                {
                    if (candidateHit.collider.transform.IsChildOf(this.transform)) continue;
                    if (toolRoot != null && candidateHit.collider.transform.IsChildOf(toolRoot.transform)) continue;

                    StoneGenerator stoneGen = candidateHit.collider.GetComponentInParent<StoneGenerator>();
                    HitAnchor anchor = candidateHit.collider.GetComponent<HitAnchor>();
                    if (anchor == null) anchor = candidateHit.collider.GetComponentInParent<HitAnchor>();

                    bool isTarget = (stoneGen != null || anchor != null || candidateHit.collider.CompareTag("Stone") || candidateHit.collider.CompareTag("Jade"));

                    if (isTarget)
                    {
                        if (candidateHit.distance < minDistance)
                        {
                            minDistance = candidateHit.distance;
                            bestHit = candidateHit;
                            hitFound = true;
                        }
                    }
                }

                if (hitFound)
                {
                    StartGrinding();
                }
            }
        }

        // Manual Forward/Backward Movement...
        if (manualMoveDirection != 0 && extendBone != null && !isExtending)
        {
            bool pathBlocked = false;
            float moveStep = manualMoveDirection * manualMoveSpeed * Time.deltaTime;

            if (manualMoveDirection > 0 && dremelTip != null)
            {
                Vector3 worldMoveDir = extendBone.parent.TransformDirection(manualMoveAxis.normalized);
                float backOffset = 2.0f; 
                Vector3 rayStartPos = dremelTip.position - (worldMoveDir * backOffset);
                float checkDistance = backOffset + moveStep + collisionOffset;

                RaycastHit hit;
                if (Physics.Raycast(rayStartPos, worldMoveDir, out hit, checkDistance)) 
                {
                    bool isSelf = false;
                    if (toolRoot != null && hit.collider.transform.IsChildOf(toolRoot.transform)) isSelf = true;
                    if (hit.collider.transform.IsChildOf(this.transform)) isSelf = true;

                    if (!isSelf)
                    {
                        StoneGenerator stoneGen = hit.collider.GetComponentInParent<StoneGenerator>();
                        HitAnchor anchor = hit.collider.GetComponent<HitAnchor>();
                        if (anchor == null) anchor = hit.collider.GetComponentInParent<HitAnchor>();

                        bool isTarget = (stoneGen != null || anchor != null || hit.collider.CompareTag("Stone") || hit.collider.CompareTag("Jade"));

                        if (isTarget)
                        {
                            pathBlocked = true;
                            float actualDistFromTip = hit.distance - backOffset;

                            if (actualDistFromTip > 0)
                            {
                                float safeDistance = Mathf.Max(0, actualDistFromTip - collisionOffset);
                                if (safeDistance > 0.001f) currentManualExtension += safeDistance;
                            }
                            else
                            {
                                currentManualExtension += (actualDistFromTip - collisionOffset);
                            }

                            ApplyGrindEffectsAtPoint(hit, stoneGen, anchor);
                        }
                    }
                }
            }

            if (!pathBlocked || manualMoveDirection < 0)
            {
                if (!pathBlocked) currentManualExtension += moveStep;
            }

            currentManualExtension = Mathf.Clamp(currentManualExtension, -maxBackwardDistance, maxForwardDistance);
            Vector3 movementAxis = manualMoveAxis.normalized;
            extendBone.localPosition = initialExtendLocalPos + (movementAxis * currentManualExtension);
        }

        // Head Aiming...
        if (!isExtending)
        {
            HandleHeadAiming();
        }
    }

    void HandleHeadAiming()
    {
        float joyX = 0f;
        float joyY = 0f;

        if (joystick != null)
        {
            // 🌟 Safety clamp to prevent virtual joystick raw bug spikes
            joyX = Mathf.Clamp(joystick.InputVector.x, -1f, 1f);
            joyY = Mathf.Clamp(joystick.InputVector.y, -1f, 1f);
        }

        if (enableGyro)
        {
            Vector3 calibAccel = GyroCalibration.GetCalibratedAcceleration();
            joyX += calibAccel.x * gyroSensitivity;
            joyY += calibAccel.y * gyroSensitivity;
        }

        if (swapJoystickAxes)
        {
            float temp = joyX;
            joyX = joyY;
            joyY = temp;
        }

        if (invertHorizontal) joyX = -joyX;
        if (invertVertical) joyY = -joyY;

        // Calculate input but apply rotation dynamically every frame
        if (Mathf.Abs(joyX) > 0.05f || Mathf.Abs(joyY) > 0.05f)
        {
            currentAimSide += joyX * headAimSpeed * Time.deltaTime; 
            currentAimUp -= joyY * headAimSpeed * Time.deltaTime; 

            currentAimSide = Mathf.Clamp(currentAimSide, minTiltSide, maxTiltSide);
            currentAimUp = Mathf.Clamp(currentAimUp, minTiltUp, maxTiltUp);
        }

        if (rootBone != null)
        {
            // 🌟 FIX: এখন Root Bone তার X-axis বরাবর ঘুরবে
            rootBone.localRotation = initialRootRotation * Quaternion.Euler(currentAimSide, 0, 0);
        }

        if (upDownBone != null)
        {
            // Applies Up/Down tilt relative to initial Z axis
            upDownBone.localRotation = initialUpDownRotation * Quaternion.Euler(0, 0, currentAimUp);
        }
    }

    // ... [Rest of your script remains exactly the same: MoveBackward, StartGrinding, etc.]

    public void MoveBackward() { manualMoveDirection = -1; }
    public void MoveForward() { manualMoveDirection = 1; }
    public void StopMovement() { manualMoveDirection = 0; }

    public void StartGrinding()
    {
        if (!isEquipped) return;
        if (!isExtending && extendBone != null && dremelTip != null)
        {
            StartCoroutine(GrindingRoutine());
        }
    }

    IEnumerator GrindingRoutine()
    {
        isExtending = true;
        Vector3 targetLocalPos = initialExtendLocalPos + (strikeAxis.normalized * autoStrikeDistance);
        bool impactOccurred = false;
        Vector3 previousTipPos = dremelTip.position;

        while (Vector3.Distance(extendBone.localPosition, targetLocalPos) > 0.01f && !impactOccurred)
        {
            extendBone.localPosition = Vector3.MoveTowards(extendBone.localPosition, targetLocalPos, Time.deltaTime * approachSpeed);

            Vector3 currentTipPos = dremelTip.position;
            Vector3 moveDirection = currentTipPos - previousTipPos;
            float moveDistance = moveDirection.magnitude;

            if (moveDistance > 0.0001f)
            {
                RaycastHit[] hits = Physics.RaycastAll(previousTipPos, moveDirection.normalized, moveDistance);
                
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.transform.IsChildOf(this.transform) || (toolRoot != null && hit.collider.transform.IsChildOf(toolRoot.transform)))
                    {
                        continue;
                    }

                    StoneGenerator stoneGen = hit.collider.GetComponentInParent<StoneGenerator>();
                    HitAnchor anchor = hit.collider.GetComponent<HitAnchor>();
                    if (anchor == null) anchor = hit.collider.GetComponentInParent<HitAnchor>();

                    bool isTarget = (stoneGen != null || anchor != null || hit.collider.CompareTag("Stone") || hit.collider.CompareTag("Jade"));

                    if (isTarget)
                    {
                        impactOccurred = true;
                        float grindDuration = 1.0f; 
                        float elapsed = 0;
                        
                        while (elapsed < grindDuration)
                        {
                            elapsed += Time.deltaTime;
                            ApplyGrindEffectsAtPoint(hit, stoneGen, anchor);
                            yield return null;
                        }
                        break;
                    }
                }
            }

            previousTipPos = currentTipPos;
            yield return null;
        }

        while (Vector3.Distance(extendBone.localPosition, initialExtendLocalPos) > 0.01f)
        {
            extendBone.localPosition = Vector3.MoveTowards(extendBone.localPosition, initialExtendLocalPos, Time.deltaTime * returnSpeed);
            yield return null;
        }

        extendBone.localPosition = initialExtendLocalPos; 
        isExtending = false;
    }

    private void ApplyGrindEffectsAtPoint(RaycastHit hit, StoneGenerator stoneGen, HitAnchor anchor)
    {
        isGrindingThisFrame = true;
        grindTimer -= Time.deltaTime;
        
        if (grindTimer <= 0f)
        {
            grindTimer = grindInterval;

            TriggerHitEffects(hit.point, hit.normal);

            if (anchor != null)
            {
                if (anchor.stoneManager != null)
                {
                    anchor.stoneManager.RegisterToolStrike();
                    anchor.stoneManager.AnchorDestroyed(anchor);
                }
            }
            else if (stoneGen != null)
            {
                stoneGen.RegisterToolStrike();
            }

            if (dentPrefab != null)
            {
                GameObject dent = Instantiate(dentPrefab, hit.point + (hit.normal * 0.001f), Quaternion.LookRotation(hit.normal));
                dent.transform.SetParent(hit.collider.transform);   
            }
        }
    }

    private void TriggerHitEffects(Vector3 point, Vector3 normal)
    {
        if (hitEffectPrefab != null) 
        {
            GameObject fx = Instantiate(hitEffectPrefab, point, Quaternion.LookRotation(normal) * Quaternion.Euler(particleRotationOffset));
            Destroy(fx, 2f);
        }
    }

    private void LateUpdate()
    {
        if (localAudioSource != null)
        {
            localAudioSource.mute = (PlayerPrefs.GetInt("SoundEnabled", 1) == 0);
        }

        if (isGrindingThisFrame && isEquipped)
        {
            if (localAudioSource != null && primaryHitSound != null)
            {
                if (!localAudioSource.isPlaying || localAudioSource.clip != primaryHitSound)
                {
                    localAudioSource.clip = primaryHitSound;
                    localAudioSource.loop = true;
                    localAudioSource.volume = hitSoundVolume;
                    localAudioSource.Play();
                }
            }
        }
        else
        {
            if (localAudioSource != null && localAudioSource.isPlaying)
            {
                localAudioSource.Stop();
            }
        }
    }

    public void EquipDremel() 
    { 
        isEquipped = true; 
        GyroCalibration.Calibrate();
        if (forwardButton != null && forwardButton.transform.parent != null)
        {
            if (forwardButton.transform.parent.name.StartsWith("Forward-Backward"))
            {
                forwardButton.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                forwardButton.gameObject.SetActive(false);
                if (backwardButton != null) backwardButton.gameObject.SetActive(false);
            }
        }
    }
    
    public void UnequipDremel() 
    { 
        isEquipped = false; 
        if (localAudioSource != null && localAudioSource.isPlaying)
        {
            localAudioSource.Stop();
        }
    }
}