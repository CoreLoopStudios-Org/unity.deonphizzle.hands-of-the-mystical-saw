using UnityEngine;

public class ButtonGroupManager : MonoBehaviour
{
    [Header("Assign Highlight Images Here")]
    public GameObject[] highlightImages; 

    void Start()
    {
        // 🌟 বুলডোজার লজিক: গেম শুরু হলে ইনডেক্স যাই থাকুক, সব হাইলাইট জোর করে অফ করে দাও!
        foreach (var img in highlightImages)
        {
            if (img != null)
            {
                img.SetActive(false);
            }
        }
    }

    // বাটন ক্লিক হলে এই ফাংশনটা কল হবে (Inspector থেকে 0, 1, 2 পাঠাবে)
    public void SelectButton(int selectedIndex)
    {
        for (int i = 0; i < highlightImages.Length; i++)
        {
            if (highlightImages[i] != null)
            {
                // যেটায় ক্লিক পড়বে শুধু সেটা অন হবে, বাকি সব অফ!
                highlightImages[i].SetActive(i == selectedIndex);
            }
        }
    }
}