using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MainMenuController : MonoBehaviour, ISavable
{
    public Image blackout;
    public TMP_Dropdown profilesDropdown;
    public SettingsManager settingsManager;

    [Space]
    public Sprite plusIcon;
    public GameObject profileCreationWindow;
    public TMP_InputField profileCreation_profileName;
    public TextMeshProUGUI profileCreation_errorLabel;
    public Button profileCreation_createButotn;
    [Space]
    public GameObject profileDeleteMenu;
    public TextMeshProUGUI profileDeleteMenu_profileNameLabel;
    [Space]
    public Button[] characterButtons;


    // Start is called before the first frame update
    void Awake() {
        if (SceneManager.GetActiveScene().name != "SceneManagement") {
            AsyncOperation addLoadingScene = SceneManager.LoadSceneAsync("SceneManagement", LoadSceneMode.Additive);
        }
    }
    void Start() {
        blackout.color = Color.black;
        blackout.DOFade(0, 1).SetDelay(1);
        SaveManager.instance.saveObjects.Add(this);
        SaveManager.instance.saveObjects.Add(settingsManager);
    }

    void InitProfilesDropdown(){
        profilesDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();
        foreach (string s in SaveManager.instance.allProfiles) {
            dropdownOptions.Add(new TMP_Dropdown.OptionData(s));
        }
        
        dropdownOptions.Add(new TMP_Dropdown.OptionData("New Profile", plusIcon));
        profilesDropdown.AddOptions(dropdownOptions);

        if (profilesDropdown.options.Count == 1) OpenProfileCreation();
        else profilesDropdown.value = SaveManager.instance.currentProfileIndex;
    }

    public void OnDropdownValueChange() {
        if (profilesDropdown.value == profilesDropdown.options.Count - 1) {
            OpenProfileCreation();
        } else {
            SwitchSelectedProfile();
        }
    }
    public void OnProfileCreationInputFieldValueChange() {
        profileCreation_errorLabel.text = "";

        if (profileCreation_profileName.text.Length < 1) {
            profileCreation_createButotn.interactable = false;
            return;
        }
        string textToCompare = profileCreation_profileName.text;
        while (textToCompare[textToCompare.Length-1] == ' ') {
            textToCompare = textToCompare.Substring(0, textToCompare.Length - 1);
        }
        for (int i = 0; i < SaveManager.instance.allProfiles.Count; i++) {
            if (textToCompare.ToLower() == SaveManager.instance.allProfiles[i].ToLower()){
                profileCreation_createButotn.interactable = false;
                profileCreation_errorLabel.text = $"Profile with name \"{textToCompare}\" already exists.";
                return;
            }
        }
        profileCreation_createButotn.interactable = true;
    }

    void OpenProfileCreation (){
        profileCreation_profileName.text = "";
        profileCreation_createButotn.interactable = false;
        profileCreationWindow.SetActive(true);
    }
    void CloseProfileCreation () {
        profileCreationWindow.SetActive(false);
        SaveManager.instance.SaveProfiles();
    }
    
    public void OpenProfileDelete() {
        profileDeleteMenu_profileNameLabel.text = $"<color=red>Delete <color=#DEBE94><b>{SaveManager.instance.currentProfile}</color>?";
        profileDeleteMenu.SetActive(true);
    }

    public void CreateNewProfile () {
        string inputedProfileName = profileCreation_profileName.text;
        while (profileCreation_profileName.text[profileCreation_profileName.text.Length-1] == ' ') {
            profileCreation_profileName.text = profileCreation_profileName.text.Substring(0, profileCreation_profileName.text.Length - 1);
        }
        if (inputedProfileName != profileCreation_profileName.text) print($"Fixed profile name from \"{inputedProfileName}\" to \"{profileCreation_profileName.text}\")");

        SaveManager.instance.allProfiles.Add(profileCreation_profileName.text);
        SaveManager.instance.currentProfileIndex = SaveManager.instance.allProfiles.Count-1;
        InitProfilesDropdown();
        settingsManager.Load();
        CloseProfileCreation();
    }
    public void DeleteCurrentProfile (){
        ES3.DeleteDirectory(SaveManager.instance.getCurrentProfileFolderPath());
        
        SaveManager.instance.allProfiles.RemoveAt(SaveManager.instance.currentProfileIndex);
        SaveManager.instance.currentProfileIndex --;
        SaveManager.instance.currentProfileIndex = Mathf.Clamp(SaveManager.instance.currentProfileIndex, 0, SaveManager.instance.allProfiles.Count-1);
        InitProfilesDropdown();
        if (profileDeleteMenu.activeInHierarchy) profileDeleteMenu.SetActive(false);
        SaveManager.instance.SaveProfiles();
    }
    void SwitchSelectedProfile () {
        SaveManager.instance.currentProfileIndex = profilesDropdown.value;
        settingsManager.Load();
        InitCharacterButtons();
    }
    
    void InitCharacterButtons () {
        
        for (int i = 0; i < characterButtons.Length; i ++) {
            characterButtons[i].onClick.RemoveAllListeners();
            bool containsCharacter = SaveManager.instance.allCharacters.Count > i;
            if (!containsCharacter) {
                characterButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "- Create new character -";
                characterButtons[i].GetComponent<CanvasGroup>().alpha = 0.3f;
                characterButtons[i].onClick.AddListener(delegate{LoadCharacterCreation();});
                continue;
            }
            int charIndex = i;
            characterButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = SaveManager.instance.allCharacters[i];
            characterButtons[i].GetComponent<CanvasGroup>().alpha = 1;
            characterButtons[i].onClick.AddListener(delegate{LoadLevel("City", charIndex);});
        }
    }

    public void LoadCharacterCreation () {
        StartCoroutine(loadCharacterCreatio());
    }
    IEnumerator loadCharacterCreatio () {
        blackout.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        ScenesManagement.instance.LoadCharacterCreation();
    }

#region Loading

    public void LoadLevel (string levelName, int characterIndex) {
        SaveManager.instance.currentCharacterIndex = characterIndex;
        StartCoroutine(loadLevel(levelName));
    }
    IEnumerator loadLevel (string levelName) {
        blackout.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        ScenesManagement.instance.LoadLevel(levelName);
    }

    public void ExitGame () {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

#endregion

#region ISavable

    public LoadPriority loadPriority {
        get {return LoadPriority.First;}
    }

    public void Save()
    {
        //
    }

    public void Load()
    {
        InitProfilesDropdown();
        InitCharacterButtons();
    }
#endregion
}
