    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;

    public class ManualChiselController : MonoBehaviour
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

        [Header("--- Extra Custom Aiming ---")]
        public Transform upDownJoystickPart; 

        [Header("--- Base Rotation Settings (Buttons) ---")]
        public float baseTurnSpeed = 50f; 
        public Vector3 baseRotationAxis = Vector3.right; 
        private int baseRotationDirection = 0; 

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

        [Header("--- Strike Settings (Hit) ---")]
        [Range(0.1f, 10f)] public float maxExtensionDistance = 5f;   
        public float hitSpeed = 25f;     
        public float returnSpeed = 10f;  
        public Vector3 strikeAxis = Vector3.forward;

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
        private Quaternion initialRootRotation;

        private float initialPartX = 0f;
        private float initialPartY = 0f;
        private float initialPartZ = 0f;

        void Start()
        {
            if (extendBone != null) initialExtendLocalPos = extendBone.localPosition;

            if (upDownJoystickPart == null)
            {
                upDownJoystickPart = FindChildRecursive(transform, "Up_down-for-joystick-UP-DOWN");
                if (upDownJoystickPart == null)
                {
                    upDownJoystickPart = FindChildRecursive(transform, "Up_down");
                }
            }

            if (upDownJoystickPart != null)
            {
                Vector3 localEulers = upDownJoystickPart.localEulerAngles;
                initialPartX = localEulers.x;
                initialPartY = localEulers.y;
                
                float z = localEulers.z;
                if (z > 180f) z -= 360f;
                initialPartZ = z;
            }

            // 🌟 Programmatically perform the "Wrapper Trick" at runtime!
            if (rootBone != null && rootBone.name != "Perfect_Yaw")
            {
                Transform originalRoot = rootBone;
                Transform originalParent = originalRoot.parent;
                Vector3 originalLocalPos = originalRoot.localPosition;
                Quaternion originalLocalRot = originalRoot.localRotation;
                Vector3 originalLocalScale = originalRoot.localScale;

                // 1. Create Perfect_Yaw Game Object
                GameObject perfectYawObj = new GameObject("Perfect_Yaw");
                Transform perfectYaw = perfectYawObj.transform;
                perfectYaw.SetParent(originalParent, false);
                perfectYaw.localPosition = originalLocalPos;
                perfectYaw.localRotation = Quaternion.identity;
                perfectYaw.localScale = Vector3.one;

                // 2. Create Perfect_Pitch Game Object
                GameObject perfectPitchObj = new GameObject("Perfect_Pitch");
                Transform perfectPitch = perfectPitchObj.transform;
                perfectPitch.SetParent(perfectYaw, false);
                perfectPitch.localPosition = Vector3.zero;
                perfectPitch.localRotation = Quaternion.identity;
                perfectPitch.localScale = Vector3.one;

                // 3. Reparent originalRoot under Perfect_Pitch
                originalRoot.SetParent(perfectPitch, false);
                originalRoot.localPosition = Vector3.zero;
                originalRoot.localRotation = originalLocalRot;
                originalRoot.localScale = originalLocalScale;

                // 4. Reassign bones to our new wrapper transforms
                rootBone = perfectYaw;
                tiltBone = perfectPitch;
            }

            // 🌟 Force clean, cardinally-aligned joystick inputs and parameters unconditionally
            swapJoystickAxes = false;
            invertHorizontal = false;
            invertVertical = false;
            minTiltSide = -30f;
            maxTiltSide = 30f;
            minTiltUp = -90f;
            maxTiltUp = 90f;

            if (joystick != null)
            {
                RectTransform rt = joystick.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localRotation = Quaternion.identity;
                }
            }

            if (rootBone != null) initialRootRotation = rootBone.localRotation;
            currentAimSide = 0f;
            currentAimUp = 0f;
        }

        void Update()
        {
            if (!isEquipped) return;

            if (baseRotationDirection != 0 && rootBone != null)
            {
                float baseAngleDelta = baseRotationDirection * baseTurnSpeed * Time.deltaTime;
                initialRootRotation *= Quaternion.AngleAxis(baseAngleDelta, baseRotationAxis);
                rootBone.localRotation = initialRootRotation * Quaternion.Euler(0f, currentAimSide, 0f);
            }

            if (!isStriking) HandleHeadAiming();
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

            if (swapJoystickAxes) { float temp = joyX; joyX = joyY; joyY = temp; }
            if (invertHorizontal) joyX = -joyX;
            if (invertVertical) joyY = -joyY;

            if (Mathf.Abs(joyX) > 0.05f || Mathf.Abs(joyY) > 0.05f)
            {
                if (rootBone != null && Mathf.Abs(joyX) > 0.05f)
                {
                    currentAimSide += joyX * headAimSpeed * Time.deltaTime;
                    currentAimSide = Mathf.Clamp(currentAimSide, minTiltSide, maxTiltSide);
                    rootBone.localRotation = initialRootRotation * Quaternion.Euler(0f, currentAimSide, 0f); 
                }

                // tiltBone vertical control has been disabled per user request so only left/right yaw is applied to root parts
                /*
                if (tiltBone != null && Mathf.Abs(joyY) > 0.05f)
                {
                    currentAimUp -= joyY * headAimSpeed * Time.deltaTime;
                    currentAimUp = Mathf.Clamp(currentAimUp, minTiltUp, maxTiltUp);
                    tiltBone.localRotation = Quaternion.Euler(currentAimUp, 0f, 0f); 
                }
                */
            }

            // Apply custom rotation to upDownJoystickPart based on joyY (vertical aiming input)
            if (upDownJoystickPart != null)
            {
                float targetZ = initialPartZ;
                if (joyY < -0.05f)
                {
                    // joystick is down (joyY < 0). Normalize joyY from [-0.05, -1] to [0, 1] range for smoother lerping
                    float t = Mathf.InverseLerp(-0.05f, -1.0f, joyY);
                    targetZ = Mathf.Lerp(initialPartZ, -50f, t);
                }
                else if (joyY > 0.05f)
                {
                    // joystick is up (joyY > 0). Normalize joyY from [0.05, 1] to [0, 1] range for smoother lerping
                    float t = Mathf.InverseLerp(0.05f, 1.0f, joyY);
                    targetZ = Mathf.Lerp(initialPartZ, -75f, t);
                }
                upDownJoystickPart.localRotation = Quaternion.Euler(initialPartX, initialPartY, targetZ);
            }
        }

        public void RotateBaseLeft() { baseRotationDirection = -1; }
        public void RotateBaseRight() { baseRotationDirection = 1; }
        public void StopBaseRotation() { baseRotationDirection = 0; }

        public void StrikeStone()
        {
            if (!isEquipped) return;
            if (!isStriking && extendBone != null && chiselTip != null) StartCoroutine(StrikeRoutine());
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

                        if (stoneGen != null || anchor != null || hit.collider.CompareTag("Stone") || hit.collider.CompareTag("Jade"))
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
                return;
            }
            if (stoneGen != null) stoneGen.RegisterToolStrike();
        }

        private void TriggerHitEffects(Vector3 point, Vector3 normal)
        {
            if (hitEffectPrefab != null) Destroy(Instantiate(hitEffectPrefab, point, Quaternion.LookRotation(normal) * Quaternion.Euler(particleRotationOffset)), 2f);
            if (primaryHitSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1) AudioSource.PlayClipAtPoint(primaryHitSound, point, hitSoundVolume);
            if (secondaryHitEffectPrefab != null) Destroy(Instantiate(secondaryHitEffectPrefab, point, Quaternion.LookRotation(normal) * Quaternion.Euler(secondaryParticleRotationOffset)), 2f);
            if (secondaryHitSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1) AudioSource.PlayClipAtPoint(secondaryHitSound, point, hitSoundVolume);
        }

        public void EquipChisel() 
        { 
            isEquipped = true; 
            GyroCalibration.Calibrate();
        }
        public void UnequipChisel() { isEquipped = false; }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }