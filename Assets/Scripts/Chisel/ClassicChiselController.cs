using UnityEngine;
using System.Collections;

public class ClassicChiselController : MonoBehaviour
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

    [Header("--- Rig Parts (Assign from Hierarchy) ---")]
    public Transform rootBone;       
    public Transform tiltBone;       
    public Transform extendBone;     
    public Transform chiselTip; 

    [Header("--- Base Rotation Settings (Buttons) ---")]
    public float baseTurnSpeed = 50f; 
    public Vector3 baseRotationAxis = Vector3.right; 
    private int baseRotationDirection = 0; 

    // 🌟 New movement mode panel
    public enum MovementMode { Hold_Last_Position, Return_To_Center }

    [Header("--- JOYSTICK BEHAVIOR MODE ---")]
    [Tooltip("Hold Last Position: When you release the joystick, the chisel will stay there.\nReturn To Center: When you release the joystick, the chisel will straighten itself.")]
    public MovementMode joystickMode = MovementMode.Hold_Last_Position;

    [Header("--- Head Aim Settings (Joystick) ---")]
    public float headAimSpeed = 60f;    
    public float minTiltUp = -90f;     
    public float maxTiltUp = 90f;      
    public float minTiltSide = -90f;
    public float maxTiltSide = 90f;

    [Header("--- Invert & Swap Controls ---")]
    public bool swapJoystickAxes = false;
    public bool invertVertical = true;   
    public bool invertHorizontal = true; 

    // 🌟 dropdown control panel
    public enum ControlAxis { None, X, Y, Z, Negative_X, Negative_Y, Negative_Z }

    [Header("--- EASY ADJUSTMENT PANEL ---")]
    [Tooltip("On which axis will the joystick act for left-right rotation?")]
    public ControlAxis rootTurnAxis = ControlAxis.X; 
    
    [Tooltip("On which axis will the joystick work for up-down?")]
    public ControlAxis tiltTurnAxis = ControlAxis.Z; 

    [Header("--- Strike Settings (Hit) ---")]
    [Range(0.1f, 10f)] public float maxExtensionDistance = 5f;   
    public float hitSpeed = 25f;     
    public float returnSpeed = 10f;  
    public Vector3 strikeAxis = new Vector3(0, -1, 0);

    [Header("--- Effects, Sound & Logic ---")]
    [Range(0f, 1f)] public float hitSoundVolume = 1f;
    public GameObject hitEffectPrefab;  
    public Vector3 particleRotationOffset = new Vector3(0, 0, 0);
    public AudioClip primaryHitSound;
    public GameObject secondaryHitEffectPrefab;  
    public Vector3 secondaryParticleRotationOffset = new Vector3(0, 0, 0);
    public AudioClip secondaryHitSound;

    private float currentAimUp = 0f;
    private float currentAimSide = 0f;
    private Vector3 initialExtendLocalPos;
    private bool isStriking = false;

    // Pure baseline quaternion memory for screw-lock protection
    private Quaternion initialRootRotation;
    private Quaternion initialTiltRotation;

    void Start()
    {
        if (extendBone != null) initialExtendLocalPos = extendBone.localPosition;
        
        // Fixed locking the editor's default quaternion frame to memory (no warping!).
        if (rootBone != null) initialRootRotation = rootBone.localRotation;
        if (tiltBone != null) initialTiltRotation = tiltBone.localRotation;
    }

    void Update()
    {
        if (!isEquipped) return;

        if (baseRotationDirection != 0 && rootBone != null)
        {
            float baseAngleDelta = baseRotationDirection * baseTurnSpeed * Time.deltaTime;
            initialRootRotation *= Quaternion.AngleAxis(baseAngleDelta, baseRotationAxis);
        }

        if (!isStriking)
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
            joyX = joystick.InputVector.x;
            joyY = joystick.InputVector.y;
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

        // 🌟 Joystick's new mode logic
        if (joystickMode == MovementMode.Hold_Last_Position)
        {
            // Mode 1: Stop where you left off and start from there
            if (Mathf.Abs(joyX) > 0.05f)
            {
                currentAimSide += joyX * headAimSpeed * Time.deltaTime;
                currentAimSide = Mathf.Clamp(currentAimSide, minTiltSide, maxTiltSide);
            }
            if (Mathf.Abs(joyY) > 0.05f)
            {
                currentAimUp += joyY * headAimSpeed * Time.deltaTime;
                currentAimUp = Mathf.Clamp(currentAimUp, minTiltUp, maxTiltUp);
            }
        }
        else if (joystickMode == MovementMode.Return_To_Center)
        {
            // Mode 2: Joystick will automatically spring back to center when released
            float targetSide = joyX >= 0 ? joyX * maxTiltSide : -joyX * minTiltSide;
            float targetUp = joyY >= 0 ? joyY * maxTiltUp : -joyY * minTiltUp;

            currentAimSide = Mathf.Lerp(currentAimSide, targetSide, Time.deltaTime * (headAimSpeed / 5f));
            currentAimUp = Mathf.Lerp(currentAimUp, targetUp, Time.deltaTime * (headAimSpeed / 5f));
        }

        // 🌟 Quaternion screw-lock multiplication with axis from dropdown menu!
        if (rootBone != null)
        {
            Vector3 rAxis = GetAxisVector(rootTurnAxis);
            if (rAxis != Vector3.zero) rootBone.localRotation = initialRootRotation * Quaternion.AngleAxis(currentAimSide, rAxis);
        }

        if (tiltBone != null)
        {
            Vector3 tAxis = GetAxisVector(tiltTurnAxis);
            if (tAxis != Vector3.zero) tiltBone.localRotation = initialTiltRotation * Quaternion.AngleAxis(currentAimUp, tAxis);
        }
    }

    // Engine to convert dropdown menus to vectors in Unity
    private Vector3 GetAxisVector(ControlAxis axis)
    {
        switch (axis)
        {
            case ControlAxis.X: return Vector3.right;
            case ControlAxis.Y: return Vector3.up;
            case ControlAxis.Z: return Vector3.forward;
            case ControlAxis.Negative_X: return Vector3.left;
            case ControlAxis.Negative_Y: return Vector3.down;
            case ControlAxis.Negative_Z: return Vector3.back;
            default: return Vector3.zero;
        }
    }

    // --- Strike and UI logic are kept completely intact ---
    public void RotateBaseLeft() { baseRotationDirection = -1; }
    public void RotateBaseRight() { baseRotationDirection = 1; }
    public void StopBaseRotation() { baseRotationDirection = 0; }

    public void StrikeStone()
    {
        if (!isEquipped || isStriking || extendBone == null || chiselTip == null) return;
        StartCoroutine(StrikeRoutine());
    }

    IEnumerator StrikeRoutine()
    {
        isStriking = true;
        Vector3 targetLocalPos = initialExtendLocalPos + (strikeAxis.normalized * maxExtensionDistance);
        bool impactOccurred = false;
        Vector3 previousTipPos = chiselTip.position;

        while (Vector3.Distance(extendBone.localPosition, targetLocalPos) > 0.01f && !impactOccurred)
        {
            extendBone.localPosition = Vector3.MoveTowards(extendBone.localPosition, targetLocalPos, Time.deltaTime * hitSpeed);
            Vector3 currentTipPos = chiselTip.position;
            Vector3 moveDirection = currentTipPos - previousTipPos;
            float moveDistance = moveDirection.magnitude;

            if (moveDistance > 0.0001f) 
            {
                RaycastHit[] hits = Physics.RaycastAll(previousTipPos, moveDirection.normalized, moveDistance);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.transform.IsChildOf(this.transform)) continue;

                    StoneGenerator stoneGen = hit.collider.GetComponentInParent<StoneGenerator>();
                    HitAnchor anchor = hit.collider.GetComponent<HitAnchor>();
                    if (anchor == null) anchor = hit.collider.GetComponentInParent<HitAnchor>();

                    bool isTarget = (stoneGen != null || anchor != null || hit.collider.CompareTag("Stone") || hit.collider.CompareTag("Jade"));
                    if (isTarget)
                    {
                        impactOccurred = true;
                        ProcessHitResult(hit, stoneGen, anchor);
                        break;
                    }
                }
            }
            previousTipPos = currentTipPos;
            yield return null;
        }

        if (impactOccurred) yield return new WaitForSeconds(0.05f);

        while (Vector3.Distance(extendBone.localPosition, initialExtendLocalPos) > 0.01f)
        {
            extendBone.localPosition = Vector3.MoveTowards(extendBone.localPosition, initialExtendLocalPos, Time.deltaTime * returnSpeed);
            yield return null;
        }

        extendBone.localPosition = initialExtendLocalPos; 
        isStriking = false;
    }

    private void ProcessHitResult(RaycastHit hit, StoneGenerator stoneGen, HitAnchor anchor)
    {
        TriggerHitEffects(hit.point, hit.normal);

        if (anchor != null)
        {
            if (anchor.stoneManager != null)
            {
                anchor.stoneManager.RegisterToolStrike();
                anchor.stoneManager.AnchorDestroyed(anchor);
            }
            Destroy(anchor.gameObject);
            Debug.Log("Chisel Hit Anchor Destroyed at: " + hit.point);
            return;
        }

        if (stoneGen != null)
        {
            stoneGen.RegisterToolStrike();
            Debug.Log("Chisel Hit Stone Body at: " + hit.point);
        }
    }

    private void TriggerHitEffects(Vector3 point, Vector3 normal)
    {
        if (hitEffectPrefab != null) 
        {
            GameObject fx = Instantiate(hitEffectPrefab, point, Quaternion.LookRotation(normal) * Quaternion.Euler(particleRotationOffset));
            Destroy(fx, 2f);
        }
        if (primaryHitSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1) AudioSource.PlayClipAtPoint(primaryHitSound, point, hitSoundVolume);
        if (secondaryHitEffectPrefab != null) 
        {
            GameObject secFx = Instantiate(secondaryHitEffectPrefab, point, Quaternion.LookRotation(normal) * Quaternion.Euler(secondaryParticleRotationOffset));
            Destroy(secFx, 2f);
        }
        if (secondaryHitSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1) AudioSource.PlayClipAtPoint(secondaryHitSound, point, hitSoundVolume);
    }

    public void EquipChisel() 
    { 
        isEquipped = true; 
        GyroCalibration.Calibrate();
    }
    public void UnequipChisel() { isEquipped = false; }
}