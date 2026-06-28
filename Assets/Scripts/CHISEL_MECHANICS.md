# Chisel System Technical Documentation: `Chisel_rigged (1)`

This document provides a detailed breakdown of the movement, rotation, and stability logic for the Chisel tool, managed by the `ManualChiselController.cs` script. Use this as a reference to maintain the "Missile Launcher" style behavior and prevent regressions.

---

## 🏗 System Architecture

The Chisel is a rigged tool consisting of three primary bones, each with a specific responsibility:

1.  **`rootBone` (Yaw/Base Rotation)**: Handles side-to-side rotation.
2.  **`tiltBone` (Pitch/Head Aiming)**: Handles up-and-down aiming.
3.  **`extendBone` (Strike/Translation)**: Handles the physical forward-and-back hitting motion.

---

## 🕹 Movement Logic: "Missile Launcher" Style

The chisel uses **Incremental (Delta-based) Movement**. Unlike standard controls that snap back to center, this system remembers its last orientation.

### 1. Rotation (Aiming)
*   **Joystick Input**: The joystick does not set an absolute angle. Instead, it adds a "delta" (change) to the current angle every frame.
*   **Freeze-on-Release**: When the joystick input is zero, the code adds `0` to the current rotation. This causes the chisel to stay exactly where it was last pointed, similar to a missile launcher tracking a target.
*   **Axes**:
    *   **Horizontal**: Rotates the `rootBone` on the **Y-axis**.
    *   **Vertical**: Rotates the `tiltBone` on the **Z-axis**.

### 2. Strike (Hitting)
*   Controlled by the `StrikeStone()` method.
*   Moves the `extendBone` along the `strikeAxis` (Local Forward) using a Coroutine.
*   It moves forward by `hitDistance`, triggers effects, and returns to `initialExtendLocalPos`.

---

## 📍 Default Position & Stability

To prevent the "Sinking Head" or "Jerk at Start" bugs, the system follows these stability rules:

1.  **Smart Initialization (`Start`)**:
    *   The script **reads** the actual rotation of the bones in the Hierarchy before any code runs.
    *   It stores these in `currentAimUp` and `currentAimSide`.
    *   *Result*: The chisel starts exactly where the artist placed it in the scene.

2.  **Angle Normalization**:
    *   The system converts Unity's 0–360 degree Euler angles into a -180 to 180 range to ensure smooth clamping and rotation math.

---

## ⚙️ Configuration Parameters (Inspector)

| Parameter | Default | Description |
| :--- | :--- | :--- |
| `headAimSpeed` | 60 | How fast the chisel moves when the joystick is pushed. |
| `joystickSensitivity` | 1.0 | Multiplier for the movement speed (Slider controlled). |
| `minTiltUp` / `maxTiltUp` | -30 / 45 | Stops the chisel from tilting too far up or into its own base. |
| `minTiltSide` / `maxTiltSide` | -360 / 360 | Sideways rotation limits (set to -360/360 for full orbit). |
| `hitDistance` | 2.0 | How far the chisel "jabs" forward during a strike. |

---

## ⚠️ Maintenance Rules for AI/Developers
*   **NEVER** change `HandleHeadAiming` to use direct assignment (e.g., `rotation = input`). Always use `currentRotation += input`.
*   **ALWAYS** ensure `currentAimUp` and `currentAimSide` are initialized in `Start()` from the bone's `localEulerAngles`.
*   **AXIS SYNC**: The UI buttons and Joystick must update the same state variables (`currentAimSide`) to prevent conflicting movements.

---
*Last Updated: May 2026*
