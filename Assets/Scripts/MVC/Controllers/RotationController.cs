using UnityEngine;
using UnityEngine.UI;

public class RotationController : MonoBehaviour
{
    [Header("References")]
    public Transform target3DObject;
    public RectTransform uiPointer;

    [Header("Settings")]
    public float rotationSpeed = 5f;
    
    [Header("UI Calibration")]
    [Tooltip("If the UI arrow doesn't move to the right place, fix it by changing the value here (eg: 90, -90, 45 etc.)")]
    public float uiRotationOffset = 0f; 

    // 🌟 Using Float instead of Quaternion so it doesn't search for Shortest-Path!
    private float currentUIAngle = 0f;
    private float targetUIAngle = 0f;

    private float currentObjectAngle = 0f;
    private float targetObjectAngle = 0f;

    void Start()
    {
        // Initializing the current rotation
        if (uiPointer != null) currentUIAngle = -uiPointer.localEulerAngles.z;
        if (target3DObject != null) currentObjectAngle = target3DObject.localEulerAngles.y;
        
        targetUIAngle = currentUIAngle;
        targetObjectAngle = currentObjectAngle;
    }

    void Update()
    {
        if (uiPointer != null)
        {
            // 🌟 Using Mathf.Lerp will always rotate serially (Clockwise / Anti-Clockwise)
            currentUIAngle = Mathf.Lerp(currentUIAngle, targetUIAngle, Time.deltaTime * rotationSpeed);
            uiPointer.localRotation = Quaternion.Euler(0, 0, -currentUIAngle);
        }

        if (target3DObject != null)
        {
            // The 3D object will also rotate the same way
            currentObjectAngle = Mathf.Lerp(currentObjectAngle, targetObjectAngle, Time.deltaTime * rotationSpeed);
            target3DObject.localRotation = Quaternion.Euler(0, currentObjectAngle, 0);
        }
    }

    public void OnRotationButtonClicked(float angle)
    {
        float visualUIAngle = angle;

        // ==========================================
        // 🌟 Custom UI rotation logic (for your design)
        // ==========================================
        if (angle == 180f)
        {
            // Extra 90 is added to get the fork in the right place for 180 (270 degrees)
            visualUIAngle = 230f; 
        }
        else if (angle == 90f)
        {
            visualUIAngle = 90f;
        }
        else if (angle == 45f)
        {
            // 🌟 Fix: logic to move the fork to the opposite direction (-45) for 45 degrees
            // visualUIAngle = 2f; 
        }
        else if (angle == 0f) 
        {
            visualUIAngle = 0f;
        }

        // Target rotation Float value for UI (with offset)
        targetUIAngle = visualUIAngle + uiRotationOffset; 

        // Target rotation Float value for 3D object
        targetObjectAngle = angle; 

        // ==========================================
        // 🌟 Update predictor database 
        // ==========================================
        if (PredictorUIManager.Instance != null)
        {
            PredictorUIManager.Instance.selectedAngle = angle;
            
            // Status is updated to Generating
            if (PredictorUIManager.Instance.statusText != null) 
            {
                PredictorUIManager.Instance.statusText.text = "Generating...";
            }
        }
    }
}