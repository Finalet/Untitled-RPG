using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DevTools
{
    public static int getUniqueIndex (List<int> list, int maxIndexExclusive, int minIndexInclusive = 0) {
        int currentTry = 0;

        int ID = Random.Range(minIndexInclusive, maxIndexExclusive);
        while (list.Contains(ID)){
            ID = Random.Range(minIndexInclusive, maxIndexExclusive);
            currentTry++;
            if (currentTry >= 100000) {
                Debug.LogError($"Could not find unique ID in {currentTry} tries");
                return -1;
            }
        }
        list.Add(ID);
        return ID;
    }
}
