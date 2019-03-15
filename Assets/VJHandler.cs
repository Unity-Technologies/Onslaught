using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class VJHandler : MonoBehaviour,IDragHandler, IPointerUpHandler, IPointerDownHandler {
    private Image jsContainer;
    private Image joystick;
    
    private Vector2 InputDirection;
    public InputAbstraction inputAbstraction;
    
    void Start(){
        
        jsContainer = GetComponent<Image>();
        joystick = transform.GetChild(0).GetComponent<Image>(); //this command is used because there is only one child in hierarchy
        InputDirection = Vector2.zero;
    }
    
    public void OnDrag(PointerEventData ped){
        Vector2 position = Vector2.zero;
        
        //To get InputDirection
        RectTransformUtility.ScreenPointToLocalPointInRectangle
                (jsContainer.rectTransform, 
                ped.position,
                ped.pressEventCamera,
                out position);
            
        position.x = (position.x/jsContainer.rectTransform.sizeDelta.x);
        position.y = (position.y/jsContainer.rectTransform.sizeDelta.y);

        InputDirection = position;
        InputDirection = (InputDirection.magnitude > 1) ? InputDirection.normalized : InputDirection;
            
        //to define the area in which joystick can move around
        joystick.rectTransform.anchoredPosition = new Vector2 (InputDirection.x * jsContainer.rectTransform.sizeDelta.x
                                                                ,InputDirection.y * jsContainer.rectTransform.sizeDelta.y);
            
        if (GameManager.instance != null)
            GameManager.instance.inputAbstraction.ARSetFireDirection(InputDirection);
    }
    
    public void OnPointerDown(PointerEventData ped){
        
        OnDrag(ped);
        if (GameManager.instance != null)
        {
            GameManager.instance.inputAbstraction.ARSetFireDirection(InputDirection);
            GameManager.instance.inputAbstraction.ARSetFire();
        }
    }
    
    public void OnPointerUp(PointerEventData ped){
        
        InputDirection = Vector2.zero;
        joystick.rectTransform.anchoredPosition = Vector2.zero;
        if (GameManager.instance != null)
        {
            GameManager.instance.inputAbstraction.ARSetFireDirection(InputDirection);
            GameManager.instance.inputAbstraction.ARUnsetFire();
        }
    }
}