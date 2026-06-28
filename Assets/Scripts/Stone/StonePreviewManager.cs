using UnityEngine;

public class StonePreviewManager : MonoBehaviour
{
    [Header("--- Preview Setup ---")]
    public GameObject stonePrefab;      
    public Transform previewHolder;     

    [Header("--- Dynamic Size Settings ---")]
    public float smallScale = 1.0f;     
    public float mediumScale = 1.5f;    
    public float largeScale = 2.0f;     

    private GameObject currentPreviewStone;
    
    // লাইভ আপডেটের জন্য ভেরিয়েবল
    private float currentSpinSpeed = 10f;
    private string currentPattern = "Static";
    private float currentAngle = 45f; // 🌟 নতুন: এঙ্গেল ট্র্যাকিং
    private Vector3 originalPosition;

    void Start()
    {
        SpawnPreviewStone();
    }

    public void SpawnPreviewStone()
    {
        if (currentPreviewStone != null) Destroy(currentPreviewStone);
        
        currentPreviewStone = Instantiate(stonePrefab, previewHolder.position, previewHolder.rotation, previewHolder);
        originalPosition = currentPreviewStone.transform.localPosition;
        
        UpdateSize("Small");
    }

    public void UpdateSize(string size)
    {
        if (currentPreviewStone == null) return;
        
        float scaleToApply = mediumScale; 
        if (size == "Small") scaleToApply = smallScale;
        else if (size == "Medium") scaleToApply = mediumScale;
        else if (size == "Large") scaleToApply = largeScale;
        
        currentPreviewStone.transform.localScale = Vector3.one * scaleToApply;
    }

    public void UpdateSpeed(float speed)
    {
        currentSpinSpeed = speed;
    }

    // 🌟 নতুন: রোটেশন এঙ্গেল আপডেট করা
    public void UpdateAngle(float angle)
    {
        currentAngle = angle;
        ApplyTilt(); // এঙ্গেল চেঞ্জ হলেই পাথরটা একটু বেঁকে যাবে
    }

    public void UpdatePattern(string pattern)
    {
        currentPattern = pattern;
        if (currentPreviewStone != null)
        {
            // প্যাটার্ন চেঞ্জ হলে পাথর আগের জায়গায় রিসেট করে নেওয়া
            currentPreviewStone.transform.localPosition = originalPosition; 
            ApplyTilt(); 
        }
    }

    // 🌟 পাথরটাকে নির্দিষ্ট এঙ্গেলে বাঁকানোর ফাংশন
    private void ApplyTilt()
    {
        if (currentPreviewStone != null)
        {
            currentPreviewStone.transform.localRotation = Quaternion.Euler(currentAngle, 0, 0);
        }
    }

    void Update()
    {
        if (currentPreviewStone == null) return;

        // 🌟 ফিক্স করা মুভমেন্ট এবং রোটেশন লজিক
        if (currentPattern == "Static")
        {
            // ফিক্স: Static এ এখন আর ঘুরবে না, একদম স্থির থাকবে (শুধু Angle অনুযায়ী বাঁকা থাকবে)
        }
        else if (currentPattern == "Linear")
        {
            // 🌟 ফিক্স: Linear আগে কাজ করতো না। এখন ডানে-বামে সোজা মুভ করবে
            float offset = Mathf.PingPong(Time.time * (currentSpinSpeed * 0.05f), 2f) - 1f; 
            currentPreviewStone.transform.localPosition = originalPosition + new Vector3(offset, 0, 0);
        }
        else if (currentPattern == "Oscillation")
        {
            float offset = Mathf.Sin(Time.time * (currentSpinSpeed * 0.1f)) * 0.5f; 
            currentPreviewStone.transform.localPosition = originalPosition + new Vector3(offset, 0, 0);
        }
        else if (currentPattern == "Circular")
        {
            // শুধু Circular সিলেক্ট করলেই গোল হয়ে ঘুরবে
            currentPreviewStone.transform.Rotate(Vector3.up * currentSpinSpeed * Time.deltaTime, Space.World);
        }
        else if (currentPattern == "Chaotic")
        {
            float chaoticX = (Mathf.PerlinNoise(Time.time, 0) - 0.5f) * currentSpinSpeed;
            float chaoticY = (Mathf.PerlinNoise(0, Time.time) - 0.5f) * currentSpinSpeed;
            currentPreviewStone.transform.Rotate(new Vector3(chaoticX, chaoticY, 0) * Time.deltaTime);
        }
    }
}