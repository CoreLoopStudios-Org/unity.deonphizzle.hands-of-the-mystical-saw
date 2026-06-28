# Gyroscope Calibration ("Xyro") & Clamping Update Plan

This document analyzes the gyroscope drift and off-screen boundary issues in `StoneGenerator Scene` and `StoneCuttingScene_Classic`, and presents the detailed implementation plan to resolve them.

---

## 🔍 1. Problem Analysis

### A. The 90-Degree Handheld Drift
*   **The Issue**: When the phone is flat, `Input.acceleration` reads $\approx (0, 0, -1.0)$ because gravity is aligned with the Z-axis. When held vertically at $90^\circ$, gravity aligns with the Y-axis, reading $\approx (0, -1.0, 0)$. 
*   **The Bug**: This constant gravity component creates a massive offset on the Y-axis ($a_y \approx -1.0$). Adding this to the inputs pegs the aiming angles to their extreme limits and translates positional tools completely off the screen immediately.
*   **The Solution**: Introduce a **Gyro Calibration (Zeroing)** system. When a tool is equipped, the system captures the current orientation as the offset:
    $$\mathbf{a}_{\text{calibrated}} = \mathbf{a}_{\text{current}} - \mathbf{a}_{\text{offset}}$$
    This makes the game relative to whatever posture the player holds the phone in.

### B. Screen Boundary Escape
*   **The Issue**: Positional tools ([HitController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Hammer/HitController.cs) and [ToolController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Tool/ToolController.cs)) translate the tool by adding world-space offsets (`gyroOffsetX`/`gyroOffsetY`) calculated from integrating raw acceleration. This causes two bugs:
    1.  **Integration Drift**: Any slight constant tilt causes the position to accumulate infinitely until the tool drifts off-screen.
    2.  **World Offset Overrun**: Adding offsets to world coordinates bypasses screen boundaries.
*   **The Solution**:
    1.  **Direct Mapping**: Map calibrated tilt directly to screen offsets (eliminating drift-prone integration over time).
    2.  **Screen-Space Clamping**: Apply the offsets to the screen pointer coordinates and clamp them within $[10\%, 90\%]$ of the screen dimensions *before* projecting into world coordinates:
        $$x_{\text{screen}} = \text{Clamp}\left(x_{\text{pointer}} + \Delta x_{\text{gyro}}, \ 0.1 \times W, \ 0.9 \times W\right)$$
        $$y_{\text{screen}} = \text{Clamp}\left(y_{\text{pointer}} + \Delta y_{\text{gyro}}, \ 0.1 \times H, \ 0.9 \times H\right)$$

---

## 🛠️ 2. Proposed Implementation Plan

### Step 1: Create `GyroCalibration` Utility Script
Create a static helper at [GyroCalibration.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Tool/GyroCalibration.cs):
```csharp
using UnityEngine;

public static class GyroCalibration
{
    private static Vector3 baseAcceleration = Vector3.zero;
    private static bool isCalibrated = false;

    public static void Calibrate()
    {
        baseAcceleration = Input.acceleration;
        isCalibrated = true;
        Debug.Log($"[GyroCalibration] Zeroed offset to: {baseAcceleration}");
    }

    public static Vector3 GetCalibratedAcceleration()
    {
        if (!isCalibrated) Calibrate();
        return Input.acceleration - baseAcceleration;
    }
}
```

---

### Step 2: Update Equip Routines to Trigger Calibration
We will update all tools to recalibrate the zero-offset whenever they are equipped. This makes sure calibration happens automatically without requiring a manual button.

1.  **`ClassicChiselController.cs`**:
    ```csharp
    public void EquipChisel()
    {
        isEquipped = true;
        GyroCalibration.Calibrate(); // Zero on equip
    }
    ```
2.  **`ManualChiselController.cs`**:
    ```csharp
    public void EquipChisel()
    {
        isEquipped = true;
        GyroCalibration.Calibrate();
    }
    ```
3.  **`DremelToolController` (`DremelControlle.cs`)**:
    ```csharp
    public void EquipDremel()
    {
        isEquipped = true;
        GyroCalibration.Calibrate();
    }
    ```
4.  **`NewHammerController.cs`**:
    ```csharp
    public void EquipHammer()
    {
        isEquipped = true;
        GyroCalibration.Calibrate();
    }
    ```
5.  **`ClassicSawController.cs` & `SawArmController.cs`**:
    ```csharp
    public void EquipSaw()
    {
        isEquipped = true;
        hasUsedStrikeThisSession = false;
        GyroCalibration.Calibrate();
        // UI toggle code...
    }
    ```
6.  **`SawToolController.cs`**:
    ```csharp
    public void EquipSaw()
    {
        isEquipped = true;
        hasUsedStrikeThisSession = false;
        GyroCalibration.Calibrate();
        // Mesh active code...
    }
    ```

---

### Step 3: Replace `Input.acceleration` with Calibrated Values in Aiming Scripts
For joint-based aimers (`ClassicChiselController`, `ManualChiselController`, `DremelToolController`, `NewHammerController`, `ClassicSawController`, `SawArmController`), we replace the raw readings:
```csharp
if (enableGyro)
{
    Vector3 calibAccel = GyroCalibration.GetCalibratedAcceleration();
    joyX += calibAccel.x * gyroSensitivity;
    joyY += calibAccel.y * gyroSensitivity;
}
```

---

### Step 4: Revamp Screen-Space Aiming & Clamp Limits
For translation-based tools (`HitController.cs` / `HammerController` and `ToolController.cs`), we replace the integration offset logic with direct screen-space mapping and pre-projection clamping.

**Update to `MoveToolToMouse()` in `HitController.cs` & `ToolController.cs`**:
```csharp
void MoveToolToMouse()
{
    if (mainCam == null) return;

    Vector3 mousePos = Input.mousePosition;

    if (enableGyro)
    {
        Vector3 calibAccel = GyroCalibration.GetCalibratedAcceleration();
        
        // Scale calibrated tilt directly to maximum screen shift (20% of screen size)
        float maxShiftX = Screen.width * 0.20f;
        float maxShiftY = Screen.height * 0.20f;

        float targetShiftX = calibAccel.x * gyroSensitivity * maxShiftX;
        float targetShiftY = calibAccel.y * gyroSensitivity * maxShiftY;

        // Smoothly interpolate the offset to prevent jitter
        gyroOffsetX = Mathf.Lerp(gyroOffsetX, targetShiftX, Time.deltaTime * smoothSpeed);
        gyroOffsetY = Mathf.Lerp(gyroOffsetY, targetShiftY, Time.deltaTime * smoothSpeed);

        mousePos.x += gyroOffsetX;
        mousePos.y += gyroOffsetY;
    }

    // Clamp screen coordinates to keep tool visible within [10%, 90%] of screen dimensions
    mousePos.x = Mathf.Clamp(mousePos.x, Screen.width * 0.1f, Screen.width * 0.9f);
    mousePos.y = Mathf.Clamp(mousePos.y, Screen.height * 0.1f, Screen.height * 0.9f);

    mousePos.z = dynamicDistance; // (zOffset for ToolController)
    Vector3 targetPos = mainCam.ScreenToWorldPoint(mousePos);

    // Apply standard boundary clamps to coordinate spaces...
    targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
    targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
    targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
}
```

This prevents the tools from ever moving off-screen and eliminates integration-based drift.
