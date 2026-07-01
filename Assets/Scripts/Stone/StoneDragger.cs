using UnityEngine;

public class StoneDragger : MonoBehaviour
{
    private Camera mainCam;
    private float zDistance;
    private Vector3 offset;
    private bool isDragging = false;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 🌟 Pressing the right mouse button (Right Click).
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the mouse is over this rock
                if (hit.collider.gameObject == gameObject)
                {
                    isDragging = true;
                    zDistance = mainCam.WorldToScreenPoint(transform.position).z;
                    
                    // Calculate the distance (Offset) between the mouse and the stone, so that the stone does not jump
                    offset = transform.position - GetMouseWorldPos();
                }
            }
        }

        // 🌟 Hold the right button and drag to move the stone
        if (Input.GetMouseButton(1) && isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }

        // 🌟 Release the right button to stop dragging
        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }
    }

    // Function to convert screen mouse position to 3D world position
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = zDistance; // Fix the depth (Z) of the stone
        return mainCam.ScreenToWorldPoint(mousePos);
    }
}