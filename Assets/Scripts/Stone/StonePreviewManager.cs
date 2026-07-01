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
    
    // Variables for live updates
    private float currentSpinSpeed = 10f;
    private string currentPattern = "Static";
    private float currentAngle = 45f; // 🌟 New: angle tracking
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

    // 🌟 New: Updating the rotation angle
    public void UpdateAngle(float angle)
    {
        currentAngle = angle;
        ApplyTilt(); // When the angle changes, the stone will bend a little
    }

    public void UpdatePattern(string pattern)
    {
        currentPattern = pattern;
        if (currentPreviewStone != null)
        {
            // Reset stones to previous position when pattern changes
            currentPreviewStone.transform.localPosition = originalPosition; 
            ApplyTilt(); 
        }
    }

    // 🌟 Function to bend the stone to a specified angle
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

        // 🌟 Fixed movement and rotation logic
        if (currentPattern == "Static")
        {
            // Fix: Static will no longer rotate, it will be completely fixed (only curved according to Angle)
        }
        else if (currentPattern == "Linear")
        {
            // 🌟 Fix: Linear was not working before. Now move straight left and right
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
            // Only select Circular to rotate
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