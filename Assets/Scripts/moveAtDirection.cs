using System;
using Unity.VisualScripting;
using UnityEngine;

public class moveAtDirection : MonoBehaviour
{
    public Vector3 direction;
    public float speed;

    private void Update()
    {
        // 🌟 ম্যাজিক শেকল: টর্চ জ্বললে এই হাতুড়ি এক চুলও সামনে এগোবে না!
        if (StoneSpinController.GlobalTorchActive) return;

        transform.Translate(direction * (speed * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(0, 0, 0), direction); 
    }
}