using UnityEngine;

public class SimpleTorch : MonoBehaviour
{
    [Header("Torch Components")]
    public GameObject torchVisual;     
    public Light torchLight;          

    void Start()
    {
        // Light will be off at game start but position will not move
        SetState(false);
    }

    public void ToggleTorch(bool state)
    {
        SetState(state);
    }

    void SetState(bool state)
    {
        if (torchVisual != null) torchVisual.SetActive(state);
        
        if (torchLight != null) 
        {
            torchLight.enabled = state;
            // This will force render if the light is not visible in the game
            torchLight.renderMode = LightRenderMode.ForcePixel; 
            torchLight.gameObject.SetActive(state);
        }
    }
}