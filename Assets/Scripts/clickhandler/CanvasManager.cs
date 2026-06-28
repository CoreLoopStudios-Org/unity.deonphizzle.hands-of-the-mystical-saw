using UnityEngine;
using System.Collections.Generic;

public class CanvasManager : MonoBehaviour
{
    [System.Serializable]
    public class CanvasInfo
    {
        public string canvasName;   
        public GameObject canvasObject; 
    }

    [Header("All Game Canvases")]
    public List<CanvasInfo> allCanvases = new List<CanvasInfo>();

    void Start()
    {
        if (allCanvases.Count > 0)
        {
            SwitchCanvas(allCanvases[0].canvasName);
        }
    }
    
    public void SwitchCanvas(string targetCanvasName)
    {
        bool found = false;

        foreach (var info in allCanvases)
        {
            if (info.canvasName == targetCanvasName)
            {
                info.canvasObject.SetActive(true);
                found = true;
            }
            else
            {
                info.canvasObject.SetActive(false);
            }
        }

        if (!found)
        {
            Debug.LogWarning("Canvas with name " + targetCanvasName + " not found!");
        }
    }
}