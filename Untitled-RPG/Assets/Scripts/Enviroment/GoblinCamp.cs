using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CampSize {Small, Medium, Large}

public class GoblinCamp : MonoBehaviour
{
    public CampSize campSize;
    public LayerMask floorMaks;
    
    [Header("Small objects")]
    public GameObject bonefire;
    public GameObject meatRotisser;
    
    [Header("Structures")]
    public GameObject tower;
    public GameObject shortTent;
    public GameObject tallTent;
    public GameObject roundTent;

    [Header("Items & Details")]
    public GameObject chest;

    float raycastDistance = 20;

    Vector3[] allowedStructureLocations;

    public void GenerateCamp () {
        ClearCurrentCamp();
        GeneratePositionsAroundCamp();

        PlaceCenterStructure();
        PlaceOutsideStructure();
    }

    void ClearCurrentCamp (){
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        for (int i = 0; i < allChildren.Length; i++) {
            if (allChildren[i] == transform)
                continue;
            DestroyImmediate(allChildren[i].gameObject);
        }
    }

    void PlaceCenterStructure() {
        switch (campSize) {
            case CampSize.Small:
                PlaceObject(bonefire, transform.position);
                PlaceObject(meatRotisser, transform.position);
                break;
            case CampSize.Medium:
                break;
            case CampSize.Large:
                break;
        }
    }

    void PlaceOutsideStructure() {
        List<int> usedInts = new List<int>();
        switch (campSize) {
            case CampSize.Small:
                PlaceObject(tower, transform.position + allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), true, true);
                PlaceObject(shortTent, transform.position + allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), false, true);
                PlaceObject(tallTent, transform.position + allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), true, true);
                PlaceObject(roundTent, transform.position + allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), false, true);
                break;
            case CampSize.Medium:
                break;
            case CampSize.Large:
                break;
        }
    }

    void PlaceObject (GameObject prefab, Vector3 initialPosition) {
        PlaceObject(prefab, initialPosition, Vector3.zero);
    }
    void PlaceObject (GameObject prefab, Vector3 initialPosition, Vector3 lookAtPos, bool invertForward = false, bool ignoreNormal = false) {
        Vector3 normal;
        Vector3 pos = getPositionBelow(initialPosition, out normal);
        Quaternion rot = lookAtPos == Vector3.zero ? getRandomRotation() : getLookAtRotation(lookAtPos, pos, invertForward);
        GameObject go = Instantiate(prefab, pos, rot, transform);
        if (!ignoreNormal) go.transform.rotation = getRotationAdjustedForNormal(go.transform, normal);
    }

    Vector3 getCampCenter () {
        Vector3 normal;
        return getPositionBelow(transform.position, out normal);
    }
    Vector3 getCampCenter (out Vector3 normal) {
        return getPositionBelow(transform.position, out normal);
    }

    Vector3 getPositionBelow (Vector3 initialPosition, out Vector3 normal) {
        RaycastHit hit;
        if (Physics.Raycast(initialPosition, Vector3.down, out hit, raycastDistance, floorMaks)) {
            normal = hit.normal;
            return hit.point;
        }
        normal = Vector3.up;
        return initialPosition;
    }

    Quaternion getRandomRotation (float maxRotationY = 360, float minRotationY = 0) {
        return Quaternion.Euler(0, Random.Range(minRotationY, maxRotationY), 0);
    }

    Quaternion getLookAtRotation (Vector3 lookAtPos, Vector3 objectPos, bool invertForward = false) {
        return Quaternion.LookRotation( invertForward ? (objectPos-lookAtPos) : (lookAtPos-objectPos) );
    }

    Quaternion getRotationAdjustedForNormal (Transform rotationTransform, Vector3 normal) {
        return Quaternion.FromToRotation (rotationTransform.up, normal) * rotationTransform.rotation;
    }

    void GeneratePositionsAroundCamp () {
        allowedStructureLocations = new Vector3[getNumberOfStructures()];
        float angle = 360 / getNumberOfStructures();
        float prevAngle = 0;
        for (int i = 0; i < allowedStructureLocations.Length; i++) {
            float newAngle = Random.Range( 0.5f * angle, 1.5f * angle) * Mathf.Deg2Rad;
            float rad = prevAngle + newAngle;
            prevAngle = rad;
            allowedStructureLocations[i] = new Vector3(Mathf.Sin( rad ), 0, Mathf.Cos( rad )) * getCampRadius(0.2f);
        }
    }

    Vector3 RandomPointInCircle(float radius, float angle){
     float rad = angle * Mathf.Deg2Rad;
     Vector3 position = new Vector3(Mathf.Sin( rad ), 0, Mathf.Cos( rad ));
     return position * radius;
    }

    Vector3 getRandomPositionOnCircle () {
        Vector3 randPos = (Random.insideUnitCircle).normalized * getCampRadius();
        return new Vector3(randPos.x, 0, randPos.y);
    }

    float getCampRadius (float approximate = 0) {
        switch (campSize) {
            case CampSize.Small:
                return 7 * Random.Range(1-approximate, 1+approximate);
            case CampSize.Medium:
                return 14 * Random.Range(1-approximate, 1+approximate);
            case CampSize.Large:
                return 21 * Random.Range(1-approximate, 1+approximate);
            default:
                return 7 * Random.Range(1-approximate, 1+approximate);
        }
    }

    int getNumberOfStructures() {
        switch (campSize) {
            case CampSize.Small:
                return 4;
            case CampSize.Medium:
                return 5;
            case CampSize.Large:
                return 6;
            default:
                return 7;
        }
    }

    int getUniqueIndex (List<int> list) {
        int ID = Random.Range(0, getNumberOfStructures());
        while (list.Contains(ID)){
            ID = Random.Range(0, getNumberOfStructures());
        }
        list.Add(ID);
        return ID;
    }

}
