using UnityEngine;

public class ButtonGroupManager : MonoBehaviour
{
    [Header("Assign Highlight Images Here")]
    public GameObject[] highlightImages; 

    void Start()
    {
        // 🌟 Bulldozer Logic: Force all highlights to turn off at game start regardless of index!
        foreach (var img in highlightImages)
        {
            if (img != null)
            {
                img.SetActive(false);
            }
        }
    }

    // This function will be called when the button is clicked (will send 0, 1, 2 from Inspector)
    public void SelectButton(int selectedIndex)
    {
        for (int i = 0; i < highlightImages.Length; i++)
        {
            if (highlightImages[i] != null)
            {
                // Only the one that will be clicked will be on, all the rest are off!
                highlightImages[i].SetActive(i == selectedIndex);
            }
        }
    }
}