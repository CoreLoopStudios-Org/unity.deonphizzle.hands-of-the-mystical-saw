using UnityEngine;

public class LoopDurationController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("এখানে আপনার পুরো ডায়াল বা স্পিন (Spin) ইমেজটি টেনে দিন")]
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

    // 🌟 আপডেট করা মেথড (ইউনিটি বাটনের জন্য একদম পারফেক্ট)
// 🌟 আপডেট করা মেথড (অ্যাঙ্গেলের দিক ঠিক করা হয়েছে)
    public void OnDialButtonClicked(float durationValue)
    {
        // ১. মেইন ডেটাবেসে ডেটা পাঠানো হলো
        if (PredictorUIManager.Instance != null)
        {
            PredictorUIManager.Instance.SetLoopDurationFromDial(durationValue);
        }

        // ২. আপনার ডিজাইনের অ্যাঙ্গেলগুলো ঠিক করা হলো (প্লাস-মাইনাস উল্টে দিয়েছি)
        if (durationValue == 0f) targetAngle = -135f;      // বামে নিচে
        else if (durationValue == 10f) targetAngle = -90f;   // বামে মাঝখানে
        else if (durationValue == 20f) targetAngle = -45f;   // বামে উপরে
        else if (durationValue == 30f) targetAngle = 0f;     // একদম সোজা উপরে
        else if (durationValue == 40f) targetAngle = 45f;    // ডানে উপরে
        else if (durationValue == 50f) targetAngle = 90f;    // ডানে মাঝখানে
        else if (durationValue == 60f) targetAngle = 135f;   // ডানে নিচে
    }
}