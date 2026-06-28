using UnityEngine;

public class SimpleTorch : MonoBehaviour
{
    [Header("Torch Components")]
    public GameObject torchVisual;     
    public Light torchLight;          

    void Start()
    {
        // গেম শুরুর সময় লাইট অফ থাকবে কিন্তু পজিশন নড়বে না
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
            // লাইট গেমে না দেখা গেলে এটি ফোর্স রেন্ডার করবে
            torchLight.renderMode = LightRenderMode.ForcePixel; 
            torchLight.gameObject.SetActive(state);
        }
    }
}