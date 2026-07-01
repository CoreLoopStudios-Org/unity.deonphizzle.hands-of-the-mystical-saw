using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NewHammerController : MonoBehaviour
{
    [Header("--- Tool State ---")]
    public bool isEquipped = true;      

    [Header("--- UI Elements ---")]
    public VirtualJoystick virtualJoystick; 
    public Button hitUIButton; 

    [Header("--- Input Source (Gyro / Tilt) ---")]
    public bool enableGyro = true;  
    [Range(0.5f, 5f)]
    public float gyroSensitivity = 2.0f; 

    public void SetGyroSensitivityFromSlider(float sliderValue)
    {
        gyroSensitivity = sliderValue;
    }

    [Header("--- New Articulated Hammer Setup ---")]
    public Transform rootBone;       
    public Transform extendBone;       
    public Transform topBone;          
    public Transform hammerTip;         
    
    [Header("--- Root Rotation Settings (Joystick X) ---")]
    public Vector3 rootRotationAxis = new Vector3(1, 0, 0); 
    public float rootTurnSpeed = 60f;
    public float minRootAngle = -360f; 
    public float maxRootAngle = 360f;

    [Header("--- Up/Down Tilt Settings (Joystick Y) ---")]
    public Vector3 tiltRotationAxis = new Vector3(0, 0, 1);
    public float tiltSpeed = 60f;    
    public float minTiltZ = -180f;    
    public float maxTiltZ = -20f;     
    public bool invertJoystickY = false;
    public bool invertJoystickX = true;

    [Header("--- Strict Strike Settings ---")]
    // 🟢 FIX 2: Fixed option to rotate the hammer from 10 to -100
    [Tooltip("How many degrees will the hammer go back before striking (eg: -180)")]
    public float pullbackAngleZ = -180f;
    [Tooltip("Exact degree to stop when hitting rock (eg: -20)")]
    public float strikeAngleZ = -20f;
    
    public float stopMargin = 0.5f;

    [Header("--- Movement Speeds ---")]
    public float swingSpeed = 25f;      
    public float returnSpeed = 10f;      

    [Header("--- Collision Settings ---")]
    public LayerMask stoneLayerMask; 

    [Header("--- Effects, Sound & Logic ---")]
    [Range(0f, 1f)] public float hitSoundVolume = 1f;

    [Space(10)]
    public GameObject hitEffectPrefab;  
    public Vector3 particleRotationOffset = new Vector3(0, 0, 0);
    public AudioClip primaryHitSound;

    [Space(10)]
    public GameObject secondaryHitEffectPrefab;  
    public Vector3 secondaryParticleRotationOffset = new Vector3(0, 0, 0);
    public AudioClip secondaryHitSound;

    // Internal State
    private float currentTiltZ = 0f;
    private float startingTiltZ = 0f;
    private float currentRootAngle = 0f;
    private Quaternion originalTopRotation;
    private Quaternion initialRootLocalRot;
    private bool isHitting = false;     

    void Start()
    {
        if (hitUIButton != null)
        {
            hitUIButton.onClick.AddListener(StrikeStone);
        }

        if (topBone != null) 
        {
            originalTopRotation = topBone.localRotation;
            
            float initialZ = topBone.localEulerAngles.z;
            if (initialZ > 180f) initialZ -= 360f;
            currentTiltZ = initialZ;
            startingTiltZ = initialZ;
        }

        if (rootBone != null)
        {
            initialRootLocalRot = rootBone.localRotation;
            currentRootAngle = 0f;
        }
    }

    void Update()
    {
        if (!isEquipped) return; 

        if (!isHitting)
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

            if (topBone != null && Mathf.Abs(joyY) > 0.05f)
            {
                if (tiltSpeed > 0f)
                {
                    // Subtracting joyY so that pushing joystick UP decreases angle towards -180 (moving it UP)
                    currentTiltZ -= joyY * tiltSpeed * Time.deltaTime;
                    currentTiltZ = Mathf.Clamp(currentTiltZ, minTiltZ, maxTiltZ);
                    
                    topBone.localRotation = originalTopRotation * Quaternion.AngleAxis(currentTiltZ - startingTiltZ, tiltRotationAxis.normalized);
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && !isHitting)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            RaycastHit bestHit = default;
            bool hitFound = false;
            float minDistance = float.MaxValue;

            foreach (RaycastHit candidateHit in hits)
            {
                if (candidateHit.collider.transform.IsChildOf(this.transform))
                    continue;

                StoneGenerator stoneGen = candidateHit.collider.GetComponentInParent<StoneGenerator>();
                HitAnchor anchor = candidateHit.collider.GetComponent<HitAnchor>();
                if (anchor == null) anchor = candidateHit.collider.GetComponentInParent<HitAnchor>();

                if (stoneGen != null || anchor != null || candidateHit.collider.CompareTag("Stone") || candidateHit.collider.CompareTag("Jade"))
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
                StartCoroutine(MechanicalSwingSequence(bestHit.point, bestHit.normal, bestHit.collider));
            }
        }
    }

    public void StrikeStone()
    {
        if (!isEquipped || isHitting) return;

        if (extendBone != null && hammerTip != null)
        {
            Vector3 worldExtendDir = extendBone.parent.TransformDirection(Vector3.forward);
            Ray ray = new Ray(extendBone.position, worldExtendDir);
            
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            RaycastHit bestHit = default;
            bool hitFound = false;
            float minDistance = float.MaxValue;

            foreach (RaycastHit candidateHit in hits)
            {
                if (candidateHit.collider.transform.IsChildOf(this.transform))
                    continue;

                StoneGenerator stoneGen = candidateHit.collider.GetComponentInParent<StoneGenerator>();
                HitAnchor anchor = candidateHit.collider.GetComponent<HitAnchor>();
                if (anchor == null) anchor = candidateHit.collider.GetComponentInParent<HitAnchor>();

                if (stoneGen != null || anchor != null || candidateHit.collider.CompareTag("Stone") || candidateHit.collider.CompareTag("Jade"))
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
                StartCoroutine(MechanicalSwingSequence(bestHit.point, bestHit.normal, bestHit.collider));
            }
            else
            {
                Vector3 defaultPoint = extendBone.position + worldExtendDir * 10f;
                StartCoroutine(MechanicalSwingSequence(defaultPoint, -worldExtendDir, null));
            }
        }
    }

    IEnumerator MechanicalSwingSequence(Vector3 targetPoint, Vector3 surfaceNormal, Collider stoneCollider)
    {
        isHitting = true;

        // Capture current aimed state in angle-space
        float aimedAngle = currentTiltZ;
        float pullbackAngle = pullbackAngleZ;
        float strikeAngle = strikeAngleZ;

        // PHASE 1: Pullback (Rotate from aimedAngle to pullbackAngle, e.g. -180)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * swingSpeed;
            float angle = Mathf.Lerp(aimedAngle, pullbackAngle, t);
            topBone.localRotation = originalTopRotation * Quaternion.AngleAxis(angle - startingTiltZ, tiltRotationAxis.normalized);
            yield return null;
        }

        // PHASE 2: Strike (Swing from pullbackAngle to strikeAngle, e.g. -20)
        t = 0;
        Vector3 previousTipPos = hammerTip.position;
        bool impactOccurred = false;
        float currentSwingAngle = pullbackAngle;

        while (t < 1f && !impactOccurred)
        {
            t += Time.deltaTime * swingSpeed;
            currentSwingAngle = Mathf.Lerp(pullbackAngle, strikeAngle, t);
            
            topBone.localRotation = originalTopRotation * Quaternion.AngleAxis(currentSwingAngle - startingTiltZ, tiltRotationAxis.normalized);

            Vector3 currentTipPos = hammerTip.position;
            
            if (Physics.Linecast(previousTipPos, currentTipPos, out RaycastHit tipHit))
            {
                bool isSelf = tipHit.collider.transform.IsChildOf(this.transform);
                if (!isSelf)
                {
                    StoneGenerator stoneGen = tipHit.collider.GetComponentInParent<StoneGenerator>();
                    HitAnchor anchor = tipHit.collider.GetComponent<HitAnchor>();
                    if (anchor == null) anchor = tipHit.collider.GetComponentInParent<HitAnchor>();

                    if (stoneGen != null || anchor != null || tipHit.collider.CompareTag("Stone") || tipHit.collider.CompareTag("Jade"))
                    {
                        impactOccurred = true;
                        HandleImpact(tipHit.point, tipHit.normal, tipHit.collider);
                    }
                }
            }
            previousTipPos = currentTipPos;
            yield return null;
        }

        if (!impactOccurred)
        {
            topBone.localRotation = originalTopRotation * Quaternion.AngleAxis(strikeAngle - startingTiltZ, tiltRotationAxis.normalized);
            HandleImpact(targetPoint, surfaceNormal, stoneCollider);
            currentSwingAngle = strikeAngle;
        }

        yield return new WaitForSeconds(0.15f);

        // PHASE 3: Return arm from currentSwingAngle back to aimed angle
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            float angle = Mathf.Lerp(currentSwingAngle, aimedAngle, t);
            topBone.localRotation = originalTopRotation * Quaternion.AngleAxis(angle - startingTiltZ, tiltRotationAxis.normalized);
            yield return null;
        }
        topBone.localRotation = originalTopRotation * Quaternion.AngleAxis(aimedAngle - startingTiltZ, tiltRotationAxis.normalized);

        isHitting = false; 
    }

    void HandleImpact(Vector3 point, Vector3 normal, Collider stone)
    {
        if (hitEffectPrefab != null)
        {
            Quaternion particleRot = Quaternion.LookRotation(normal) * Quaternion.Euler(particleRotationOffset);
            GameObject spark = Instantiate(hitEffectPrefab, point, particleRot);
            Destroy(spark, 2f); 
        }
        if (primaryHitSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1) AudioSource.PlayClipAtPoint(primaryHitSound, point, hitSoundVolume);

        if (secondaryHitEffectPrefab != null)
        {
            Quaternion secParticleRot = Quaternion.LookRotation(normal) * Quaternion.Euler(secondaryParticleRotationOffset);
            GameObject secSpark = Instantiate(secondaryHitEffectPrefab, point, secParticleRot);
            Destroy(secSpark, 2f); 
        }
        if (secondaryHitSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1) AudioSource.PlayClipAtPoint(secondaryHitSound, point, hitSoundVolume);

        if (stone != null)
        {
            HitAnchor anchor = stone.GetComponent<HitAnchor>();
            if (anchor == null) anchor = stone.GetComponentInParent<HitAnchor>();

            if (anchor != null)
            {
                if (anchor.stoneManager != null)
                {
                    anchor.stoneManager.RegisterToolStrike();
                    anchor.stoneManager.AnchorDestroyed(anchor);
                }
                Destroy(anchor.gameObject);
            }
            else
            {
                StoneGenerator stoneGen = stone.GetComponent<StoneGenerator>();
                if (stoneGen == null) stoneGen = stone.GetComponentInParent<StoneGenerator>();
                if (stoneGen != null)
                {
                    stoneGen.RegisterToolStrike();
                }
            }
        }

        Debug.Log("💥 Impact Locked at: " + point);
    }

    void OnDestroy()
    {
        if (hitUIButton != null)
        {
            hitUIButton.onClick.RemoveListener(StrikeStone);
        }
    }



    public void EquipHammer() 
    { 
        isEquipped = true; 
        GyroCalibration.Calibrate();
    }
    public void UnequipHammer() { isEquipped = false ; }
}