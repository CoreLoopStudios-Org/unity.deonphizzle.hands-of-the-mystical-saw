# Rigged Tools Configuration, Prefabs & Reconstruction Guide

This document provides a comprehensive technical manual for recreating, configuring, and deploying the four rigged mechanical tools inside any Unity scene. It maps the relationship between the rigged 3D models, their physical prefabs, and the C# script controller variants.

---

## 1. Tool Prefabs & Controller Implementations

Each tool has dedicated prefab files and can operate under two distinct scripting philosophies: **Joint-Based Robotic Armature** (joystick joint rotations) or **Whole-Body Translation** (free-floating movement).

| Tool | Rigged Prefab Options | Attached/Available Scripts |
| :--- | :--- | :--- |
| **1. Rigged Saw** | - [SawControllerManager (1).prefab](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/UPDATED-TOOLS-PREFAB/SawControllerManager%20(1).prefab)<br>- [Saw_rigged.prefab](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/tools%20-prefab/sawprefab/Saw_rigged.prefab) | - **Joint-Based Rig**: [SawArmController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawArmController.cs)<br>- **Translation-Based**: [SawToolController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawToolController.cs) |
| **2. Rigged Hammer** | - [NewHammer3dModel.prefab](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/UPDATED-TOOLS-PREFAB/NewHammer3dModel.prefab)<br>- [Hammer_rigged.prefab](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/tools%20-prefab/hammer/Hammer_rigged.prefab) | - **Joint-Based Rig**: [NewHammerController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Hammer/NewHammerController.cs) |
| **3. Rigged Dremel** | - [Dramel_rigged.prefab](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/tools%20-prefab/Dramel/Dramel_rigged.prefab) | - **Grinding & Auto-Strike**: [DremelToolController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Dramel/DremelControlle.cs) |
| **4. Rigged Chisel** | - [UPDATECHISEL.prefab](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/UPDATED-TOOLS-PREFAB/UPDATECHISEL.prefab)<br>- [Chisel_rigged (1).prefab](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/tools%20-prefab/Chisel/Chisel_rigged%20(1).prefab) | - **Joint-Based Rig**: [ManualChiselController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Chisel/ManualChiselController.cs)<br>- **Click-to-Strike**: [ChiselController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Chisel/ChiselController.cs) |

---

## 2. Tool Architecture & Mechanics Breakdown

### 🪚 Tool 1: The Saw

#### Option A: Joint-Based Robotic Saw ([SawArmController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawArmController.cs))
* **Hierarchy**:
  ```mermaid
  graph TD
      Root["rootBone: Root"]
      Root -->|Joystick X Yaw| UpDown["upDownBone: Up_down-base rotate"]
      UpDown -->|UI Button Extension| Extend["extendBone: Extend"]
      Extend -->|Blade Spin| Blade["sawBlade: Saw_rotate"]
  ```
* **Inspector Constants**: `rootRotationAxis = (0,1,0)` (Y), `tiltRotationAxis = (0,0,1)` (Z), `extendAxis = (0,0,1)` (Z).
* **Mechanics**: Aiming rotates the base and tilts the arm via joystick. Translating the blade forward is driven by manual UI Buttons (using Linecast blocking to prevent passing through the stone). Slicing cuts mesh at blade's position using `EzySlice`.

#### Option B: Translation-Based Saw ([SawToolController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawToolController.cs))
* **Hierarchy**: Uses the same prefab, but ignores the joint hierarchy rotation. The entire tool body translates dynamically in world coordinates.
* **Controls**: Joystick X/Y translates X/Z coordinates, Gyro sensors input tilt, and UI Height sliders adjust Y position.
* **Mechanics**: Overlaps a sphere around the blade to apply continuous sparks and sawing audio on contact, then slices the mesh on command.

---

### 🔨 Tool 2: The Hammer

#### Joint-Based Robotic Hammer ([NewHammerController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Hammer/NewHammerController.cs))
* **Hierarchy**:
  ```mermaid
  graph TD
      Root["rootBone: Root"]
      Root -->|Joystick X Yaw| UpDownBottom["topBone: Up_down_bottom"]
      UpDownBottom --> NoNeed["No_need"]
      NoNeed --> UpDownTop["Up_down_top (unused)"]
      UpDownTop --> Extend["extendBone: Entend (unmoved)"]
      Extend --> Tip["hammerTip: Tip (sensor)"]
  ```
* **Inspector Constants**: `rootRotationAxis = (1,0,0)` (X), `tiltRotationAxis = (0,0,1)` (Z).
* **Mechanics**: Yaw is controlled by joystick X, and pitch tilt is controlled by joystick Y. Swing pulls back to `-180°` and strikes to `-20°` on Z. Checks for target impact using continuous frame-by-frame linecast sweeps from the tip.

---

### 🔌 Tool 3: The Dremel

#### Grinding & Auto-Strike Dremel ([DremelToolController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Dramel/DremelControlle.cs))
* **Hierarchy**:
  ```mermaid
  graph TD
      Root["rootBone: Root"]
      Root -->|Joystick X Yaw| UpDown["upDownBone: Up_down_base rotate"]
      UpDown -->|Piston Slide| Extend["extendBone: Extend"]
      Extend -->|Grind Spin| Tip["dremelTip: Tip"]
  ```
* **Inspector Constants**: `manualMoveAxis = (0,0,1)`, `spinAxis = (0,0,1)`, `strikeAxis = (0,0,1)`.
* **Mechanics**:
  1. Aiming is handled via Virtual Joystick.
  2. Manual Forward/Backward translations are triggered by UI Buttons.
  3. Automatic Grinding: Extends the piston forward; on collision, it applies continuous sparks, audio, and visual dents on the stone for 1.0 second, then retracts.

---

### ⛏️ Tool 4: The Chisel

#### Option A: Joint-Based Robotic Chisel ([ManualChiselController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Chisel/ManualChiselController.cs))
* **Hierarchy**:
  ```mermaid
  graph TD
      Root["rootBone: Root"]
      Root -->|UI Button Base Rotate| Pivot["tiltBone: Pivot"]
      Pivot -->|Piston Strike| Extend["extendBone: Extend"]
  ```
* **Mechanics**: Aims the head using joysticks, rotates the base with buttons, and lerps the piston forward to execute the strike.

#### Option B: Click-to-Strike Chisel ([ChiselController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Chisel/ChiselController.cs))
* **Hierarchy**: The entire chisel follows the cursor.
* **Mechanics**: When the mouse is clicked, the entire chisel model transforms directly to the click coordinates, aligning its forward vector to face `-surfaceNormal` (perpendicular to the stone face), executes a quick hit animation, and returns.

---

## 3. Step-by-Step Reconstruction Guide for a New Scene

To rebuild and configure these tools inside a new scene, follow this checklist:

### Step 1: Place the Prefabs
1. Drag the desired tool prefab (e.g. `SawControllerManager (1)`) from `Assets/UPDATED-TOOLS-PREFAB/` or `Assets/tools -prefab/` into the scene hierarchy.
2. Center the tool base near the stone platform.

### Step 2: Set Up Virtual Joysticks & UI Buttons
1. Ensure the scene has a Canvas containing a **Virtual Joystick** script (or UI Joystick panel).
2. Create UI Buttons for tool activation and manual movement:
   - For **Chisel** / **Saw** / **Dremel**: Add manual Forward/Backward buttons.
   - For **Chisel**: Add Base Rotate Left/Right buttons.
   - For **Hammer** / **Chisel** / **Dremel**: Add a **Strike/Cut** button.

### Step 3: Link Components in the Script Inspector
Select the tool script component in the Inspector and link:
- **`joystick` / `virtualJoystick`**: Drag the UI Joystick.
- **UI Buttons**: Link the corresponding OnClick listener events to:
  - `StrikeStone()` for Hammer/Chisel.
  - `StartGrinding()` for Dremel.
  - `SelectTool("ToolName")` in the scene's [ToolManager](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Tool/ToolManager.cs).
- **Manual Buttons (Dremel / Saw)**: Link `PointerDown` / `PointerUp` events to `MoveForward`/`MoveBackward` and `StopMovement`.

### Step 4: Assign Bones (Skeletal Mapping)
Drag the corresponding bones from the tool's armature hierarchy into the script fields:
- `rootBone` -> Drag the top-level parent bone.
- `topBone` / `upDownBone` -> Drag the pitching joint.
- `extendBone` -> Drag the piston bone.
- `hammerTip` / `dremelTip` -> Drag the endpoint tip sensor.

### Step 5: Verify Physics Layers & tags
1. The stone object in the scene must have the **`Stone`** tag and reside on the **`Stone`** physics layer.
2. Set the `stoneLayerMask` field on the tool controller scripts to include the **`Stone`** layer so collision checks, raycasts, and linecasts function correctly.
