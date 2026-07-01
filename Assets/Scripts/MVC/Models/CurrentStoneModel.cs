using UnityEngine;
using System;
using StoneCutter.Sdk; // Added the SDK

[CreateAssetMenu(fileName = "NewStoneModel", menuName = "Game Models/Current Stone Model")]
public class CurrentStoneModel : ScriptableObject
{
    [Header("Active SDK Stone")]
    public StoneData activeSdkStone; // SDK's class

    [Header("Parsed Game Blueprint")]
    public StoneBlueprint parsedBlueprint; // Details of our game

    public Action OnStoneDataUpdated;

    public void UpdateStoneData(StoneData newData)
    {
        activeSdkStone = newData;
        
        // Extracting our game blueprint from the SDK's jsonContext !
        if (!string.IsNullOrEmpty(newData.JsonContext))
        {
            parsedBlueprint = JsonUtility.FromJson<StoneBlueprint>(newData.JsonContext);
        }

        OnStoneDataUpdated?.Invoke();
        Debug.Log("Model Updated with SDK Stone ID: " + activeSdkStone.Id);
    }
}