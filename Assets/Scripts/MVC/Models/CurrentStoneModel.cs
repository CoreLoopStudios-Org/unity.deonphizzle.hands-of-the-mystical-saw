using UnityEngine;
using System;
using StoneCutter.Sdk; // SDK যুক্ত করা হলো

[CreateAssetMenu(fileName = "NewStoneModel", menuName = "Game Models/Current Stone Model")]
public class CurrentStoneModel : ScriptableObject
{
    [Header("Active SDK Stone")]
    public StoneData activeSdkStone; // SDK-এর ক্লাস

    [Header("Parsed Game Blueprint")]
    public StoneBlueprint parsedBlueprint; // আমাদের গেমের ডিটেইলস

    public Action OnStoneDataUpdated;

    public void UpdateStoneData(StoneData newData)
    {
        activeSdkStone = newData;
        
        // SDK-এর jsonContext থেকে আমাদের গেমের ব্লুপ্রিন্ট বের করে আনছি!
        if (!string.IsNullOrEmpty(newData.JsonContext))
        {
            parsedBlueprint = JsonUtility.FromJson<StoneBlueprint>(newData.JsonContext);
        }

        OnStoneDataUpdated?.Invoke();
        Debug.Log("Model Updated with SDK Stone ID: " + activeSdkStone.Id);
    }
}