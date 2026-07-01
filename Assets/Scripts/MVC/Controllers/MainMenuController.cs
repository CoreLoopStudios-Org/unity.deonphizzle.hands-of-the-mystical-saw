using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // 🌟 for TextMeshPro

public class MainMenuController : MonoBehaviour
{
    [Header("Top Bar Dynamic Data")]
    public TextMeshProUGUI tierText;
    public TextMeshProUGUI pointsText;

    [Header("Main Menu Panel")]
    public GameObject menuPanel; 

    [Header("Sub Panels")]
    public GameObject toolsPanel;
    public GameObject profilePanel;
    public GameObject leaderboardPanel;
    public GameObject storePanel;

    public static MainMenuController Instance;

    // Theme variants of panels resolved at runtime
    private GameObject menuPanelClassic, menuPanelModern;
    private GameObject toolsPanelClassic, toolsPanelModern;
    private GameObject profilePanelClassic, profilePanelModern;
    private GameObject leaderboardPanelClassic, leaderboardPanelModern;
    private GameObject storePanelClassic, storePanelModern;

    private void Awake()
    {
        Instance = this;
        ResolveThemePanels();
    }

    private void ResolveThemePanels()
    {
        UIThemeApplier applier = Object.FindFirstObjectByType<UIThemeApplier>(FindObjectsInactive.Include);
        if (applier != null)
        {
            GameObject classicRoot = applier.classicUIParent;
            GameObject modernRoot = applier.modernUIParent;

            menuPanelClassic = FindPanel(classicRoot, "Menu-Panel");
            toolsPanelClassic = FindPanel(classicRoot, "Tool-Panel");
            profilePanelClassic = FindPanel(classicRoot, "Profile-Panel");
            leaderboardPanelClassic = FindPanel(classicRoot, "Leadership-Panel");
            storePanelClassic = FindPanel(classicRoot, "Player-StoneMarket-Panel");

            menuPanelModern = FindPanel(modernRoot, "Menu-Panel");
            toolsPanelModern = FindPanel(modernRoot, "Tool-Panel");
            profilePanelModern = FindPanel(modernRoot, "Profile-Panel");
            leaderboardPanelModern = FindPanel(modernRoot, "Leadership-Panel");
            storePanelModern = FindPanel(modernRoot, "Player-StoneMarket-Panel");
        }
        else
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform classicUI = canvas.transform.Find("Classic_UI");
                if (classicUI != null)
                {
                    menuPanelClassic = FindPanel(classicUI.gameObject, "Menu-Panel");
                    toolsPanelClassic = FindPanel(classicUI.gameObject, "Tool-Panel");
                    profilePanelClassic = FindPanel(classicUI.gameObject, "Profile-Panel");
                    leaderboardPanelClassic = FindPanel(classicUI.gameObject, "Leadership-Panel");
                    storePanelClassic = FindPanel(classicUI.gameObject, "Player-StoneMarket-Panel");
                }

                Transform modernUI = canvas.transform.Find("Modern_UI");
                if (modernUI != null)
                {
                    menuPanelModern = FindPanel(modernUI.gameObject, "Menu-Panel");
                    toolsPanelModern = FindPanel(modernUI.gameObject, "Tool-Panel");
                    profilePanelModern = FindPanel(modernUI.gameObject, "Profile-Panel");
                    leaderboardPanelModern = FindPanel(modernUI.gameObject, "Leadership-Panel");
                    storePanelModern = FindPanel(modernUI.gameObject, "Player-StoneMarket-Panel");
                }
            }
        }
    }

    private GameObject FindPanel(GameObject parent, string prefix)
    {
        if (parent == null) return null;
        foreach (Transform child in parent.transform)
        {
            if (child.name.StartsWith(prefix))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    private void Start()
    {
        // Fix the parent and alignment of Mode-Selection Panel-Button-Modern at runtime if it is in root Canvas
        GameObject modernButton = GameObject.Find("Mode-Selection Panel-Button-Modern");
        if (modernButton == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform tButton = canvas.transform.Find("Mode-Selection Panel-Button-Modern");
                if (tButton != null) modernButton = tButton.gameObject;
            }
        }

        GameObject modernMenu = menuPanelModern != null ? menuPanelModern : menuPanel;
        if (modernButton != null && modernMenu != null && modernButton.transform.parent != modernMenu.transform)
        {
            modernButton.transform.SetParent(modernMenu.transform, false);
            GameObject classicButton = GameObject.Find("Mode-Selection Panel-Button-Classic");
            if (classicButton != null)
            {
                RectTransform rtModern = modernButton.GetComponent<RectTransform>();
                RectTransform rtClassic = classicButton.GetComponent<RectTransform>();
                if (rtModern != null && rtClassic != null)
                {
                    rtModern.anchorMin = rtClassic.anchorMin;
                    rtModern.anchorMax = rtClassic.anchorMax;
                    rtModern.anchoredPosition = rtClassic.anchoredPosition;
                    rtModern.sizeDelta = rtClassic.sizeDelta;
                    rtModern.pivot = rtClassic.pivot;
                }
            }
        }

        UpdateTopBarData();

        // Auto-open Marketplace (Store Panel) if returning from a challenge
        if (PlayerPrefs.GetInt("AutoOpenStoneMarket", 0) == 1)
        {
            PlayerPrefs.SetInt("AutoOpenStoneMarket", 0);
            PlayerPrefs.Save();
            OpenStore();
        }
        else
        {
            BackToMainMenu();
        }
    }

    private void UpdateTopBarData()
    {
        if (DataManager.Instance == null) return;
        if (tierText != null)
            tierText.text = DataManager.Instance.tier; 

        if (pointsText != null)
            pointsText.text = DataManager.Instance.totalPoints.ToString(); 
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading Scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("🛑 Quitting Game...");
        Application.Quit();
    }

    public void OpenTools()
    {
        ToggleMainMenu(false);
        ShowPanel(toolsPanel);
        Debug.Log("Tools Panel Opened!");
    }

    public void OpenProfile()
    {
        ToggleMainMenu(false);
        ShowPanel(profilePanel);
    }

    public void OpenLeaderboard()
    {
        ToggleMainMenu(false);
        ShowPanel(leaderboardPanel);
    }

    public void OpenStore()
    {
        ToggleMainMenu(false);
        ShowPanel(storePanel);
    }

    public void BackToMainMenu()
    {
        CloseAllPanels();
        ToggleMainMenu(true); 
        UpdateTopBarData(); 
    }

    private void ToggleMainMenu(bool status)
    {
        SetPanelActive(menuPanelClassic, menuPanelModern, menuPanel, status);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        CloseAllPanels();
        
        if (panelToShow == toolsPanel)
            SetPanelActive(toolsPanelClassic, toolsPanelModern, toolsPanel, true);
        else if (panelToShow == profilePanel)
            SetPanelActive(profilePanelClassic, profilePanelModern, profilePanel, true);
        else if (panelToShow == leaderboardPanel)
            SetPanelActive(leaderboardPanelClassic, leaderboardPanelModern, leaderboardPanel, true);
        else if (panelToShow == storePanel)
            SetPanelActive(storePanelClassic, storePanelModern, storePanel, true);
        else if (panelToShow != null)
            panelToShow.SetActive(true);
    }

    public void CloseAllPanels()
    {
        SetPanelActive(toolsPanelClassic, toolsPanelModern, toolsPanel, false);
        SetPanelActive(profilePanelClassic, profilePanelModern, profilePanel, false);
        SetPanelActive(leaderboardPanelClassic, leaderboardPanelModern, leaderboardPanel, false);
        SetPanelActive(storePanelClassic, storePanelModern, storePanel, false);
    }

    private void SetPanelActive(GameObject classicPanel, GameObject modernPanel, GameObject fallbackPanel, bool active)
    {
        if (classicPanel != null) classicPanel.SetActive(active);
        if (modernPanel != null) modernPanel.SetActive(active);
        
        if (classicPanel == null && modernPanel == null && fallbackPanel != null)
        {
            fallbackPanel.SetActive(active);
        }
    }

    public void OpenModeSelectionPanel()
    {
        if (ModeSelectionManager.Instance != null)
        {
            ModeSelectionManager.Instance.ShowPanel();
        }
        else
        {
            ModeSelectionManager manager = Object.FindFirstObjectByType<ModeSelectionManager>(FindObjectsInactive.Include);
            if (manager != null)
            {
                manager.ShowPanel();
            }
            else
            {
                Debug.LogError("ModeSelectionManager Instance not found in this scene!");
            }
        }
    }

    public void OnThemeChanged()
    {
        bool isClassic = (GameModeManager.Instance != null && GameModeManager.Instance.currentTheme == GameModeManager.GameTheme.Classic);

        // Find if settings panel was open in either theme
        bool settingsOpen = false;
        SettingsPanelController[] settingsControllers = Object.FindObjectsByType<SettingsPanelController>(FindObjectsInactive.Include);
        foreach (var controller in settingsControllers)
        {
            if (controller.gameObject.activeSelf)
            {
                settingsOpen = true;
                controller.gameObject.SetActive(false);
            }
        }

        // Check active states of main sub-panels
        bool toolsActive = (toolsPanelClassic != null && toolsPanelClassic.activeSelf) || (toolsPanelModern != null && toolsPanelModern.activeSelf);
        bool profileActive = (profilePanelClassic != null && profilePanelClassic.activeSelf) || (profilePanelModern != null && profilePanelModern.activeSelf);
        bool leaderboardActive = (leaderboardPanelClassic != null && leaderboardPanelClassic.activeSelf) || (leaderboardPanelModern != null && leaderboardPanelModern.activeSelf);
        bool storeActive = (storePanelClassic != null && storePanelClassic.activeSelf) || (storePanelModern != null && storePanelModern.activeSelf);

        CloseAllPanels();
        ToggleMainMenu(false);

        // Activate corresponding panels in the new theme
        if (toolsActive)
        {
            SetPanelActive(toolsPanelClassic, toolsPanelModern, toolsPanel, true);
        }
        else if (profileActive)
        {
            SetPanelActive(profilePanelClassic, profilePanelModern, profilePanel, true);
        }
        else if (leaderboardActive)
        {
            SetPanelActive(leaderboardPanelClassic, leaderboardPanelModern, leaderboardPanel, true);
        }
        else if (storeActive)
        {
            SetPanelActive(storePanelClassic, storePanelModern, storePanel, true);
        }
        else
        {
            ToggleMainMenu(true);
        }

        // If settings panel was open, open it in the new theme
        if (settingsOpen)
        {
            foreach (var controller in settingsControllers)
            {
                bool isClassicController = controller.name.Contains("Classic");
                if (isClassic == isClassicController)
                {
                    controller.gameObject.SetActive(true);
                }
            }
        }

        UpdateTopBarData();
    }
}