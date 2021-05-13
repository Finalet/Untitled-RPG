using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CampSize {Small, Medium, Large}

public class GoblinCampGenerator : MonoBehaviour
{
    public CampSize campSize;
    public LayerMask floorMask;
    public ChestQuality chestQuality;
    [Header("Enemis")]
    public int numberOfGoblinWarriors;
    public int numberofGoblinArchers;
    public int numberOfBigGoblins;
    public int numberOfWolfs;
    
    [Header("Small objects")]
    public GameObject bonefire;
    public GameObject meatRotisser;
    
    [Header("Structures")]
    public GameObject tower;
    public GameObject shortTent;
    public GameObject tallTent;
    public GameObject roundTent;
    public GameObject ribCage;
    public GameObject[] boxes;

    [Header("Items & Details")]
    public GameObject chest;
    public GameObject[] bones;

    [Header("Mobs")]
    public GameObject goblinWarrior;
    public GameObject goblinArcher;
    public GameObject wolf;
    public GameObject bigGoblin;

    [Header("Spawned")]
    public GameObject spawnedTower;
    public GameObject spawnedShortTent;
    public GameObject spawnedTallTent;
    public GameObject spawnedRoundTent;
    public Transform details;
    public Transform enemies;

    float raycastDistance = 20;
    Vector3[] allowedStructureLocations;

    public void GenerateCamp () {
        ClearCurrentCamp();
        GeneratePositionsAroundCamp();

        PlaceCenterStructure();
        PlaceOutsideStructure();
        PlaceDetails();
    }

    public void BakeCamp () {
        GoblinCamp camp = Instantiate(gameObject).AddComponent<GoblinCamp>();
        camp.gameObject.name = "Goblin Camp Baked";

        GameObject[] allEnemies = getAllEnemies();
        foreach (GameObject enemy in allEnemies){
            if (enemy.GetComponent<Enemy>() is GoblinWarrior)
                camp.campEnemies.Add(new CampEnemyInfo(goblinWarrior, enemy.transform.position, enemy.transform.rotation));
            else if (enemy.GetComponent<Enemy>() is GoblinArcher)
                camp.campEnemies.Add(new CampEnemyInfo(goblinArcher, enemy.transform.position, enemy.transform.rotation));
            else if (enemy.GetComponent<Enemy>() is BigGoblin)
                camp.campEnemies.Add(new CampEnemyInfo(bigGoblin, enemy.transform.position, enemy.transform.rotation));
            else if (enemy.GetComponent<Enemy>() is Wolf)
                camp.campEnemies.Add(new CampEnemyInfo(wolf, enemy.transform.position, enemy.transform.rotation));
        }
        
        camp.spawnedEnemies.AddRange(allEnemies);
        
        DestroyImmediate(camp.GetComponent<GoblinCampGenerator>());
        ClearCurrentCamp();
    }

    void ClearCurrentCamp (){
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        for (int i = allChildren.Length-1; i >= 0; i--) {
            if (allChildren[i] == transform)
                continue;
            DestroyImmediate(allChildren[i].gameObject);
        }
    }
    void ClearCurrentDetails() {
        if (details == null)
            return;
        Transform[] allChildren = details.GetComponentsInChildren<Transform>();
        for (int i = allChildren.Length-1; i >= 0; i--) {
            DestroyImmediate(allChildren[i].gameObject);
        }
    }
    void ClearCurrentEnemies() {
        if (enemies == null)
            return;
        Transform[] allChildren = enemies.GetComponentsInChildren<Transform>();
        for (int i = allChildren.Length-1; i >= 0; i--) {
            DestroyImmediate(allChildren[i].gameObject);
        }
    }

    void PlaceCenterStructure() {
        switch (campSize) {
            case CampSize.Small:
                PlaceObject(bonefire);
                PlaceObject(meatRotisser);
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
                spawnedTower = PlaceObject(tower, allowedStructureLocations[getUniqueIndex(usedInts)], -getCampCenter(), true, Vector3.up);
                spawnedShortTent = PlaceObject(shortTent, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), false, Vector3.up);
                spawnedRoundTent = PlaceObject(roundTent, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), false, Vector3.up);
                spawnedTallTent = PlaceObject(tallTent, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), true, Vector3.up);
                GameObject spawnedRibCage = PlaceObject(ribCage, allowedStructureLocations[getUniqueIndex(usedInts)]);
                spawnedRibCage.transform.localScale = Vector3.one * 0.3f;

                Vector3 boxesPos = getRandomPositionAroundCamp(getCampRadius() * 1.5f, 0.15f);
                for (int i = 0; i < 2; i++) {
                    Vector3 pos = boxesPos + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                    PlaceObject(boxes[Random.Range(0, boxes.Length)], pos);
                }
                break;
            case CampSize.Medium:
                break;
            case CampSize.Large:
                break;
        }
    }

    public void PlaceEnemies () {
        ClearCurrentEnemies();

        enemies = new GameObject().transform;
        enemies.SetParent(transform);
        enemies.name = "Enemies";
        switch (campSize) {
            case CampSize.Small:
                if (numberofGoblinArchers > 0){
                    GameObject go = PlaceObject(goblinArcher, removeYposition(spawnedTower.transform.localPosition), -getCampCenter(), false, Vector3.up);
                    go.GetComponent<NavMeshAgent>().enabled = false;
                    go.transform.SetParent(enemies);
                }
                for (int i = 0; i < numberOfGoblinWarriors; i++) {
                    PlaceObject(goblinWarrior, getRandomPositionAroundCamp(2, 0.15f), getCampCenter()).transform.SetParent(enemies);
                }
                Vector3 wolfPackPos = getRandomPositionAroundCamp(10, 0.15f);
                for (int i = 0; i < numberOfWolfs; i++) {
                    Vector3 pos = wolfPackPos + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f))* numberOfWolfs;
                    PlaceObject(wolf, pos, getCampCenter()).transform.SetParent(enemies);
                }
                for (int i = 0; i < numberOfBigGoblins; i++) {
                    PlaceObject(bigGoblin, getRandomPositionAroundCamp(7, 0.15f), getCampCenter()).transform.SetParent(enemies);
                }
                for (int i = 0; i < numberofGoblinArchers-1; i++) {
                    PlaceObject(goblinArcher, getRandomPositionAroundCamp(3, 0.15f), getCampCenter()).transform.SetParent(enemies);
                }
                break;
            case CampSize.Medium:
                break;
            case CampSize.Large:
                break;
        }
    }
    public void PlaceDetails () {
        ClearCurrentDetails();

        details = new GameObject().transform;
        details.SetParent(transform);
        details.name = "Details";
        switch (campSize) {
            case CampSize.Small:
                Chest go = PlaceObject(chest, removeYposition(spawnedTallTent.transform.localPosition), getCampCenter()).GetComponent<Chest>();
                go.chestQuality = chestQuality;
                go.UpdateMesh();
                go.transform.SetParent(details);

                foreach (GameObject g in bones) {
                    PlaceObject(g, getRandomPosition()).transform.SetParent(details);
                }
                break;
            case CampSize.Medium:
                break;
            case CampSize.Large:
                break;
        }
    }

    GameObject PlaceObject (GameObject prefab) {
        return PlaceObject(prefab, Vector3.zero, Vector3.zero);
    }
    GameObject PlaceObject (GameObject prefab, Vector3 localPosition) {
        return PlaceObject(prefab, localPosition, Vector3.zero);
    }
    GameObject PlaceObject (GameObject prefab, Vector3 localPosition, Vector3 lookAt, bool invertForward = false, in Vector3 normal = new Vector3()) {
        Vector3 _normal;
        Vector3 pos = getPositionBelow(localPosition, out _normal);
        Quaternion rot = lookAt == Vector3.zero ? getRandomRotation() : getLookAtRotation(lookAt, pos, invertForward);
        GameObject go = Instantiate(prefab, pos, rot, transform);
        _normal = normal == Vector3.zero ? _normal : normal;
        go.transform.rotation = getRotationAdjustedForNormal(go.transform, _normal);
        return go;
    }

    Vector3 getCampCenter () {
        Vector3 normal;
        return getPositionBelow(Vector3.zero, out normal);
    }
    Vector3 getCampCenter (out Vector3 normal) {
        return getPositionBelow(Vector3.zero, out normal);
    }

    Vector3 getPositionBelow (Vector3 localPosition){
        Vector3 normal;
        return getPositionBelow(localPosition, out normal);
    }
    Vector3 getPositionBelow (Vector3 localPosition, out Vector3 normal) {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + localPosition, Vector3.down, out hit, raycastDistance, floorMask)) {
            normal = hit.normal;
            return hit.point;
        }
        normal = Vector3.up;
        return transform.position + localPosition;
    }

    Vector3 getRandomPosition () {
        Vector2 x = Random.insideUnitCircle;
        return  new Vector3 (x.x, 0, x.y) * getCampRadius();
    }
    Vector3 getRandomPositionAroundCamp (float radius, float randomness = 0) {
        float rad = Mathf.Deg2Rad * Random.Range(0, 360);
        return new Vector3(Mathf.Sin( rad ), 0, Mathf.Cos( rad )) * radius * (Random.Range(1-randomness, 1+randomness));
    }

    Vector3 removeYposition(Vector3 initialPosition){
        return new Vector3(initialPosition.x, 0, initialPosition.z);
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

    GameObject[] getAllEnemies() {
        GameObject[] allChildren = new GameObject[enemies.childCount];
        for (int i = 0; i < allChildren.Length; i++)
        {
            allChildren[i] = enemies.GetChild(i).gameObject;
        }
        return allChildren;
    }

    void GeneratePositionsAroundCamp (float randomness = 0.5f) {
        allowedStructureLocations = new Vector3[getNumberOfStructures()];
        float angle = 360 / getNumberOfStructures();
        for (int i = 0; i < allowedStructureLocations.Length; i++) {
            float rad = Mathf.Deg2Rad * (angle * i + Random.Range(-angle/2, angle/2) * randomness);
            allowedStructureLocations[i] = new Vector3(Mathf.Sin( rad ), 0, Mathf.Cos( rad )) * getCampRadius(0.2f);
        }
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
                return 5;
            case CampSize.Medium:
                return 6;
            case CampSize.Large:
                return 7;
            default:
                return 8;
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
