using UnityEngine;
using System.Collections;

public class TrimSawController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform targetPoint; // 🌟 Exactly how far the saw will go (the vanishing point).
    public float moveSpeed = 3f;  // 🌟 Departure and arrival speed (can be controlled from inspector)

    private Vector3 startPosition;
    private bool isMoving = false;

    void Start()
    {
        // Save saw position at game start
        startPosition = transform.position;
    }

    void Update()
    {
        // New click won't work if torch is on or saw is moving
        if (StoneSpinController.GlobalTorchActive || isMoving) return;

        // If the mouse is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 🌟 If the click is on the stone
                if (hit.collider.CompareTag("Stone"))
                {
                    StartCoroutine(MoveSawRoutine());
                }
            }
        }
    }

    IEnumerator MoveSawRoutine()
    {
        isMoving = true;

        // 1. go forward (to cut stone)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPosition, targetPoint.position, t);
            yield return null;
        }

        // 🌟 Lightly wet the stone after cutting (so that it looks realistic)
        yield return new WaitForSeconds(0.2f);

        // 2. Back to the previous place
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(targetPoint.position, startPosition, t);
            yield return null;
        }

        isMoving = false;
    }
}