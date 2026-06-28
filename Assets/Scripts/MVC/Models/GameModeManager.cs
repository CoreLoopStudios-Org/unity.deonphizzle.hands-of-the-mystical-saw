using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    private static GameModeManager _instance;

    public static GameModeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameModeManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameModeManager");
                    _instance = go.AddComponent<GameModeManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public enum GameTheme { Classic, Modern }
    public GameTheme currentTheme;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTheme();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Called from Settings panel to change and save the theme
    public void SetTheme(GameTheme newTheme)
    {
        currentTheme = newTheme;
        PlayerPrefs.SetInt("SavedGameTheme", (int)currentTheme);
        PlayerPrefs.Save();

        // Keep existing StoneServer.Instance.ChosenMode in sync
        if (StoneServer.Instance != null)
        {
            StoneServer.Instance.ChosenMode = (newTheme == GameTheme.Classic) ? GameMode.Classic : GameMode.Modern;
        }
        
        Debug.Log("Game Theme Changed To: " + currentTheme);
        NotifyThemeAppliers();
    }

    private void LoadTheme()
    {
        // Default to Modern (1) if no preference is saved yet
        currentTheme = (GameTheme)PlayerPrefs.GetInt("SavedGameTheme", 1); 

        // Make sure StoneServer is synced with loaded theme
        if (StoneServer.Instance != null)
        {
            StoneServer.Instance.ChosenMode = (currentTheme == GameTheme.Classic) ? GameMode.Classic : GameMode.Modern;
        }
    }

    public void NotifyThemeAppliers()
    {
        UIThemeApplier[] appliers = FindObjectsByType<UIThemeApplier>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var applier in appliers)
        {
            applier.ApplyTheme();
        }
    }
}
