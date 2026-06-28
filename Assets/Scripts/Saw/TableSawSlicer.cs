using UnityEngine;
using EzySlice;

public class TableSawSlicer : MonoBehaviour
{
    public Material crossSectionMaterial;

    void OnTriggerEnter(Collider other)
    {
        // পাথর ব্লেডে লাগলেই কাটবে
        if (other.CompareTag("Stone"))
        {
            SliceStone(other.gameObject);
        }
    }

    private void SliceStone(GameObject target)
    {
        // ব্লেডের পজিশন এবং রোটেশন অনুযায়ী কাটার প্লেন
        Vector3 planePoint = transform.position;
        
        // 🌟 ব্লেড যদি সোজা না কাটে, তবে transform.up এর বদলে transform.right বা transform.forward দিয়ে চেক করবে
        Vector3 planeNormal = transform.up; 

        SlicedHull hull = target.Slice(planePoint, planeNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);

            // --- 🌟 তোমার আগের স্ক্রিপ্টের জাদুকরী লজিক ---
            
            // ১. মূল পাথরটাকে জায়গাতেই স্থির রাখা (শুধু মেশ আপডেট করা)
            Mesh newMesh = upperHull.GetComponent<MeshFilter>().sharedMesh;
            Material[] newMaterials = upperHull.GetComponent<MeshRenderer>().sharedMaterials;

            target.GetComponent<MeshFilter>().sharedMesh = newMesh;
            target.GetComponent<MeshRenderer>().sharedMaterials = newMaterials;

            MeshCollider targetCollider = target.GetComponent<MeshCollider>();
            if (targetCollider != null)
            {
                targetCollider.sharedMesh = null;
                targetCollider.sharedMesh = newMesh;
                targetCollider.enabled = false;
                targetCollider.enabled = true; // কলিশন রিফ্রেশ করা
            }

            // ২. পাথরের গায়ে থাকা অ্যাঙ্কর (Anchor) গুলো ঠিকমতো ফেলা
            for (int i = target.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = target.transform.GetChild(i);
                if (child.GetComponent<HitAnchor>() != null)
                {
                    float side = Vector3.Dot(child.position - planePoint, planeNormal);
                    if (side < 0) 
                    {
                        child.SetParent(lowerHull.transform);
                    }
                }
            }

            Destroy(upperHull); // বাড়তি অংশ ডিলিট করে দেওয়া

            // ৩. শুধু নিচের টুকরোটাকে ফিজিক্স দিয়ে ফেলে দেওয়া
            SetupSlicedComponent(lowerHull, target.layer);
        }
    }

    void SetupSlicedComponent(GameObject slicedObject, int originalLayer)
    {
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        slicedObject.layer = originalLayer;
        slicedObject.tag = "Untagged"; // যাতে এই টুকরোটা আবার ব্লেডে লেগে না কাটে

        // একটু ধাক্কা দিয়ে ফেলে দেওয়া
        rb.AddExplosionForce(100f, slicedObject.transform.position, 5f);

        // 🌟 ২ সেকেন্ড পর হাওয়ায় মিলিয়ে যাবে!
        Destroy(slicedObject, 2f);
    }
}