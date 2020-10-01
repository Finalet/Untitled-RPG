using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
    public override void Use (){}
    public override void Use (UI_InventorySlot initialSlot){}
    public override IEnumerator UseEnum (){
        yield return null;
    }
}
