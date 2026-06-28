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
    [Tooltip("UI অ্যারো ঠিক জায়গায় না গেলে এখানে ভ্যালু পরিবর্তন করে ঠিক করুন (যেমন: 90, -90, 45 ইত্যাদি)")]
    public float uiRotationOffset = 0f; 

    // 🌟 Quaternion-এর বদলে Float ব্যবহার করছি যাতে সে Shortest-Path না খোঁজে!
    private float currentUIAngle = 0f;
    private float targetUIAngle = 0f;

    private float currentObjectAngle = 0f;
    private float targetObjectAngle = 0f;

    void Start()
    {
        // শুরুতে বর্তমান রোটেশন সেট করে নেওয়া হচ্ছে
        if (uiPointer != null) currentUIAngle = -uiPointer.localEulerAngles.z;
        if (target3DObject != null) currentObjectAngle = target3DObject.localEulerAngles.y;
        
        targetUIAngle = currentUIAngle;
        targetObjectAngle = currentObjectAngle;
    }

    void Update()
    {
        if (uiPointer != null)
        {
            // 🌟 Mathf.Lerp ব্যবহার করার ফলে সে সবসময় সিরিয়ালি ঘুরবে (Clockwise / Anti-Clockwise)
            currentUIAngle = Mathf.Lerp(currentUIAngle, targetUIAngle, Time.deltaTime * rotationSpeed);
            uiPointer.localRotation = Quaternion.Euler(0, 0, -currentUIAngle);
        }

        if (target3DObject != null)
        {
            // 3D অবজেক্টটিও একইভাবে ঘুরবে
            currentObjectAngle = Mathf.Lerp(currentObjectAngle, targetObjectAngle, Time.deltaTime * rotationSpeed);
            target3DObject.localRotation = Quaternion.Euler(0, currentObjectAngle, 0);
        }
    }

    public void OnRotationButtonClicked(float angle)
    {
        float visualUIAngle = angle;

        // ==========================================
        // 🌟 কাস্টম UI রোটেশন লজিক (আপনার ডিজাইনের জন্য)
        // ==========================================
        if (angle == 180f)
        {
            // ১৮০ এর জন্য কাঁটাকে সঠিক জায়গায় নিতে এক্সট্রা ৯০ যোগ করা হলো (২৭০ ডিগ্রি)
            visualUIAngle = 230f; 
        }
        else if (angle == 90f)
        {
            visualUIAngle = 90f;
        }
        else if (angle == 45f)
        {
            // 🌟 ফিক্স: ৪৫ ডিগ্রির জন্য কাঁটাকে উল্টোদিকে (-৪৫) যাওয়ার লজিক
            // visualUIAngle = 2f; 
        }
        else if (angle == 0f) 
        {
            visualUIAngle = 0f;
        }

        // UI-এর জন্য টার্গেট রোটেশন Float ভ্যালু (অফসেট সহ)
        targetUIAngle = visualUIAngle + uiRotationOffset; 

        // 3D অবজেক্টের জন্য টার্গেট রোটেশন Float ভ্যালু
        targetObjectAngle = angle; 

        // ==========================================
        // 🌟 প্রেডিক্টর ডেটাবেস আপডেট 
        // ==========================================
        if (PredictorUIManager.Instance != null)
        {
            PredictorUIManager.Instance.selectedAngle = angle;
            
            // স্ট্যাটাস আপডেট করে Generating করে দেওয়া হলো
            if (PredictorUIManager.Instance.statusText != null) 
            {
                PredictorUIManager.Instance.statusText.text = "Generating...";
            }
        }
    }
}