using UnityEngine;
using UnityEngine.EventSystems;

public class HammerController : MonoBehaviour
{
    [Header("Dynamic Distance Control")]
    [Tooltip("Distance of the hammer from the camera. Reduce this if you bring the camera closer (e.g., 4 or 5).")]
    [Range(1f, 15f)] 
    public float dynamicDistance = 8f;

    [Header("Movement Settings")]
    public Vector3 startPosition;
    public Quaternion startRotation;
    public float smoothSpeed = 15f; 

    [Header("Movement Boundaries")]
    public float minX = -10f; 
    public float maxX = 10f;  
    public float minY = -5f; 
    public float maxY = 10f;  
    public float minZ = -10f;  
    public float maxZ = 20f; 

    [Header("Hitting Settings")]
    public float cuttingDepth = 0.2f; 
    public string cuttableTag = "Stone"; 

    [Header("Camera Follow Settings")]
    public bool enableCameraFollow = true;
    public float cameraZoomAmount = 2f; 
    public float cameraSmoothSpeed = 5f;

    [Header("Juice & Effects")]
    public GameObject hitParticlePrefab; 
    private bool isAnimating = false; 

    [Header("State")]
    public bool isWorking = true;
    public bool isSelected = true; 

    [Header("--- Input Source (Gyro / Tilt) ---")]
    public bool enableGyro = true;  
    [Range(0.5f, 5f)]
    public float gyroSensitivity = 2.0f; 
    private float gyroOffsetX = 0f;
    private float gyroOffsetY = 0f;

    public void SetGyroSensitivityFromSlider(float sliderValue)
    {
        gyroSensitivity = sliderValue;
    }

    private Camera mainCam;
    private Vector3 initialCamPos;

    void OnEnable()
    {
        GyroCalibration.Calibrate();
    }

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        mainCam = Camera.main;
        if(mainCam != null)
        {
            initialCamPos = mainCam.transform.position;
        }
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (!isAnimating) ReturnToStart();
            return; 
        }

        if (isSelected && Input.GetMouseButton(0))
        {
            if (!isAnimating) MoveToolToMouse(); 
            
            // Camera zoom logic
            if (enableCameraFollow && mainCam != null)
            {
                Vector3 targetCamPos = initialCamPos + new Vector3(0, 0, cameraZoomAmount);
                mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, targetCamPos, Time.deltaTime * cameraSmoothSpeed);
            }
        }
        else
        {
            if (!isAnimating) ReturnToStart();
            
            if (enableCameraFollow && mainCam != null)
            {
                mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, initialCamPos, Time.deltaTime * cameraSmoothSpeed);
            }
        }
    }

    void MoveToolToMouse()
    {
        if (mainCam == null) return;

        // 1. Set an initial target based on the mouse position
        Vector3 mousePos = Input.mousePosition;

        if (enableGyro)
        {
            Vector3 calibAccel = GyroCalibration.GetCalibratedAcceleration();
            
            // Map calibrated tilt directly to screen offset to avoid integration drift
            float maxShiftX = Screen.width * 0.20f;
            float maxShiftY = Screen.height * 0.20f;

            float targetShiftX = calibAccel.x * gyroSensitivity * maxShiftX;
            float targetShiftY = calibAccel.y * gyroSensitivity * maxShiftY;

            // Smoothly interpolate the offset to prevent jitter
            gyroOffsetX = Mathf.Lerp(gyroOffsetX, targetShiftX, Time.deltaTime * smoothSpeed);
            gyroOffsetY = Mathf.Lerp(gyroOffsetY, targetShiftY, Time.deltaTime * smoothSpeed);

            mousePos.x += gyroOffsetX;
            mousePos.y += gyroOffsetY;
        }

        // Clamp screen coordinates to keep tool visible within [10%, 90%] of screen bounds
        mousePos.x = Mathf.Clamp(mousePos.x, Screen.width * 0.1f, Screen.width * 0.9f);
        mousePos.y = Mathf.Clamp(mousePos.y, Screen.height * 0.1f, Screen.height * 0.9f);

        mousePos.z = dynamicDistance; 
        Vector3 targetPos = mainCam.ScreenToWorldPoint(mousePos);

        // 2. 🌟 Real-time Raycast - This keeps the hammer glued to the stone's surface
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // If the mouse is over a stone, the hammer goes to that exact point
        if (Physics.Raycast(ray, out hit, 50f)) 
        {
            if (hit.collider.CompareTag(cuttableTag) || hit.collider.gameObject.name.Contains("Hull"))
            {
                // Push the hammer slightly into the stone (cuttingDepth) so the trigger activates
                targetPos = hit.point + (ray.direction * cuttingDepth);
            }
        }

        // 3. Boundary check (to prevent the hammer from getting lost off-screen)
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }

    void ReturnToStart()
    {
        gyroOffsetX = 0f;
        gyroOffsetY = 0f;
        transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, startRotation, Time.deltaTime * smoothSpeed);
    }

    // Hammer hit logic (kept as before so your jade extraction logic works seamlessly)
    void OnTriggerEnter(Collider other)
    {
        if (isSelected && isWorking && !isAnimating) 
        {
            if (other.CompareTag("Stone"))
            {
                bool isValidHit = false;

                StoneGenerator stoneGen = other.GetComponent<StoneGenerator>();
                if (stoneGen == null) stoneGen = other.GetComponentInParent<StoneGenerator>();

                if (stoneGen != null)
                {
                    stoneGen.RegisterToolStrike();
                    isValidHit = true;
                }

                StoneShatter stoneShatter = other.GetComponent<StoneShatter>();
                if (stoneShatter != null) isValidHit = true;

                if (isValidHit)
                {
                    if (hitParticlePrefab != null)
                    {
                        Vector3 hitPoint = other.ClosestPoint(transform.position);
                        GameObject particle = Instantiate(hitParticlePrefab, hitPoint, Quaternion.identity);
                        Destroy(particle, 1.5f); 
                    }
                    StartCoroutine(HitAnimation());
                }
            }
        }
    }

    System.Collections.IEnumerator HitAnimation()
    {
        isAnimating = true; 
        Quaternion currentRot = transform.rotation;
        Quaternion pullbackRot = currentRot * Quaternion.Euler(-45f, 0, 0); 

        float t = 0;
        while(t < 0.1f)
        {
            transform.rotation = Quaternion.Lerp(currentRot, pullbackRot, t / 0.1f);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0;
        while(t < 0.05f)
        {
            transform.rotation = Quaternion.Lerp(pullbackRot, currentRot, t / 0.05f);
            t += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = currentRot; 
        isAnimating = false; 
    }
}