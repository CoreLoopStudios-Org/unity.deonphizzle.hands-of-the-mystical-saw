using UnityEngine;

public class StoneShatter : MonoBehaviour
{
    public GameObject brokenStonePrefab; // এখানে আপনার ভাঙা পাথরের Prefab-টি ড্র্যাগ করে দিন

    void OnCollisionEnter(Collision collision)
    {
        // যদি হ্যামার পাথরকে আঘাত করে
        if (collision.gameObject.CompareTag("Hammer"))
        {
            Shatter();
        }
    }

    void Shatter()
    {
        // আস্ত পাথরের জায়গায় ভাঙা পাথরের টুকরোগুলো তৈরি করা
        Instantiate(brokenStonePrefab, transform.position, transform.rotation);

        // আসল আস্ত পাথরটি গেম থেকে ডিলিট করে দেওয়া
        Destroy(gameObject);
    }
}