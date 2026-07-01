using UnityEngine;

public class StoneShatter : MonoBehaviour
{
    public GameObject brokenStonePrefab; // Drag your broken stone prefab here

    void OnCollisionEnter(Collision collision)
    {
        // If the hammer hits the rock
        if (collision.gameObject.CompareTag("Hammer"))
        {
            Shatter();
        }
    }

    void Shatter()
    {
        // Making broken stone pieces replace whole stones
        Instantiate(brokenStonePrefab, transform.position, transform.rotation);

        // Delete the original whole stone from the game
        Destroy(gameObject);
    }
}