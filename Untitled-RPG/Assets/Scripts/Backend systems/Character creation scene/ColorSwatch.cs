using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwatch : MonoBehaviour
{
    private Color _color;
    public Color color {
        get {return _color;}
        set { _color = value;  GetComponent<Image>().color = _color;}
    }
    public Image frame;
    
    public void Pick() {
        foreach (ColorSwatch s in transform.parent.GetComponentsInChildren<ColorSwatch>())
            s.Unpick();

        frame.enabled = true;
    }
    public void Unpick (){
        frame.enabled = false;
    }
}
