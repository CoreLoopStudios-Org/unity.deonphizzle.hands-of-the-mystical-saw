using UnityEngine;
// using EzySlice; // আপনার EzySlice এর নেমস্পেস যদি প্রয়োজন হয়

public class StoneCuttingHandler : MonoBehaviour
{
    [Header("Saw Settings")]
    public Transform sawBladeVisual;     // করারতের ব্লেডের ৩ডি মডেল
    public ParticleSystem sawDustParticles; // গুঁড়ো ওড়ার পার্টিকেল ইফেক্ট
    public float cuttingDepth = 0.1f;    // করারতের ব্লেড কতটা পাথরের ভেতরে ঢুকবে
    public float bladeSpinSpeed = 1000f; // ব্লেড কত জোরে ঘুরবে

    [Header("Target")]
    public GameObject targetStone; // আপনার ঘোরন্ত পাথর (targetStone)

    private bool isSawing = false;
    private Vector3 dragStartPoint; 
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        // শুরুতে করারতের ব্লেড ও পার্টিকেল বন্ধ
        if (sawBladeVisual != null) sawBladeVisual.gameObject.SetActive(false);
        if (sawDustParticles != null) sawDustParticles.Stop();
    }

    void Update()
    {
        // ১. টুল সিলেকশন চেক: যদি আপনার টুলের Enums মডার্ন মোড বা ক্লাসিক মোড অনুযায়ী Saw সিলেক্ট করে থাকে
        if (!IsSawSelected()) return;

        HandleSawInput();
    }

    private void HandleSawInput()
    {
        if (Input.GetMouseButtonDown(0)) // ১. ক্লিক শুরু
        {
            if (CanStartCutting())
            {
                // ২. করারতের ব্লেড অন করো, পার্টিকেল অন করো
                sawBladeVisual.gameObject.SetActive(true);
                sawDustParticles.Play();
                isSawing = true;
                dragStartPoint = GetMouseWorldPosition(); // করারতের স্টার্ট পজিশন নাও
            }
        }

        if (Input.GetMouseButton(0) && isSawing) // ২. ক্লিক করে ধরে ড্র্যাগ করা (The Cutting Process)
        {
            // ৩. করারতের ব্লেড ঘোরাও
            sawBladeVisual.Rotate(Vector3.right * bladeSpinSpeed * Time.deltaTime, Space.Self);
            
            // ৪. করারতের ব্লেডকে মাউসের সাথে সাথে পাথরের ভেতর দিয়ে মুভ করাও
            UpdateSawPositionOnMesh();
        }

        if (Input.GetMouseButtonUp(0) && isSawing) // ৩. ক্লিক শেষ (The Split)
        {
            // ৫. করারতের ব্লেড ও পার্টিকেল অফ করো
            isSawing = false;
            sawBladeVisual.gameObject.SetActive(false);
            sawDustParticles.Stop();

            // ৬. এখন EzySlice দিয়ে ফাইনাল কাট করো!
            PerformFinalEzySlice(dragStartPoint, GetMouseWorldPosition()); 
        }
    }

    // মাউস দিয়ে একটা Raycast করে পাথরের উপর করারতের পজিশন বের করার জন্য ফাংশন
    private void UpdateSawPositionOnMesh()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == targetStone)
            {
                // করারতের ব্লেডকে ক্লিক করা জায়গায় নিয়ে যাও এবং একটু ভেতরের দিকে ঢুকাও
                sawBladeVisual.position = hit.point - hit.normal * cuttingDepth;
                
                // পার্টিকেল সিস্টেমকেও করারতের ব্লেডের সাথে মুভ করাও
                sawDustParticles.transform.position = hit.point;
            }
        }
    }

    // EzySlice এর মাধ্যমে মূল কাটটি করার ফাংশন
    private void PerformFinalEzySlice(Vector3 start, Vector3 end)
    {
        // এখানে আপনার আগের EzySlice এর লজিক বসাবেন
        // start এবং end পয়েন্ট দিয়ে একটি কাটিং প্লেন তৈরি করে স্লাইস করবেন।
        // কাট হওয়ার পর আপনি কাটের দাগ বা কচ্ছপের খোলসের মতো কিছু চাইলে, স্লাইসড মেশের গায়ে কাটের দাগের টেক্সচার (Texture) লাগাতে হবে।
        Debug.Log("Now EzySlice performs the actual split from: " + start + " to: " + end);
    }

    // ==========================================
    // 🌟 হেল্পার ফাংশনস (এগুলো আপনার বর্তমান কোডে আছে)
    // ==========================================
    private bool IsSawSelected() { /* আপনার টুল সিলেকশন এনুমের চেক বসান */ return true; } 
    private bool CanStartCutting() { /* পাথরটা কি কাটার উপযোগী? */ return true; }
    private Vector3 GetMouseWorldPosition() { /* Raycast করে মাউসের ওয়ার্ল্ড পজিশন বের করুন */ return Vector3.zero; }
}