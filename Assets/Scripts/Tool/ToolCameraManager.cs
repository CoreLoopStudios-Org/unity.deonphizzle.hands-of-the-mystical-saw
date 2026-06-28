using UnityEngine;
using UnityEngine.UI;

public class ToolCameraManager : MonoBehaviour
{
    public static ToolCameraManager Instance;

    [Header("--- Zoom Settings ---")]
    public float zoomInFOV = 30f; 
    public float torchZoomInFOV = 9f; 
    public float zoomSpeed = 5f;    

    [Header("--- Torch Rotation Settings ---")]
    [Tooltip("টর্চ জুম ইন করলে ক্যামেরা কতটুকু ঘুরবে (Inspector থেকে Main Camera এর X, Y, Z রোটেশন দিন)")]
    public Vector3 torchZoomRotation; // 🌟 শুধুমাত্র টর্চের রোটেশন

    [Header("--- Background Parallax ---")]
    public RectTransform backgroundImage;
    public float backgroundZoomOutScale = 0.9f;

    [Header("--- Clipping Settings ---")]
    public float nearClipPlane = 0.01f;

    [Header("--- UI Elements ---")]
    public Button torchButton; 

    private float defaultFOV;
    private Quaternion defaultLocalRotation; // 🌟 ক্যামেরার ডিফল্ট রোটেশন মনে রাখার জন্য
    private Vector3 defaultBgScale; 
    private Camera cam;
    
    private bool isZoomingIn = false;
    private float currentTargetFOV; 
    private Quaternion currentTargetRotation; 

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
        if (cam != null)
        {
            defaultFOV = cam.fieldOfView;
            defaultLocalRotation = cam.transform.localRotation; // গেম শুরুর রোটেশন সেভ
            cam.nearClipPlane = nearClipPlane;
            
            currentTargetFOV = defaultFOV; 
            currentTargetRotation = defaultLocalRotation; 
        }

        if (backgroundImage != null)
        {
            defaultBgScale = backgroundImage.localScale;
        }
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // FOV এবং রোটেশন স্মুথলি পরিবর্তন করা
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, currentTargetFOV, Time.deltaTime * zoomSpeed);
        cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, currentTargetRotation, Time.deltaTime * zoomSpeed);

        if (backgroundImage != null)
        {
            Vector3 targetScale = isZoomingIn ? (defaultBgScale * backgroundZoomOutScale) : defaultBgScale;
            backgroundImage.localScale = Vector3.Lerp(backgroundImage.localScale, targetScale, Time.deltaTime * zoomSpeed);
        }
    }

    public void ZoomInOnStone()
    {
        isZoomingIn = true;
        currentTargetFOV = zoomInFOV; 
        currentTargetRotation = defaultLocalRotation; // 🌟 সাধারণ টুলে রোটেশন চেঞ্জ হবে না, আগের জায়গাতেই থাকবে!
        Debug.Log("<color=green>Normal Zoom In Active!</color>"); 
    }

    public void ZoomInOnTorch()
    {
        isZoomingIn = true;
        currentTargetFOV = torchZoomInFOV; 
        currentTargetRotation = Quaternion.Euler(torchZoomRotation); // 🌟 শুধু টর্চের জন্য আপনার দেওয়া রোটেশন কাজ করবে
        Debug.Log("<color=orange>Torch Zoom In Active!</color>"); 
    }

    public void ZoomOutToDefault()
    {
        isZoomingIn = false;
        currentTargetFOV = defaultFOV; 
        currentTargetRotation = defaultLocalRotation; // 🌟 জুম আউট করলে আগের রোটেশনে ফিরে আসবে
        Debug.Log("<color=red>Zoom Out Active!</color>"); 
    }
}