using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using UnityEngine;

public class VegetationItemPool {
    public virtual GameObject GetObject(ItemSelectorInstanceInfo info)
    {
        return null;
    }
    
    public virtual void ReturnObject(GameObject returnObject)
    {
       
    }
}
