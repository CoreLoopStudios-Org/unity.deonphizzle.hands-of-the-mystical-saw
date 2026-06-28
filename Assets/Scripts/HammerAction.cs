using UnityEngine;
using System.Collections;

public class HammerAction : MonoBehaviour
{
    public JadeCuttingGame gameManager; 
    public GameObject fracturedStonePrefab; 
    public float breakForce = 500f;         

    void Update()
    {
        // 🌟 জাদুকরী লাইন: টর্চ অন থাকলে নিচের কোনো ক্লিক বা হাতুড়ির কোড কাজ করবে না!
        if (StoneSpinController.GlobalTorchActive) return;

        // মাউস ক্লিক করলে বা হাতুড়ি বাটনে ক্লিক করলে
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // যদি পাথরটিতে (Stone_1 বা Stone ট্যাগ যুক্ত) আঘাত লাগে
                if (hit.transform.name == "Stone_1" || hit.transform.CompareTag("Stone"))
                {
                    BreakStone(hit.transform.gameObject, hit.point);
                }
            }
        }
    }

    void BreakStone(GameObject originalStone, Vector3 hitPoint)
    {
        // ১. GameManager কে জানানো
        if (gameManager != null)
        {
            gameManager.OnActionButtonClick();
        }

        // ২. অরিজিনাল পাথরটি লুকিয়ে ফেলা
        originalStone.SetActive(false);

        // ৩. ভাঙা পাথরের টুকরোগুলো সেই জায়গায় তৈরি করা
        if (fracturedStonePrefab != null)
        {
            GameObject pieces = Instantiate(fracturedStonePrefab, originalStone.transform.position, originalStone.transform.rotation);
            pieces.transform.localScale = originalStone.transform.localScale;

            // ৪. প্রতিটি টুকরোকে ছিটকে দেওয়ার জন্য বল প্রয়োগ করা
            foreach (Rigidbody rb in pieces.GetComponentsInChildren<Rigidbody>())
            {
                rb.AddExplosionForce(breakForce, hitPoint, 2f);
            }

            // ৫. ৩ সেকেন্ড পর টুকরোগুলো মুছে ফেলা
            Destroy(pieces, 3f);
        }
    }
}