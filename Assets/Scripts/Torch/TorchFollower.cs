using UnityEngine;

public class TorchFollower : MonoBehaviour
{
    [Header("Torch Settings")]
    public float hoverDistance = 0.5f; // How far above the stone will the torch float
    public Vector3 rotationOffset = new Vector3(0, 0, 0); // To straighten the torch

    private Light torchLight;
    private MeshRenderer[] renderers;

    void Start()
    {
        // Finding the torch's lights and body
        torchLight = GetComponentInChildren<Light>();
        renderers = GetComponentsInChildren<MeshRenderer>();
        
        SetTorchVisibility(false); // Initially the torch will be invisible
    }

    void Update()
    {
        // 🌟 If the torch button is not on, hide the model
        if (!StoneSpinController.GlobalTorchActive)
        {
            SetTorchVisibility(false);
            return;
        }

        // Firing a laser (Ray) from the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // If the laser hits the stone
            if (hit.collider.CompareTag("Stone") || hit.collider.name.Contains("Stone"))
            {
                SetTorchVisibility(true); // Show the torch

                // 1. Position: HoverDistance (hoverDistance) from the stone surface
                Vector3 targetPosition = hit.point + hit.normal * hoverDistance;
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);

                // 2. Rotation: The face of the torch will always be towards the stone surface
                Quaternion targetRotation = Quaternion.LookRotation(-hit.normal) * Quaternion.Euler(rotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }
            else
            {
                // Will hide the torch when the mouse goes outside the rock
                SetTorchVisibility(false);
            }
        }
        else
        {
            SetTorchVisibility(false);
        }
    }

    // Magic function to turn torch body and light on-off
    private void SetTorchVisibility(bool isVisible)
    {
        if (torchLight != null) torchLight.enabled = isVisible;
        
        foreach (var r in renderers)
        {
            r.enabled = isVisible;
        }
    }
}