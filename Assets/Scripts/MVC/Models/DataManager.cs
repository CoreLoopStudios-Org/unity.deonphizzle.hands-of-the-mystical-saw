using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    // 🌟 This is the magical line that was giving the error because it was missing!
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
        // Fetch data from memory
        totalPoints = PlayerPrefs.GetInt("PlayerTotalPoints", totalPoints);
    }

    // 🌟 Function to add points
    public void AddPoints(int amount)
    {
        totalPoints += amount;
        SaveAndNotify();
    }

    // 🌟 Function to spend points
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

    // 🌟 Save data and signal to everyone
    private void SaveAndNotify()
    {
        PlayerPrefs.SetInt("PlayerTotalPoints", totalPoints);
        PlayerPrefs.Save();
        
        // Fire the signal (so that GlobalPointsUI is updated)
        OnPointsUpdated?.Invoke(); 
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            // Changing to Inspector will update live
            PlayerPrefs.SetInt("PlayerTotalPoints", totalPoints);
            OnPointsUpdated?.Invoke();
        }
    }
#endif
}