using UnityEngine;

public class SawBladeRotator : MonoBehaviour
{
    public float rotationSpeed = 1000f; // Blade rotation speed
    public Vector3 rotationAxis = new Vector3(1, 0, 0); // which axis to rotate on (X, Y or Z)

    void Update()
    {
        // If you want to turn off the torch or something else, you can put that logic here
        if (StoneSpinController.GlobalTorchActive) return;

        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}