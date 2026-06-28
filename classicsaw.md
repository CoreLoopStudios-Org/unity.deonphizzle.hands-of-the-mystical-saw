# Implementation Plan: Classic Scene Rigged Saw Setup

This document outlines the step-by-step plan to integrate the modern joint-based rigged saw (`Saw_rigged.prefab`) into the classic scene (`StoneCuttingScene_Classic`).

---

## 1. Objectives
- Replace the static model `Saw_rigged -Classic` in [StoneCuttingScene_Classic.unity](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/ALL-SCENE-IS%20HERE/StoneCuttingScene_Classic.unity) with the active, joint-controlled `Saw_rigged` prefab instance.
- Import the **`JoystickArea`** and **`Forward-Backward`** UI elements from the modern canvas in [StoneGenerator Scene.unity](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/ALL-SCENE-IS%20HERE/StoneGenerator%20Scene.unity) to allow joysticks and extension/retraction buttons to drive the joints.
- Wire up the joint controllers for all three interactive tools in the classic scene:
  - **Saw** (`SawArmController`)
  - **Hammer** (`NewHammerController`)
  - **Dremel** (`DremelToolController`)
- Update the `ToolSwitcher` component to point to the modern saw.

---

## 2. Automation Script
We will create a C# Editor script at [SetupClassicSceneSaw.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Editor/SetupClassicSceneSaw.cs) to handle this migration programmatically. This ensures GUID references, file linkages, and UI assignments are bound correctly.

### **Setup Code**
```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SetupClassicSceneSaw : EditorWindow
{
    [MenuItem("Tools/Setup Classic Scene Saw")]
    public static void RunSetup()
    {
        string classicScenePath = "Assets/ALL-SCENE-IS HERE/StoneCuttingScene_Classic.unity";
        string modernScenePath = "Assets/ALL-SCENE-IS HERE/StoneGenerator Scene.unity";

        // Save current changes first
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Load classic scene
        Scene classicScene = EditorSceneManager.OpenScene(classicScenePath, OpenSceneMode.Single);
        if (!classicScene.IsValid())
        {
            Debug.LogError("Failed to open classic scene: " + classicScenePath);
            return;
        }

        // Find existing classic saw
        GameObject classicSawGo = GameObject.Find("Saw_rigged -Classic");
        if (classicSawGo == null)
        {
            Debug.LogWarning("Could not find 'Saw_rigged -Classic' in classic scene.");
        }

        // Find the modern saw (disabled) already in the classic scene
        GameObject modernSawGo = null;
        foreach (GameObject go in classicScene.GetRootGameObjects())
        {
            if (go.name == "Saw_rigged")
            {
                modernSawGo = go;
                break;
            }
        }

        if (modernSawGo == null)
        {
            Debug.LogError("Could not find disabled 'Saw_rigged' (modern prefab instance) in classic scene root order!");
            return;
        }

        // Load modern scene additively to copy UI components
        Scene modernScene = EditorSceneManager.OpenScene(modernScenePath, OpenSceneMode.Additive);
        if (!modernScene.IsValid())
        {
            Debug.LogError("Failed to open modern scene additively: " + modernScenePath);
            return;
        }

        // Find the Canvas in both scenes
        GameObject classicCanvas = null;
        foreach (GameObject go in classicScene.GetRootGameObjects())
        {
            if (go.name == "Canvas")
            {
                classicCanvas = go;
                break;
            }
        }

        GameObject modernCanvas = null;
        foreach (GameObject go in modernScene.GetRootGameObjects())
        {
            if (go.name == "Canvas")
            {
                modernCanvas = go;
                break;
            }
        }

        if (classicCanvas == null || modernCanvas == null)
        {
            Debug.LogError("Failed to find Canvas in one of the scenes.");
            EditorSceneManager.CloseScene(modernScene, true);
            return;
        }

        // Find JoystickArea and Forward-Backward in modern canvas
        Transform modernJoystick = modernCanvas.transform.Find("JoystickArea");
        Transform modernFB = modernCanvas.transform.Find("Forward-Backward "); // Note the space in the name

        if (modernJoystick == null || modernFB == null)
        {
            Debug.LogError("Failed to find JoystickArea or Forward-Backward panel in modern canvas.");
            EditorSceneManager.CloseScene(modernScene, true);
            return;
        }

        // Copy JoystickArea and Forward-Backward to classic canvas
        GameObject copiedJoystick = Instantiate(modernJoystick.gameObject, classicCanvas.transform);
        copiedJoystick.name = "JoystickArea";

        GameObject copiedFB = Instantiate(modernFB.gameObject, classicCanvas.transform);
        copiedFB.name = "Forward-Backward ";

        // Close the modern scene (do not save)
        EditorSceneManager.CloseScene(modernScene, true);

        // Position of the modern saw in classic scene
        if (classicSawGo != null)
        {
            modernSawGo.transform.position = classicSawGo.transform.position;
            modernSawGo.transform.rotation = classicSawGo.transform.rotation;
            modernSawGo.transform.localScale = classicSawGo.transform.localScale;

            // Deactivate classic saw
            classicSawGo.SetActive(false);
            classicSawGo.name = "Saw_rigged -Classic (OLD)";
        }

        // Activate modern saw
        modernSawGo.SetActive(true);

        // Configure SawArmController on the modern saw
        SawArmController sawArmCtrl = modernSawGo.GetComponent<SawArmController>();
        if (sawArmCtrl == null)
        {
            Debug.LogError("SawArmController component not found on 'Saw_rigged'!");
            return;
        }

        // Find the references in the copied UI
        VirtualJoystick virtualJoystick = copiedJoystick.GetComponent<VirtualJoystick>();
        
        Button forwardBtn = null;
        Transform fwdTrans = copiedFB.transform.Find("Forward");
        if (fwdTrans != null) forwardBtn = fwdTrans.GetComponent<Button>();

        Button backwardBtn = null;
        Transform bwdTrans = copiedFB.transform.Find("Backward");
        if (bwdTrans != null) backwardBtn = bwdTrans.GetComponent<Button>();

        if (virtualJoystick == null || forwardBtn == null || backwardBtn == null)
        {
            Debug.LogError("Failed to find VirtualJoystick or Buttons in copied UI!");
            return;
        }

        // Assign the references
        sawArmCtrl.virtualJoystick = virtualJoystick;
        sawArmCtrl.forwardButton = forwardBtn;
        sawArmCtrl.backwardButton = backwardBtn;
        sawArmCtrl.isEquipped = true;

        // Connect the Virtual Joystick to the Hammer Controller in the classic scene
        GameObject hammerTool = GameObject.Find("hammerTool");
        if (hammerTool == null) hammerTool = GameObject.Find("HammerTool");
        if (hammerTool == null)
        {
            GameObject switcherGo = GameObject.Find("GameManager-toolswitcher");
            if (switcherGo != null)
            {
                ToolSwitcher switcher = switcherGo.GetComponent<ToolSwitcher>();
                if (switcher != null && switcher.hammerTool != null)
                {
                    hammerTool = switcher.hammerTool;
                }
            }
        }

        if (hammerTool != null)
        {
            NewHammerController hammerCtrl = hammerTool.GetComponentInChildren<NewHammerController>(true);
            if (hammerCtrl != null)
            {
                hammerCtrl.virtualJoystick = virtualJoystick;
                EditorUtility.SetDirty(hammerCtrl);
                Debug.Log("Connected Virtual Joystick to NewHammerController.");
            }
        }

        // Connect Dremel Tool
        GameObject switcherGoForDremel = GameObject.Find("GameManager-toolswitcher");
        if (switcherGoForDremel != null)
        {
            ToolSwitcher switcher = switcherGoForDremel.GetComponent<ToolSwitcher>();
            if (switcher != null && switcher.dremelTool != null)
            {
                DremelToolController dremelCtrl = switcher.dremelTool.GetComponentInChildren<DremelToolController>(true);
                if (dremelCtrl != null)
                {
                    dremelCtrl.joystick = virtualJoystick;
                    dremelCtrl.forwardButton = forwardBtn;
                    dremelCtrl.backwardButton = backwardBtn;
                    EditorUtility.SetDirty(dremelCtrl);
                    Debug.Log("Connected Virtual Joystick and Buttons to DremelToolController.");
                }
            }
        }

        // Update the ToolSwitcher sawTool reference
        GameObject toolSwitcherGo = GameObject.Find("GameManager-toolswitcher");
        if (toolSwitcherGo != null)
        {
            ToolSwitcher switcher = toolSwitcherGo.GetComponent<ToolSwitcher>();
            if (switcher != null)
            {
                switcher.sawTool = modernSawGo;
                EditorUtility.SetDirty(switcher);
                Debug.Log("Updated ToolSwitcher sawTool reference to modern 'Saw_rigged'.");
            }
        }

        // Save Scene
        EditorUtility.SetDirty(modernSawGo);
        EditorUtility.SetDirty(sawArmCtrl);
        EditorSceneManager.MarkSceneDirty(classicScene);
        EditorSceneManager.SaveScene(classicScene);

        Debug.Log("Successfully set up Joint-Based Robotic Saw (Saw_rigged) in StoneCuttingScene_Classic!");
        EditorUtility.DisplayDialog("Success", "Joint-Based Robotic Saw successfully configured in StoneCuttingScene_Classic! Please run the game and test it.", "OK");
    }
}
```

---

## 3. How to Execute
1. Let the AI save this helper script to [SetupClassicSceneSaw.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Editor/SetupClassicSceneSaw.cs).
2. Open the Unity project.
3. Once compiled, click on **`Tools > Setup Classic Scene Saw`** in the top menu bar.
4. Run and play the classic scene to verify the functionality of the rigged saw, hammer, and dremel tools with the joystick UI!
