using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SetupClassicSceneHammer : EditorWindow
{
    [MenuItem("Tools/Setup Classic Scene Hammer")]
    public static void RunSetup()
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

        // 1. Find Classic Rigged Hammer GameObject
        GameObject hammerClassicGo = FindGameObjectInScene(classicScene, "Hammer_rigged-Classic");
        if (hammerClassicGo == null)
        {
            Debug.LogError("Could not find 'Hammer_rigged-Classic' in classic scene!");
            return;
        }

        // 2. Add or Get NewHammerController component
        NewHammerController hammerCtrl = hammerClassicGo.GetComponent<NewHammerController>();
        if (hammerCtrl == null)
        {
            hammerCtrl = hammerClassicGo.AddComponent<NewHammerController>();
            Debug.Log("Added NewHammerController component to 'Hammer_rigged-Classic'.");
        }

        // 3. Find Joystick in Canvas
        VirtualJoystick virtualJoystick = null;
        GameObject canvasGo = FindGameObjectInScene(classicScene, "Canvas");
        if (canvasGo != null)
        {
            VirtualJoystick[] joysticks = canvasGo.GetComponentsInChildren<VirtualJoystick>(true);
            foreach (var j in joysticks)
            {
                if (j.name == "JoystickArea" || j.name == "JoystickArea-Classic" || j.name.Contains("Joystick"))
                {
                    virtualJoystick = j;
                    break;
                }
            }
            if (virtualJoystick == null && joysticks.Length > 0)
            {
                virtualJoystick = joysticks[0];
            }
        }

        if (virtualJoystick == null)
        {
            Debug.LogError("Failed to find VirtualJoystick (JoystickArea) in Canvas!");
            return;
        }

        // 4. Find Bones recursively
        hammerCtrl.rootBone = FindChildRecursive(hammerClassicGo.transform, "Root");
        hammerCtrl.topBone = FindChildRecursive(hammerClassicGo.transform, "Up_down_bottom");
        hammerCtrl.extendBone = FindChildRecursive(hammerClassicGo.transform, "Entend");
        hammerCtrl.hammerTip = FindChildRecursive(hammerClassicGo.transform, "Entend"); // tip is extend bone in legacy prefab

        // Fallback checks for bones
        if (hammerCtrl.rootBone == null) hammerCtrl.rootBone = FindChildRecursive(hammerClassicGo.transform, "root");
        if (hammerCtrl.topBone == null) hammerCtrl.topBone = FindChildRecursive(hammerClassicGo.transform, "up_down");
        if (hammerCtrl.extendBone == null) hammerCtrl.extendBone = FindChildRecursive(hammerClassicGo.transform, "extend");
        if (hammerCtrl.hammerTip == null) hammerCtrl.hammerTip = hammerCtrl.extendBone;

        if (hammerCtrl.rootBone == null || hammerCtrl.topBone == null || hammerCtrl.extendBone == null || hammerCtrl.hammerTip == null)
        {
            Debug.LogError($"Bones hierarchy not fully matched! Root: {hammerCtrl.rootBone}, Top: {hammerCtrl.topBone}, Extend: {hammerCtrl.extendBone}, Tip: {hammerCtrl.hammerTip}");
            return;
        }

        // 5. Configure NewHammerController properties (Correct Single-Axis Tilt Mechanics)
        hammerCtrl.virtualJoystick = virtualJoystick;
        hammerCtrl.hitUIButton = null; // direct click interaction
        hammerCtrl.isEquipped = true;

        hammerCtrl.rootRotationAxis = new Vector3(1, 0, 0);
        hammerCtrl.rootTurnSpeed = 60f;
        hammerCtrl.minRootAngle = -360f;
        hammerCtrl.maxRootAngle = 360f;

        hammerCtrl.tiltRotationAxis = new Vector3(0, 0, 1); // Z-axis for clean tilt
        hammerCtrl.tiltSpeed = 60f;
        hammerCtrl.minTiltZ = -180f;
        hammerCtrl.maxTiltZ = -20f;
        hammerCtrl.invertJoystickX = true;
        hammerCtrl.invertJoystickY = false;

        hammerCtrl.pullbackAngleZ = -180f;
        hammerCtrl.strikeAngleZ = -20f;
        hammerCtrl.stopMargin = 0.5f;
        hammerCtrl.swingSpeed = 25f;
        hammerCtrl.returnSpeed = 10f;

        hammerCtrl.stoneLayerMask = 1 << 6; // Layer 6: Stone
        hammerCtrl.hitSoundVolume = 1f;

        // Load FX and Sound assets by GUID
        string sparksPath = AssetDatabase.GUIDToAssetPath("9b21333b068a5084ba09535839bee3c8");
        if (!string.IsNullOrEmpty(sparksPath))
        {
            hammerCtrl.hitEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sparksPath);
            Debug.Log("Successfully assigned hitEffectPrefab.");
        }

        string hitSoundPath = AssetDatabase.GUIDToAssetPath("d4c9aeedc366235468899e7a7c95a36f");
        if (!string.IsNullOrEmpty(hitSoundPath))
        {
            hammerCtrl.primaryHitSound = AssetDatabase.LoadAssetAtPath<AudioClip>(hitSoundPath);
            Debug.Log("Successfully assigned primaryHitSound.");
        }

        // 6. Deactivate modern hammer in classic scene to prevent duplication
        GameObject modernHammerGo = FindGameObjectInScene(classicScene, "NewHammer3dModel");
        if (modernHammerGo != null)
        {
            modernHammerGo.SetActive(false);
            modernHammerGo.name = "NewHammer3dModel (OLD)";
            EditorUtility.SetDirty(modernHammerGo);
            Debug.Log("Deactivated redundant 'NewHammer3dModel' in classic scene.");
        }

        // 7. Activate Classic Rigged Hammer
        hammerClassicGo.SetActive(true);

        // 8. Update ToolSwitcher hammerTool reference to Hammer_rigged-Classic
        GameObject toolSwitcherGo = FindGameObjectInScene(classicScene, "GameManager-toolswitcher");
        if (toolSwitcherGo != null)
        {
            ToolSwitcher switcher = toolSwitcherGo.GetComponent<ToolSwitcher>();
            if (switcher != null)
            {
                switcher.hammerTool = hammerClassicGo;
                EditorUtility.SetDirty(switcher);
                Debug.Log("Updated ToolSwitcher hammerTool reference to 'Hammer_rigged-Classic'.");
            }
        }

        // Save Scene and Editor Dirty State
        EditorUtility.SetDirty(hammerClassicGo);
        EditorUtility.SetDirty(hammerCtrl);
        EditorSceneManager.MarkSceneDirty(classicScene);
        EditorSceneManager.SaveScene(classicScene);

        Debug.Log("🎉 Successfully set up Classic Rigged Hammer (Hammer_rigged-Classic) in StoneCuttingScene_Classic!");
        EditorUtility.DisplayDialog("Success", "Classic Rigged Hammer successfully configured in StoneCuttingScene_Classic!", "OK");
    }

    private static GameObject FindGameObjectInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform match = FindChildExact(root.transform, name);
            if (match != null) return match.gameObject;
        }
        return null;
    }

    private static Transform FindChildExact(Transform parent, string exactName)
    {
        if (parent.name == exactName) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildExact(child, exactName);
            if (result != null) return result;
        }
        return null;
    }

    private static Transform FindChildRecursive(Transform parent, string keyword)
    {
        if (parent.name.ToLower().Contains(keyword.ToLower())) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, keyword);
            if (result != null) return result;
        }
        return null;
    }
}
