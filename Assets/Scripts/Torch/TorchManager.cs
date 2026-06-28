using UnityEngine;

public class TorchManager : MonoBehaviour
{
    [Header("Torch Components")]
    public GameObject torchVisual;     
    public Light torchLight;          

    private bool isActive = false;

    // Awake এবং Start থেকে পজিশন সেভ করার লজিক সরিয়ে দেওয়া হয়েছে 
    // যাতে ইউনিটি আপনার সেট করা পজিশনে হাত না দেয়।

    void Start()
    {
        // শুরুতে সবকিছু বন্ধ থাকবে
        SetState(false);
    }

    public void ToggleTorch(bool state)
    {
        isActive = state;
        SetState(state);
    }

    void SetState(bool state)
    {
        // ১. ভিজ্যুয়াল মডেল অন/অফ
        if (torchVisual != null) 
            torchVisual.SetActive(state);

        // ২. লাইট অন/অফ (গেম অবজেক্ট এবং কম্পোনেন্ট দুইটাই অন করা হচ্ছে)
        if (torchLight != null) 
        {
            torchLight.gameObject.SetActive(state); 
            torchLight.enabled = state;
            
            // লাইট গেমে না দেখা গেলে নিচের লাইনটি ফোর্স রেন্ডার করতে সাহায্য করবে
            torchLight.renderMode = LightRenderMode.ForcePixel; 
        }
        
        Debug.Log("Torch State: " + state + " | Object Position: " + transform.position);
    }
}