using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AssetHolder : MonoBehaviour
{
    public static AssetHolder instance;
    
    public TextMeshProUGUI ddText;
    public Canvas canvas;
    public Camera PlayersCamera;
    public GameObject dragDisplayObject;

    public GameObject[] Skills;

    void Awake() {
        if (instance == null)
            instance = this;
    }
}
