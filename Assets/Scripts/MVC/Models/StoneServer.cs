using System.Collections.Generic;
using UnityEngine;

// 🌟 Enum to define modes
public enum GameMode { Modern, Classic }

public class StoneServer : MonoBehaviour
{
    // Singleton pattern: to send data from any scene
    public static StoneServer Instance;

    // List of live generated stones
    public List<StoneBlueprint> liveStonesList = new List<StoneBlueprint>();

    // 🌟 NEW: The mode the player selected will be saved on the server
    public GameMode ChosenMode
    {
        get
        {
            if (GameModeManager.Instance != null)
            {
                return (GameModeManager.Instance.currentTheme == GameModeManager.GameTheme.Classic) ? GameMode.Classic : GameMode.Modern;
            }
            // Fallback to PlayerPrefs (SavedGameTheme: 0 for Classic, 1 for Modern)
            return (PlayerPrefs.GetInt("SavedGameTheme", 1) == 0) ? GameMode.Classic : GameMode.Modern;
        }
        set
        {
            GameMode current = (GameModeManager.Instance != null)
                ? ((GameModeManager.Instance.currentTheme == GameModeManager.GameTheme.Classic) ? GameMode.Classic : GameMode.Modern)
                : ((PlayerPrefs.GetInt("SavedGameTheme", 1) == 0) ? GameMode.Classic : GameMode.Modern);

            if (current == value) return;

            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.SetTheme((value == GameMode.Classic) ? GameModeManager.GameTheme.Classic : GameModeManager.GameTheme.Modern);
            }
            else
            {
                PlayerPrefs.SetInt("SavedGameTheme", (value == GameMode.Classic) ? 0 : 1);
                PlayerPrefs.Save();
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Going from one scene to another will not delete the data
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // If stones are generated from Predictor Mode, call this function and send data
    public void AddNewGeneratedStone(StoneBlueprint newStone)
    {
        liveStonesList.Add(newStone);
        Debug.Log($"🟢 New Stone Added to Server! Total Live Stones: {liveStonesList.Count}");
    }
}