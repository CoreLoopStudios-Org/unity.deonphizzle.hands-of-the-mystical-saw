# 📐 Chisel Model Analysis: `Chisel_rigged (1).fbx`

This document provides a technical breakdown of the primary Chisel 3D model used in the Jade Stone Cutting mechanism.

## 🛠 Model Technical Specifications

| Property | Value |
| :--- | :--- |
| **File Name** | `Chisel_rigged (1).fbx` |
| **Location** | `Assets/DeonPhizzle-/Chisel Final/` |
| **Format** | FBX (Filmbox) |
| **Rig Type** | Generic (Rigged with Skeleton) |
| **Scale** | 1.0 (File Units) |
| **Vertices/Polygons** | Mid-poly optimized for mobile/desktop |

## 🦴 Rigging & Animation

Although the file is named **"rigged"**, it is currently utilized in a **Procedural Framework**:
*   **Skeleton:** The model contains internal joints/bones. These can be used for manual hand-keyed animations (like a handle vibration or a slight bend upon impact) if an `Animator` component is added.
*   **Current Usage:** The `ChiselController.cs` ignores the internal rig and moves the **Root Transform** of the model. This ensures that the entire tool moves as a solid unit during the striking phase.

## 🎨 Materials & Textures

*   **Material Location:** Internal (Embedded). The materials are mapped directly from the FBX import settings.
*   **Shader Compatibility:** Optimized for the **Universal Render Pipeline (URP)**. It is recommended to use the `URP/Lit` or `URP/Simple Lit` shaders to maintain consistency with the stone's lighting environment.

## 📍 Integration Details (for replacement)

If you are replacing this model with a custom one, keep the following "Analysis" points in mind:

1.  **Pivot Alignment:** The model's pivot should ideally be at the **Handle (Base)**. This allows the procedural code to rotate the chisel naturally without swinging the tip in a wide arc.
2.  **Forward Vector:** The "Cutting Tip" of the chisel should point along the **Positive Z-Axis (Forward)**.
3.  **Naming Convention:** The current prefab `UPDATECHISEL` expects a child mesh. If you rename the internal mesh nodes in your new FBX, you may need to re-assign references in the Unity Inspector.

## 🚀 Performance Notes
*   The model uses **Weld Vertices** and **Normal Smoothing (60 degrees)** to ensure clean highlights on the metallic surface of the chisel bit.
*   **Mesh Compression** is currently off to maintain high-fidelity edges for close-up "Torch Mode" inspection.

---
*Reference file for the DeonPhizzle Jade Cutting Toolset.*
