using UnityEngine;

// 🌟 Right click to create data file from the menu
[CreateAssetMenu(fileName = "NewStoneData", menuName = "Three Cuts/Stone Data")]
public class StoneDataSO : ScriptableObject
{
    [Header("UI Visuals")]
    public Sprite stoneIcon; // The stone image will be directly here

    [Header("Stone Core Data (Blueprint)")]
    public StoneBlueprint blueprint; // Your entire data structure will be here
}