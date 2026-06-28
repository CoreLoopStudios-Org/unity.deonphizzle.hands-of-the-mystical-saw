using UnityEngine;
using System.Collections;

public class TrimSawController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform targetPoint; // 🌟 করাত ঠিক কতদূর যাবে (সেই অদৃশ্য পয়েন্ট)
    public float moveSpeed = 3f;  // 🌟 যাওয়ার এবং আসার স্পিড (ইন্সপেক্টর থেকে কন্ট্রোল করতে পারবে)

    private Vector3 startPosition;
    private bool isMoving = false;

    void Start()
    {
        // গেম শুরুর সময় করাতের পজিশন সেভ করে রাখা
        startPosition = transform.position;
    }

    void Update()
    {
        // টর্চ অন থাকলে বা করাত মুভ করা অবস্থায় থাকলে নতুন ক্লিক কাজ করবে না
        if (StoneSpinController.GlobalTorchActive || isMoving) return;

        // যদি মাউসে ক্লিক করা হয়
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 🌟 যদি ক্লিকটা পাথরের গায়ে লাগে
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

        // ১. সামনের দিকে যাওয়া (পাথর কাটার জন্য)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPosition, targetPoint.position, t);
            yield return null;
        }

        // 🌟 পাথর কাটার পর হালকা একটু ওয়েট করা (যাতে দেখতে রিয়েলিস্টিক লাগে)
        yield return new WaitForSeconds(0.2f);

        // ২. আবার আগের জায়গায় ফিরে আসা
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