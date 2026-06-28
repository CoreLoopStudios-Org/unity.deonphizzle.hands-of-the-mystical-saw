using UnityEngine;

public class StoneMovement : MonoBehaviour
{
    public float speed = 5f;
    public float friction = 0.98f; // Slowly stops the stone after being hit
    private Vector3 _moveDirection;
    private float _currentSpeed;

    private void Update()
    {
        // Move the stone based on the calculated direction and speed
        if (_currentSpeed > 0.1f)
        {
            transform.Translate(_moveDirection * (_currentSpeed * Time.deltaTime), Space.World);
            
            // Apply friction so it doesn't slide forever
            _currentSpeed *= friction;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object hitting the stone is tagged as "Hammer"
        if (collision.gameObject.CompareTag("Hammer"))
        {
            // Calculate direction: (Stone Position - Hammer Position)
            // This ensures the stone flies AWAY from the point of impact
            _moveDirection = (transform.position - collision.transform.position).normalized;
            
            // Reset speed to the initial push speed
            _currentSpeed = speed;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _moveDirection * 2);
    }
}