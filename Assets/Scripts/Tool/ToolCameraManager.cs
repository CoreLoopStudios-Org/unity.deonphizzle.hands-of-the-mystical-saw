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
    [Tooltip("How much will the camera rotate when the torch zooms in (Give X, Y, Z rotation of Main Camera from Inspector)")]
    public Vector3 torchZoomRotation; // 🌟 Torch rotation only

    [Header("--- Background Parallax ---")]
    public RectTransform backgroundImage;
    public float backgroundZoomOutScale = 0.9f;

    [Header("--- Clipping Settings ---")]
    public float nearClipPlane = 0.01f;

    [Header("--- UI Elements ---")]
    public Button torchButton; 

    private float defaultFOV;
    private Quaternion defaultLocalRotation; // 🌟 To remember the camera's default rotation
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
            defaultLocalRotation = cam.transform.localRotation; // Save game start rotation
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

        // Changing FOV and rotation smoothly
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
        currentTargetRotation = defaultLocalRotation; // 🌟 Normal tool rotation will not change, will stay in place!
        Debug.Log("<color=green>Normal Zoom In Active!</color>"); 
    }

    public void ZoomInOnTorch()
    {
        isZoomingIn = true;
        currentTargetFOV = torchZoomInFOV; 
        currentTargetRotation = Quaternion.Euler(torchZoomRotation); // 🌟 Only the rotation you give for the torch will work
        Debug.Log("<color=orange>Torch Zoom In Active!</color>"); 
    }

    public void ZoomOutToDefault()
    {
        isZoomingIn = false;
        currentTargetFOV = defaultFOV; 
        currentTargetRotation = defaultLocalRotation; // 🌟 Zooming out will return to previous rotation
        Debug.Log("<color=red>Zoom Out Active!</color>"); 
    }
}