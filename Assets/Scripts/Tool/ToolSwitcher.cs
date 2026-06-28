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
        // গেম শুরুর সময় ডিফল্টভাবে হাতুড়ি সিলেক্ট করা থাকবে
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
        // ১. হাতুড়ি কন্ট্রোল
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

        // ২. করাত (Saw) কন্ট্রোল
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

        // ৩. 🌟 ড্রেমেল কন্ট্রোল (এখানে আপডেট করা হয়েছে)
        if(dremelTool != null) 
        {
            bool isActive = (dremelTool == activeTool);

            // ড্রেমেলের স্ক্রিপ্ট খুঁজে Equip/Unequip কল করা হচ্ছে
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

        // ৪. ছেনি (Chisel) কন্ট্রোল
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