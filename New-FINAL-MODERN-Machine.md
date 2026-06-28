# New-FINAL-MODERN-Machine Mechanism Analysis

This document provides a detailed technical breakdown of the structure, animation sequence, and scripting driving the modern machine cover in the `StoneGenerator Scene`.

---

## 1. Physical 3D Model Structure

The **`New-FINAL-MODERN-Machine`** is instantiated from the FBX source model **`Machine (1).fbx`** (located at `Assets/DeonPhizzle-/Machine final/Machine (1).fbx`). It is organized as a segmented stone chamber cover designed to encapsulate the target jade/stone block.

The model is divided into three key structural parts:
*   **`newmodel-left`**: The left half of the sliding shell cover.
*   **`newmodel-right`**: The right half of the sliding shell cover.
*   **`newmodel-base`**: The bottom structural base panel.

---

## 2. Chamber Opening Mechanism (`StoneChamberOpener.cs`)

The lifecycle of the machine cover is programmatically controlled by the `StoneChamberOpener.cs` script (located at `Assets/Scripts/Stone/StoneChamberOpener.cs`). This script is attached to a manager object in the scene and coordinates a multi-stage procedural animation sequence when the scene loads.

### A. Stage 1: Delay before Activation
*   **Property**: `delayBeforeOpen` (Default: `5` seconds)
*   **Action**: The chamber remains completely closed for the first 5 seconds of play mode, keeping the stone hidden.

### B. Stage 2: Chamber Vibration / Warning
*   **Properties**: 
    *   `vibrationIntensity`: `0.05` units
    *   `vibrationDuration`: `0.5` seconds
*   **Action**: A coroutine triggers a rapid position jitter of the cover parts:
    *   `leftPart` and `rightPart` shake horizontally along the X-axis.
    *   `bottomPart` shakes vertically along the Y-axis.
*   **Visual Effect**: Gives the mechanical impression of locking pins releasing and pressure building inside the chamber.

### C. Stage 3: Split & Fly Away
*   **Properties**:
    *   `leftFlyDistance`: `70` units (Left movement)
    *   `rightFlyDistance`: `70` units (Right movement)
    *   `backwardFlyDistance`: `20` units (Backward movement)
    *   `bottomFlyDistance`: `70` units (Downward movement)
    *   `flySpeed`: `2` (Lerp speed multiplier)
*   **Action**: The three parts split and fly open:
    *   **Left Cover (`newmodel-left`)** moves left by 70 units and backward by 20 units.
    *   **Right Cover (`newmodel-right`)** moves right by 70 units and backward by 20 units.
    *   **Bottom Base (`newmodel-base`)** slides straight down by 70 units.
*   **Result**: The chamber slides away entirely to fully expose the stone cutting bed.

### D. Stage 4: Deletion & Cleanup
*   **Action**: 10 seconds after the opening animation completes, `Destroy()` is called on all three parts (`leftPart`, `rightPart`, `bottomPart`) to release system memory and clean up the scene geometry.

---

## 3. Configuration Summary in Modern Scene

Below are the exact values mapped to the `StoneChamberOpener` component in `StoneGenerator Scene.unity`:

| Property | Value | Description |
| :--- | :--- | :--- |
| **`leftPart`** | `newmodel-left` | Left shell reference |
| **`rightPart`** | `newmodel-right` | Right shell reference |
| **`bottomPart`** | `newmodel-base` | Bottom base reference |
| **`delayBeforeOpen`** | `5` | Wait duration before opening |
| **`vibrationIntensity`** | `0.05` | Vibration distance multiplier |
| **`vibrationDuration`** | `0.5` | Vibration shake duration |
| **`leftFlyDistance`** | `70` | Distance the left shell flies to the left |
| **`rightFlyDistance`** | `70` | Distance the right shell flies to the right |
| **`backwardFlyDistance`** | `20` | Distance both shells move backward |
| **`bottomFlyDistance`** | `70` | Distance the base slides down |
| **`flySpeed`** | `2` | Lerping speed factor |
