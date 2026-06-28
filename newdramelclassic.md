# Implementation Plan: Classic Scene Rigged Dremel Setup

This document outlines the step-by-step plan to integrate the modern joint-based Dremel mechanism (`DremelToolController`) into the classic rigged Dremel model (`Dramel_rigged-Classic`) in [StoneCuttingScene_Classic.unity](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/ALL-SCENE-IS%20HERE/StoneCuttingScene_Classic.unity).

---

## 1. Current State & Analysis
1. **Model**: The classic rigged model `Dramel_rigged-Classic` (instantiated from [Dramel_rigged.fbx](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/DeonPhizzle-/Classic_mode/Dramel/Dramel_rigged.fbx)) is already present and active in the classic scene.
2. **References**: In the `ToolSwitcher` component, `dremelTool` points correctly to `Dramel_rigged-Classic`.
3. **Missing Scripts**:
   - The `Dramel_rigged-Classic` object does **not** have the `DremelToolController` script attached.
   - An unused, old GameObject named `Drameltool` contains a `DremelController` component, but its variables (joystick, bones, tip, etc.) are unassigned and set to `null`.
4. **Joystick/UI**: A `VirtualJoystick` (`fileID: 694492753`) and extension buttons exist in the canvas of `StoneCuttingScene_Classic.unity`.

---

## 2. Proposed Solution
We will create a C# Editor script at [SetupClassicSceneDremel.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Editor/SetupClassicSceneDremel.cs) to automate the setup:

1. **Locate Target GameObject**: Find `Dramel_rigged-Classic` in the scene.
2. **Attach Controller**: Add the [DremelToolController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Dramel/DremelControlle.cs) script component to it.
3. **Bone Identification**: Automatically search and bind the local bone transforms recursively by matching keyword substrings:
   - **`rootBone`**: Match name containing `"root"` (case-insensitive).
   - **`upDownBone`**: Match name containing `"up"` or `"down"`.
   - **`extendBone`**: Match name containing `"extend"`.
   - **`dremelTip`**: Match name containing `"tip"`.
4. **UI Linkage**: Find the existing canvas controls and link them to the controller:
   - `joystick` -> `VirtualJoystick` component (`694492753`).
   - `forwardButton` -> `Forward` UI button (`1096032610`).
   - `backwardButton` -> `Backward` UI button (`2104049697`).
5. **Parameters Assignment**:
   - `stoneLayer` -> Set to Layer 6 (`Stone`).
   - `spinAxis` -> `(0, 0, 1)` (Z-Axis).
   - `manualMoveAxis` -> `(0, 0, 1)` (Z-Axis).
   - `strikeAxis` -> `(0, 0, 1)` (Z-Axis).
   - `isEquipped` -> `true`.
6. **Clean Up**: Remove or deactivate the redundant old `Drameltool` GameObject in the scene.

---

## 3. Automation Script (`SetupClassicSceneDremel.cs`)

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.Events;
using UnityEngine.Events;

public class SetupClassicSceneDremel : EditorWindow
{
    [MenuItem("Tools/Setup Classic Scene Dremel")]
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

        // Find rigged dremel
        GameObject dremelGo = FindGameObjectInScene(classicScene, "Dramel_rigged-Classic");
        if (dremelGo == null)
        {
            Debug.LogError("Could not find 'Dramel_rigged-Classic' in classic scene!");
            return;
        }

        // Remove any existing DremelToolController component from Dramel_rigged-Classic
        DremelToolController oldOnRig = dremelGo.GetComponent<DremelToolController>();
        if (oldOnRig != null)
        {
            DestroyImmediate(oldOnRig);
            Debug.Log("Removed legacy DremelToolController component from Dramel_rigged-Classic to match root manager structure.");
        }

        // Find or create root-level DramelController-modern GameObject
        GameObject managerGo = FindGameObjectInScene(classicScene, "DramelController-modern");
        if (managerGo == null)
        {
            managerGo = new GameObject("DramelController-modern");
            Debug.Log("Created root-level manager GameObject 'DramelController-modern'.");
        }

        // Add/Get DremelToolController on root manager
        DremelToolController dremelCtrl = managerGo.GetComponent<DremelToolController>();
        if (dremelCtrl == null)
        {
            dremelCtrl = managerGo.AddComponent<DremelToolController>();
            Debug.Log("Added DremelToolController component to 'DramelController-modern'.");
        }

        // Find Joystick and Buttons in classic Canvas
        VirtualJoystick virtualJoystick = null;
        Button forwardBtn = null;
        Button backwardBtn = null;

        // 1. Try to get references from SawArmController (which has the correct active references)
        SawArmController sawCtrl = FindFirstObjectByType<SawArmController>();
        if (sawCtrl != null)
        {
            virtualJoystick = sawCtrl.virtualJoystick;
            forwardBtn = sawCtrl.forwardButton;
            backwardBtn = sawCtrl.backwardButton;
            Debug.Log("Obtained Joystick and Buttons from SawArmController.");
        }

        // 2. Fallback to NewHammerController for joystick reference
        if (virtualJoystick == null)
        {
            NewHammerController hammerCtrl = FindFirstObjectByType<NewHammerController>();
            if (hammerCtrl != null)
            {
                virtualJoystick = hammerCtrl.virtualJoystick;
                Debug.Log("Obtained Joystick from NewHammerController.");
            }
        }

        // 3. Fallback to manual Canvas search
        if (virtualJoystick == null || forwardBtn == null || backwardBtn == null)
        {
            GameObject canvasGo = FindGameObjectInScene(classicScene, "Canvas");
            if (canvasGo != null)
            {
                // Find all VirtualJoysticks under Canvas
                VirtualJoystick[] joysticks = canvasGo.GetComponentsInChildren<VirtualJoystick>(true);
                foreach (var j in joysticks)
                {
                    if (j.gameObject.activeInHierarchy && j.name == "JoystickArea")
                    {
                        virtualJoystick = j;
                        break;
                    }
                }
                if (virtualJoystick == null && joysticks.Length > 0) virtualJoystick = joysticks[0];

                // Find buttons under Forward-Backward
                Button[] buttons = canvasGo.GetComponentsInChildren<Button>(true);
                foreach (var btn in buttons)
                {
                    if (btn.name == "Forward" && btn.transform.parent.name.StartsWith("Forward-Backward"))
                    {
                        if (btn.transform.parent.gameObject.activeInHierarchy || forwardBtn == null)
                        {
                            forwardBtn = btn;
                        }
                    }
                    if (btn.name == "Backward" && btn.transform.parent.name.StartsWith("Forward-Backward"))
                    {
                        if (btn.transform.parent.gameObject.activeInHierarchy || backwardBtn == null)
                        {
                            backwardBtn = btn;
                        }
                    }
                }
            }
        }

        if (virtualJoystick == null || forwardBtn == null || backwardBtn == null)
        {
            Debug.LogError("Failed to locate virtual joystick or forward/backward buttons in the scene!");
            return;
        }

        // Find Bones recursively
        dremelCtrl.rootBone = FindChildRecursive(dremelGo.transform, "root");
        dremelCtrl.upDownBone = FindChildRecursive(dremelGo.transform, "updown");
        if (dremelCtrl.upDownBone == null) dremelCtrl.upDownBone = FindChildRecursive(dremelGo.transform, "up_down");
        dremelCtrl.extendBone = FindChildRecursive(dremelGo.transform, "extend");
        dremelCtrl.dremelTip = FindChildRecursive(dremelGo.transform, "tip");

        // Verify bones
        if (dremelCtrl.rootBone == null || dremelCtrl.upDownBone == null || dremelCtrl.extendBone == null || dremelCtrl.dremelTip == null)
        {
            Debug.LogWarning("Some bones were not automatically identified! Please assign them manually if needed.");
            Debug.Log($"Root: {dremelCtrl.rootBone}, UpDown: {dremelCtrl.upDownBone}, Extend: {dremelCtrl.extendBone}, Tip: {dremelCtrl.dremelTip}");
        }

        // Assign script properties
        dremelCtrl.joystick = virtualJoystick;
        dremelCtrl.forwardButton = forwardBtn;
        dremelCtrl.backwardButton = backwardBtn;
        dremelCtrl.toolRoot = dremelGo;
        dremelCtrl.isEquipped = true;
        dremelCtrl.stoneLayer = 1 << 6; // Layer 6 = Stone
        
        dremelCtrl.spinAxis = new Vector3(0, 0, 1);
        dremelCtrl.manualMoveAxis = new Vector3(0, 0, 1);
        dremelCtrl.strikeAxis = new Vector3(0, 0, 1);
        dremelCtrl.autoStrikeDistance = 5f;
        dremelCtrl.rotationSpeed = 3000f;
        dremelCtrl.manualMoveSpeed = 2f;
        dremelCtrl.collisionOffset = 0.05f;
        dremelCtrl.maxForwardDistance = 5f;
        dremelCtrl.maxBackwardDistance = 2f;
        dremelCtrl.headAimSpeed = 20f;
        dremelCtrl.minTiltUp = -90f;
        dremelCtrl.maxTiltUp = -30f;
        dremelCtrl.minTiltSide = -90f;
        dremelCtrl.maxTiltSide = 90f;
        dremelCtrl.grindInterval = 0.1f;
        dremelCtrl.approachSpeed = 25f;
        dremelCtrl.returnSpeed = 10f;

        // Load Prefabs by GUID
        string dentPath = AssetDatabase.GUIDToAssetPath("ebfc857ccbd0fcd4b9a7086f7a46ea6f");
        if (!string.IsNullOrEmpty(dentPath))
        {
            dremelCtrl.dentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dentPath);
            Debug.Log("Successfully assigned dentPrefab from GUID.");
        }
        else
        {
            Debug.LogWarning("Could not resolve dentPrefab GUID ebfc857ccbd0fcd4b9a7086f7a46ea6f");
        }

        string sparksPath = AssetDatabase.GUIDToAssetPath("9b21333b068a5084ba09535839bee3c8");
        if (!string.IsNullOrEmpty(sparksPath))
        {
            dremelCtrl.hitEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sparksPath);
            Debug.Log("Successfully assigned hitEffectPrefab from GUID.");
        }
        else
        {
            Debug.LogWarning("Could not resolve hitEffectPrefab GUID 9b21333b068a5084ba09535839bee3c8");
        }

        // Deactivate old unused Drameltool GameObject
        GameObject oldDremelGo = FindGameObjectInScene(classicScene, "Drameltool");
        if (oldDremelGo != null)
        {
            oldDremelGo.SetActive(false);
            oldDremelGo.name = "Drameltool (OLD)";
            Debug.Log("Deactivated redundant old 'Drameltool' GameObject.");
            EditorUtility.SetDirty(oldDremelGo);
        }

        // Configure ToolSwitcher dremelTool reference
        GameObject toolSwitcherGo = FindGameObjectInScene(classicScene, "GameManager-toolswitcher");
        if (toolSwitcherGo != null)
        {
            ToolSwitcher switcher = toolSwitcherGo.GetComponent<ToolSwitcher>();
            if (switcher != null)
            {
                switcher.dremelTool = dremelGo; // references the model instance Dramel_rigged-Classic
                EditorUtility.SetDirty(switcher);
                Debug.Log("Updated ToolSwitcher dremelTool reference to 'Dramel_rigged-Classic'.");
            }
        }

        // Set up persistent onClick button listeners
        SetupButtonListeners(classicScene, dremelCtrl);

        // Save scene modifications
        EditorUtility.SetDirty(managerGo);
        EditorUtility.SetDirty(dremelCtrl);
        EditorSceneManager.MarkSceneDirty(classicScene);
        EditorSceneManager.SaveScene(classicScene);

        Debug.Log("🎉 Successfully set up root manager 'DramelController-modern' mechanism in StoneCuttingScene_Classic!");
        EditorUtility.DisplayDialog("Success", "DramelController-modern mechanism successfully configured in StoneCuttingScene_Classic!", "OK");
    }

    private static void SetupButtonListeners(Scene scene, DremelToolController dremelCtrl)
    {
        GameObject dramelBtnGo = FindGameObjectInScene(scene, "DramelButton");
        GameObject sawBtnGo = FindGameObjectInScene(scene, "SawButton");
        GameObject chiselBtnGo = FindGameObjectInScene(scene, "ChiselButton");
        GameObject hammerBtnGo = FindGameObjectInScene(scene, "HammerButton");

        if (dramelBtnGo != null) SetupButtonOnClick(dramelBtnGo.GetComponent<Button>(), dremelCtrl, true);
        if (sawBtnGo != null) SetupButtonOnClick(sawBtnGo.GetComponent<Button>(), dremelCtrl, false);
        if (chiselBtnGo != null) SetupButtonOnClick(chiselBtnGo.GetComponent<Button>(), dremelCtrl, false);
        if (hammerBtnGo != null) SetupButtonOnClick(hammerBtnGo.GetComponent<Button>(), dremelCtrl, false);
    }

    private static void SetupButtonOnClick(Button button, DremelToolController dremelCtrl, bool isEquip)
    {
        if (button == null) return;

        // Remove any old references to DremelToolController in onClick list
        int count = button.onClick.GetPersistentEventCount();
        for (int i = count - 1; i >= 0; i--)
        {
            Object target = button.onClick.GetPersistentTarget(i);
            string method = button.onClick.GetPersistentMethodName(i);
            if (target != null && (target is DremelToolController || target.name.Contains("Dramel") || target.name.Contains("Dremel") || method == "EquipDremel" || method == "UnequipDremel"))
            {
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            }
        }

        // Add the correct Equip/Unequip listener
        UnityAction action = isEquip ? new UnityAction(dremelCtrl.EquipDremel) : new UnityAction(dremelCtrl.UnequipDremel);
        UnityEventTools.AddVoidPersistentListener(button.onClick, action);
        
        EditorUtility.SetDirty(button);
        Debug.Log($"Configured button '{button.name}' persistent click to call {(isEquip ? "EquipDremel" : "UnequipDremel")}.");
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
```

---

## 4. How to Execute
1. Let the AI save this helper script to [SetupClassicSceneDremel.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Editor/SetupClassicSceneDremel.cs).
2. Open the Unity project.
3. Once compiled, click on **`Tools > Setup Classic Scene Dremel`** in the top menu bar.
4. Run the classic scene to verify the Dremel aiming and extension movements.
