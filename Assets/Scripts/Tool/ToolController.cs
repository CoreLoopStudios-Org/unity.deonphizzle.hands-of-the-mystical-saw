using UnityEngine;
using EzySlice; 

public class ToolController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 startPosition;
    public Quaternion startRotation;
    public float smoothSpeed = 10f;
    public float zOffset = 8f; 

    [Header("Movement Boundaries (Inspector)")]
    public float minX = -5f; // How far left to go
    public float maxX = 5f;  // How far to the right to go
    public float minY = -2f; // How far down to go
    public float maxY = 5f;  // How far up will go
    public float minZ = 0f;  // How far back will come
    public float maxZ = 10f; // How far forward

    [Header("Dynamic Depth Settings")]
    public float forwardDistance = 1.5f; 
    public float rayDistance = 10f;

    [Header("State")]
    public bool isWorking = true;
    public bool isSelected = true; 

    [Header("Slicing Settings (EzySlice)")]
    public Material crossSectionMaterial; 
    public string cuttableTag = "Stone"; 

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

    void OnEnable()
    {
        GyroCalibration.Calibrate();
    }

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // To work with game start
        isWorking = true;
        isSelected = true; 
        
        Debug.Log(gameObject.name + " is forced to Working Mode ON at Start.");
    }

    void Update()
    {
        // 🌟 When the torch is lit, the saw will not do anything, will not be selected and will immediately return to the previous position!
        if (StoneSpinController.GlobalTorchActive)
        {
            isSelected = false; // Force the saw to be de-selected
            ReturnToStart();
            return;
        }

        // Will work if the tool is selected and the mouse is pressed
        if (isSelected && Input.GetMouseButton(0))
        {
            MoveToolToMouse();
        }
        else
        {
            ReturnToStart();
        }
    }
    void MoveToolToMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        if (Camera.main != null)
        {
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

            mousePos.z = zOffset; 
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(mousePos);

            // Raycast logic:
            Ray ray = new Ray(transform.position, Vector3.forward); 
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance)) 
            {
                if (hit.collider.gameObject.name == "Stone_1" || hit.collider.CompareTag(cuttableTag))
                {
                    targetPos.z = hit.point.z; 
                }
            }
            else 
            {
                targetPos.z = startPosition.z + forwardDistance; 
            }

            // Clamping the position limit
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
            targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
        }
    }

    void ReturnToStart()
    {
        gyroOffsetX = 0f;
        gyroOffsetY = 0f;
        transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, startRotation, Time.deltaTime * smoothSpeed);
    }

    public void ToggleWorkingState()
    {
        isWorking = !isWorking;
        Debug.Log(gameObject.name + " Working Mode: " + (isWorking ? "ON" : "OFF"));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isSelected && isWorking && (collision.gameObject.name == "Stone_1" || collision.gameObject.CompareTag(cuttableTag)))
        {
            JadeCuttingGame gameManager = FindObjectOfType<JadeCuttingGame>();
            if (gameManager != null)
            {
                gameManager.ProcessToolHit(gameObject.tag);
            }
        }
    }

    // When the tool collides with or enters something
    void OnTriggerEnter(Collider other)
    {
        // Tool must be selected and in working mode
        if (isSelected && isWorking)
        {
            // 1. If the tool hits the stone, the strike count increases by 1
            if (other.CompareTag("Stone"))
            {
                // Finding the generator script from stone
                StoneGenerator stoneGen = other.GetComponent<StoneGenerator>();
                if (stoneGen != null)
                {
                    // Notify the generator that a strike has occurred
                    stoneGen.RegisterToolStrike();
                }
            }
            
            // 2. If the tool hits the anchor inside the rock, the anchor itself will handle it.
        }
    }

    // (can use this if you need to cut in the future)
    private void SliceObject(GameObject targetStone)
    {
        Vector3 slicePosition = transform.position;
        Vector3 sliceDirection = transform.forward; 

        SlicedHull hull = targetStone.Slice(slicePosition, sliceDirection, crossSectionMaterial);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(targetStone, crossSectionMaterial);
            GameObject lowerHull = hull.CreateLowerHull(targetStone, crossSectionMaterial);

            SetupSlicedComponent(upperHull);
            SetupSlicedComponent(lowerHull);

            Destroy(targetStone);
        }
        else
        {
            Debug.LogWarning("Failed to cut EzySlice!");
        }
    }

    private void SetupSlicedComponent(GameObject slicedObject)
    {
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        slicedObject.tag = cuttableTag; 
        rb.AddExplosionForce(50f, transform.position, 1f);
    }
} // <-- This bracket was missing in your previous code!