using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DeleteConfirmWindow : MonoBehaviour
{
    public Button confirmButton;
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI descriptionLabel;

    bool isProfile;
    int charIndex;
    MainMenuController mainMenuController;

    string profileDeleteWarning = "All profile data like your game progress, characters, and settings will be lost. Are you sure you want to proceed?";
    string characterDeleteWarning = "All your character progress will be lost. Are you sure you want to proceed?";

    public void Init (bool _isProfile, MainMenuController _mainMenuController, int _charIndex) {
        isProfile = _isProfile;
        mainMenuController = _mainMenuController;
        charIndex = _charIndex;

        string name = isProfile ? SaveManager.instance.currentProfile : SaveManager.instance.allCharacters[charIndex];
        titleLabel.text = $"<color=red>Delete <color=#DEBE94><b>{name}</color>?";
        descriptionLabel.text = isProfile ? profileDeleteWarning : characterDeleteWarning;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmButton);

        gameObject.SetActive(true);
    }

    void OnConfirmButton () {
        if (isProfile) {
            mainMenuController.DeleteCurrentProfile();
        } else {
            mainMenuController.DeleteCharacter(charIndex);
        }
    }
}
