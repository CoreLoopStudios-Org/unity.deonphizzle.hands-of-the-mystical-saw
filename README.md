# Unity Jade Stone Cutting Core Mechanism

A sophisticated Unity-based simulation of jade stone cutting and carving. This project implements a detailed physics-based system for analyzing, inspecting, and cutting raw stones to reveal high-value jade cores.

## 💎 Core Features

- **Procedural Stone Generation:** Stones are dynamically generated using JSON-based blueprints (`StoneBlueprint`), featuring unique physics properties, anchor networks, and jade core metrics.
- **Advanced Slicing Mechanics:** Real-time mesh slicing using the `EzySlice` framework, allowing players to physically cut through stone layers.
- **Torch Inspection System:** A specialized X-ray/Torch mode that allows players to inspect the internal structure of the stone, identify jade quality, and locate optimal strike points.
- **Multi-Tool Interaction:** Supports various tools with distinct success chances and behaviors:
  - **Hammer:** For direct impact and breaking outer shells.
  - **Chisel:** For precise carving and final jade extraction.
  - **Saw:** For clean slices through the stone body.
  - **Dramel:** For fine-tuned detailing.
- **Dynamic Physics & AI Validation:** Stones feature internal stress, fracture tolerance, and density. An AI validation layer ensures challenges are solvable and identifies optimal strike vectors.
- **MVC Architecture:** Clean separation of concerns with Model-View-Controller patterns for stone data management.

## 🛠 Technical Specifications

- **Engine:** Unity 6 (6000.3.2f1)
- **Input System:** Unity New Input System
- **Rendering:** Universal Render Pipeline (URP) with custom shaders (e.g., `WetXRayShader`)
- **UI:** TextMesh Pro (TMP) for high-fidelity data display
- **Data Format:** JSON for stone blueprints and game state
- **Dependencies:** 
  - `EzySlice` for mesh manipulation
  - `TextMesh Pro` for UI

## 📂 Project Structure

- `Assets/Scripts/Stone/`: Core logic for stone generation, movement, and shattering.
- `Assets/Scripts/Tool/`: Management and controllers for various cutting tools.
- `Assets/Scripts/MVC/`: Data models and controllers for the stone blueprint system.
- `Assets/Scripts/JadeCuttingGame.cs`: Main game loop and state management.
- `Assets/ALL-SCENE-IS HERE/`: Scene files including Main Menu, Stone Cutting, and Viewer modes.
- `Assets/Data/`: JSON data files for stone specifications.

## 🎮 How to Play

1. **Select a Stone:** Load a stone from a blueprint or generate a new one.
2. **Inspect:** Use the **Torch** to see through the outer shell and identify the location and quality of the jade core.
3. **Calibrate:** Freeze or rotate the stone to find the best angle for cutting.
4. **Choose Tool:** Select between Hammer, Saw, or Chisel depending on the required precision.
5. **Cut/Carve:** Click or drag (depending on the tool) to remove the stone shell. Be careful! Excessive "Strikes" (failed hits) will shatter the stone and destroy the jade.
6. **Extract:** Once the anchors are removed, use the Chisel for the final extraction to claim your reward.

## 🚀 Getting Started

1. Open the project in **Unity 6 (6000.3.2f1)**.
2. Ensure all packages are resolved via the Package Manager.
3. Open `Assets/ALL-SCENE-IS HERE/MainMenu.unity` to start.
4. If you encounter missing script references, check the `EzySlice` or `TextMesh Pro` installations.

---
*Developed as a core mechanism for jade stone simulation.*
