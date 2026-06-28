#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class SceneHierarchyRestructureEditor : EditorWindow
{
    [MenuItem("Tools/Restructure Main Menu UI")]
    public static void RestructureUI()
    {
        // Find main Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in the active scene!", "OK");
            return;
        }

        Transform canvasTransform = canvas.transform;

        // Check if already restructured
        Transform existingClassic = canvasTransform.Find("Classic_UI");
        Transform existingModern = canvasTransform.Find("Modern_UI");

        if (existingClassic != null || existingModern != null)
        {
            if (!EditorUtility.DisplayDialog("Warning", "Classic_UI or Modern_UI already exists under Canvas. Restructuring again may create duplicates. Do you want to proceed?", "Yes", "No"))
            {
                return;
            }
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Restructure UI Hierarchy");
        int groupIndex = Undo.GetCurrentGroup();

        // Create Classic_UI and Modern_UI Parents under Canvas
        GameObject classicUIGO = new GameObject("Classic_UI", typeof(RectTransform));
        classicUIGO.transform.SetParent(canvasTransform, false);
        RectTransform classicRect = classicUIGO.GetComponent<RectTransform>();
        classicRect.anchorMin = Vector2.zero;
        classicRect.anchorMax = Vector2.one;
        classicRect.sizeDelta = Vector2.zero;
        Undo.RegisterCreatedObjectUndo(classicUIGO, "Create Classic_UI");

        GameObject modernUIGO = new GameObject("Modern_UI", typeof(RectTransform));
        modernUIGO.transform.SetParent(canvasTransform, false);
        RectTransform modernRect = modernUIGO.GetComponent<RectTransform>();
        modernRect.anchorMin = Vector2.zero;
        modernRect.anchorMax = Vector2.one;
        modernRect.sizeDelta = Vector2.zero;
        Undo.RegisterCreatedObjectUndo(modernUIGO, "Create Modern_UI");

        // Find existing panels
        string[] panelNames = { "Menu-Panel", "Profile-Panel", "Tool-Panel", "Player-StoneMarket-Panel", "Leadership-Panel", "Mode-Selection Panel" };
        foreach (string panelName in panelNames)
        {
            Transform panel = canvasTransform.Find(panelName);
            if (panel != null)
            {
                // Instantiate a copy for Modern UI
                GameObject modernCopy = Instantiate(panel.gameObject, modernUIGO.transform, false);
                modernCopy.name = panelName + " (Modern)";
                Undo.RegisterCreatedObjectUndo(modernCopy, "Clone Panel to Modern");

                // Move original to Classic UI
                Undo.SetTransformParent(panel, classicUIGO.transform, "Move Panel to Classic");
                panel.gameObject.name = panelName + " (Classic)";
            }
        }

        // Add UIThemeApplier to Canvas if it doesn't exist
        UIThemeApplier applier = canvas.GetComponent<UIThemeApplier>();
        if (applier == null)
        {
            applier = Undo.AddComponent<UIThemeApplier>(canvas.gameObject);
        }

        applier.classicUIParent = classicUIGO;
        applier.modernUIParent = modernUIGO;

        Undo.CollapseUndoOperations(groupIndex);

        // Mark the active scene as dirty so changes can be saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Success", "MainMenu UI Restructured successfully!\n\n1. Created 'Classic_UI' and 'Modern_UI' parents.\n2. Moved original panels to Classic_UI.\n3. Cloned panels to Modern_UI.\n4. Added 'UIThemeApplier' and mapped references on Canvas GameObject.", "Awesome");
    }
}
#endif
