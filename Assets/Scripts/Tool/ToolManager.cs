using UnityEngine;

public class ToolManager : MonoBehaviour
{
    [Header("Assign Tool Objects from Hierarchy")]
    public ToolController hammerCtrl;
    public ToolController sawCtrl;
    public ToolController chiselCtrl;

    [HideInInspector] 
    public ToolController activeToolController;

    void Start()
    {
        DeselectAll();
    }

    // বাটন থেকে এই ফাংশনটি কল হবে
    public void SelectTool(string toolName)
    {
        Debug.Log("SelectTool Button Clicked: " + toolName);
        DeselectAll();

        // নামের বানান চেক করুন (Case Sensitive: Saw, Hammer, Chisel)
        if (toolName == "Hammer") SetupTool(hammerCtrl);
        else if (toolName == "Saw") SetupTool(sawCtrl);
        else if (toolName == "Chisel") SetupTool(chiselCtrl);
        else Debug.LogError("Tool Name match korche na! Button er box e spelling check krun.");
    }

    private void SetupTool(ToolController controller)
    {
        if (controller == null) {
            Debug.LogError("Inspector e Tool Controller assign kora nei!");
            return;
        }
        
        controller.gameObject.SetActive(true);
        controller.isSelected = true; 
        activeToolController = controller;
        Debug.Log(controller.gameObject.name + " is now SELECTED.");
    }

    public void DeselectAll()
    {
        if (hammerCtrl) { hammerCtrl.isSelected = false; hammerCtrl.gameObject.SetActive(false); hammerCtrl.isWorking = false; }
        if (sawCtrl) { sawCtrl.isSelected = false; sawCtrl.gameObject.SetActive(false); sawCtrl.isWorking = false; }
        if (chiselCtrl) { chiselCtrl.isSelected = false; chiselCtrl.gameObject.SetActive(false); chiselCtrl.isWorking = false; }
        
        activeToolController = null;
    }

    public float GetSuccessChance(bool isTorchOn)
    {
        float baseChance = 20f;
        if (activeToolController == hammerCtrl) baseChance = 30f;
        else if (activeToolController == sawCtrl) baseChance = 60f;
        else if (activeToolController == chiselCtrl) baseChance = 45f;

        return isTorchOn ? baseChance + 25f : baseChance;
    }
}