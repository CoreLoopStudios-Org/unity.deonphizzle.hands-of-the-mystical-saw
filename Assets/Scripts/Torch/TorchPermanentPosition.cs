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
        // Bring the torch to the position saved at game start
        transform.position = savedPosition;
        transform.eulerAngles = savedRotation;

        if (torchVisual != null) torchVisual.SetActive(true);
        if (torchLight != null) torchLight.enabled = true;
    }

    // This option will appear when you right click on the component in the Unity inspector
    [ContextMenu("Save Current Transform Data")]
    public void SaveData()
    {
        savedPosition = transform.position;
        savedRotation = transform.eulerAngles;
        Debug.Log("Position & Rotation saved to script variables!");
    }
}