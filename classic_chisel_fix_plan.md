# Implementation Plan: Classic Chisel Control & Axis Alignment Fix (V2)

This plan outlines the analysis and steps to fix the remaining virtual joystick mapping issues for the classic chisel (**`Chissel_classic_rigged-`**) in `StoneCuttingScene_Classic.unity`.

---

## 🔍 Root Cause Analysis

### 1. Inverted Controls & Scene Overwrite
* **What's Happening**: Move joystick **LEFT** turns chisel **RIGHT**, and vice versa.
* **Why**: The Unity Editor was in Play Mode when we modified the scene file `.unity` on disk. When Play Mode stopped, Unity saved its cached state and overwrote our scene modifications, reverting `invertHorizontal` to `0` (false).
* **Solution**: Instead of relying on the inspector or scene serialization, we will configure the correct axes and inversions **programmatically in `Start()`** by checking if the model is the classic one. This is 100% robust and cannot be overwritten by the editor.

### 2. Configured Axes
* **Yaw (LEFT/RIGHT)**: The root bone's local X-axis `(1, 0, 0)` points vertically down. Rotating around it yaws the chisel horizontally. Setting `invertHorizontal = true` programmatically will align it with the joystick direction.
* **Pitch (UP/DOWN)**: The first `Up_down` bone's local Z-axis `(0, 0, 1)` points horizontally. Rotating around it tilts the chisel up/down. Setting `invertVertical = true` programmatically will align it with the joystick direction.

---

## 🛠️ Proposed Changes

### Step 1: Programmatic Configuration in Start Method
We will modify [ManualChiselController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Chisel/ManualChiselController.cs#L63) to automatically apply the correct settings at runtime based on `strikeAxis` (classic chisel has `strikeAxis.y != 0`):

```csharp
    void Start()
    {
        if (extendBone != null)
        {
            initialExtendLocalPos = extendBone.localPosition;
        }

        // Programmatically detect if this is the classic chisel and configure axes/inversions
        if (strikeAxis.y != 0) // Classic chisel uses (0, -1, 0)
        {
            Debug.Log("[ManualChiselController] Classic chisel detected. Applying correct axes and inversions programmatically.");
            yawAxis = new Vector3(1, 0, 0);   // Local X-axis
            pitchAxis = new Vector3(0, 0, 1); // Local Z-axis
            invertVertical = true;
            invertHorizontal = true;
        }
        else // Modern chisel
        {
            Debug.Log("[ManualChiselController] Modern chisel detected. Applying default axes.");
            yawAxis = new Vector3(0, 1, 0);   // Local Y-axis
            pitchAxis = new Vector3(0, 0, 1); // Local Z-axis
        }
            
        // 🌟 Lock default positions and rotations as baseline
        if (rootBone != null)
        {
            initialRootRotation = rootBone.localRotation;
        }
        
        if (tiltBone != null)
        {
            initialTiltRotation = tiltBone.localRotation;
        }

        currentAimSide = 0f;
        currentAimUp = 0f;
        ...
```

---

## 🚀 Execution & Verification
Once you approve this plan, I will:
1. Apply the programmatic change to [ManualChiselController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Chisel/ManualChiselController.cs).
2. Bring Unity to focus to compile and reload.
