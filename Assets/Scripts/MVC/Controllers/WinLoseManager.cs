using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinLoseManager : MonoBehaviour
{
    public static WinLoseManager Instance;

    [Header("--- UI Panels & Overlays ---")]
    public GameObject winPanel;       
    public GameObject losePanel;      
    public GameObject darkBackgroundOverlay; 

    [Header("--- Elements to Hide Completely ---")]
    public GameObject[] gameplayTools; 
    public GameObject[] backgroundUIButtons; 

    [Header("--- Win/Lose Panel Buttons ---")]
    public Button nextChallengeButton; 
    public Button playAgainButton;     
    public Button[] goHomeButtons; 

    [Header("--- Scene Navigation Settings ---")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        HideAllPanels();

        if (nextChallengeButton != null) nextChallengeButton.onClick.AddListener(LoadNextChallenge);
        if (playAgainButton != null) playAgainButton.onClick.AddListener(RestartGame);
        
        foreach (Button btn in goHomeButtons)
        {
            if (btn != null) btn.onClick.AddListener(GoToMainMenu);
        }
    }

    public void ShowWinPanel()
    {
        ToggleGameplayElements(false); 
        if (darkBackgroundOverlay != null) darkBackgroundOverlay.SetActive(true);
        if (winPanel != null) winPanel.SetActive(true);
    }

    public void ShowLosePanel()
    {
        ToggleGameplayElements(false); 
        if (darkBackgroundOverlay != null) darkBackgroundOverlay.SetActive(true);
        if (losePanel != null) losePanel.SetActive(true);
    }

    public void HideAllPanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (darkBackgroundOverlay != null) darkBackgroundOverlay.SetActive(false);
        ToggleGameplayElements(true);
    }

    private void ToggleGameplayElements(bool state)
    {
        foreach (GameObject tool in gameplayTools)
        {
            if (tool != null) tool.SetActive(state);
        }
        foreach (GameObject btn in backgroundUIButtons)
        {
            if (btn != null) btn.SetActive(state);
        }
    }

    // --- Scene Navigation Methods ---

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 🌟 Updated function: to open panel by redirecting to main menu
    public void LoadNextChallenge()
    {
        Debug.Log("Redirecting to Stone Market in Main Menu...");

        // 🌟 Saving the signal
        PlayerPrefs.SetInt("AutoOpenStoneMarket", 1);
        PlayerPrefs.Save();

        // Loading the main menu
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName); 
        }
        else
        {
            Debug.LogError("Main Menu Scene Name is empty! Please assign it in the Inspector.");
        }
    }
}