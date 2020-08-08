using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuildingTimelaps : MonoBehaviour
{
    public float placementSpeed;
    public bool start;

    [Space]
    public float progress;

    public List<Transform> objects = new List<Transform>();
    float timer;
    int index;
    // Start is called before the first frame update
    void OnEnable()
    {
        timer = 1/placementSpeed;

        objects.AddRange(transform.GetComponentsInChildren<Transform>());

        for (int i = 1; i < objects.Count; i++) {
            if (objects[i].GetComponent<BoatAlignNormal>() != null) {
                Transform[] boatObjects = objects[i].transform.GetComponentsInChildren<Transform>();
                for (int j = 1; j < boatObjects.Length; j++) {
                    objects.Remove(boatObjects[j]);
                }
            }
        }

        for (int i = 1; i < objects.Count; i++) {
            objects[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!start || index >= objects.Count)
            return;

        if (timer <= 0) {
            ActivateObject();
            timer = 1/placementSpeed;
        } else {
            timer -= Time.fixedDeltaTime;
        }

        progress = Mathf.Round(index*100/objects.Count)/100f;
    }

    void ActivateObject() {
        objects[index].gameObject.SetActive(true);
        index++;
    }
}
