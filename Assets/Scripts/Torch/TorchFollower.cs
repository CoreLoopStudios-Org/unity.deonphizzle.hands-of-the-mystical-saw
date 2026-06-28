using UnityEngine;

public class TorchFollower : MonoBehaviour
{
    [Header("Torch Settings")]
    public float hoverDistance = 0.5f; // পাথর থেকে টর্চটা কতটুকু ওপরে ভাসবে
    public Vector3 rotationOffset = new Vector3(0, 0, 0); // টর্চ সোজা করার জন্য

    private Light torchLight;
    private MeshRenderer[] renderers;

    void Start()
    {
        // টর্চের লাইট এবং বডি খুঁজে বের করা
        torchLight = GetComponentInChildren<Light>();
        renderers = GetComponentsInChildren<MeshRenderer>();
        
        SetTorchVisibility(false); // শুরুতে টর্চ গায়েব থাকবে
    }

    void Update()
    {
        // 🌟 যদি টর্চ বাটন অন না থাকে, তাহলে মডেলটা গায়েব রাখো
        if (!StoneSpinController.GlobalTorchActive)
        {
            SetTorchVisibility(false);
            return;
        }

        // মাউসের পজিশন থেকে একটা লেজার (Ray) মারা হচ্ছে
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // লেজারটা যদি পাথরের গায়ে লাগে
            if (hit.collider.CompareTag("Stone") || hit.collider.name.Contains("Stone"))
            {
                SetTorchVisibility(true); // টর্চ দেখাও

                // ১. পজিশন: পাথরের সারফেস থেকে একটু দূরে (hoverDistance) ভাসবে
                Vector3 targetPosition = hit.point + hit.normal * hoverDistance;
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);

                // ২. রোটেশন: টর্চের মুখ সবসময় পাথরের সারফেসের দিকে থাকবে
                Quaternion targetRotation = Quaternion.LookRotation(-hit.normal) * Quaternion.Euler(rotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }
            else
            {
                // মাউস পাথরের বাইরে গেলে টর্চ লুকিয়ে ফেলবে
                SetTorchVisibility(false);
            }
        }
        else
        {
            SetTorchVisibility(false);
        }
    }

    // টর্চের বডি এবং আলো অন-অফ করার ম্যাজিক ফাংশন
    private void SetTorchVisibility(bool isVisible)
    {
        if (torchLight != null) torchLight.enabled = isVisible;
        
        foreach (var r in renderers)
        {
            r.enabled = isVisible;
        }
    }
}