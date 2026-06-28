# 🏗 Jade Core Project Architecture & Mechanics

This document provides a comprehensive technical breakdown of the **Jade Stone Cutting Simulation** system, covering mechanics, tools, generation, and gameplay modes.

---

## 1. Core Mechanics Index

The project is built on **5 foundational mechanics** that define the interaction loop:

| Mechanic | Logic & Scripting | Purpose |
| :--- | :--- | :--- |
| **Procedural Slicing** | `SliceController.cs` + `EzySlice` | Allows players to physically cut through stone meshes at any angle. |
| **Surface Striking** | `ChiselController.cs`, `HammerAction.cs` | Discrete, high-impact events that interact with "Anchors" on the stone surface. |
| **X-Ray Inspection** | `TorchInspectionManager.cs`, `SimpleTorch.cs` | Replaces stone material with a transparent shader to reveal the internal Jade Core. |
| **Anchor Network** | `HitAnchor.cs`, `StoneGenerator.cs` | A hidden graph of points within the stone. All anchors must be destroyed to reveal the jade. |
| **Spin & Commit** | `StoneSpinController.cs` | Stones rotate dynamically. Players must "Commit" (Freeze) the stone to perform a precise cut. |

---

## 2. Functional Toolset

The simulation currently supports **4 active tools**, each with distinct success rates and interaction styles:

1.  **🪚 The Saw:** 
    *   **Type:** Continuous Slicer.
    *   **Behavior:** Uses drag-to-slice logic.
    *   **Best For:** Removing large chunks of the outer stone shell quickly.
2.  **🔨 The Hammer:**
    *   **Type:** High-Impact Striker.
    *   **Behavior:** Single-click impact. High power but lower precision.
    *   **Best For:** Breaking "Hardened Shell" adversities.
3.  **⛏️ The Chisel:**
    *   **Type:** Precision Striker.
    *   **Behavior:** Surface-normal aligned strikes.
    *   **Best For:** Final extraction and delicate carving near the Jade Core.
4.  **🔦 The Torch:**
    *   **Type:** Inspection Utility.
    *   **Behavior:** Non-destructive. Toggles the X-Ray view.
    *   **Note:** Using the torch often consumes "Prize Points" or has limited uses per session.

---

## 3. Stone Lifecycle & Generation

### 🛠 Stone Generation (Procedural)
Stones are not static models; they are **generated at runtime** via the `StoneGenerator`:
*   **Data Source:** JSON Blueprints (`StoneBlueprint`).
*   **Physics:** Every stone has unique **Density**, **Fracture Tolerance**, and **Internal Stress**.
*   **Visuals:** Randomly selected realistic stone materials and procedural scales.
*   **Jade Core:** A high-value object embedded inside with varying clarity and mass.

### 🧩 Stone Cutting Workflow
1.  **Generate:** Load JSON data -> Spawn Stone + Jade Core + Anchor Network.
2.  **Inspect:** Use Torch to locate the Jade and Anchors.
3.  **Cut:** Use Saw/Hammer/Chisel to remove the "Stone" layer.
4.  **Validate:** Each hit is registered. If the player exceeds the "Max Strikes" without clearing the anchors, the stone **shatters**.
5.  **Extract:** Once all anchors are cleared, the final jade is revealed.

---

## 4. Gameplay Modes

The project features multiple modes accessible via distinct scenes:

### 🏆 Classic Mode (`StoneCuttingScene_Classic`)
*   The standard game loop.
*   Balanced difficulty with a focus on score management and time limits.
*   Manual tool selection and resource management.

### 🔮 Predictor Mode (`PredictorScene`)
*   **Logic:** Uses `predictor_challenge_data`.
*   **Movement Sequences:** Features complex rotation patterns (Clockwise -> Wait -> Counter-Clockwise).
*   **Purpose:** Designed for "Challenge" levels where players must time their strikes against predictable but difficult movement patterns.

### ⚡ Modern/Generator Mode (`StoneGenerator Scene`)
*   Focused on the **MVC (Model-View-Controller)** architecture.
*   Uses `CurrentStoneModel` to dynamically update visuals and physics based on external data.
*   Highly modular and used for testing new stone blueprints.

### 🔍 Stone Viewer Mode (`StoneViewerScene`)
*   A "Catalog" style view.
*   Allows users to inspect the detailed properties of a stone (mass, clarity, value) without the pressure of the cutting timer.

---

## 5. System Dependencies

*   **MVC Architecture:** Clean separation between `StoneBlueprint` (Model), `StoneView` (Visuals), and `JadeCuttingGame` (Controller).
*   **Input System:** Built on the Unity **New Input System** for cross-platform (Mobile/Desktop) compatibility.
*   **Rendering:** **Universal Render Pipeline (URP)** with custom ShaderGraphs for X-Ray effects.

---
*Documentation generated for the Jade Core Mechanism Project.*
