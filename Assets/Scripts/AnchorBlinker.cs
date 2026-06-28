using UnityEngine;

public class AnchorBlinker : MonoBehaviour
{
    [Header("--- Anchor Type ---")]
    [Tooltip("Check this if this prefab is a PRIMARY Anchor. Uncheck for SECONDARY.")]
    public bool isPrimaryAnchor = true;

    [Header("--- Visibility Settings ---")]
    public float blinkSpeed = 5f;
    
    [Tooltip("How bright it remains when the torch is OFF.")]
    public float dimGlow = 0.5f;
    
    [Tooltip("Maximum brightness when blinking or for X-Ray visibility!")]
    public float maxGlow = 25f; 
    
    [Tooltip("Color when the primary anchor is blinking.")]
    public Color primaryBlinkColor = Color.cyan; 

    [Tooltip("Color for the secondary anchor (constant glow).")]
    public Color secondaryGlowColor = Color.green;

    [Header("--- X-Ray Inspection Mode ---")]
    [Tooltip("If true, the anchor will render through the stone (like an X-Ray) when the torch is active.")]
    public bool enableXRayMode = true;

    private Renderer anchorRenderer;
    private Material anchorMat;
    private int originalRenderQueue;

    void Start()
    {
        anchorRenderer = GetComponent<Renderer>();
        if (anchorRenderer != null)
        {
            // Create a local material instance so each anchor can act independently
            anchorMat = anchorRenderer.material;
            anchorMat.EnableKeyword("_EMISSION");

            // Save the original render queue to revert back when torch is off
            originalRenderQueue = anchorMat.renderQueue;
        }
    }

    void Update()
    {
        if (anchorMat == null) return;

        bool isTorchOn = StoneSpinController.GlobalTorchActive;

        if (isTorchOn)
        {
            // 🌟 ENABLE X-RAY FOR BOTH ANCHORS 🌟
            if (enableXRayMode) 
            {
                // Force it to ignore depth and render on top of the stone
                anchorMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                anchorMat.renderQueue = 4000; // Overlay Queue
            }

            if (isPrimaryAnchor)
            {
                // Primary Anchor: Blinks with high intensity
                float currentGlow = Mathf.Lerp(dimGlow, maxGlow, Mathf.PingPong(Time.time * blinkSpeed, 1f));
                anchorMat.SetColor("_EmissionColor", primaryBlinkColor * currentGlow);
            }
            else
            {
                // Secondary Anchor: Stays static but gets a brightness boost to be visible through the stone
                anchorMat.SetColor("_EmissionColor", secondaryGlowColor * (maxGlow * 0.4f)); 
            }
        }
        else
        {
            // 🌟 DISABLE X-RAY 🌟
            if (enableXRayMode) 
            {
                // Revert to normal depth checking and rendering order
                anchorMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                anchorMat.renderQueue = originalRenderQueue; 
            }

            // Both anchors revert to their dim state based on their type
            if (isPrimaryAnchor)
            {
                anchorMat.SetColor("_EmissionColor", primaryBlinkColor * dimGlow);
            }
            else
            {
                anchorMat.SetColor("_EmissionColor", secondaryGlowColor * dimGlow);
            }
        }
    }
}