using DG.Tweening;
using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    private bool _isOpen;
    public bool isOpen {
        set {
            _isOpen = value;
            if (_isOpen) OpenDoor();
            else CloseDoor();
        }
        get { return _isOpen; }
    }

    BoxCollider boxCollider;
    MeshRenderer meshRenderer;

    void Awake() {
        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material = new Material(meshRenderer.material);
        meshRenderer.material.name = "Runtime material";
    }

    void OpenDoor() {
        boxCollider.enabled = false;
        meshRenderer.material.DOFloat(0, "Progress", 1);
    }
    void CloseDoor () {
        boxCollider.enabled = true;
        meshRenderer.material.DOFloat(1, "Progress", 1);
    }
}
