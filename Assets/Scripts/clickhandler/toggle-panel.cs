using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro use korle eta lagbe

public class DynamicMenuManager : MonoBehaviour
{
    [System.Serializable]
    public struct PanelMapping
    {
        public Button button;      
        public TextMeshProUGUI buttonText;
        public GameObject panel;   
        public string label;        
    }

    public PanelMapping[] panelSettings;

    void Start()
    {
        foreach (var mapping in panelSettings)
        {
            if (mapping.panel != null) mapping.panel.SetActive(false);
            
            if (mapping.buttonText != null && !string.IsNullOrEmpty(mapping.label))
            {
                mapping.buttonText.text = mapping.label;
            }
            
            if (mapping.button != null)
            {
                GameObject p = mapping.panel;
                mapping.button.onClick.AddListener(() => OpenPanel(p));
            }
        }
    }

    void OpenPanel(GameObject panelToOpen)
    {
        foreach (var item in panelSettings)
        {
            if (item.panel != null) item.panel.SetActive(false);
        }
        
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }
   
}