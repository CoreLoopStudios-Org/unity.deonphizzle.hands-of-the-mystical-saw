# 📋 Implementation Plan: Modern Saw Slicing Fix

This plan details the analysis and proposed steps to fix the cutting issue with the `Saw_rigged-Modern` model in the **StoneGeneratorScene** (`StoneGenerator Scene.unity`).

---

## 🔍 1. Root Cause Analysis

We identified two major issues preventing the modern saw from slicing the stone:

### Issue A: Missing Script Reference in `ToolSwitcher.cs`
The `Saw_rigged-Modern` prefab utilizes [SawArmController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawArmController.cs) for joystick movement, blade spin, and slicing.
However, [ToolSwitcher.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Tool/ToolSwitcher.cs#L69-L89) only registers and activates three controller types when equipping the saw:
1. `SawController` (Screen-space touch swipe)
2. `SawToolController` (Free-translation mode)
3. `ClassicSawController` (Rigged Classic Saw)

Because `SawArmController` is not in this list, its `EquipSaw()` method is never invoked, meaning `isEquipped` remains `false` forever. This disables blade spin, joystick/gyro aiming, and all slicing functionality.

### Issue B: Slicing Normal Axis Mismatch
* In the **Classic Saw**, the blade is aligned so it spins around the Local Z-axis `(0, 0, 1)`. The slicing plane normal is correctly set to `(0, 0, 1)`.
* In the **Modern Saw**, the blade is rotated 90 degrees and spins around the Local X-axis `(-1, 0, 0)`.
* However, in the modern saw prefab modifications, the slicing normal `bladeCutNormal` is still set to the Local Z-axis `(0, 0, 1)`. This forces EzySlice to cut at a perpendicular 90-degree angle relative to the physical blade disc.

---

## 🛠️ 2. Proposed Changes

### Step 1: Update `ToolSwitcher.cs`
Modify [ToolSwitcher.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Tool/ToolSwitcher.cs#L80-L85) to search for, equip, and unequip the `SawArmController` script component:
```csharp
            // Modern controller (Rigged Modern Saw)
            SawArmController sac = sawTool.GetComponentInChildren<SawArmController>(true);
            if (sac != null)
            {
                if (isActive) sac.EquipSaw();
                else sac.UnequipSaw();
            }
```

### Step 2: Auto-align Cut Normal in `SawArmController.cs`
Modify [SawArmController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawArmController.cs#L99-L113) inside `Start()` to automatically align the slicing plane normal with the blade's rotation axis:
```csharp
        // Force the slice cut normal to align with the blade's rotation axis
        bladeCutNormal = spinAxis.normalized;
```

---

## 🚀 3. Verification Plan

1. Select the **Saw** tool in **StoneGeneratorScene**.
2. Verify that the blade spins and responds to the joystick/gyro.
3. Drive the saw forward into the stone block using UI manual buttons.
4. Verify that sparks, audio, and physical debris fall away correctly.
