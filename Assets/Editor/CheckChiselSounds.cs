#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CheckChiselSounds : MonoBehaviour
{
    [MenuItem("Tools/Analyze Active Chisel Sounds")]
    public static void AnalyzeSounds()
    {
        string[] scenes = new string[]
        {
            "Assets/ALL-SCENE-IS HERE/StoneGenerator Scene.unity",
            "Assets/ALL-SCENE-IS HERE/StoneCuttingScene_Classic.unity"
        };

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== CHISEL SOUNDS ANALYSIS ===");

        foreach (string scenePath in scenes)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                sb.AppendLine($"\n[-] Scene not found: {scenePath}");
                continue;
            }

            sb.AppendLine($"\n[Scene] {System.IO.Path.GetFileName(scenePath)}");
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Find all GameObjects in scene
            GameObject[] rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            
            // Search for modern chisel in StoneGenerator Scene
            if (scenePath.Contains("StoneGenerator"))
            {
                GameObject modernChisel = FindObjectByName(rootObjects, "Chisel_rigged -modern");
                if (modernChisel == null) modernChisel = FindObjectByName(rootObjects, "Chisel_rigged-modern");
                
                if (modernChisel != null)
                {
                    sb.AppendLine($"  Found Modern Chisel: '{modernChisel.name}'");
                    var manual = modernChisel.GetComponentInChildren<ManualChiselController>(true);
                    if (manual != null)
                    {
                        sb.AppendLine($"    Component: ManualChiselController");
                        sb.AppendLine($"    primaryHitSound: {(manual.primaryHitSound != null ? manual.primaryHitSound.name : "None")} (GUID: {GetAssetGuid(manual.primaryHitSound)})");
                        sb.AppendLine($"    secondaryHitSound: {(manual.secondaryHitSound != null ? manual.secondaryHitSound.name : "None")} (GUID: {GetAssetGuid(manual.secondaryHitSound)})");
                        sb.AppendLine($"    hitSoundVolume: {manual.hitSoundVolume}");
                    }
                    else
                    {
                        sb.AppendLine("    [Warning] No ManualChiselController component found on modern chisel!");
                    }
                }
                else
                {
                    sb.AppendLine("    [-] Modern Chisel 'Chisel_rigged -modern' NOT found in scene.");
                }
            }

            // Search for classic chisel in StoneCuttingScene_Classic
            if (scenePath.Contains("Classic"))
            {
                GameObject classicChisel = FindObjectByName(rootObjects, "Chissel_classic_rigged-");
                if (classicChisel == null) classicChisel = FindObjectByName(rootObjects, "Chissel_classic_rigged");
                
                if (classicChisel != null)
                {
                    sb.AppendLine($"  Found Classic Chisel: '{classicChisel.name}'");
                    var classic = classicChisel.GetComponentInChildren<ClassicChiselController>(true);
                    if (classic != null)
                    {
                        sb.AppendLine($"    Component: ClassicChiselController");
                        sb.AppendLine($"    primaryHitSound: {(classic.primaryHitSound != null ? classic.primaryHitSound.name : "None")} (GUID: {GetAssetGuid(classic.primaryHitSound)})");
                        sb.AppendLine($"    secondaryHitSound: {(classic.secondaryHitSound != null ? classic.secondaryHitSound.name : "None")} (GUID: {GetAssetGuid(classic.secondaryHitSound)})");
                        sb.AppendLine($"    hitSoundVolume: {classic.hitSoundVolume}");
                    }
                    else
                    {
                        var general = classicChisel.GetComponentInChildren<ChiselController>(true);
                        if (general != null)
                        {
                            sb.AppendLine($"    Component: ChiselController");
                            sb.AppendLine($"    primaryHitSound: {(general.primaryHitSound != null ? general.primaryHitSound.name : "None")} (GUID: {GetAssetGuid(general.primaryHitSound)})");
                        }
                        else
                        {
                            sb.AppendLine("    [Warning] No ClassicChiselController or ChiselController component found on classic chisel!");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("    [-] Classic Chisel 'Chissel_classic_rigged-' NOT found in scene.");
                }
            }
        }

        // Restore scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog("Chisel Sound Status Report", sb.ToString(), "OK");
    }

    private static GameObject FindObjectByName(GameObject[] roots, string name)
    {
        foreach (var root in roots)
        {
            if (root.name == name) return root;
            var child = FindDeepChild(root.transform, name);
            if (child != null) return child.gameObject;
        }
        return null;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private static string GetAssetGuid(Object obj)
    {
        if (obj == null) return "N/A";
        string path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path)) return "No Path";
        return AssetDatabase.AssetPathToGUID(path);
    }
}
#endif
