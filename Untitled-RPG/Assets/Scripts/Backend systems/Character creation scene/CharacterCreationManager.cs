using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeStudios.ModularCharacters;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterCreationManager : MonoBehaviour
{
    public ModularCharacterManager modularCharacterManager;
    public Material defaultCharacterMat;
    public GameObject colorPickerPrefab;
    public Canvas canvas;
    Material newCharacterMat;

    [Space]
    public Sprite customColorSprite;
    public TMP_InputField nameInputField;
    public CanvasGroup maleButton;
    public CanvasGroup femaleButton;
    public TextMeshProUGUI headLabel;
    public TextMeshProUGUI hairLabel;
    public TextMeshProUGUI facialhairLabel;
    public TextMeshProUGUI eyebrowLabel;
    public TextMeshProUGUI earsLabel;
    public Transform skinColorGrid;
    public Transform hairColorGrid;
    public Transform eyesColorGrid;
    public Transform stubbleColorGrid;
    public Transform bodyArtColorGrid;
    public Transform scarColorGrid;
    [Space]
    public CanvasGroup confirmMenu;
    public TextMeshProUGUI confirmTitle;

    [Space]
    public Button[] headButtons;
    public Button[] hairButtons;
    public Button[] facialHairButtons;
    public Button[] eyebrowsButtons;
    public Button[] earsButtons;

    Dictionary<ModularBodyPart, GameObject[]> characterBody;
    Animator animator;

    private int headCurrentIndex;
    private int headMaxIndex;

    private int hairCurrentIndex;
    private int hairMaxIndex;

    private int eyebrowCurrentIndex;
    private int eyebrowMaxIndex;

    private int earCurrentIndex;
    private int earMaxIndex;

    private int facialHairCurrentIndex;
    private int facialHairMaxIndex;

    Color[] skinColors;
    Color[] hairColors;
    Color[] eyesColors;
    Color[] stubbleColors;
    Color[] bodyArtColors;
    Color[] scarColors;

    GameObject instanciatedColorPicker;

    void Awake() {
        animator = modularCharacterManager.GetComponent<Animator>();
        
        StartCoroutine(PickRandomIdleAnimation());

        InitSkinColors();
        InitHairColors();
        InitEyesColors();
        InitStubbleColors();
        InitBodyArtColors();
        InitScarColors();
        Init();
    }

    void Init() {
        newCharacterMat = new Material(defaultCharacterMat);
        newCharacterMat.name = "New material";
        modularCharacterManager.SetupNewCharacter(Gender.Male, newCharacterMat);
        modularCharacterManager.SetAllPartsMaterial(newCharacterMat);

        SetupBodyPart(ModularBodyPart.Head, ref headCurrentIndex, ref headMaxIndex);
        SetupBodyPart(ModularBodyPart.Hair, ref hairCurrentIndex, ref hairMaxIndex);
        SetupBodyPart(ModularBodyPart.Eyebrow, ref eyebrowCurrentIndex, ref eyebrowMaxIndex);
        SetupBodyPart(ModularBodyPart.Ear, ref earCurrentIndex, ref earMaxIndex);
        SetupBodyPart(ModularBodyPart.FacialHair, ref facialHairCurrentIndex, ref facialHairMaxIndex);

        UpdateLabels();
    }

    public void OnGenderSelect (bool isMale) {
        modularCharacterManager.SwapGender(isMale ? Gender.Male : Gender.Female);

        if (isMale) {
            maleButton.alpha = 1;
            femaleButton.alpha = 0.4f;
        } else {
            maleButton.alpha = 0.4f;
            femaleButton.alpha = 1;
        }

        SetupBodyPart(ModularBodyPart.Eyebrow, ref eyebrowCurrentIndex, ref eyebrowMaxIndex);
        eyebrowCurrentIndex = -1;
        UpdateLabels();
    }

    public void OnHeadSelect (bool plus) {
        headCurrentIndex = Mathf.Clamp(headCurrentIndex + (plus ? 1 : -1), 0, headMaxIndex);
        SetupPartSlider(ModularBodyPart.Head, headCurrentIndex);
        UpdateLabels();
    }
    public void OnEarSelect (bool plus) {
        earCurrentIndex = Mathf.Clamp(earCurrentIndex + (plus ? 1 : -1), -1, earMaxIndex);
        SetupPartSlider(ModularBodyPart.Ear, earCurrentIndex);
        UpdateLabels();
    }
    public void OnHairSelect (bool plus) {
        hairCurrentIndex = Mathf.Clamp(hairCurrentIndex + (plus ? 1 : -1), -1, hairMaxIndex);
        SetupPartSlider(ModularBodyPart.Hair, hairCurrentIndex);
        UpdateLabels();
    }
    public void OnFacialHairSelect (bool plus) {
        facialHairCurrentIndex = Mathf.Clamp(facialHairCurrentIndex + (plus ? 1 : -1), -1, facialHairMaxIndex);
        SetupPartSlider(ModularBodyPart.FacialHair, facialHairCurrentIndex);
        UpdateLabels();
    }
    public void OnEyebrowSelect (bool plus) {
        eyebrowCurrentIndex = Mathf.Clamp(eyebrowCurrentIndex + (plus ? 1 : -1), -1, eyebrowMaxIndex);
        SetupPartSlider(ModularBodyPart.Eyebrow, eyebrowCurrentIndex);
        UpdateLabels();
    }

    public void OnColorSelect (ColorSwatch swatch, Color selectedColor, string shaderProperty) {
        swatch.Pick();
        newCharacterMat.SetColor(shaderProperty, selectedColor);

        if (instanciatedColorPicker) Destroy(instanciatedColorPicker);
    }
    public void OnCustomColorSelect (ColorSwatch swatch, GameObject result, string shaderProperty, bool leftSidedModal = false) {        
        if (instanciatedColorPicker) Destroy(instanciatedColorPicker);
        instanciatedColorPicker = Instantiate(colorPickerPrefab, canvas.transform);

        CUIColorPicker colorPicker = instanciatedColorPicker.GetComponent<CUIColorPicker>();
        RectTransform resultRect = result.GetComponent<RectTransform>();

        RectTransform rect = colorPicker.GetComponent<RectTransform>();
        rect.position = resultRect.position;
        rect.anchoredPosition += resultRect.sizeDelta * Vector2.right;
        
        colorPicker.result = result;
        colorPicker.Color = newCharacterMat.GetColor(shaderProperty);
        colorPicker.confirmButton.onClick.AddListener(delegate{OnColorSelect(swatch, colorPicker.Color, shaderProperty);});

        if (leftSidedModal) {
            rect.anchoredPosition -= resultRect.sizeDelta * Vector2.right * 3;
            rect.pivot = Vector2.right;
        }
    }

    void UpdateLabels () {
        headLabel.text = $"Face: {headCurrentIndex + 1}";
        earsLabel.text = $"Ears style: {earCurrentIndex + 2}";
        hairLabel.text = $"Hair style: {(hairCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (hairCurrentIndex+1).ToString())}";
        facialhairLabel.text = $"Facial hair style: {(facialHairCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (facialHairCurrentIndex+1).ToString())}";
        eyebrowLabel.text = $"Eyebrows style: {(eyebrowCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (eyebrowCurrentIndex+1).ToString())}";
    
        headButtons[0].interactable = headCurrentIndex == 0 ? false : true;
        headButtons[1].interactable = headCurrentIndex == headMaxIndex ? false : true;

        earsButtons[0].interactable = earCurrentIndex == -1 ? false : true;
        earsButtons[1].interactable = earCurrentIndex == earMaxIndex ? false : true;

        hairButtons[0].interactable = hairCurrentIndex == -1 ? false : true;
        hairButtons[1].interactable = hairCurrentIndex == hairMaxIndex ? false : true;

        facialHairButtons[0].interactable = facialHairCurrentIndex == -1 ? false : true;
        facialHairButtons[1].interactable = facialHairCurrentIndex == facialHairMaxIndex ? false : true;

        eyebrowsButtons[0].interactable = eyebrowCurrentIndex == -1 ? false : true;
        eyebrowsButtons[1].interactable = eyebrowCurrentIndex == eyebrowMaxIndex ? false : true;
    }

    void InitSkinColors () {
        skinColors = new Color[] {
            //Regular skin
            new Color(255f/255f, 204f/255f, 174f/255f),
            new Color(243f/255f, 215f/255f, 201f/255f),
            new Color(234f/255f, 183f/255f, 138f/255f),
            new Color(211f/255f, 142f/255f, 111f/255f),
            new Color(197f/255f, 132f/255f, 92f/255f),
            new Color(88f/255f, 59f/255f, 43f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(0, 0, 0),
            new Color(255f/255f, 255f/255f, 255f/255f),

            //Fantasy skin
            new Color(233f/255f, 208f/255f, 178f/255f),
            new Color(177f/255f, 159f/255f, 139f/255f),
            new Color(175f/255f, 141f/255f, 178f/255f),
            new Color(158f/255f, 125f/255f, 154f/255f),
            new Color(204f/255f, 208f/255f, 217f/255f),
            new Color(151f/255f, 150f/255f, 166f/255f),
            new Color(174f/255f, 102f/255f, 103f/255f),
            new Color(85f/255f, 9f/255f, 9f/255f),
        };
        
        GameObject buttonTemplate = skinColorGrid.GetChild(0).gameObject;

        for (int i = 0; i <= skinColors.Length; i++) {
            ColorSwatch swatch = Instantiate(buttonTemplate, skinColorGrid).GetComponent<ColorSwatch>();
            
            if (i == skinColors.Length) {
                SetupCustomColorSwatch(swatch.gameObject, "_Color_Skin");
                continue;
            }

            int c = i;
            swatch.color = skinColors[i];
            swatch.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(swatch, skinColors[c], "_Color_Skin");});

            if (i==0) swatch.Pick();
        }

        Destroy(buttonTemplate);
    }
    void InitHairColors () {
        hairColors = new Color[] {
            new Color(96f/255f, 33f/255f, 0),
            new Color(255f/255f, 255f/255f, 255f/255f),
            new Color(214f/255f, 196f/255f, 194f/255f),
            new Color(230f/255f, 206f/255f, 168f/255f),
            new Color(165f/255f, 107f/255f, 70f/255f),
            new Color(113f/255f, 99f/255f, 90f/255f),
            new Color(83f/255f, 61f/255f, 50f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(181f/255f, 82f/255f, 57f/255f),
            new Color(253f/255f, 185f/255f, 200f/255f),
            new Color(158f/255f, 125f/255f, 154f/255f),
            new Color(123/255f, 0f, 0f),
            new Color(79f/255f, 121f/255f, 66f/255f),
            new Color(21f/255f, 146f/255f, 202f/255f),
            new Color(0, 0, 0)
            
        };
        
        GameObject buttonTemplate = hairColorGrid.GetChild(0).gameObject;

        for (int i = 0; i <= hairColors.Length; i++) {
            ColorSwatch swatch = Instantiate(buttonTemplate, hairColorGrid).GetComponent<ColorSwatch>();

            if (i == hairColors.Length) {
                SetupCustomColorSwatch(swatch.gameObject, "_Color_Hair", true);
                continue;
            }

            swatch.color = hairColors[i];
            int c = i;
            swatch.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(swatch, hairColors[c], "_Color_Hair");});

            if (i==0) swatch.Pick();
        }

        Destroy(buttonTemplate);
    }
    void InitEyesColors () {
        eyesColors = new Color[] {
            new Color(0f/255f, 0f/255f, 0f/255f),
            new Color(157f/255f, 125f/255f, 84f/255f),
            new Color(116f/255f, 70f/255f, 36f/255f),
            new Color(90f/255f, 59f/255f, 31f/255f),
            new Color(144f/255f, 159f/255f, 104f/255f),
            new Color(72f/255f, 104f/255f, 81f/255f),
            new Color(169f/255f, 227f/255f, 228f/255f),
            new Color(92f/255f, 185f/255f, 193f/255f),
            new Color(161f/255f, 180f/255f, 238f/255f),
            new Color(115f/255f, 133f/255f, 207f/255f),
            new Color(33f/255f, 22f/255f, 88f/255f),
            new Color(108f/255f, 25f/255f, 17f/255f),
            new Color(67f/255f, 18f/255f, 13f/255f),
            new Color(1, 1, 1),
            new Color(0.75f, 0.75f, 0.75f),
        };
        
        GameObject buttonTemplate = eyesColorGrid.GetChild(0).gameObject;

        for (int i = 0; i <= eyesColors.Length; i++) {
            ColorSwatch swatch = Instantiate(buttonTemplate, eyesColorGrid).GetComponent<ColorSwatch>();
            
            if (i == eyesColors.Length) {
                SetupCustomColorSwatch(swatch.gameObject, "_Color_Eyes", true);
                continue;
            }
            
            int c = i;
            swatch.color = eyesColors[i];
            swatch.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(swatch, eyesColors[c], "_Color_Eyes");});

            if (i==0) swatch.Pick();
        }

        Destroy(buttonTemplate);
    }
    void InitStubbleColors () {
        stubbleColors = new Color[] {
            new Color(205f/255f, 179f/255f, 161f/255f),
            new Color(255f/255f, 255f/255f, 255f/255f), 
            new Color(255f/255f, 204f/255f, 174f/255f),
            new Color(234f/255f, 183f/255f, 138f/255f),
            new Color(211f/255f, 142f/255f, 111f/255f),
            new Color(197f/255f, 132f/255f, 92f/255f),
            new Color(88f/255f, 59f/255f, 43f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(0, 0, 0),
            new Color(233f/255f, 208f/255f, 178f/255f),
            new Color(177f/255f, 159f/255f, 139f/255f),
            new Color(158f/255f, 125f/255f, 154f/255f),
            new Color(204f/255f, 208f/255f, 217f/255f),
            new Color(151f/255f, 150f/255f, 166f/255f),
            new Color(174f/255f, 102f/255f, 103f/255f),
        };
        
        GameObject buttonTemplate = stubbleColorGrid.GetChild(0).gameObject;

        for (int i = 0; i <= stubbleColors.Length; i++) {
            ColorSwatch swatch = Instantiate(buttonTemplate, stubbleColorGrid).GetComponent<ColorSwatch>();
            
            if (i == stubbleColors.Length) {
                SetupCustomColorSwatch(swatch.gameObject, "_Color_Stubble", true);
                continue;
            }
            
            int c = i;
            swatch.color = stubbleColors[i];
            swatch.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(swatch, stubbleColors[c], "_Color_Stubble");});

            if (i==0) swatch.Pick();
        }

        Destroy(buttonTemplate);
    }
    void InitBodyArtColors () {
        bodyArtColors = new Color[] {
            new Color(79f/255f, 96f/255f, 165f/255f), 
            new Color(214f/255f, 196f/255f, 194f/255f),
            new Color(230f/255f, 206f/255f, 168f/255f),
            new Color(165f/255f, 107f/255f, 70f/255f),
            new Color(113f/255f, 99f/255f, 90f/255f),
            new Color(83f/255f, 61f/255f, 50f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(96f/255f, 33f/255f, 0),
            new Color(181f/255f, 82f/255f, 57f/255f),
            new Color(253f/255f, 185f/255f, 200f/255f),
            new Color(158f/255f, 125f/255f, 154f/255f),
            new Color(123/255f, 0f, 0f),
            new Color(79f/255f, 121f/255f, 66f/255f),
            new Color(21f/255f, 146f/255f, 202f/255f),
            new Color(0, 0, 0)
            
        };
        
        GameObject buttonTemplate = bodyArtColorGrid.GetChild(0).gameObject;

        for (int i = 0; i <= bodyArtColors.Length; i++) {
            ColorSwatch swatch = Instantiate(buttonTemplate, bodyArtColorGrid).GetComponent<ColorSwatch>();
            
            if (i == hairColors.Length) {
                SetupCustomColorSwatch(swatch.gameObject, "_Color_BodyArt", true);
                continue;
            }
            
            swatch.color = bodyArtColors[i];
            int c = i;
            swatch.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(swatch, bodyArtColors[c], "_Color_BodyArt");});

            if (i==0) swatch.Pick();
        }

        Destroy(buttonTemplate);
    }
    void InitScarColors () {
        scarColors = new Color[] {
            new Color(237f/255f, 175f/255f, 151f/255f),
            new Color(255f/255f, 255f/255f, 255f/255f),
            new Color(230f/255f, 206f/255f, 168f/255f),
            new Color(165f/255f, 107f/255f, 70f/255f),
            new Color(113f/255f, 99f/255f, 90f/255f),
            new Color(83f/255f, 61f/255f, 50f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(96f/255f, 33f/255f, 0),
            new Color(181f/255f, 82f/255f, 57f/255f),
            new Color(253f/255f, 185f/255f, 200f/255f),
            new Color(158f/255f, 125f/255f, 154f/255f),
            new Color(123/255f, 0f, 0f),
            new Color(79f/255f, 121f/255f, 66f/255f),
            new Color(21f/255f, 146f/255f, 202f/255f),
            new Color(0, 0, 0)
            
        };
        
        GameObject buttonTemplate = scarColorGrid.GetChild(0).gameObject;

        for (int i = 0; i <= scarColors.Length; i++) {
            ColorSwatch swatch = Instantiate(buttonTemplate, scarColorGrid).GetComponent<ColorSwatch>();

            if (i == hairColors.Length) {
                SetupCustomColorSwatch(swatch.gameObject, "_Color_Scar", true);
                continue;
            }

            swatch.color = scarColors[i];
            int c = i;
            swatch.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(swatch, scarColors[c], "_Color_Scar");});

            if (i==0) swatch.Pick();
        }

        Destroy(buttonTemplate);
    }

    void SetupCustomColorSwatch(GameObject swatchObject, string shaderProperty, bool leftSidedModal = false) {
        ColorSwatch swatch = swatchObject.GetComponent<ColorSwatch>();
        
        swatch.color = Color.grey;
        swatchObject.GetComponent<Button>().onClick.AddListener(delegate{OnCustomColorSelect(swatch, swatchObject, shaderProperty, leftSidedModal);});

        RectTransform icon = new GameObject().AddComponent<RectTransform>();
        icon.SetParent(swatchObject.transform);
        icon.anchorMin = Vector2.zero;
        icon.anchorMax = Vector2.one;
        icon.offsetMax = -Vector2.one * 7;
        icon.offsetMin = Vector2.one * 7;

        Image img = icon.gameObject.AddComponent<Image>();
        img.sprite = customColorSprite;
    }

    private void SetupPartSlider(ModularBodyPart bodyPart, int partIndex) {
        if (partIndex == -1)
            modularCharacterManager.DeactivatePart(bodyPart);
        else
            modularCharacterManager.ActivatePart(bodyPart, partIndex);
    }

    private void SetupBodyPart(ModularBodyPart bodyPart, ref int index, ref int maxIndex) {
        characterBody = modularCharacterManager.GetCharacterBody();
        
        if (characterBody.TryGetValue(bodyPart, out GameObject[] partsArray)) {
            maxIndex = partsArray.Length - 1;
            bool hasActivePart = false;
            for (int i = 0; i < partsArray.Length; i++) {
                if (partsArray[i].activeSelf) {
                    index = i;
                    hasActivePart = true;
                }
            }
            if (!hasActivePart)
                index = -1;
        } else {
            index = -1;
        }
    }

    public void RandomizeAll () {
        PickRandomGender();
        
        PickRandomHead();
        PickRandomEar();
        PickRandomHair();
        PickRandomFacialHair();
        PickRandomEyebrows();
        
        PickRandomSkinColor();
        PickRandomHairColor();
        PickRandomEyeColor();
        PickRandomStubbleColor();
        PickRandomBodyArtColor();
        PickRandomScarColor();

        UpdateLabels();
    }

    public void PickRandomGender () {
        OnGenderSelect(Random.value < 0.5f);
    }

    public void PickRandomHead() {
        headCurrentIndex = Random.Range(0, headMaxIndex+1);
        SetupPartSlider(ModularBodyPart.Head, headCurrentIndex);
    }
    public void PickRandomEar() {
        earCurrentIndex = Random.Range(0, earMaxIndex+1);
        SetupPartSlider(ModularBodyPart.Ear, earCurrentIndex);
    }
    public void PickRandomHair() {
        hairCurrentIndex = Random.Range(0, hairMaxIndex+1);
        SetupPartSlider(ModularBodyPart.Hair, hairCurrentIndex);
    }
    public void PickRandomFacialHair() {
        facialHairCurrentIndex = Random.Range(0, facialHairMaxIndex+1);
        SetupPartSlider(ModularBodyPart.FacialHair, facialHairCurrentIndex);
    }
    public void PickRandomEyebrows() {
        eyebrowCurrentIndex = Random.Range(0, eyebrowMaxIndex+1);
        SetupPartSlider(ModularBodyPart.Eyebrow, eyebrowCurrentIndex);
    }

    public void PickRandomSkinColor () {
        int index = Random.Range(0, skinColors.Length);
        ColorSwatch swatch = skinColorGrid.GetChild(index+1).GetComponent<ColorSwatch>();
        OnColorSelect(swatch, swatch.color, "_Color_Skin");
    }
    public void PickRandomHairColor () {
        int index = Random.Range(0, hairColors.Length);
        ColorSwatch swatch = hairColorGrid.GetChild(index+1).GetComponent<ColorSwatch>();
        OnColorSelect(swatch, swatch.color, "_Color_Hair");
    }
    public void PickRandomEyeColor () {
        int index = Random.Range(0, eyesColors.Length);
        ColorSwatch swatch = eyesColorGrid.GetChild(index+1).GetComponent<ColorSwatch>();
        OnColorSelect(swatch, swatch.color, "_Color_Eyes");
    }
    public void PickRandomStubbleColor () {
        int index = Random.Range(0, stubbleColors.Length);
        ColorSwatch swatch = stubbleColorGrid.GetChild(index+1).GetComponent<ColorSwatch>();
        OnColorSelect(swatch, swatch.color, "_Color_Stubble");
    }
    public void PickRandomBodyArtColor () {
        int index = Random.Range(0, bodyArtColors.Length);
        ColorSwatch swatch = bodyArtColorGrid.GetChild(index+1).GetComponent<ColorSwatch>();
        OnColorSelect(swatch, swatch.color, "_Color_BodyArt");
    }
    public void PickRandomScarColor () {
        int index = Random.Range(0, scarColors.Length);
        ColorSwatch swatch = scarColorGrid.GetChild(index+1).GetComponent<ColorSwatch>();
        OnColorSelect(swatch, swatch.color, "_Color_Scar");
    }

    IEnumerator PickRandomIdleAnimation () {
        while (true) {
            animator.SetInteger("AnimationID", Random.Range(0,5));
            yield return new WaitForSeconds(3);
        }
    }

    public void ToggleConfirmationMenu() {
        if (confirmMenu.alpha < 1) {
            string color = "#" + ColorUtility.ToHtmlStringRGB(nameInputField.GetComponentInChildren<TextMeshProUGUI>().color);
            confirmTitle.text = $"Create <color={color}>{nameInputField.text}</color>?";
            confirmMenu.DOFade(1, 0.2f);
            confirmMenu.interactable = true;
            confirmMenu.blocksRaycasts = true;
        } else {
            confirmMenu.DOFade(0, 0.2f);
            confirmMenu.interactable = false;
            confirmMenu.blocksRaycasts = false;
        }
    }

    public void ConfirmCharacter () {
        string saveFilePath = "saves/characterAppearance.json";

        ES3.Save<string>("name", nameInputField.text, saveFilePath);
        ES3.Save<Gender>("gender", modularCharacterManager.CharacterGender, saveFilePath);
        
        ES3.Save<int>("headID", headCurrentIndex, saveFilePath);
        ES3.Save<int>("hairID", hairCurrentIndex, saveFilePath);
        ES3.Save<int>("eyebrowID", eyebrowCurrentIndex, saveFilePath);
        ES3.Save<int>("earID", earCurrentIndex ,saveFilePath);
        ES3.Save<int>("facialHairID", facialHairCurrentIndex ,saveFilePath);
        
        ES3.Save<Color>("skinColor", newCharacterMat.GetColor("_Color_Skin"), saveFilePath);
        ES3.Save<Color>("hairColor", newCharacterMat.GetColor("_Color_Hair"), saveFilePath);
        ES3.Save<Color>("eyesColor", newCharacterMat.GetColor("_Color_Eyes"), saveFilePath);
        ES3.Save<Color>("stubbleColor", newCharacterMat.GetColor("_Color_Stubble"), saveFilePath);
        ES3.Save<Color>("bodyArtColor", newCharacterMat.GetColor("_Color_BodyArt"), saveFilePath);
        ES3.Save<Color>("scarColor", newCharacterMat.GetColor("_Color_Scar"), saveFilePath);
    }

}
