using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureMap : Resource
{
    [Header("Treasure Map")]
    public Sprite mapSprite;

    public override void Use()
    {
        base.Use();

        PeaceCanvas.instance.OpenSpritePanel(mapSprite);
    }
}
