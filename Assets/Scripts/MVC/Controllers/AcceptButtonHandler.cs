using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AcceptButtonHandler : MonoBehaviour
{
    public Button acceptButton;

    void Start()
    {
        if (acceptButton == null) acceptButton = GetComponent<Button>();
        acceptButton.onClick.AddListener(OnAcceptClicked);
    }

    void OnAcceptClicked()
    {
        Debug.Log("<color=cyan>🔥 Accept Button Clicked: Launching gameplay directly...</color>");

        // Check saved theme
        GameModeManager.GameTheme activeTheme = GameModeManager.GameTheme.Modern;
        if (GameModeManager.Instance != null)
        {
            activeTheme = GameModeManager.Instance.currentTheme;
        }
        else
        {
            activeTheme = (GameModeManager.GameTheme)PlayerPrefs.GetInt("SavedGameTheme", 1);
        }

        // Route dynamically based on setting
        string targetScene = (activeTheme == GameModeManager.GameTheme.Classic) 
            ? "StoneCuttingScene_Classic" 
            : "StoneGenerator Scene";

        Debug.Log($"Loading game mode: {activeTheme} -> Scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}