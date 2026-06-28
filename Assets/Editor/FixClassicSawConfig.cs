using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixClassicSawConfig : EditorWindow
{
    [MenuItem("Tools/Fix Classic Saw Configuration")]
    public static void RunFix()
    {
        string classicScenePath = "Assets/ALL-SCENE-IS HERE/StoneCuttingScene_Classic.unity";

        // Save current changes first
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Load classic scene
        Scene classicScene = EditorSceneManager.OpenScene(classicScenePath, OpenSceneMode.Single);
        if (!classicScene.IsValid())
        {
            Debug.LogError("Failed to open classic scene: " + classicScenePath);
            return;
        }

        // Find the newclassic saw object
        GameObject sawGo = GameObject.Find("Saw_rigged -newclassic-please add this ");
        if (sawGo == null)
        {
            Debug.LogError("Could not find 'Saw_rigged -newclassic-please add this ' in the classic scene!");
            return;
        }

        SawArmController sawArmCtrl = sawGo.GetComponent<SawArmController>();
        if (sawArmCtrl == null)
        {
            Debug.LogError("SawArmController component not found on the newclassic saw!");
            return;
        }

        // 1. Fix Stone Layer Mask (Bit 6 / Layer 6 = Stone)
        sawArmCtrl.stoneLayer = 1 << 6;
        Debug.Log("Set SawArmController stoneLayer to Stone (layer 6).");

        // 2. Find and assign the water particle effect from the scene
        if (sawArmCtrl.waterEffectParticle == null)
        {
            GameObject waterEffectGo = GameObject.Find("Watereffect");
            if (waterEffectGo != null)
            {
                ParticleSystem waterPs = waterEffectGo.GetComponent<ParticleSystem>();
                if (waterPs != null)
                {
                    sawArmCtrl.waterEffectParticle = waterPs;
                    Debug.Log("Assigned 'Watereffect' ParticleSystem to SawArmController.");
                }
            }
        }

        // Save scene
        EditorUtility.SetDirty(sawGo);
        EditorUtility.SetDirty(sawArmCtrl);
        EditorSceneManager.MarkSceneDirty(classicScene);
        EditorSceneManager.SaveScene(classicScene);

        Debug.Log("🎉 Classic Saw configuration fixed successfully!");
        EditorUtility.DisplayDialog("Success", "Classic Saw configuration fixed successfully!", "OK");
    }
}
