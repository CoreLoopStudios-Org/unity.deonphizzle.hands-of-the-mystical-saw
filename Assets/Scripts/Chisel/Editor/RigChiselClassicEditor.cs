#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RigChiselClassicEditor : EditorWindow
{
    [MenuItem("Tools/Upgrade Classic Chisel to Modern Rig")]
    public static void UpgradeChisel()
    {
        // Find the classic chisel
        GameObject classicChisel = GameObject.Find("Chissel_classic_rigged");
        if (classicChisel == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'Chissel_classic_rigged' in the scene. Please open the StoneCuttingScene_Classic scene.", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Upgrade Classic Chisel");
        int groupIndex = Undo.GetCurrentGroup();

        // 1. Remove the old ChiselController
        ChiselController legacyController = classicChisel.GetComponent<ChiselController>();
        if (legacyController != null)
        {
            Undo.DestroyObjectImmediate(legacyController);
            Debug.Log("Removed legacy ChiselController.");
        }

        // 2. Add the modern ManualChiselController
        ManualChiselController modernController = classicChisel.GetComponent<ManualChiselController>();
        if (modernController == null)
        {
            modernController = Undo.AddComponent<ManualChiselController>(classicChisel);
            Debug.Log("Added ManualChiselController.");
        }

        // 3. Map the rig bones by finding them in children
        Transform root = FindDeepChild(classicChisel.transform, "Root");
        Transform tilt = FindDeepChild(classicChisel.transform, "Tilt");
        if (tilt == null) tilt = FindDeepChild(classicChisel.transform, "Up_down_1");
        
        Transform extend = FindDeepChild(classicChisel.transform, "Extend");
        if (extend == null) extend = FindDeepChild(classicChisel.transform, "Up_down_extended");

        Transform tip = FindDeepChild(classicChisel.transform, "Tip");
        if (tip == null && extend != null)
        {
            // Create a tip if it doesn't exist
            GameObject tipGo = new GameObject("ChiselTip");
            tipGo.transform.SetParent(extend);
            tipGo.transform.localPosition = new Vector3(0, 0, 1.5f); // Approximate tip offset
            tip = tipGo.transform;
            Undo.RegisterCreatedObjectUndo(tipGo, "Create Tip");
        }

        // Apply mappings
        modernController.rootBone = root;
        modernController.tiltBone = tilt;
        modernController.extendBone = extend;
        modernController.chiselTip = tip;

        // 4. Map the joystick
        VirtualJoystick joystick = Object.FindFirstObjectByType<VirtualJoystick>();
        modernController.joystick = joystick;

        // 5. Default settings (matching modern feel)
        modernController.baseTurnSpeed = 50f;
        modernController.headAimSpeed = 60f;
        modernController.maxExtensionDistance = 2f;
        modernController.hitSpeed = 25f;
        modernController.returnSpeed = 10f;
        modernController.minTiltUp = -90f;
        modernController.maxTiltUp = -30f;
        modernController.minTiltSide = -90f;
        modernController.maxTiltSide = 90f;
        
        // 6. Optional: Wire UI buttons if they exist
        Button leftBtn = FindButton("LeftTurn_Button");
        Button rightBtn = FindButton("RightTurn_Button");
        Button hitBtn = FindButton("Hit_Button");
        
        if (hitBtn != null)
        {
            // We can't perfectly bind persistent events via code without serialization, 
            // but we can warn the developer to do it.
            Debug.Log("Hit button found. Please bind its OnClick event to ManualChiselController.StrikeStone().");
        }

        Undo.CollapseUndoOperations(groupIndex);

        EditorUtility.DisplayDialog("Success", "Classic Chisel has been upgraded!\n\nPlease verify:\n1. Bone assignments in Inspector (Root, Tilt, Extend, Tip)\n2. Wire Left/Right UI Buttons using EventTriggers (PointerDown/Up) to RotateBaseLeft/Right and StopBaseRotation.\n3. Wire the Hit button to StrikeStone().", "OK");
    }

    private static Transform FindDeepChild(Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name.Contains(aName)) return c;
            foreach (Transform t in c) queue.Enqueue(t);
        }
        return null;
    }
    
    private static Button FindButton(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.GetComponent<Button>() : null;
    }
}
#endif