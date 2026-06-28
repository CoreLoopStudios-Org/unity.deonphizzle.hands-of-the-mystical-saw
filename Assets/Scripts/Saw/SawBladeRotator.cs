using UnityEngine;

public class SawBladeRotator : MonoBehaviour
{
    public float rotationSpeed = 1000f; // ব্লেড ঘোরার স্পিড
    public Vector3 rotationAxis = new Vector3(1, 0, 0); // কোন অক্ষে ঘুরবে (X, Y বা Z)

    void Update()
    {
        // টর্চ বা অন্য কিছু অন থাকলে এটা বন্ধ রাখতে চাইলে এখানে সেই লজিক দিতে পারো
        if (StoneSpinController.GlobalTorchActive) return;

        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}