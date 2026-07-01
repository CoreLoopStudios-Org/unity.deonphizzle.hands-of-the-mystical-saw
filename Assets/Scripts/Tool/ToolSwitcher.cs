using UnityEngine;

public class ToolSwitcher : MonoBehaviour 
{
    [Header("Tool Objects")]
    public GameObject hammerTool; 
    public GameObject sawTool;    
    public GameObject dremelTool; 
    public GameObject chiselTool; 

    void Start()
    {
        // Hammer will be selected by default at game start
        SelectHammer();
    }

    public void SelectHammer()
    {
        if (StoneSpinController.GlobalTorchActive) return;
        ActivateTool(hammerTool);
        Debug.Log("🪓 Hammer Selected!");
    }

    public void SelectSaw()
    {
        if (StoneSpinController.GlobalTorchActive) return;
        ActivateTool(sawTool);
        Debug.Log("🪚 Saw Selected!");
    }

    public void SelectDremel()
    {
        if (StoneSpinController.GlobalTorchActive) return;
        ActivateTool(dremelTool);
        Debug.Log("🔌 Dremel Selected!");
    }

    public void SelectChisel()
    {
        if (StoneSpinController.GlobalTorchActive) return;
        ActivateTool(chiselTool);
        Debug.Log("⛏️ Chisel Selected!");
    }

    public void DisableAllTools()
    {
        ActivateTool(null); 
        Debug.Log("🔦 All Tools Hidden for Torch!");
    }

    private void ActivateTool(GameObject activeTool)
    {
        // 1. Hammer control
        if(hammerTool != null) 
        {
            bool isActive = (hammerTool == activeTool);
            
            NewHammerController nhc = hammerTool.GetComponentInChildren<NewHammerController>(true);
            if (nhc != null) 
            {
                if (isActive) nhc.EquipHammer();
                else nhc.UnequipHammer();
            }
            
            hammerTool.SetActive(isActive);
        }

        // 2. Saw control
        if(sawTool != null) 
        {
            bool isActive = (sawTool == activeTool);

            SawController cutScript = sawTool.GetComponentInChildren<SawController>(true);
            SawToolController visualScript = sawTool.GetComponentInChildren<SawToolController>(true);

            if (cutScript != null) cutScript.enabled = isActive;
            if (visualScript != null) visualScript.enabled = isActive;

            // Classic controller (Rigged Classic Saw)
            ClassicSawController csc = sawTool.GetComponentInChildren<ClassicSawController>(true);
            if (csc != null)
            {
                if (isActive) csc.EquipSaw();
                else csc.UnequipSaw();
            }

            // Modern controller (Rigged Modern Saw)
            SawArmController sac = sawTool.GetComponentInChildren<SawArmController>(true);
            if (sac != null)
            {
                if (isActive) sac.EquipSaw();
                else sac.UnequipSaw();
            }

            sawTool.SetActive(isActive);
        }

        // 3. 🌟 Dremel Control (updated here)
        if(dremelTool != null) 
        {
            bool isActive = (dremelTool == activeTool);

            // Calling Equip/Unequip to find Dremel's script
            DremelToolController dtc = dremelTool.GetComponentInChildren<DremelToolController>(true);
            if (dtc == null)
            {
                dtc = GameObject.Find("DramelController-modern")?.GetComponent<DremelToolController>();
                if (dtc == null) dtc = GameObject.Find("DramelController-classic")?.GetComponent<DremelToolController>();
            }
            if (dtc != null)
            {
                if (isActive) dtc.EquipDremel();
                else dtc.UnequipDremel();
            }

            dremelTool.SetActive(isActive);
        }

        // 4. Chisel control
        if(chiselTool != null) 
        {
            bool isActive = (chiselTool == activeTool);

            // Legacy controller
            ChiselController cc = chiselTool.GetComponentInChildren<ChiselController>(true);
            if (cc != null) 
            {
                if (isActive) cc.EquipChisel();
                else cc.UnequipChisel();
            }

            // Modern controller (Rigged Chisel)
            ManualChiselController mcc = chiselTool.GetComponentInChildren<ManualChiselController>(true);
            if (mcc != null)
            {
                if (isActive) mcc.EquipChisel();
                else mcc.UnequipChisel();
            }

            // Classic controller (Rigged Classic Chisel)
            ClassicChiselController ccc = chiselTool.GetComponentInChildren<ClassicChiselController>(true);
            if (ccc != null)
            {
                if (isActive) ccc.EquipChisel();
                else ccc.UnequipChisel();
            }
            
            chiselTool.SetActive(isActive);
        }
    }
}