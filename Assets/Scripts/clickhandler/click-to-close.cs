using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOutsideClose : MonoBehaviour, IPointerDownHandler
{
    void Update()
    {
        if (StoneSpinController.GlobalTorchActive) return;
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            gameObject.SetActive(false);
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
    }
}