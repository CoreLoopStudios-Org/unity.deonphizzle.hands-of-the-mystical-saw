# Detailed Comparison: Modern Saw_rigged vs. Classic SawUpdatePibot-old-classic

This document analyzes the differences between the **Modern Rigged Saw (`Saw_rigged`)** used in the `StoneGenerator Scene` (Modern) and the **Classic Saw Model (`SawUpdatePibot-old-classic`)** used in `StoneCuttingScene_Classic` (Classic).

---

## 1. Subsystem Comparison Matrix

| Feature | Modern Mode: `Saw_rigged` | Classic Mode: `SawUpdatePibot-old-classic` |
| :--- | :--- | :--- |
| **Scene File** | [StoneGenerator Scene.unity](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/ALL-SCENE-IS%20HERE/StoneGenerator%20Scene.unity) | [StoneCuttingScene_Classic.unity](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/ALL-SCENE-IS%20HERE/StoneCuttingScene_Classic.unity) |
| **Object Name** | `Saw_rigged` | `SawUpdatePibot-old-classic` |
| **Active Code** | [SawArmController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawArmController.cs) (or [SawToolController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawToolController.cs)) | [SawController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawController.cs) (Screen-Swipe) |
| **Role of 3D Model** | **Interactive Controller**: The model's joints rotate and slide dynamically in response to joystick/button inputs. | **Static Visual / Prop**: The model is a static mesh that rests in the environment. It does not move dynamically via joysticks. |
| **Input Style** | Joysticks (Yaw/Pitch base tilt) + UI Buttons (Piston extension). | Screen-space swiping (dragging a 2D line on the screen with the mouse). |
| **Mesh Slicing** | Slices the stone mesh dynamically at the blade's current 3D position. | Projects the 2D screen swipe line into a 3D slicing plane to split the stone. |
| **Grinding Interaction** | Overlaps the stone to generate spark effects, water effects, sawing sound loops, and visual dent decals. | None. Slicing occurs instantly upon mouse release, with no interactive surface contact. |

---

## 2. Structural & Joint Differences

### Modern Model: `Saw_rigged`
* **Skeleton Armature**: Features a fully rigged armature containing:
  * `Root` (Yaw Pivot)
  * `Up_down-base rotate` (Pitch/Tilt Pivot)
  * `Extend` (Linear Slider)
  * `Saw_rotate` (Spins blade disc)
* **Mechanics**: The script [SawArmController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawArmController.cs) maps virtual joysticks and UI buttons directly to these bone transforms, rotating or translating them locally. The saw blade behaves like a physical industrial cutter.

### Classic Model: `SawUpdatePibot-old-classic`
* **Structure**: A static 3D model node hierarchy containing `Tool_01_update (1)` and child mesh renderers. It has no active script modifying its bones or base coordinates at runtime.
* **Mechanics**: The actual gameplay interaction is offloaded to the [SawController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawController.cs) component attached to a separate UI manager object. The saw slice is calculated purely mathematically from screen-space mouse drag vectors:
  $$\vec{Normal}_{plane} = \vec{Forward}_{camera} \times \vec{Direction}_{swipe}$$
  Once the swipe is released, the target stone splits, and the visual feedback is managed via particle emitters and temporary debris gravity. The static `SawUpdatePibot-old-classic` model remains static on the side of the platform.

---

## 3. Rebuilding Instructions

If you need to copy or migrate these configurations:
* **To build the Modern robotic setup**: Instantiate the [SawControllerManager (1).prefab](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/UPDATED-TOOLS-PREFAB/SawControllerManager%20(1).prefab) prefab and configure the [SawArmController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawArmController.cs) script by linking virtual joysticks, movement buttons, and assigning the rigged bone hierarchy.
* **To build the Classic swipe setup**: Place a static saw mesh as a visual prop, create a `LineRenderer` to draw swipes, and attach the [SawController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawController.cs) script to a manager object in the scene.
