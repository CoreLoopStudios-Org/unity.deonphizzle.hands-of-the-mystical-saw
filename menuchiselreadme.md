# 🕹️ Manual Chisel System & Rig Analysis

This document details the architecture of the **Manual Chisel Mechanism**, specifically focusing on the integration between the `Chisel_rigged (1)` model and the `ManualChiselController.cs` script.

## 🦴 Model Structure (Hierarchy)

The `Chisel_rigged (1)` model is rigged with a hierarchical bone structure designed for mechanical articulation. The `ManualChiselController` targets three specific nodes:

1.  **Root Bone (Base):**
    *   **Function:** Handles the primary horizontal/base orientation.
    *   **Control:** Driven by UI Buttons (`RotateBaseLeft`/`Right`).
    *   **Logic:** Rotates on the `baseRotationAxis` (defaulted to X-axis per configuration).
2.  **Tilt Bone (Head/Pivot):**
    *   **Function:** Controls the vertical and horizontal "aim" of the chisel tip.
    *   **Control:** Driven by the `VirtualJoystick`.
    *   **Logic:** Clamped rotation on X (Tilt) and Y (Pan) axes to prevent unnatural mechanical clipping.
3.  **Extend Bone (Piston):**
    *   **Function:** The physical "striker" that hits the stone.
    *   **Control:** Triggered by the `StrikeStone()` method (via "CUT" button).
    *   **Logic:** Moves along the `strikeAxis` (default: Forward) using a procedural Lerp.

## 📜 Script Analysis: `ManualChiselController.cs`

The controller is a specialized state-machine that manages aiming, physics-checking, and visual feedback.

### Key Features:
*   **Joystick Deadzone:** Includes a small deadzone (`0.05f`) to prevent "drift" when the joystick is slightly off-center.
*   **Raycast Validation:** Unlike the automated chisel, the Manual Chisel performs a `Physics.Raycast` from the **Tilt Bone's** position toward the `strikeAxis` during the impact phase. This ensures hits are only registered if the chisel is actually pointed at the stone.
*   **Base Rotation:** Uses a direction-based input (`-1`, `0`, `1`) allowing for smooth, button-held rotation.
*   **Dual-Phase Strike:**
    *   **Phase 1 (Forward):** Snappy movement toward the stone at `hitSpeed`.
    *   **Phase 2 (Return):** Slower, controlled retraction at `returnSpeed`.

## ⚙️ Setup & Configuration

To ensure the model behaves correctly, the following Inspector settings are critical:

| Variable | Recommended Value | Why? |
| :--- | :--- | :--- |
| **Base Rotation Axis** | `(1, 0, 0)` | To match the mechanical hinge of the root bone. |
| **Strike Axis** | `(0, 0, 1)` | The "Forward" direction of the piston bone. |
| **Min/Max Tilt X** | `-30` to `45` | Prevents the head from looking into the machine's body. |
| **Hit Distance** | `2.0` | Adjust this based on how far the stone is from the machine base. |

## 🚀 Performance & Gameplay Integration
*   **Stone Registration:** Successfully hits are communicated to the `StoneGenerator` to trigger anchor destruction.
*   **Audio-Visuals:** Support for primary and secondary particle/sound layers (e.g., Sparks + Dust).
*   **Equip System:** The script respects an `isEquipped` flag, allowing it to be part of a larger multi-tool management system.

---
*Technical Manual for the Jade Core Manual Chisel Unit.*
