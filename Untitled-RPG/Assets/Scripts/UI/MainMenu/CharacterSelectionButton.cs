using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSelectionButton : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    public Button deleteButton;

    int index;
    bool containsCharacter;
    MainMenuController mainMenuController;
    
    CanvasGroup canvasGroup;
    TextMeshProUGUI buttonLabel;

    public void Init(int _index, bool _containsCharacter, MainMenuController _mainMenuController) {
        canvasGroup = GetComponent<CanvasGroup>();
        buttonLabel = GetComponentInChildren<TextMeshProUGUI>();
        
        index = _index;
        containsCharacter = _containsCharacter;
        mainMenuController = _mainMenuController;

        if (!containsCharacter) {
            buttonLabel.text = "- Create new character -";
            canvasGroup.alpha = SaveManager.instance.allCharacters.Count > 0 ? 0.3f : 1f;
        } else {
            buttonLabel.text = SaveManager.instance.allCharacters[index];
            canvasGroup.alpha = 1;
            deleteButton.onClick.AddListener(OnDeleteCharacterButton);
        }

        deleteButton.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) {
            LeftMouseClick();
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            RightMouseClick();
        }
    }

    void LeftMouseClick () {
        deleteButton.gameObject.SetActive(false);
        canvasGroup.alpha *= 0.5f;
        if (!containsCharacter) mainMenuController.LoadCharacterCreation();
        else mainMenuController.LoadLevel("City", index);
    }
    void RightMouseClick () {
        if (containsCharacter) deleteButton.gameObject.SetActive(true);
    }
    
    void OnDeleteCharacterButton (){
        mainMenuController.OpenDeletionMenu(false, index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        deleteButton.gameObject.SetActive(false);
    }
}
