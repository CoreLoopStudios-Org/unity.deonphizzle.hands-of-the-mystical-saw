using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("--- UI Elements ---")]
    public Image backgroundImage;     
    public Image joystickKnobImage;

    [Header("--- Output ---")]
    [Tooltip("Final input vector (X ranges from -1 to 1, Y ranges from -1 to 1).")]
    public Vector2 InputVector;     
    private Vector2 pos;
    
    private void Start()
    {
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        if (joystickKnobImage == null) joystickKnobImage = transform.GetChild(0).GetComponent<Image>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        InputVector = Vector2.zero;
        joystickKnobImage.rectTransform.anchoredPosition = Vector2.zero;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            pos.x = (pos.x / backgroundImage.rectTransform.sizeDelta.x);
            pos.y = (pos.y / backgroundImage.rectTransform.sizeDelta.y);
            
            InputVector = new Vector2(pos.x * 2, pos.y * 2);
            
            if (InputVector.magnitude > 1.0f)
            {
                InputVector = InputVector.normalized;
            }
            
            joystickKnobImage.rectTransform.anchoredPosition = new Vector2(
                InputVector.x * (backgroundImage.rectTransform.sizeDelta.x / 2.5f),
                InputVector.y * (backgroundImage.rectTransform.sizeDelta.y / 2.5f));
        }
    }
}