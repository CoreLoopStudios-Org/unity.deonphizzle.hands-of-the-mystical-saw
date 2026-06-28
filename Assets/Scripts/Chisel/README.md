# 🔨 Chisel Striking Mechanism

The **Chisel Mechanism** is a specialized precision tool system designed for the final stages of the Jade Stone Cutting simulation. Unlike bulk-removal tools like the Saw, the Chisel provides discrete, high-impact strikes used to trigger anchors and extract jade cores.

## 📖 Overview

The mechanism handles the transition from a "resting" tool state to a "striking" state based on user interaction. It uses procedural animation (Lerp) rather than traditional keyframe animation to allow for dynamic alignment with varying stone surfaces.

## ⚙️ How It Works

### 1. Surface Detection (Raycasting)
The script casts a ray from the camera to the mouse position. If it hits an object tagged as `Stone`, it triggers the `HitChisel` coroutine.

### 2. Dynamic Alignment
Upon impact, the chisel automatically rotates to align with the **Surface Normal** of the stone. 
*   **Upright Constraint:** If `keepBodyUpright` is enabled, the chisel maintains its vertical orientation (Y-axis) while still facing the impact point, preventing awkward upside-down striking angles.

### 3. Procedural Animation Loop
*   **Strike Phase:** Moves the chisel toward the `targetPoint` plus a customizable `hitOffset`.
*   **Effect Trigger:** At the peak of the strike, it instantiates visual effects (sparks/dust) and plays audio clips.
*   **Logic Registration:** It notifies the `StoneGenerator` via `RegisterToolStrike()` to update game progress.
*   **Return Phase:** Smoothly interpolates the tool back to its original "Equipped" position and rotation.

## 🛠 Inspector Configuration

| Parameter | Description |
| :--- | :--- |
| **Hit Speed** | How fast the chisel moves toward the stone. |
| **Return Speed** | How fast the chisel moves back to the starting position. |
| **Rotation Offset** | Fine-tune the chisel's orientation if the 3D model is not naturally aligned. |
| **Keep Body Upright** | Prevents the chisel from tilting too far when hitting the sides/bottom of a stone. |
| **Hit Offset** | The distance from the surface where the chisel "stops" (useful for preventing mesh clipping). |
| **Hit Effect Prefabs** | Slots for Primary (Sparks) and Secondary (Dust) particle systems. |
| **Audio Clips** | Slots for impact sounds and crumbling audio. |

## 🔄 Replacing the Model

To swap the chisel model while keeping this logic:
1.  Open the `UPDATECHISEL` prefab.
2.  Replace the child mesh with your new 3D model.
3.  Ensure the new model's **Forward (Z) axis** points toward the chisel's tip.
4.  Adjust the **Rotation Offset** in the `ChiselController` component if the model is rotated incorrectly.
5.  Set the **Pivot Point** of the new model to the handle/base for the most realistic movement.

## 🔗 Dependencies
*   `StoneSpinController`: Checked to ensure the tool doesn't fire while the inspection torch is active.
*   `StoneGenerator`: Used to register hits and progress the game state.

---
*Part of the Unity Jade Core Mechanism System.*
