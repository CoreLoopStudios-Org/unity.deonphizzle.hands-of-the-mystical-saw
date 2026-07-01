using UnityEngine;
using UnityEngine.EventSystems;
using EzySlice;

public class SawToolController : MonoBehaviour
{
    [Header("--- Tool State ---")]
    public bool isEquipped = false;      
    private bool hasUsedStrikeThisSession = false; 

    [Header("--- Input Source (Joystick) ---")]
    [Tooltip("Drag the Virtual Joystick object here.")]
    public VirtualJoystick virtualJoystick; 

    [Header("--- Input Source (Gyro / Tilt) ---")]
    public bool enableGyro = true;  
    [Tooltip("The more you tilt the phone, the harder the saw will move.")]
    [Range(0.5f, 5f)]
    public float gyroSensitivity = 2.0f; 

    [Header("--- Saw Visuals ---")]
    public GameObject sawVisualObject;   
    public Transform sawBlade;           
    public ParticleSystem sparksParticle;
    public ParticleSystem waterEffectParticle; 
    public float bladeSpinSpeed = 1500f; 

    [Header("--- Environment ---")]
    public GameObject groundPlane;

    [Header("--- Controller Movement ---")]
    public float controllerMoveSpeed = 10f; 
    private float moveX = 0f;
    private float moveZ = 0f;

    [Header("--- Movement Boundaries (Limits) ---")]
    public Transform baseSpawnPoint; 
    public float minX = -10f;
    public float maxX = 10f;
    public float minZ = -10f;
    public float maxZ = 10f;
    public float minY = 0f;
    public float maxY = 5f;

    [Header("--- Realistic Cutting / Grinding ---")]
    public GameObject sawCutMarkPrefab;  
    public float grindInterval = 0.1f;   
    private float grindTimer = 0f;
    public float bladeRadius = 1.5f;   

    [Header("--- Final Slicing Settings (EzySlice) ---")]
    public Vector3 bladeCutNormal = new Vector3(0, 0, 1); 
    public float sliceCooldown = 1.0f; 
    public Material crossSectionMaterial; 
    public Material jadeCrossSectionMaterial; 
    public LayerMask stoneLayer; 

    [Header("--- Audio Effects ---")]
    public AudioSource sawAudioSource; 
    public AudioClip sawingSound;      
    public AudioClip sliceSound;       

    private Camera mainCam;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float lastSliceTime = 0f; 

    void Start()
    {
        mainCam = Camera.main;

        if (sawVisualObject != null)
        {
            originalRotation = sawVisualObject.transform.rotation;
            if (baseSpawnPoint != null) sawVisualObject.transform.position = baseSpawnPoint.position;
            originalPosition = sawVisualObject.transform.position;
        }

        if (sparksParticle != null) sparksParticle.Stop();
        if (waterEffectParticle != null) waterEffectParticle.Stop(); 
        if (sawAudioSource != null) sawAudioSource.Stop();
    }

    void Update()
    {
        if (sawAudioSource != null)
        {
            sawAudioSource.mute = (PlayerPrefs.GetInt("SoundEnabled", 1) == 0);
        }

        if (!isEquipped) return;

        if (sawVisualObject != null) 
        {
            sawVisualObject.transform.rotation = originalRotation;

            moveX = 0f;
            moveZ = 0f;

            if (virtualJoystick != null && virtualJoystick.InputVector != Vector2.zero)
            {
                moveX = virtualJoystick.InputVector.x;
                moveZ = virtualJoystick.InputVector.y;
            }

            if (enableGyro)
            {
                Vector3 calibAccel = GyroCalibration.GetCalibratedAcceleration();
                float tiltX = calibAccel.x * gyroSensitivity;
                float tiltY = calibAccel.y * gyroSensitivity; 

                moveX += tiltX;
                moveZ += tiltY;

                moveX = Mathf.Clamp(moveX, -1f, 1f);
                moveZ = Mathf.Clamp(moveZ, -1f, 1f);
            }

            if (moveX != 0 || moveZ != 0)
            {
                Vector3 camForward = mainCam.transform.forward;
                camForward.y = 0; camForward.Normalize();
                Vector3 camRight = mainCam.transform.right;
                camRight.y = 0; camRight.Normalize();

                Vector2 combinedInput = new Vector2(moveX, moveZ);
                float inputMagnitude = Mathf.Clamp01(combinedInput.magnitude);

                Vector3 moveDir = (camRight * moveX + camForward * moveZ).normalized;
                
                float currentSpeed = controllerMoveSpeed * inputMagnitude;

                sawVisualObject.transform.Translate(moveDir * currentSpeed * Time.deltaTime, Space.World);

                Vector3 clampedPos = sawVisualObject.transform.position;
                
                if (baseSpawnPoint != null)
                {
                    clampedPos.x = Mathf.Clamp(clampedPos.x, baseSpawnPoint.position.x + minX, baseSpawnPoint.position.x + maxX);
                    clampedPos.z = Mathf.Clamp(clampedPos.z, baseSpawnPoint.position.z + minZ, baseSpawnPoint.position.z + maxZ);
                    clampedPos.y = Mathf.Clamp(clampedPos.y, baseSpawnPoint.position.y + minY, baseSpawnPoint.position.y + maxY); 
                }
                
                sawVisualObject.transform.position = clampedPos;
            }
        }

        HandleSawGrindingAndEffects();
    }

    // 🌟 New Method: To control Gyro Sensitivity from UI Slider 🌟
    public void SetGyroSensitivityFromSlider(float sliderValue)
    {
        gyroSensitivity = sliderValue;
    }

    public void SetSawHeightFromSlider(float sliderValue)
    {
        if (sawVisualObject != null)
        {
            Vector3 pos = sawVisualObject.transform.position;
            if (baseSpawnPoint != null)
            {
                pos.y = Mathf.Lerp(baseSpawnPoint.position.y + minY, baseSpawnPoint.position.y + maxY, sliderValue); 
            }
            else
            {
                pos.y = Mathf.Lerp(minY, maxY, sliderValue); 
            }
            sawVisualObject.transform.position = pos;
        }
    }

    private void HandleSawGrindingAndEffects()
    {
        if (sawBlade == null) return; 
        
        sawBlade.Rotate(Vector3.forward * bladeSpinSpeed * Time.deltaTime, Space.Self); 

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

                if (sparksParticle != null)
                {
                    if (!sparksParticle.isPlaying) sparksParticle.Play();
                    sparksParticle.transform.position = closestPoint;
                    sparksParticle.transform.rotation = Quaternion.LookRotation(hitNormal);
                }
                
                if (waterEffectParticle != null)
                {
                    if (!waterEffectParticle.isPlaying) waterEffectParticle.Play();
                    waterEffectParticle.transform.position = sawVisualObject.transform.position + Vector3.up * 0.2f; 
                }

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
                        GameObject dent = Instantiate(sawCutMarkPrefab, closestPoint + (hitNormal * 0.001f), Quaternion.LookRotation(hitNormal));
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
            Material crossMat;
            bool isJade = target.CompareTag("Jade");
            
            if (isJade) crossMat = jadeCrossSectionMaterial != null ? jadeCrossSectionMaterial : target.GetComponent<MeshRenderer>().sharedMaterial;
            else crossMat = crossSectionMaterial != null ? crossSectionMaterial : target.GetComponent<MeshRenderer>().sharedMaterial;

            GameObject upperHull = hull.CreateUpperHull(target, crossMat);
            GameObject lowerHull = hull.CreateLowerHull(target, crossMat);

            Vector3 upperSize = upperHull.GetComponent<MeshRenderer>().bounds.size;
            Vector3 lowerSize = lowerHull.GetComponent<MeshRenderer>().bounds.size;

            float upperWeight = upperSize.x * upperSize.y * upperSize.z;
            float lowerWeight = lowerSize.x * lowerSize.y * lowerSize.z;

            bool keepUpper = upperWeight >= lowerWeight;

            GameObject pieceToStay = keepUpper ? upperHull : lowerHull;
            GameObject pieceToFall = keepUpper ? lowerHull : upperHull;

            Mesh newMesh = pieceToStay.GetComponent<MeshFilter>().sharedMesh;
            target.GetComponent<MeshFilter>().sharedMesh = newMesh;
            target.GetComponent<MeshRenderer>().sharedMaterials = pieceToStay.GetComponent<MeshRenderer>().sharedMaterials; 

            MeshCollider targetCollider = target.GetComponent<MeshCollider>();
            if (targetCollider != null) { targetCollider.sharedMesh = null; targetCollider.sharedMesh = newMesh; }

            for (int i = target.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = target.transform.GetChild(i);
                if (child.GetComponent<HitAnchor>() != null)
                {
                    float dot = Vector3.Dot(child.position - planePoint, planeNormal);
                    bool belongsToUpper = dot >= 0;

                    if ((belongsToUpper && !keepUpper) || (!belongsToUpper && keepUpper))
                    {
                        child.SetParent(pieceToFall.transform);
                    }
                }
            }
            
            Destroy(pieceToStay); 
            
            Vector3 originalPos = target.transform.position;
            Vector3 fallPos = originalPos;

            if (isJade)
            {
                float pushDir = keepUpper ? -1f : 1f; 
                target.transform.position += planeNormal * 0.003f * pushDir; 
                fallPos -= planeNormal * 0.003f * pushDir; 
            }
            
            SetupSlicedComponent(pieceToFall, target.transform.lossyScale, fallPos, target.transform.rotation);
        }
    }

    void SetupSlicedComponent(GameObject slicedObject, Vector3 originalScale, Vector3 originalPos, Quaternion originalRot)
    {
        slicedObject.transform.position = originalPos;
        slicedObject.transform.rotation = originalRot;
        slicedObject.transform.localScale = originalScale; 

        AudioSource[] audios = slicedObject.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audio in audios) Destroy(audio);

        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;
        
        slicedObject.layer = 2; // Ignore Raycast 
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
        
        if (sawVisualObject != null) 
        {
            if (baseSpawnPoint != null)
            {
                sawVisualObject.transform.position = baseSpawnPoint.position;
                sawVisualObject.transform.rotation = originalRotation; 
                originalPosition = baseSpawnPoint.position;
            }
            sawVisualObject.SetActive(true);
        }
        if (groundPlane != null) groundPlane.SetActive(false);
    }

    public void UnequipSaw()
    {
        isEquipped = false;
        if (sawVisualObject != null) sawVisualObject.SetActive(false);
        StopAllEffects();
        if (groundPlane != null) groundPlane.SetActive(true);
    }
}