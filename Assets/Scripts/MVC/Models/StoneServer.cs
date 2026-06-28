using System.Collections.Generic;
using UnityEngine;

// 🌟 মোডগুলোকে ডিফাইন করার জন্য এনাম (Enum)
public enum GameMode { Modern, Classic }

public class StoneServer : MonoBehaviour
{
    // Singleton প্যাটার্ন: যাতে যেকোনো সিন থেকে ডাটা পাঠানো যায়
    public static StoneServer Instance;

    // লাইভ জেনারেট হওয়া পাথরের লিস্ট
    public List<StoneBlueprint> liveStonesList = new List<StoneBlueprint>();

    // 🌟 নতুন: প্লেয়ার কোন মোড সিলেক্ট করেছে তা সার্ভারে সেভ থাকবে
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
            DontDestroyOnLoad(gameObject); // এক সিন থেকে অন্য সিনে গেলেও ডাটা মুছবে না
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Predictor Mode থেকে পাথর জেনারেট হলে এই ফাংশন কল করে ডাটা পাঠাতে হবে
    public void AddNewGeneratedStone(StoneBlueprint newStone)
    {
        liveStonesList.Add(newStone);
        Debug.Log($"🟢 New Stone Added to Server! Total Live Stones: {liveStonesList.Count}");
    }
}