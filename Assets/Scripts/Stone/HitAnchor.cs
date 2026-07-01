using UnityEngine;

public class HitAnchor : MonoBehaviour
{
    [HideInInspector]
    public StoneGenerator stoneManager; 
    public bool isPrimary; 

    // 🌟 NEW ADDED: He will automatically find the manager at the start of the game
    void Start()
    {
        if (stoneManager == null)
        {
            stoneManager = FindFirstObjectByType<StoneGenerator>();
        }
    }

    // When the tool hits this anchor
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tool") || other.CompareTag("Hammer"))
        {
            // Tells the generator that the anchor has been destroyed
            if (stoneManager != null)
            {
                stoneManager.AnchorDestroyed(this);
            }
            
            // Make yourself disappear
            Destroy(gameObject);
        }
    }
}