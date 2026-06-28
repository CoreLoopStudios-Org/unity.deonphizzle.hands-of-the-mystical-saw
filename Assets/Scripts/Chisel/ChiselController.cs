using UnityEngine;
using System.Collections;

public class ChiselController : MonoBehaviour
{
    [Header("Tool State")]
    public bool isEquipped = false;    

    [Header("Straight Hit Settings")]
    public float hitSpeed = 20f;        
    public float returnSpeed = 10f;      
    public Vector3 rotationOffset = new Vector3(0, 0, 0); 
    
    [Tooltip("বাটালির বেস সবসময় খাড়া (Upright) থাকবে কি না")]
    public bool keepBodyUpright = true; 
    
    [Header("Hit Distance Tuning")]
    [Range(0f, 1f)]
    public float hitOffset = 0.1f;      

    // 🌟 নতুন যোগ করা হয়েছে: হাতুড়ির মতো ইফেক্ট এবং সাউন্ড সেটিংস 🌟
    [Header("Effects, Sound & Logic")]
    [Range(0f, 1f)] public float hitSoundVolume = 1f;

    [Space(10)]
    [Tooltip("প্রথম ইফেক্ট (যেমন: Sparks)")]
    public GameObject hitEffectPrefab;  
    public Vector3 particleRotationOffset = new Vector3(0, 0, 0);
    [Tooltip("পাথরে আঘাত করার প্রধান সাউন্ড")]
    public AudioClip primaryHitSound;

    [Space(10)]
    [Tooltip("দ্বিতীয় ইফেক্ট (যেমন: Dust বা Smoke)")]
    public GameObject secondaryHitEffectPrefab;  
    public Vector3 secondaryParticleRotationOffset = new Vector3(0, 0, 0);
    [Tooltip("দ্বিতীয় ইফেক্টের সাউন্ড (যেমন: পাথর ভাঙার শব্দ - ঐচ্ছিক)")]
    public AudioClip secondaryHitSound;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isHitting = false;     

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (StoneSpinController.GlobalTorchActive || !isEquipped) return;

        if (Input.GetMouseButtonDown(0) && !isHitting)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Stone") || hit.collider.name.Contains("Stone"))
                {
                    StartCoroutine(HitChisel(hit.point, hit.normal, hit.collider.gameObject));
                }
            }
        }
    }

    IEnumerator HitChisel(Vector3 targetPoint, Vector3 surfaceNormal, GameObject hitObject)
    {
        isHitting = true;

        Vector3 hitPosition = targetPoint + surfaceNormal * hitOffset; 
        
        Vector3 lookDirection = -surfaceNormal;
        
        if (keepBodyUpright)
        {
            lookDirection.y = 0; 
            
            if (lookDirection.magnitude < 0.001f) 
            {
                lookDirection = targetPoint - originalPosition;
                lookDirection.y = 0;
                if (lookDirection.magnitude < 0.001f) lookDirection = Vector3.forward;
            }
        }

        Quaternion targetRot = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(rotationOffset);

        float t = 0;
        Vector3 startPos = transform.position;
        
        transform.rotation = targetRot; 
        
        while (t < 1f)
        {
            t += Time.deltaTime * hitSpeed;
            transform.position = Vector3.Lerp(startPos, hitPosition, t);
            yield return null;
        }

        Debug.Log("💥 [Chisel Strike]: Chisel hits the stone straight!");

        // 🌟 প্রথম ইফেক্ট ও সাউন্ড স্পন করা 🌟
        if (hitEffectPrefab != null)
        {
            Quaternion particleRot = Quaternion.LookRotation(surfaceNormal) * Quaternion.Euler(particleRotationOffset);
            GameObject spark = Instantiate(hitEffectPrefab, targetPoint, particleRot);
            Destroy(spark, 2f); 
        }
        if (primaryHitSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1)
        {
            AudioSource.PlayClipAtPoint(primaryHitSound, targetPoint, hitSoundVolume);
        }

        // 🌟 দ্বিতীয় ইফেক্ট ও সাউন্ড স্পন করা 🌟
        if (secondaryHitEffectPrefab != null)
        {
            Quaternion secParticleRot = Quaternion.LookRotation(surfaceNormal) * Quaternion.Euler(secondaryParticleRotationOffset);
            GameObject secSpark = Instantiate(secondaryHitEffectPrefab, targetPoint, secParticleRot);
            Destroy(secSpark, 2f); 
        }
        if (secondaryHitSound != null && PlayerPrefs.GetInt("SoundEnabled", 1) == 1)
        {
            AudioSource.PlayClipAtPoint(secondaryHitSound, targetPoint, hitSoundVolume);
        }

        StoneGenerator stoneGen = hitObject.GetComponentInParent<StoneGenerator>();
        if (stoneGen != null)
        {
            stoneGen.RegisterToolStrike(); 
        }
        else
        {
            Debug.LogWarning("⚠️ StoneGenerator not found!");
        }

        yield return new WaitForSeconds(0.1f);
        
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.position = Vector3.Lerp(hitPosition, originalPosition, t);
            transform.rotation = Quaternion.Slerp(targetRot, originalRotation, t); 
            yield return null;
        }
        
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        isHitting = false; 
    }
    
    public void EquipChisel()
    {
        isEquipped = true;
    }

    public void UnequipChisel()
    {
        isEquipped = false;
    }
}