using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECM.Controllers;

[System.Serializable]
public class BodyOnDeck {
    public Transform transform;
    public float timeLeftDeck;

    public BodyOnDeck(Transform transform, float timeLeftDeck)
    {
        this.transform = transform;
        this.timeLeftDeck = timeLeftDeck;
    }
}

public class DeckCollider : MonoBehaviour
{
    Transform sourceDeck;
    public ShipController sourceShip;

    float unparentFromDeckDelay = 0.6f;
    public List<BodyOnDeck> bodiesOnDeck = new List<BodyOnDeck>();

    public void Init (ShipController _sourceShip) {
        sourceDeck = _sourceShip.deckMesh.transform;
        sourceShip = _sourceShip;

        gameObject.layer = LayerMask.NameToLayer("ShipPlayerCollider");
        transform.position = _sourceShip.deckMesh.transform.position;
        transform.rotation = _sourceShip.deckMesh.transform.rotation;
        gameObject.AddComponent<MeshCollider>().sharedMesh = _sourceShip.deckMesh.GetComponent<MeshFilter>().mesh;
        gameObject.name = $"{_sourceShip.shipName} Deck Collider";
    }

    void FixedUpdate() {
        SyncDeckCollider();
        RunUnparentQueue();
    }

    void SyncDeckCollider () {
        transform.position = sourceDeck.position;
        transform.rotation = sourceDeck.rotation;
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.TryGetComponent(out Rigidbody rb)) {
            AddBodyToDeck(other.transform);
        }
    }
    void OnCollisionExit(Collision other) {
        if (other.gameObject.TryGetComponent(out Rigidbody rb)) {
            RemoveBodyFromDeck(other.transform);
        }
    }

    void RunUnparentQueue() {
        for (int i = bodiesOnDeck.Count - 1; i >= 0; i--) {
            if (bodiesOnDeck[i].timeLeftDeck > 0 && Time.time - bodiesOnDeck[i].timeLeftDeck > unparentFromDeckDelay) {
                bodiesOnDeck[i].transform.SetParent(null);
                bodiesOnDeck.RemoveAt(i);
            }
        }
    }

    void AddBodyToDeck(Transform transformToAdd) {
        for (int i = 0; i < bodiesOnDeck.Count; i++) {
            if (bodiesOnDeck[i].transform == transformToAdd) {
                bodiesOnDeck[i].timeLeftDeck = -1;
                return;
            }
        }

        bodiesOnDeck.Add(new BodyOnDeck(transformToAdd, -1));
        transformToAdd.SetParent(transform);
    }
    void RemoveBodyFromDeck(Transform transformToRemove) {
        for (int i = 0; i < bodiesOnDeck.Count; i++) {
            if (bodiesOnDeck[i].transform == transformToRemove) {
                bodiesOnDeck[i].timeLeftDeck = Time.time;
                return;
            }
        }
            
        bodiesOnDeck.Add(new BodyOnDeck(transformToRemove, Time.time));
    }
}
