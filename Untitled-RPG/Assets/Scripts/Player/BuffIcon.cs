using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Buff buff;

    public float timer;
    public TextMeshProUGUI timerText;

    public GameObject toolTipPrefab;
    BuffTooltip currentTooltip;

    Image image;
    bool noTimer;
    void Start() {
        image = GetComponent<Image>();
        image.sprite = buff.icon;

        timer = buff.duration;
        
        if (timer == 0) {
            noTimer = true;
            timerText.text = "";
            image.color = Color.white;
        } else {
            image.color = Color.gray;
        }
    }
    void Update() {
        if (!noTimer) {
            if (timer <= 0) {
                RemoveBuff();
            } else {
                timer -= Time.deltaTime;
            }
            timerText.text = timer > 120 ? Mathf.RoundToInt(timer/60).ToString() + "m" : Mathf.RoundToInt(timer).ToString();
        }
    }

    void RemoveBuff () {
        Characteristics.instance.RemoveBuff(buff);
    }

    void ShowTooltip(){
        currentTooltip = Instantiate(toolTipPrefab, transform).GetComponent<BuffTooltip>();
        currentTooltip.Init(buff);
        currentTooltip.transform.SetParent(PeaceCanvas.instance.transform);
    }
    void HideTooltip (){
        Destroy(currentTooltip.gameObject);
    }

    public virtual void OnPointerEnter (PointerEventData eventData) {
        ShowTooltip();
    }
    public virtual void OnPointerExit (PointerEventData eventData) {
        HideTooltip();
    }
    public virtual void OnPointerClick (PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right){
            RemoveBuff();
        }
    }
}
