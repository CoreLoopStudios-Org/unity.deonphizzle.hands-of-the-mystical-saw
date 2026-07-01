using UnityEngine;
using EzySlice;

public class TableSawSlicer : MonoBehaviour
{
    public Material crossSectionMaterial;

    void OnTriggerEnter(Collider other)
    {
        // The stone will cut when it hits the blade
        if (other.CompareTag("Stone"))
        {
            SliceStone(other.gameObject);
        }
    }

    private void SliceStone(GameObject target)
    {
        // Cutting plane according to blade position and rotation
        Vector3 planePoint = transform.position;
        
        // 🌟 If the blade doesn't cut straight, check with transform.right or transform.forward instead of transform.up
        Vector3 planeNormal = transform.up; 

        SlicedHull hull = target.Slice(planePoint, planeNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);

            // --- 🌟 magic logic from your previous script ---
            
            // 1. Keeping the keystone in place (updating the mesh only)
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
                targetCollider.enabled = true; // Refresh the collision
            }

            // 2. The anchors on the stone should be dropped properly
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

            Destroy(upperHull); // Delete the extra part

            // 3. Just dropping the bottom piece with physics
            SetupSlicedComponent(lowerHull, target.layer);
        }
    }

    void SetupSlicedComponent(GameObject slicedObject, int originalLayer)
    {
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        slicedObject.layer = originalLayer;
        slicedObject.tag = "Untagged"; // so that this piece doesn't cut back on the blade

        // throw away with a little push
        rb.AddExplosionForce(100f, slicedObject.transform.position, 5f);

        // 🌟 Disappears into the air after 2 seconds!
        Destroy(slicedObject, 2f);
    }
}