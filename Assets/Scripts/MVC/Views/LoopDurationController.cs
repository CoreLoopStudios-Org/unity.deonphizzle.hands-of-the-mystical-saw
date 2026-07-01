using UnityEngine;

public class LoopDurationController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag your entire dial or spin image here")]
    public RectTransform spinDialRectTransform;

    [Header("Settings")]
    public float rotationSpeed = 5f;

    private float currentAngle = 0f;
    private float targetAngle = 0f;

    void Start()
    {
        if (spinDialRectTransform != null) currentAngle = -spinDialRectTransform.localEulerAngles.z;
        targetAngle = currentAngle;
    }

    void Update()
    {
        if (spinDialRectTransform != null)
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
            spinDialRectTransform.localRotation = Quaternion.Euler(0, 0, -currentAngle);
        }
    }

    // 🌟 updated method (perfect for unity buttons)
// 🌟 updated method (fixed angle direction)
    public void OnDialButtonClicked(float durationValue)
    {
        // 1. The data is sent to the main database
        if (PredictorUIManager.Instance != null)
        {
            PredictorUIManager.Instance.SetLoopDurationFromDial(durationValue);
        }

        // 2. Your design angles are fixed (plus-minus reversed).
        if (durationValue == 0f) targetAngle = -135f;      // bottom left
        else if (durationValue == 10f) targetAngle = -90f;   // left middle
        else if (durationValue == 20f) targetAngle = -45f;   // top left
        else if (durationValue == 30f) targetAngle = 0f;     // Straight up
        else if (durationValue == 40f) targetAngle = 45f;    // top right
        else if (durationValue == 50f) targetAngle = 90f;    // center right
        else if (durationValue == 60f) targetAngle = 135f;   // bottom right
    }
}