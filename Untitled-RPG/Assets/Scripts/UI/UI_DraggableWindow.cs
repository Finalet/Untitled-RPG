using UnityEngine.EventSystems;
using UnityEngine;

public class UI_DraggableWindow : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData) {
        transform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0);
    }
}