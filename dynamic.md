# Dynamic UI & GameFlow Implementation Guide

This guide explains how the **Dynamic UI Theme Switching** and **Global Mode Selection** are implemented in the project, and how you can manage or customize the UI layout for different panels (Main Menu, Profile, Leaderboard, and Stone Market).

---

## 1. The Core Concept: Parent Panel Toggling

Instead of dynamically changing individual buttons, fonts, and background images via code (which is error-prone and hard to maintain), the system uses **Parent Panel Toggling**. 

Under your main Canvas, you create two distinct parent GameObjects—one for the **Classic** theme version, and one for the **Modern** theme version. Toggling the active state of these parents automatically updates the design for all child panels (Main Menu, Profile, Leaderboard, and Stone Market) instantly.

### Recommended Canvas Hierarchy
```text
Canvas (Main UI)
  ├── Classic_UI (Classic Parent Panel) ── [UIThemeApplier classicUIParent]
  │     ├── Menu-Panel (Classic)
  │     ├── Profile-Panel (Classic)
  │     ├── Tool-Panel (Classic)
  │     ├── Player-StoneMarket-Panel (Classic)
  │     └── Leadership-Panel (Classic)
  │
  └── Modern_UI (Modern Parent Panel) ── [UIThemeApplier modernUIParent]
        ├── Menu-Panel (Modern)
        ├── Profile-Panel (Modern)
        ├── Tool-Panel (Modern)
        ├── Player-StoneMarket-Panel (Modern)
        └── Leadership-Panel (Modern)
```

---

## 2. Automated Editor Setup Tool 🛠️

To align your existing hierarchy with the target design instantly, you can use the custom editor tool we created:

1. Open your project in the Unity Editor and load the `MainMenu` scene.
2. In the top menu bar, click on **`Tools > Restructure Main Menu UI`**.
3. The editor script will automatically:
   * Create the `Classic_UI` and `Modern_UI` containers under your Canvas.
   * Move your original panels into `Classic_UI` (renaming them to have a `(Classic)` suffix).
   * Instantiate exact duplicate templates of those panels under `Modern_UI` (with a `(Modern)` suffix).
   * Add the `UIThemeApplier` component to your Canvas and configure its reference fields automatically.

---

## 3. Technical Script Architecture

Four scripts work together to manage the state and trigger UI changes:

1. **[GameModeManager.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/MVC/Models/GameModeManager.cs)**:
   * Acts as the single source of truth for the game theme (`Classic = 0`, `Modern = 1`).
   * Saves settings persistently using `PlayerPrefs` (`"SavedGameTheme"`).
   * Automatically initializes itself (Self-Instantiating Singleton) when accessed by any script.
   * Calls `NotifyThemeAppliers()` whenever the settings change to update all layouts in the scene.

2. **[UIThemeApplier.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/MVC/Views/UIThemeApplier.cs)**:
   * Attached to canvases or UI Managers in the scene.
   * Contains reference fields for `classicUIParent` and `modernUIParent`.
   * Toggles the active states of the parents inside `OnEnable()`, ensuring hidden panels automatically apply the correct theme whenever they are opened.

3. **[SettingsPanelController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/MVC/Controllers/SettingsPanelController.cs)**:
   * Links button click events inside the settings UI to `GameModeManager` to update the global theme.

4. **[AcceptMatchController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/MVC/Controllers/AcceptMatchController.cs)**:
   * Bypasses the pop-up panel when accepting a card and launches the pre-selected gameplay scene immediately.

---

## 4. How to Customize and Change UI in Different Panels

Follow these steps when editing UI layouts or adding new visual components in the Unity Editor:

### Step 1: Select the Target Parent in Hierarchy
* **To Edit Classic Style Panels**: Expand `Classic_UI` in the hierarchy, find the panel you want to change (e.g., `Profile-Panel (Classic)`), and edit its visual layout.
* **To Edit Modern Style Panels**: Expand `Modern_UI` in the hierarchy, find the panel you want to change (e.g., `Profile-Panel (Modern)`), and edit its visual layout.

### Step 2: Configure the UIThemeApplier
1. Select the GameObject acting as your Canvas or UI Manager.
2. Attach the `UIThemeApplier` script.
3. Drag your `Classic_UI` parent GameObject into the **`Classic UI Parent`** slot in the inspector.
4. Drag your `Modern_UI` parent GameObject into the **`Modern UI Parent`** slot in the inspector.

### Step 3: Run Theme-Specific Logic in Custom Scripts
If you are writing scripts that need to adapt their runtime logic depending on the active theme, use the following template:
```csharp
if (GameModeManager.Instance.currentTheme == GameModeManager.GameTheme.Classic)
{
    // Run Classic-specific logic
}
else
{
    // Run Modern-specific logic
}
```
