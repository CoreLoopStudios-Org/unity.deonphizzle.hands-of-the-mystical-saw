using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EzySlice;

public class SawArmController : MonoBehaviour
{
    [Header("--- Tool State ---")]
    public bool isEquipped = false;      
    private bool hasUsedStrikeThisSession = false; 

    [Header("--- UI Elements ---")]
    public VirtualJoystick virtualJoystick; 
    public Button forwardButton;
    public Button backwardButton;

    [Header("--- Input Source (Gyro / Tilt) ---")]
    public bool enableGyro = true;  
    [Range(0.5f, 5f)]
    public float gyroSensitivity = 2.0f; 

    public void SetGyroSensitivityFromSlider(float sliderValue)
    {
        gyroSensitivity = sliderValue;
    }

    [Header("--- Rig Parts (Assign from Hierarchy) ---")]
    [Tooltip("Rotates Left/Right via Joystick X")]
    public Transform rootBone;       
    [Tooltip("Tilts Up/Down (Z-axis) via Joystick Y")]
    public Transform upDownBone;     
    [Tooltip("Moves Forward/Backward via UI Buttons")]
    public Transform extendBone;     
    [Tooltip("The spinning saw blade (Saw_rotate)")]
    public Transform sawBlade;       

    [Header("--- Spin Settings ---")]
    public float bladeSpinSpeed = 1500f; 
    public Vector3 spinAxis = new Vector3(1, 0, 0); 

    [Header("--- Root Rotation Settings (Joystick X) ---")]
    [Tooltip("কোন অক্ষে রুট ঘুরবে? (যেমন: X=0, Y=1, Z=0)")]
    public Vector3 rootRotationAxis = new Vector3(0, 1, 0); 
    public float rootTurnSpeed = 60f;
    [Tooltip("বামে সর্বোচ্চ কত ডিগ্রি ঘুরবে")]
    public float minRootAngle = -45f;
    [Tooltip("ডানে সর্বোচ্চ কত ডিগ্রি ঘুরবে")]
    public float maxRootAngle = 45f;

    [Header("--- Up/Down Tilt Settings (Joystick Y) ---")]
    [Tooltip("কোন অক্ষে টিল্ট ঘুরবে? (যেমন: X=0, Y=0, Z=1)")]
    public Vector3 tiltRotationAxis = new Vector3(0, 0, 1); 
    public float tiltSpeed = 60f;    
    public float minTiltZ = -60f;    
    public float maxTiltZ = 60f;     
    public bool invertJoystickY = false;
    public bool invertJoystickX = true;

    [Header("--- Manual Extension Settings (Buttons) ---")]
    public float extendSpeed = 5f;
    [Tooltip("How far the tool can move FORWARD from its starting position")]
    public float maxForwardDistance = 15f;
    [Tooltip("How far the tool can move BACKWARD from its starting position")]
    public float maxBackwardDistance = 0f;
    public Vector3 extendAxis = Vector3.forward;
    [Tooltip("The direction of physical movement in parent space for collision checking. If zero, defaults to extendAxis.")]
    public Vector3 collisionCheckAxis = Vector3.zero;

    [Header("--- Realistic Cutting / Grinding ---")]
    public float bladeRadius = 1.5f;   
    public LayerMask stoneLayer; 
    public GameObject sawCutMarkPrefab;  
    public float grindInterval = 0.1f;   

    [Header("--- Final Slicing Settings (EzySlice) ---")]
    public Vector3 bladeCutNormal = new Vector3(0, 0, 1); 
    public float sliceCooldown = 1.0f; 
    public Material crossSectionMaterial; 
    public Material jadeCrossSectionMaterial; 

    [Header("--- Visual & Audio Effects ---")]
    public ParticleSystem sparksParticle;
    public ParticleSystem waterEffectParticle; 
    public AudioSource sawAudioSource; 
    public AudioClip sawingSound;      
    public AudioClip sliceSound;       

    // Internal State
    private float currentTiltZ = 0f;
    private float currentRootAngle = 0f; 
    private Quaternion initialRootLocalRot; 
    private Quaternion initialUpDownLocalRot; 
    private int manualMoveDirection = 0; 
    private float currentExtension = 0f; 
    private Vector3 initialExtendLocalPos;
    private float grindTimer = 0f;
    private float lastSliceTime = 0f; 

    void Start()
    {
        if (extendBone != null) initialExtendLocalPos = extendBone.localPosition;
        if (rootBone != null) initialRootLocalRot = rootBone.localRotation;
        if (upDownBone != null)
        {
            initialUpDownLocalRot = upDownBone.localRotation;
            currentTiltZ = 0f; 
        }

        // Align the slicing plane normal to the blade spin rotation axis
        bladeCutNormal = spinAxis.normalized;

        SetupButtonListeners();
        
        if (sparksParticle != null) sparksParticle.Stop();
        if (waterEffectParticle != null) waterEffectParticle.Stop(); 
    }

    void SetupButtonListeners()
    {
        if (forwardButton != null)
        {
            AddEventTrigger(forwardButton, EventTriggerType.PointerDown, MoveForward);
            AddEventTrigger(forwardButton, EventTriggerType.PointerUp, StopMovement);
        }

        if (backwardButton != null)
        {
            AddEventTrigger(backwardButton, EventTriggerType.PointerDown, MoveBackward);
            AddEventTrigger(backwardButton, EventTriggerType.PointerUp, StopMovement);
        }
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
        if (sawAudioSource != null)
        {
            sawAudioSource.mute = (PlayerPrefs.GetInt("SoundEnabled", 1) == 0);
        }

        if (!isEquipped) return;

        // 1. Constant Blade Spin
        if (sawBlade != null)
        {
            sawBlade.Rotate(spinAxis.normalized * bladeSpinSpeed * Time.deltaTime, Space.Self);
        }

        // 2. Joystick/Gyro Aim
        {
            float joyX = 0f;
            float joyY = 0f;

            if (virtualJoystick != null && virtualJoystick.InputVector != Vector2.zero)
            {
                joyX = virtualJoystick.InputVector.x;
                joyY = virtualJoystick.InputVector.y;
            }

            if (enableGyro)
            {
                Vector3 calibAccel = GyroCalibration.GetCalibratedAcceleration();
                joyX += calibAccel.x * gyroSensitivity;
                joyY += calibAccel.y * gyroSensitivity;
            }

            if (invertJoystickX) joyX = -joyX;
            if (invertJoystickY) joyY = -joyY;

            if (Mathf.Abs(joyX) >= Mathf.Abs(joyY)) joyY = 0f;
            else joyX = 0f;

            if (rootBone != null && Mathf.Abs(joyX) > 0.05f)
            {
                currentRootAngle += joyX * rootTurnSpeed * Time.deltaTime;
                currentRootAngle = Mathf.Clamp(currentRootAngle, minRootAngle, maxRootAngle);
                rootBone.localRotation = initialRootLocalRot * Quaternion.AngleAxis(currentRootAngle, rootRotationAxis.normalized);
            }

            if (upDownBone != null && Mathf.Abs(joyY) > 0.05f)
            {
                currentTiltZ -= joyY * tiltSpeed * Time.deltaTime;
                currentTiltZ = Mathf.Clamp(currentTiltZ, minTiltZ, maxTiltZ);
                upDownBone.localRotation = initialUpDownLocalRot * Quaternion.AngleAxis(currentTiltZ, tiltRotationAxis.normalized);
            }
        }

        // 3. Extend/Retract via Buttons
        if (manualMoveDirection != 0 && extendBone != null)
        {
            bool pathBlocked = false;
            float moveStep = manualMoveDirection * extendSpeed * Time.deltaTime;

            if (manualMoveDirection > 0 && sawBlade != null)
            {
                Vector3 checkAxis = collisionCheckAxis != Vector3.zero ? collisionCheckAxis : extendAxis;
                Vector3 worldMoveDir = extendBone.parent.TransformDirection(checkAxis.normalized);
                Vector3 startPos = sawBlade.position;
                Vector3 endPos = startPos + (worldMoveDir * (moveStep + bladeRadius));

                RaycastHit hit;
                if (Physics.Linecast(startPos, endPos, out hit, stoneLayer)) 
                {
                    bool isSelf = hit.collider.transform.IsChildOf(this.transform);
                    if (!isSelf)
                    {
                        pathBlocked = true;
                        ApplySawGrindEffects(hit.point, hit.normal, hit.collider);
                    }
                }
            }

            if (!pathBlocked)
            {
                currentExtension += moveStep;
                currentExtension = Mathf.Clamp(currentExtension, -maxBackwardDistance, maxForwardDistance);
                extendBone.localPosition = initialExtendLocalPos + (extendAxis.normalized * currentExtension);
            }
        }

        // 4. Collision and Sparks Detection
        HandleSawGrindingAndEffects();
    }

    public void MoveForward() { manualMoveDirection = 1; }
    public void MoveBackward() { manualMoveDirection = -1; }
    public void StopMovement() { manualMoveDirection = 0; }

    // --- Cutting & Effects Logic ---
    private void ApplySawGrindEffects(Vector3 point, Vector3 normal, Collider stoneCol)
    {
        if (sparksParticle != null && !sparksParticle.isPlaying)
        {
            sparksParticle.Play();
            sparksParticle.transform.position = point;
            sparksParticle.transform.rotation = Quaternion.LookRotation(normal);
        }
        
        if (waterEffectParticle != null && !waterEffectParticle.isPlaying) waterEffectParticle.Play();

        if (sawAudioSource != null && sawingSound != null)
        {
            if (!sawAudioSource.isPlaying || sawAudioSource.clip != sawingSound)
            {
                sawAudioSource.clip = sawingSound;
                sawAudioSource.loop = true; 
                sawAudioSource.Play();
            }
        }

        grindTimer -= Time.deltaTime;
        if (grindTimer <= 0f)
        {
            grindTimer = grindInterval; 

            if (sawCutMarkPrefab != null)
            {
                GameObject dent = Instantiate(sawCutMarkPrefab, point + (normal * 0.001f), Quaternion.LookRotation(normal));
                dent.transform.SetParent(stoneCol.transform); 
            }

            if (!hasUsedStrikeThisSession)
            {
                StoneGenerator stoneGen = stoneCol.GetComponentInParent<StoneGenerator>();
                if (stoneGen != null) 
                {
                    stoneGen.RegisterToolStrike(); 
                    hasUsedStrikeThisSession = true; 
                }
            }
        }

        // 🌟 [AUTOMATIC SLICE] ব্লেড পাথরে টাচ করা মাত্রই স্লাইস ফাংশন কল হবে!
        SliceStoneAtBladePosition();
    }

    private void HandleSawGrindingAndEffects()
    {
        if (sawBlade == null) return; 

        Collider[] hits = Physics.OverlapSphere(sawBlade.position, bladeRadius, stoneLayer);
        if (hits.Length > 0)
        {
            Collider stoneCol = hits[0];
            Vector3 closestPoint = stoneCol.ClosestPoint(sawBlade.position);
            float distance = Vector3.Distance(sawBlade.position, closestPoint);

            if (distance <= bladeRadius + 0.1f) 
            {
                Vector3 hitNormal = (sawBlade.position - closestPoint).normalized;
                if (hitNormal == Vector3.zero) hitNormal = Vector3.up;

                ApplySawGrindEffects(closestPoint, hitNormal, stoneCol);
                return; 
            }
        }
        StopAllEffects();
    }

    private void StopAllEffects()
    {
        if (sparksParticle != null && sparksParticle.isPlaying) sparksParticle.Stop();
        if (waterEffectParticle != null && waterEffectParticle.isPlaying) waterEffectParticle.Stop();
        if (sawAudioSource != null && sawAudioSource.isPlaying && sawAudioSource.clip == sawingSound) sawAudioSource.Stop();
    }

    public void SliceStoneAtBladePosition()
    {
        if (sawBlade == null || Time.time - lastSliceTime < sliceCooldown) return;

        Collider[] hits = Physics.OverlapSphere(sawBlade.position, bladeRadius, stoneLayer, QueryTriggerInteraction.Collide);
        bool didSlice = false;

        Vector3 planePoint = sawBlade.position;
        Vector3 planeNormal = sawBlade.TransformDirection(bladeCutNormal).normalized;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Stone") || hit.CompareTag("Jade") || hit.gameObject.name.Contains("Hull")) 
            {
                SliceObject(hit.gameObject, planePoint, planeNormal);
                didSlice = true;
            }
        }

        if (didSlice)
        {
            lastSliceTime = Time.time; 
            if (sawAudioSource != null && sliceSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1) sawAudioSource.PlayOneShot(sliceSound);
        }
    }

    void SliceObject(GameObject target, Vector3 planePoint, Vector3 planeNormal)
    {
        SlicedHull hull = target.Slice(planePoint, planeNormal);
        
        if (hull != null)
        {
            Material crossMat = target.CompareTag("Jade") && jadeCrossSectionMaterial != null 
                ? jadeCrossSectionMaterial 
                : (crossSectionMaterial != null ? crossSectionMaterial : target.GetComponent<MeshRenderer>().sharedMaterial);

            GameObject upperHull = hull.CreateUpperHull(target, crossMat);
            GameObject lowerHull = hull.CreateLowerHull(target, crossMat);

            Vector3 upperSize = upperHull.GetComponent<MeshRenderer>().bounds.size;
            Vector3 lowerSize = lowerHull.GetComponent<MeshRenderer>().bounds.size;

            bool keepUpper = (upperSize.x * upperSize.y * upperSize.z) >= (lowerSize.x * lowerSize.y * lowerSize.z);

            GameObject pieceToStay = keepUpper ? upperHull : lowerHull;
            GameObject pieceToFall = keepUpper ? lowerHull : upperHull;

            MeshFilter targetFilter = target.GetComponent<MeshFilter>();
            Mesh oldMesh = targetFilter.sharedMesh;
            Mesh newMesh = pieceToStay.GetComponent<MeshFilter>().sharedMesh;

            targetFilter.sharedMesh = newMesh;
            target.GetComponent<MeshRenderer>().sharedMaterials = pieceToStay.GetComponent<MeshRenderer>().sharedMaterials; 

            MeshCollider targetCollider = target.GetComponent<MeshCollider>();
            if (targetCollider != null) { targetCollider.sharedMesh = null; targetCollider.sharedMesh = newMesh; }

            if (oldMesh != null && !oldMesh.name.Contains("Original")) Destroy(oldMesh);

            for (int i = target.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = target.transform.GetChild(i);
                if (child.GetComponent<HitAnchor>() != null)
                {
                    float dot = Vector3.Dot(child.position - planePoint, planeNormal);
                    if (((dot >= 0) && !keepUpper) || (!(dot >= 0) && keepUpper)) child.SetParent(pieceToFall.transform);
                }
            }
            
            Destroy(pieceToStay); 
            SetupSlicedComponent(pieceToFall, target.transform.lossyScale, target.transform.position, target.transform.rotation);
        }
    }

    void SetupSlicedComponent(GameObject slicedObject, Vector3 originalScale, Vector3 originalPos, Quaternion originalRot)
    {
        slicedObject.transform.position = originalPos;
        slicedObject.transform.rotation = originalRot;
        slicedObject.transform.localScale = originalScale; 

        foreach (AudioSource audio in slicedObject.GetComponentsInChildren<AudioSource>()) Destroy(audio);

        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;
        
        slicedObject.layer = 2; 
        slicedObject.tag = "Untagged"; 
        
        rb.mass = 15f; 
        rb.AddExplosionForce(40f, originalPos + Vector3.up * 0.1f, 2f); 
        
        Destroy(slicedObject, 3f); 
    }

    public void EquipSaw()
    {
        isEquipped = true;
        hasUsedStrikeThisSession = false; 
        GyroCalibration.Calibrate();
        if (forwardButton != null && forwardButton.transform.parent != null)
        {
            if (forwardButton.transform.parent.name.StartsWith("Forward-Backward"))
            {
                forwardButton.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                forwardButton.gameObject.SetActive(true);
                if (backwardButton != null) backwardButton.gameObject.SetActive(true);
            }
        }
    }

    public void UnequipSaw()
    {
        isEquipped = false;
        StopAllEffects();
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
}