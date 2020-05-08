using UnityEngine.EventSystems;
using UnityEngine;

public class UI_DraggableWindow : MonoBehaviour, IDragHandler
{
    Vector3 drag;

    float oldX;
    float oldY;
    float deltaX;
    float deltaY;
    void Update() {
        deltaX = Input.mousePosition.x - oldX;
        deltaY = Input.mousePosition.y - oldY;

        oldX = Input.mousePosition.x;
        oldY = Input.mousePosition.y;
    }

    public void OnDrag(PointerEventData eventData) {
        drag = new Vector3(deltaX, deltaY, 0);
        transform.position += drag;
    }
}
