using UnityEngine;

public class StoneDragger : MonoBehaviour
{
    private Camera mainCam;
    private float zDistance;
    private Vector3 offset;
    private bool isDragging = false;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 🌟 মাউসের ডান বাটন (Right Click) চাপলে
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // চেক করবে মাউসটি এই পাথরের ওপরে আছে কি না
                if (hit.collider.gameObject == gameObject)
                {
                    isDragging = true;
                    zDistance = mainCam.WorldToScreenPoint(transform.position).z;
                    
                    // মাউস এবং পাথরের মাঝের দূরত্ব (Offset) হিসাব করা, যাতে পাথর লাফ না মারে
                    offset = transform.position - GetMouseWorldPos();
                }
            }
        }

        // 🌟 ডান বাটন চেপে ধরে টানলে পাথর মুভ করবে
        if (Input.GetMouseButton(1) && isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }

        // 🌟 ডান বাটন ছেড়ে দিলে ড্র্যাগিং বন্ধ হবে
        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }
    }

    // স্ক্রিনের মাউস পজিশনকে 3D দুনিয়ার পজিশনে কনভার্ট করার ফাংশন
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = zDistance; // পাথরের গভীরতা (Z) ঠিক রাখা
        return mainCam.ScreenToWorldPoint(mousePos);
    }
}