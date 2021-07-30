using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotationPanel : MonoBehaviour, IDragHandler
{
    public Transform character;

    public void OnDrag(PointerEventData eventData)
    {
        character.Rotate(Vector3.up, -eventData.delta.x * 0.2f);
    }
}
