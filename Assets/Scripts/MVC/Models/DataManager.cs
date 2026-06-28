using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    // 🌟 এই সেই জাদুকরী লাইন যেটা মিসিং থাকার কারণে এরর দিচ্ছিল!
    public event Action OnPointsUpdated; 

    [Header("Current User Global Data")]
    public string userName = "You (Maya)";
    public string tier = "Gold";
    
    [Header("Total Universal Points")]
    public int totalPoints = 45000;  

    public int tierProgressPoints = 12897; 
    public int tierMaxPoints = 20000;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPoints(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadPoints()
    {
        // মেমোরি থেকে ডাটা আনবে
        totalPoints = PlayerPrefs.GetInt("PlayerTotalPoints", totalPoints);
    }

    // 🌟 পয়েন্ট যোগ করার ফাংশন
    public void AddPoints(int amount)
    {
        totalPoints += amount;
        SaveAndNotify();
    }

    // 🌟 পয়েন্ট খরচ করার ফাংশন
    public bool SpendPoints(int amount)
    {
        if (totalPoints >= amount) 
        {
            totalPoints -= amount;
            SaveAndNotify();
            return true; 
        }
        return false; 
    }

    // 🌟 ডাটা সেভ এবং সবাইকে সিগন্যাল দেওয়া
    private void SaveAndNotify()
    {
        PlayerPrefs.SetInt("PlayerTotalPoints", totalPoints);
        PlayerPrefs.Save();
        
        // সিগন্যাল ফায়ার করা (যাতে GlobalPointsUI আপডেট হয়ে যায়)
        OnPointsUpdated?.Invoke(); 
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            // Inspector-এ চেঞ্জ করলে লাইভ আপডেট হবে
            PlayerPrefs.SetInt("PlayerTotalPoints", totalPoints);
            OnPointsUpdated?.Invoke();
        }
    }
#endif
}