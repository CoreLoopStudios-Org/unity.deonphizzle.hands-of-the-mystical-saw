# Dremel Tool Rotation Snapping Analysis & Fix

This document details the analysis of the Dremel tool rotation jumping/snapping bug in the `StoneCuttingScene_Classic` scene and explains how it was fixed in `DremelToolController`.

---

## 1. Problem Description
When moving the joystick in play mode to control the Dremel tool (`Dramel_rigged-Classic` model, managed by `DramelController-classic`), the tool would instantly jump (snap) to a completely wrong, fixed orientation.

---

## 2. Root Cause Analysis

### A. Bone Hierarchy and Rotations
In `StoneCuttingScene_Classic.unity`, the `Dramel_rigged-Classic` rig uses bones to control the yaw and pitch (tilt) of the tool head:
*   **Yaw Bone (`rootBone` / `7024483452947171021`)**: Initialized with a Y Euler rotation of `0` degrees. However, it is rotated by `-90` degrees on the Z-axis.
*   **Pitch Bone (`upDownBone` / `8067173292667300285`)**: Initialized with pitch (X) = `-179.825`, yaw (Y) = `0.049`, and roll (Z) = `78.259` degrees.

### B. The Scrambled Rotation Bug
In the original [DremelControlle.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Dramel/DremelControlle.cs) implementation, the joystick aiming logic modified the rotation like this:

```csharp
if (rootBone != null)
{
    rootBone.localRotation = Quaternion.Euler(
        rootBone.localRotation.eulerAngles.x, // ⚠️ DYNAMIC READ
        currentAimSide, 
        rootBone.localRotation.eulerAngles.z  // ⚠️ DYNAMIC READ
    );
}

if (upDownBone != null)
{
    upDownBone.localRotation = Quaternion.Euler(
        upDownBone.localRotation.eulerAngles.x, // ⚠️ DYNAMIC READ
        upDownBone.localRotation.eulerAngles.y, // ⚠️ DYNAMIC READ
        currentAimUp
    );
}
```

This code suffers from a **textbook Quaternion-to-Euler conversion bug**:
1. When calling `transform.localRotation.eulerAngles`, Unity converts the underlying Quaternion rotation back to Euler representation.
2. Because multiple Euler angle combinations can represent the same 3D orientation, Unity's conversion can return completely different numbers (e.g. flipping the X/Y axes by 180 degrees to represent pitch).
3. The script dynamically read these converted (scrambled) Euler values, modified only one of the axes, and reassigned them using `Quaternion.Euler`.
4. As soon as the joystick moved, this mixture of scrambled axes forced the bones to instantly snap/flip into a wrong orientation.

---

## 3. Implemented Fix

To resolve this, we modified [DremelControlle.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Dramel/DremelControlle.cs) to cache the initial Euler values on startup, preventing dynamic reads during joystick movement:

1. **State Variables Added**:
   ```csharp
   private float initialRootX;
   private float initialRootZ;
   private float initialUpDownX;
   private float initialUpDownY;
   ```

2. **Cached at Start**:
   ```csharp
   if (rootBone != null)
   {
       Vector3 rootEulers = rootBone.localEulerAngles;
       initialRootX = rootEulers.x;
       initialRootZ = rootEulers.z;
       currentAimSide = rootEulers.y;
       if (currentAimSide > 180f) currentAimSide -= 360f; 
   }
   
   if (upDownBone != null)
   {
       Vector3 upDownEulers = upDownBone.localEulerAngles;
       initialUpDownX = upDownEulers.x;
       initialUpDownY = upDownEulers.y;
       currentAimUp = upDownEulers.z;
       if (currentAimUp > 180f) currentAimUp -= 360f; 
   }
   ```

3. **Applied Safe Rotations in `HandleHeadAiming()`**:
   ```csharp
   if (rootBone != null)
   {
       rootBone.localRotation = Quaternion.Euler(
           initialRootX, 
           currentAimSide, 
           initialRootZ
       );
   }

   if (upDownBone != null)
   {
       upDownBone.localRotation = Quaternion.Euler(
           initialUpDownX, 
           initialUpDownY, 
           currentAimUp
       );
   }
   ```

This change ensures that all other rotation axes remain locked at their correct, author-time orientations, resolving the snapping issue entirely.
