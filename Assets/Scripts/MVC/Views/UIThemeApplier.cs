using UnityEngine;

public class UIThemeApplier : MonoBehaviour
{
    [Header("Assign UI Parents")]
    public GameObject classicUIParent;
    public GameObject modernUIParent;

    void OnEnable()
    {
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (GameModeManager.Instance == null) return;

        bool isClassic = (GameModeManager.Instance.currentTheme == GameModeManager.GameTheme.Classic);

        if (classicUIParent != null) classicUIParent.SetActive(isClassic);
        if (modernUIParent != null) modernUIParent.SetActive(!isClassic);

        Debug.Log($"[UIThemeApplier] Applied Theme: {(isClassic ? "Classic" : "Modern")}");

        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.OnThemeChanged();
        }
    }
}
