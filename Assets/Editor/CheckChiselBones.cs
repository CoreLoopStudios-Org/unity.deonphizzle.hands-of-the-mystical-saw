using UnityEngine;
using UnityEditor;

public class CheckChiselBones
{
    [InitializeOnLoadMethod]
    public static void RunCheck()
    {
        EditorApplication.delayCall += () => {
            GameObject chiselGo = GameObject.Find("Chissel_classic_rigged-");
            if (chiselGo == null)
            {
                Debug.LogWarning("CheckChiselBones: Could not find 'Chissel_classic_rigged-' in the scene.");
                return;
            }

            Debug.Log("=== DETAILED BONE HIERARCHY AND SCALES FOR CLASSIC CHISEL ===");
            LogTransform(chiselGo.transform, "");
        };
    }

    private static void LogTransform(Transform t, string indent)
    {
        Debug.Log($"{indent}- {t.name} | Scale: {t.localScale} | Pos: {t.localPosition} | Rot: {t.localRotation.eulerAngles}");
        foreach (Transform child in t)
        {
            LogTransform(child, indent + "  ");
        }
    }
}
