using System;
using Unity.VisualScripting;
using UnityEngine;

public class moveAtDirection : MonoBehaviour
{
    public Vector3 direction;
    public float speed;

    private void Update()
    {
        // 🌟 Magic Shackle: When the torch is lit, this hammer won't move a hair!
        if (StoneSpinController.GlobalTorchActive) return;

        transform.Translate(direction * (speed * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(0, 0, 0), direction); 
    }
}