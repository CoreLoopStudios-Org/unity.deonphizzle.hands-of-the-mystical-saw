using UnityEngine;

public class TorchPermanentPosition : MonoBehaviour
{
    [Header("Torch Components")]
    public GameObject torchVisual;     
    public Light torchLight;          

    [Header("Saved Values (Set these in Inspector)")]
    public Vector3 savedPosition;
    public Vector3 savedRotation;

    void Start()
    {
        // গেম শুরুর সময় সংরক্ষিত পজিশনে টর্চ নিয়ে আসা
        transform.position = savedPosition;
        transform.eulerAngles = savedRotation;

        if (torchVisual != null) torchVisual.SetActive(true);
        if (torchLight != null) torchLight.enabled = true;
    }

    // ইউনিটি ইন্সপেক্টরে কম্পোনেন্টের ওপর রাইট ক্লিক করলে এই অপশনটি আসবে
    [ContextMenu("Save Current Transform Data")]
    public void SaveData()
    {
        savedPosition = transform.position;
        savedRotation = transform.eulerAngles;
        Debug.Log("Position & Rotation saved to script variables!");
    }
}