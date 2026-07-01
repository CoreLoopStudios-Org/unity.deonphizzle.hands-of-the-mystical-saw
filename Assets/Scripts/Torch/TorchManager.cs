using UnityEngine;

public class TorchManager : MonoBehaviour
{
    [Header("Torch Components")]
    public GameObject torchVisual;     
    public Light torchLight;          

    private bool isActive = false;

    // Removed logic to save position from Awake and Start 
    // so that Unity doesn't touch the position you set.

    void Start()
    {
        // Everything will be closed initially
        SetState(false);
    }

    public void ToggleTorch(bool state)
    {
        isActive = state;
        SetState(state);
    }

    void SetState(bool state)
    {
        // 1. Visual model on/off
        if (torchVisual != null) 
            torchVisual.SetActive(state);

        // 2. Lights on/off (turning on both game objects and components)
        if (torchLight != null) 
        {
            torchLight.gameObject.SetActive(state); 
            torchLight.enabled = state;
            
            // The following line will force render if the light is not visible in the game
            torchLight.renderMode = LightRenderMode.ForcePixel; 
        }
        
        Debug.Log("Torch State: " + state + " | Object Position: " + transform.position);
    }
}