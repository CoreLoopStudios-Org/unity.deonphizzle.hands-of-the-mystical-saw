using UnityEngine;
using System.Collections;

public class HammerAction : MonoBehaviour
{
    public JadeCuttingGame gameManager; 
    public GameObject fracturedStonePrefab; 
    public float breakForce = 500f;         

    void Update()
    {
        // 🌟 magic line: none of the click or hammer codes below will work if the torch is on!
        if (StoneSpinController.GlobalTorchActive) return;

        // When the mouse is clicked or the hammer button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // If the stone (Stone_1 or Stone tag) is hit
                if (hit.transform.name == "Stone_1" || hit.transform.CompareTag("Stone"))
                {
                    BreakStone(hit.transform.gameObject, hit.point);
                }
            }
        }
    }

    void BreakStone(GameObject originalStone, Vector3 hitPoint)
    {
        // 1. Report to GameManager
        if (gameManager != null)
        {
            gameManager.OnActionButtonClick();
        }

        // 2. Hiding the original stone
        originalStone.SetActive(false);

        // 3. The pieces of broken stone are made in that place
        if (fracturedStonePrefab != null)
        {
            GameObject pieces = Instantiate(fracturedStonePrefab, originalStone.transform.position, originalStone.transform.rotation);
            pieces.transform.localScale = originalStone.transform.localScale;

            // 4. Apply force to knock out each piece
            foreach (Rigidbody rb in pieces.GetComponentsInChildren<Rigidbody>())
            {
                rb.AddExplosionForce(breakForce, hitPoint, 2f);
            }

            // 5. Remove the pieces after 3 seconds
            Destroy(pieces, 3f);
        }
    }
}