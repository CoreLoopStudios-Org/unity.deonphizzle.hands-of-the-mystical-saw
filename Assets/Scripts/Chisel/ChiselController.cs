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
    
    [Tooltip("Whether the base of the bottle is always upright")]
    public bool keepBodyUpright = true; 
    
    [Header("Hit Distance Tuning")]
    [Range(0f, 1f)]
    public float hitOffset = 0.1f;      

    // 🌟 Newly added: Hammer-like effects and sound settings 🌟
    [Header("Effects, Sound & Logic")]
    [Range(0f, 1f)] public float hitSoundVolume = 1f;

    [Space(10)]
    [Tooltip("First Effect (ie: Sparks)")]
    public GameObject hitEffectPrefab;  
    public Vector3 particleRotationOffset = new Vector3(0, 0, 0);
    [Tooltip("Main sound of hitting rock")]
    public AudioClip primaryHitSound;

    [Space(10)]
    [Tooltip("Second Effect (eg: Dust or Smoke)")]
    public GameObject secondaryHitEffectPrefab;  
    public Vector3 secondaryParticleRotationOffset = new Vector3(0, 0, 0);
    [Tooltip("Second effect sound (eg: stone breaking sound - optional)")]
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

        // 🌟 First effect and sound spawn 🌟
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

        // 🌟 Spawn second effect and sound 🌟
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