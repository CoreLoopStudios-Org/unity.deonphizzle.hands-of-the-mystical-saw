using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModeSelectionManager : MonoBehaviour
{
    public static ModeSelectionManager Instance;

    [Header("--- UI Elements ---")]
    [Tooltip("Dynamic fallback. The system will look for both Classic and Modern panels dynamically.")]
    public GameObject modeSelectionPanel; 
    public Button modernModeButton;       
    public Button classicModeButton;     
    public Button closeButton;          

    [Header("--- Overlay & Environment ---")]
    [Tooltip("Drag the background black screen (Image) here, which will block other buttons")]
    public GameObject darkBackgroundOverlay; // 🌟 New: Dark background

    [Header("--- Scene Names ---")]
    public string modernSceneName = "StoneGenerator Scene";
    public string classicSceneName = "StoneCuttingScene_Classic";

    // Dynamic panels resolved at runtime
    private GameObject classicPanel;
    private GameObject modernPanel;

    private void Awake()
    {
        // Singleton setup
        Instance = this; 
        
        // Find UI parents and search panels
        UIThemeApplier applier = Object.FindFirstObjectByType<UIThemeApplier>(FindObjectsInactive.Include);
        if (applier != null)
        {
            classicPanel = FindPanel(applier.classicUIParent, "ModeSelectionPanel");
            if (classicPanel == null) classicPanel = FindPanel(applier.classicUIParent, "Mode-Selection Panel");
            
            modernPanel = FindPanel(applier.modernUIParent, "ModeSelectionPanel");
            if (modernPanel == null) modernPanel = FindPanel(applier.modernUIParent, "Mode-Selection Panel");
        }
        else
        {
            // Try searching from main Canvas direct children
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform classicUI = canvas.transform.Find("Classic_UI");
                if (classicUI != null)
                {
                    classicPanel = FindPanel(classicUI.gameObject, "ModeSelectionPanel");
                    if (classicPanel == null) classicPanel = FindPanel(classicUI.gameObject, "Mode-Selection Panel");
                }
                
                Transform modernUI = canvas.transform.Find("Modern_UI");
                if (modernUI != null)
                {
                    modernPanel = FindPanel(modernUI.gameObject, "ModeSelectionPanel");
                    if (modernPanel == null) modernPanel = FindPanel(modernUI.gameObject, "Mode-Selection Panel");
                }
            }
        }

        // If not found dynamically, fallback to the assigned inspector panel
        if (classicPanel == null && modernPanel == null && modeSelectionPanel != null)
        {
            // Figure out if the assigned panel is classic or modern
            if (modeSelectionPanel.name.Contains("Classic"))
            {
                classicPanel = modeSelectionPanel;
            }
            else
            {
                modernPanel = modeSelectionPanel;
            }
        }
    }

    private GameObject FindPanel(GameObject parent, string prefix)
    {
        if (parent == null) return null;
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
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
        // Ensure the panels and dark background are kept off at the start
        if (classicPanel != null) classicPanel.SetActive(false);
        if (modernPanel != null) modernPanel.SetActive(false);
        
        // Also keep fallback off
        if (modeSelectionPanel != null && modeSelectionPanel != classicPanel && modeSelectionPanel != modernPanel)
        {
            modeSelectionPanel.SetActive(false);
        }
        
        if (darkBackgroundOverlay != null) darkBackgroundOverlay.SetActive(false);

        // Bind buttons on both resolved panels
        SetupPanelButtons(classicPanel);
        SetupPanelButtons(modernPanel);

        // Bind fallback buttons from inspector
        if (modernModeButton != null) modernModeButton.onClick.AddListener(LoadModernMode);
        if (classicModeButton != null) classicModeButton.onClick.AddListener(LoadClassicMode);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
    }

    private void SetupPanelButtons(GameObject panel)
    {
        if (panel == null) return;
        
        Button modernBtn = FindButtonInChild(panel, "Modern");
        Button classicBtn = FindButtonInChild(panel, "Classic");
        Button closeBtn = FindButtonInChild(panel, "Cross-Button");
        
        if (modernBtn != null) 
        {
            modernBtn.onClick.RemoveListener(LoadModernMode);
            modernBtn.onClick.AddListener(LoadModernMode);
        }
        if (classicBtn != null) 
        {
            classicBtn.onClick.RemoveListener(LoadClassicMode);
            classicBtn.onClick.AddListener(LoadClassicMode);
        }
        if (closeBtn != null) 
        {
            closeBtn.onClick.RemoveListener(ClosePanel);
            closeBtn.onClick.AddListener(ClosePanel);
        }
    }

    private Button FindButtonInChild(GameObject parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null) return btn;
            }
        }
        return null;
    }

    // 🌟 Panel and black screen will turn on together
    public void ShowPanel()
    {
        Debug.Log("Showing Panels and Dark Overlay...");
        if (darkBackgroundOverlay != null) darkBackgroundOverlay.SetActive(true);
        
        // Get the active theme
        GameModeManager.GameTheme activeTheme = GameModeManager.GameTheme.Modern;
        if (GameModeManager.Instance != null)
        {
            activeTheme = GameModeManager.Instance.currentTheme;
        }
        else
        {
            activeTheme = (GameModeManager.GameTheme)PlayerPrefs.GetInt("SavedGameTheme", 1);
        }

        // Determine which panel to activate based on active theme
        GameObject targetPanel = (activeTheme == GameModeManager.GameTheme.Classic) ? classicPanel : modernPanel;

        // If target panel is null, try the opposite one as a fallback, or the inspector modeSelectionPanel
        if (targetPanel == null) targetPanel = (activeTheme == GameModeManager.GameTheme.Classic) ? modernPanel : classicPanel;
        if (targetPanel == null) targetPanel = modeSelectionPanel;

        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[ModeSelectionManager] No mode selection panels could be resolved!");
        }
    }

    // 🌟 Panel and black screen will turn off together
    public void ClosePanel()
    {
        Debug.Log("Closing Panels and Dark Overlay...");
        if (classicPanel != null) classicPanel.SetActive(false);
        if (modernPanel != null) modernPanel.SetActive(false);
        if (modeSelectionPanel != null) modeSelectionPanel.SetActive(false);
        
        if (darkBackgroundOverlay != null) darkBackgroundOverlay.SetActive(false);
    }

    public void LoadModernMode()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetTheme(GameModeManager.GameTheme.Modern);
        }
        else
        {
            if (StoneServer.Instance != null) StoneServer.Instance.ChosenMode = GameMode.Modern;
            PlayerPrefs.SetInt("SavedGameTheme", (int)GameModeManager.GameTheme.Modern);
            PlayerPrefs.Save();
        }
        Debug.Log("Modern Mode Selected as Global Setting.");
        ClosePanel(); 
    }

    public void LoadClassicMode()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetTheme(GameModeManager.GameTheme.Classic);
        }
        else
        {
            if (StoneServer.Instance != null) StoneServer.Instance.ChosenMode = GameMode.Classic;
            PlayerPrefs.SetInt("SavedGameTheme", (int)GameModeManager.GameTheme.Classic);
            PlayerPrefs.Save();
        }
        Debug.Log("Classic Mode Selected as Global Setting.");
        ClosePanel(); 
    }
}