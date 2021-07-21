using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CampSize {Small, Medium, Large}
public enum CampType {Regular, Skull}

public class GoblinCampGenerator : MonoBehaviour
{
    public CampType campType;
    public CampSize campSize;
    public ChestQuality chestQuality;
    public LayerMask floorMask;
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
    public GameObject largeStoneSkull;
    public GameObject[] largeDecorations;
    public GameObject[] boxes;
    public GameObject[] spikes;

    [Header("Items & Details")]
    public GameObject chest;
    public GameObject[] bones;

    [Header("Mobs")]
    public GameObject goblinWarrior;
    public GameObject goblinArcher;
    public GameObject wolf;
    public GameObject bigGoblin;

    [Header("Spawned")]
    GameObject spawnedTower;
    GameObject spawnedTallTent;
    GameObject spawnedLargeStoneSkull;
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
        
        camp.enemies = camp.transform.Find("Enemies");
        GameObject[] allChildren = new GameObject[camp.enemies.childCount];
        for (int i = 0; i < allChildren.Length; i++)
        {
            allChildren[i] = camp.enemies.GetChild(i).gameObject;
        }
        camp.spawnedEnemies.AddRange(allChildren);
        DestroyImmediate(camp.GetComponent<GoblinCampGenerator>());

        NavMeshSurface surface = camp.gameObject.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Volume;
        surface.agentTypeID = -1372625422; // found by switching inspector to Debug mode;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = LayerMask.GetMask("Default", "StaticLevel", "Terrain");
        surface.size = new Vector3(120, 40, 120);
        surface.center = Vector3.up*2;
        surface.BuildNavMesh();
        
        ClearCurrentCamp();
        DestroyImmediate(gameObject);
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
        if (campType == CampType.Regular) {
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
        } else if (campType == CampType.Skull) {
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
    }
    void PlaceOutsideStructure() {
        List<int> usedInts = new List<int>();
        if (campType == CampType.Regular) {
            switch (campSize) {
                case CampSize.Small:
                    spawnedTower = PlaceObject(tower, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), false, Vector3.up);
                    PlaceObject(shortTent, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), false, Vector3.up);
                    PlaceObject(roundTent, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), false, Vector3.up);
                    spawnedTallTent = PlaceObject(tallTent, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), true, Vector3.up);
                    GameObject spawnedlargeDecoration = PlaceObject(largeDecorations[Random.Range(0, largeDecorations.Length-1)], allowedStructureLocations[getUniqueIndex(usedInts)]);
                    spawnedlargeDecoration.transform.localScale = Vector3.one * 0.3f;

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
        } else if (campType == CampType.Skull) {
            switch (campSize) {
                case CampSize.Small:
                    spawnedLargeStoneSkull = PlaceObject(largeStoneSkull, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter());
                    spawnedLargeStoneSkull.transform.position += -spawnedLargeStoneSkull.transform.forward * 5;
                    PlaceObject(spikes[Random.Range(0, spikes.Length-1)], allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter());
                    GameObject spike1 = PlaceObject(spikes[Random.Range(0, spikes.Length-1)], allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter());
                    PlaceObject(spikes[Random.Range(0, spikes.Length-1)], spike1.transform.localPosition + spike1.transform.right * 1.5f, getCampCenter());

                    spawnedTower = PlaceObject(tower, allowedStructureLocations[getUniqueIndex(usedInts)], getCampCenter(), false, Vector3.up);
                    break;
                case CampSize.Medium:
                    break;
                case CampSize.Large:
                    break;
            }
        }
    }

    public void PlaceEnemies () {
        ClearCurrentEnemies();

        enemies = new GameObject().transform;
        enemies.SetParent(transform);
        enemies.name = "Enemies";
        if (campType == CampType.Regular) {
            switch (campSize) {
                case CampSize.Small:
                    if (numberofGoblinArchers > 0 && spawnedTower){
                        GameObject go = PlaceObject(goblinArcher, spawnedTower.transform.localPosition + Vector3.up * 10, -getCampCenter(), false, Vector3.up);
                        go.GetComponent<NavMeshAgent>().enabled = false;
                        go.transform.SetParent(enemies);
                    }
                    for (int i = 0; i < numberOfGoblinWarriors; i++) {
                        PlaceObject(goblinWarrior, getRandomPositionAroundCamp(2, 0.15f), getCampCenter(), false, Vector3.up).transform.SetParent(enemies);
                    }
                    Vector3 wolfPackPos = getRandomPositionAroundCamp(10, 0.15f);
                    for (int i = 0; i < numberOfWolfs; i++) {
                        Vector3 pos = wolfPackPos + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f))* numberOfWolfs;
                        PlaceObject(wolf, pos, getCampCenter(), false, Vector3.up).transform.SetParent(enemies);
                    }
                    for (int i = 0; i < numberOfBigGoblins; i++) {
                        PlaceObject(bigGoblin, getRandomPositionAroundCamp(7, 0.15f), getCampCenter(), false, Vector3.up).transform.SetParent(enemies);
                    }
                    for (int i = 0; i < numberofGoblinArchers-1; i++) {
                        PlaceObject(goblinArcher, getRandomPositionAroundCamp(3, 0.15f), getCampCenter(), false, Vector3.up).transform.SetParent(enemies);
                    }
                    break;
                case CampSize.Medium:
                    break;
                case CampSize.Large:
                    break;
            }
        } else if (campType == CampType.Skull) {
            switch (campSize) {
                case CampSize.Small:
                    if (numberofGoblinArchers > 0 && spawnedTower){
                        GameObject go = PlaceObject(goblinArcher, spawnedTower.transform.localPosition + Vector3.up * 10, -getCampCenter(), false, Vector3.up);
                        go.GetComponent<NavMeshAgent>().enabled = false;
                        go.transform.SetParent(enemies);
                    }
                    for (int i = 0; i < numberOfGoblinWarriors; i++) {
                        PlaceObject(goblinWarrior, getRandomPositionAroundCamp(2, 0.15f), getCampCenter(), false, Vector3.up).transform.SetParent(enemies);
                    }
                    Vector3 wolfPackPos = getRandomPositionAroundCamp(10, 0.15f);
                    for (int i = 0; i < numberOfWolfs; i++) {
                        Vector3 pos = wolfPackPos + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f))* numberOfWolfs;
                        PlaceObject(wolf, pos, getCampCenter(), false, Vector3.up).transform.SetParent(enemies);
                    }
                    for (int i = 0; i < numberOfBigGoblins; i++) {
                        PlaceObject(bigGoblin, getRandomPositionAroundCamp(7, 0.15f), getCampCenter(), false, Vector3.up).transform.SetParent(enemies);
                    }
                    for (int i = 0; i < numberofGoblinArchers-1; i++) {
                        PlaceObject(goblinArcher, getRandomPositionAroundCamp(3, 0.15f), getCampCenter(), false, Vector3.up).transform.SetParent(enemies);
                    }
                    break;
                case CampSize.Medium:
                    break;
                case CampSize.Large:
                    break;
            }
        }
    }
    public void PlaceDetails () {
        ClearCurrentDetails();

        details = new GameObject().transform;
        details.SetParent(transform);
        details.name = "Details";
        if (campType == CampType.Regular) {
            switch (campSize) {
                case CampSize.Small:
                    PlaceChest(chestQuality, removeYposition(spawnedTallTent.transform.localPosition), getCampCenter());

                    foreach (GameObject g in bones) {
                        PlaceObject(g, getRandomPosition()).transform.SetParent(details);
                    }
                    break;
                case CampSize.Medium:
                    break;
                case CampSize.Large:
                    break;
            }
        } else if (campType == CampType.Skull) {
            PlaceChest(chestQuality, removeYposition(spawnedLargeStoneSkull.transform.localPosition + spawnedLargeStoneSkull.transform.forward*1.5f), getCampCenter());
            foreach (GameObject g in bones) {
                PlaceObject(g, getRandomPosition()).transform.SetParent(details);
            }
        }
    }

    Chest PlaceChest (ChestQuality chestQuality, Vector3 localPosition, Vector3 lookAt, bool invertForward = false, in Vector3 normal = new Vector3()) {
        Chest go = PlaceObject(chest, localPosition, lookAt, invertForward, normal).GetComponent<Chest>();
        go.chestQuality = chestQuality;
        go.UpdateMesh();
        go.transform.SetParent(details);
        return go;
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

    void GeneratePositionsAroundCamp () {
        float randomness = campType == CampType.Regular ? 0.5f : 0.5f;

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
                return 9 * Random.Range(1-approximate, 1+approximate);
            case CampSize.Medium:
                return 18 * Random.Range(1-approximate, 1+approximate);
            case CampSize.Large:
                return 27 * Random.Range(1-approximate, 1+approximate);
            default:
                return 9 * Random.Range(1-approximate, 1+approximate);
        }
    }

    int getNumberOfStructures() {
        if (campType == CampType.Regular) {
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
        } else if (campType == CampType.Skull) {
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
        } else {
            throw new System.Exception("Wrong camp type or size");
        }
    }

    int getUniqueIndex (List<int> list) {
        int ID = Random.Range(0, getNumberOfStructures());
        int numberOfTries = 0; 
        while (list.Contains(ID)){
            ID = Random.Range(0, getNumberOfStructures());
            numberOfTries++;
            if (numberOfTries >= 10000) throw new System.Exception($"Failed to find unique index in {numberOfTries} tries.");
        }
        list.Add(ID);
        return ID;
    }

}
