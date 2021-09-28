using UnityEngine;
using UnityEngine.UI;

public class UI_SpritePanel : MonoBehaviour
{
    [Header("Setup")]
    public Image mainImage;

    public void Init(Sprite sprite) {
        mainImage.sprite = sprite;
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.OpenPage);
    }

    public void Close () {
        UIAudioManager.instance.PlayUISound(UIAudioManager.instance.ClosePage);
        Destroy(gameObject);
    }
}
