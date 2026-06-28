#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CheckDremelSetup : MonoBehaviour
{
    [MenuItem("Tools/Analyze Dremel Setup")]
    public static void AnalyzeDremels()
    {
        string[] scenes = new string[]
        {
            "Assets/ALL-SCENE-IS HERE/StoneGenerator Scene.unity",
            "Assets/ALL-SCENE-IS HERE/StoneCuttingScene_Classic.unity"
        };

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== DREMEL SETUP ANALYSIS ===");

        foreach (string scenePath in scenes)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                sb.AppendLine($"\n[-] Scene not found: {scenePath}");
                continue;
            }

            sb.AppendLine($"\n[Scene] {System.IO.Path.GetFileName(scenePath)}");
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Find all DremelToolController components in the scene
            DremelToolController[] controllers = GameObject.FindObjectsByType<DremelToolController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            sb.AppendLine($"  Found {controllers.Length} DremelToolController(s):");

            foreach (var controller in controllers)
            {
                sb.AppendLine($"  - GameObject: '{controller.gameObject.name}' (Active: {controller.gameObject.activeInHierarchy})");
                sb.AppendLine($"    isEquipped (default): {controller.isEquipped}");
                sb.AppendLine($"    rootBone: {(controller.rootBone != null ? controller.rootBone.name : "None")}");
                sb.AppendLine($"    upDownBone: {(controller.upDownBone != null ? controller.upDownBone.name : "None")}");
                sb.AppendLine($"    extendBone: {(controller.extendBone != null ? controller.extendBone.name : "None")}");
                sb.AppendLine($"    dremelTip: {(controller.dremelTip != null ? controller.dremelTip.name : "None")}");
                sb.AppendLine($"    toolRoot: {(controller.toolRoot != null ? controller.toolRoot.name : "None")}");
                sb.AppendLine($"    joystick: {(controller.joystick != null ? controller.joystick.name : "None")}");
                sb.AppendLine($"    strikeAxis: {controller.strikeAxis}");
                sb.AppendLine($"    autoStrikeDistance: {controller.autoStrikeDistance}");
                sb.AppendLine($"    approachSpeed: {controller.approachSpeed}");
                sb.AppendLine($"    returnSpeed: {controller.returnSpeed}");
                sb.AppendLine($"    primaryHitSound: {(controller.primaryHitSound != null ? controller.primaryHitSound.name : "None")}");
            }
        }

        // Restore scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog("Dremel Setup Report", sb.ToString(), "OK");
    }
}
#endif
