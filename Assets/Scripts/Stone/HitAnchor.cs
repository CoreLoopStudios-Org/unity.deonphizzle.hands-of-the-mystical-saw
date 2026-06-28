using UnityEngine;

public class HitAnchor : MonoBehaviour
{
    [HideInInspector]
    public StoneGenerator stoneManager; 
    public bool isPrimary; 

    // 🌟 নতুন যোগ করা হলো: গেম শুরুতেই সে ম্যানেজারকে নিজে নিজে খুঁজে নেবে
    void Start()
    {
        if (stoneManager == null)
        {
            stoneManager = FindFirstObjectByType<StoneGenerator>();
        }
    }

    // টুল যখন এই অ্যাঙ্করে আঘাত করবে
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tool") || other.CompareTag("Hammer"))
        {
            // জেনারেটরকে জানাবে যে অ্যাঙ্করটি ধ্বংস হয়েছে
            if (stoneManager != null)
            {
                stoneManager.AnchorDestroyed(this);
            }
            
            // নিজেকে গায়েব করে দেওয়া
            Destroy(gameObject);
        }
    }
}