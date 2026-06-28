using UnityEngine;

// 🌟 রাইট ক্লিক করে মেনু থেকে ডাটা ফাইল বানানোর জন্য
[CreateAssetMenu(fileName = "NewStoneData", menuName = "Three Cuts/Stone Data")]
public class StoneDataSO : ScriptableObject
{
    [Header("UI Visuals")]
    public Sprite stoneIcon; // পাথরের ছবিটা সরাসরি এখানেই থাকবে

    [Header("Stone Core Data (Blueprint)")]
    public StoneBlueprint blueprint; // তোমার ওই পুরো ডাটা স্ট্রাকচারটা এখানে থাকবে
}