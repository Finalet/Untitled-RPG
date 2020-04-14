using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AssetHolder : MonoBehaviour
{
    public static AssetHolder instance;
    
    public TextMeshProUGUI ddText;
    public Canvas canvas;

    void Awake() {
        if (instance == null)
            instance = this;
    }
}
