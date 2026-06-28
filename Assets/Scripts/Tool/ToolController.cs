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
    public float minX = -5f; // বামে কতটুকু যাবে
    public float maxX = 5f;  // ডানে কতটুকু যাবে
    public float minY = -2f; // নিচে কতটুকু যাবে
    public float maxY = 5f;  // উপরে কতটুকু যাবে
    public float minZ = 0f;  // পেছনে কতটুকু আসবে
    public float maxZ = 10f; // সামনে কতটুকু যাবে

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
        
        // গেম শুরুর সাথে সাথে কাজ করার জন্য
        isWorking = true;
        isSelected = true; 
        
        Debug.Log(gameObject.name + " is forced to Working Mode ON at Start.");
    }

    void Update()
    {
        // 🌟 টর্চ জ্বললে করাত কোনো কাজ করবে না, সিলেক্ট হবে না এবং সাথে সাথে আগের জায়গায় ফিরে যাবে!
        if (StoneSpinController.GlobalTorchActive)
        {
            isSelected = false; // জোর করে করাত ডি-সিলেক্ট করে দেওয়া হলো
            ReturnToStart();
            return;
        }

        // টুল সিলেক্ট করা থাকলে এবং মাউস চাপলে কাজ করবে
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

            // পজিশন লিমিট বা Clamp করা হচ্ছে
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

    // যখন টুল কোনো কিছুর সাথে ধাক্কা খাবে বা ভেতরে ঢুকবে
    void OnTriggerEnter(Collider other)
    {
        // টুল সিলেক্টেড এবং ওয়ার্কিং মোডে থাকতে হবে
        if (isSelected && isWorking)
        {
            // ১. যদি টুল পাথরের গায়ে লাগে, তবে স্ট্রাইক কাউন্ট ১ বাড়বে
            if (other.CompareTag("Stone"))
            {
                // পাথর থেকে জেনারেটর স্ক্রিপ্টটি খুঁজে বের করা
                StoneGenerator stoneGen = other.GetComponent<StoneGenerator>();
                if (stoneGen != null)
                {
                    // জেনারেটরকে জানিয়ে দেওয়া যে একটি স্ট্রাইক হয়েছে
                    stoneGen.RegisterToolStrike();
                }
            }
            
            // ২. যদি টুল পাথরের ভেতরের অ্যাঙ্করের গায়ে লাগে, সেটি অ্যাঙ্কর নিজেই হ্যান্ডেল করবে।
        }
    }

    // (ভবিষ্যতে কাটার প্রয়োজন হলে এটি ব্যবহার করতে পারবেন)
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
            Debug.LogWarning("EzySlice কাটতে ব্যর্থ হয়েছে!");
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
} // <-- এই ব্র্যাকেটটি আপনার আগের কোডে মিসিং ছিল!