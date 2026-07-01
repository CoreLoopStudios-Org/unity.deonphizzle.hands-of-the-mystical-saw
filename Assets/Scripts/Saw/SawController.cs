using UnityEngine;
using EzySlice;

public class SawController : MonoBehaviour
{
    [Header("Slice Settings")]
    public Material crossSectionMaterial; 
    public LayerMask sliceMask; 

    [Header("UI Line")]
    public LineRenderer lineRenderer; 

    private Vector3 swipeStart;
    private Vector3 swipeEnd;
    private Camera mainCam;
    private float stoneZDepth = 10f; 

    void Start()
    {
        mainCam = Camera.main;
        
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        GameObject stone = GameObject.FindGameObjectWithTag("Stone");
        if (stone != null)
        {
            stoneZDepth = mainCam.WorldToScreenPoint(stone.transform.position).z;
        }

        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, mainCam.ScreenToWorldPoint(new Vector3(swipeStart.x, swipeStart.y, stoneZDepth)));
                lineRenderer.SetPosition(1, mainCam.ScreenToWorldPoint(new Vector3(swipeStart.x, swipeStart.y, stoneZDepth)));
            }
        }
        else if (Input.GetMouseButton(0))
        {
            swipeEnd = Input.mousePosition;
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(1, mainCam.ScreenToWorldPoint(new Vector3(swipeEnd.x, swipeEnd.y, stoneZDepth)));
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (lineRenderer != null) lineRenderer.enabled = false;
            PerformSlice();
        }
    }

    void PerformSlice()
    {
        Vector3 swipeDirection = swipeEnd - swipeStart;
        if (swipeDirection.magnitude < 10f) return; 
        
        swipeDirection.Normalize();

        Vector3 planeNormal = Vector3.Cross(mainCam.transform.forward, swipeDirection).normalized;
        Vector3 planePoint = mainCam.ScreenToWorldPoint(new Vector3((swipeStart.x + swipeEnd.x) / 2f, (swipeStart.y + swipeEnd.y) / 2f, stoneZDepth));

        Collider[] hits = Physics.OverlapSphere(planePoint, 10f, sliceMask);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Stone"))
            {
                SliceObject(hit.gameObject, planePoint, planeNormal);
            }
        }
    }

    void SliceObject(GameObject target, Vector3 planePoint, Vector3 planeNormal)
    {
        SlicedHull hull = target.Slice(planePoint, planeNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);

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
                targetCollider.enabled = true; 
            }

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

            Destroy(upperHull);
            SetupSlicedComponent(lowerHull, target.layer);
        }
    }

    void SetupSlicedComponent(GameObject slicedObject, int originalLayer)
    {
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        slicedObject.layer = originalLayer; 
        slicedObject.tag = "Stone"; 
        
        slicedObject.AddComponent<StoneShatter>(); 
        
        rb.AddExplosionForce(100f, slicedObject.transform.position, 5f);

        // 🌟 Magic Line: After 2 seconds the bottom piece will vanish into thin air!
        Destroy(slicedObject, 2f);
    }
}