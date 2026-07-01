using UnityEngine;
// using EzySlice; // Your EzySlice namespace if needed

public class StoneCuttingHandler : MonoBehaviour
{
    [Header("Saw Settings")]
    public Transform sawBladeVisual;     // 3D model of the blade of the karate
    public ParticleSystem sawDustParticles; // Powder flying particle effect
    public float cuttingDepth = 0.1f;    // How far into the stone will the blade of the karat go
    public float bladeSpinSpeed = 1000f; // How hard the blade will spin

    [Header("Target")]
    public GameObject targetStone; // your rotating stone(targetStone)

    private bool isSawing = false;
    private Vector3 dragStartPoint; 
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        // Initialize the blade and particles of the carvet
        if (sawBladeVisual != null) sawBladeVisual.gameObject.SetActive(false);
        if (sawDustParticles != null) sawDustParticles.Stop();
    }

    void Update()
    {
        // 1. Tool Selection Check: If your tool's Enums have selected Saw according to Modern Mode or Classic Mode
        if (!IsSawSelected()) return;

        HandleSawInput();
    }

    private void HandleSawInput()
    {
        if (Input.GetMouseButtonDown(0)) // 1. Click Start
        {
            if (CanStartCutting())
            {
                // 2. Turn on the blade of Karnat, turn on the particles
                sawBladeVisual.gameObject.SetActive(true);
                sawDustParticles.Play();
                isSawing = true;
                dragStartPoint = GetMouseWorldPosition(); // Get the start position of the cursor
            }
        }

        if (Input.GetMouseButton(0) && isSawing) // 2. Clicking and Dragging (The Cutting Process)
        {
            // 3. Rotate the blade of the karate
            sawBladeVisual.Rotate(Vector3.right * bladeSpinSpeed * Time.deltaTime, Space.Self);
            
            // 4. Also move the blade of the crowbar through the stone with the mouse
            UpdateSawPositionOnMesh();
        }

        if (Input.GetMouseButtonUp(0) && isSawing) // 3. The Split (The Split)
        {
            // 5. Turn off the blade and particles of the karate
            isSawing = false;
            sawBladeVisual.gameObject.SetActive(false);
            sawDustParticles.Stop();

            // 6. Make the final cut with EzySlice now!
            PerformFinalEzySlice(dragStartPoint, GetMouseWorldPosition()); 
        }
    }

    // Function to find the position of the cursor on the stone by making a Raycast with the mouse
    private void UpdateSawPositionOnMesh()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == targetStone)
            {
                // Move the karat blade to the clicked area and move it slightly inward
                sawBladeVisual.position = hit.point - hit.normal * cuttingDepth;
                
                // Also move the particle system along with the blade of the karate
                sawDustParticles.transform.position = hit.point;
            }
        }
    }

    // Function to make the original cut with EzySlice
    private void PerformFinalEzySlice(Vector3 start, Vector3 end)
    {
        // Place your previous EzySlice logic here
        // Create a cutting plane with start and end points and slice.
        // After the cut, if you want something like a cut scar or a turtle shell, apply a cut scar texture to the sliced mesh.
        Debug.Log("Now EzySlice performs the actual split from: " + start + " to: " + end);
    }

    // ==========================================
    // 🌟 Helper functions (these are in your current code)
    // ==========================================
    private bool IsSawSelected() { /* check your tool selection enum */ return true; } 
    private bool CanStartCutting() { /* Is the stone suitable for cutting? */ return true; }
    private Vector3 GetMouseWorldPosition() { /* Raycast the world position of the mouse */ return Vector3.zero; }
}