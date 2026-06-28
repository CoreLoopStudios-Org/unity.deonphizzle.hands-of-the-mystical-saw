using System;
using UnityEngine;

public class destroyOnHit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        
        Destroy(other.gameObject);
    }
}
