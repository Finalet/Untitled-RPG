using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeStudios.ModularCharacters;
using TMPro;
using UnityEngine.UI;

public class CharacterCreationManager : MonoBehaviour
{
    public ModularCharacterManager modularCharacterManager;
    public Material defaultCharacterMat;
    public GameObject colorPickerPrefab;
    public Canvas canvas;
    Material newCharacterMat;

    [Space]
    public Sprite customColorSprite;
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
    }

    public void OnHeadSelect (bool plus) {
        headCurrentIndex = Mathf.Clamp(headCurrentIndex + (plus ? 1 : -1), 0, headMaxIndex);
        SetupPartSlider(ModularBodyPart.Head, headCurrentIndex);
        headLabel.text = $"Face: {headCurrentIndex + 1}";
    }
    public void OnEarSelect (bool plus) {
        earCurrentIndex = Mathf.Clamp(earCurrentIndex + (plus ? 1 : -1), -1, earMaxIndex);
        SetupPartSlider(ModularBodyPart.Ear, earCurrentIndex);
        earsLabel.text = $"Ears style: {earCurrentIndex + 2}";
    }
    public void OnHairSelect (bool plus) {
        hairCurrentIndex = Mathf.Clamp(hairCurrentIndex + (plus ? 1 : -1), -1, hairMaxIndex);
        SetupPartSlider(ModularBodyPart.Hair, hairCurrentIndex);
        hairLabel.text = $"Hair style: {(hairCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (hairCurrentIndex+1).ToString())}";
    }
    public void OnFacialHairSelect (bool plus) {
        facialHairCurrentIndex = Mathf.Clamp(facialHairCurrentIndex + (plus ? 1 : -1), -1, facialHairMaxIndex);
        SetupPartSlider(ModularBodyPart.FacialHair, facialHairCurrentIndex);
        facialhairLabel.text = $"Facial hair style: {(facialHairCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (facialHairCurrentIndex+1).ToString())}";
    }
    public void OnEyebrowSelect (bool plus) {
        eyebrowCurrentIndex = Mathf.Clamp(eyebrowCurrentIndex + (plus ? 1 : -1), -1, eyebrowMaxIndex);
        SetupPartSlider(ModularBodyPart.Eyebrow, eyebrowCurrentIndex);
        eyebrowLabel.text = $"Eyebrows style: {(eyebrowCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (eyebrowCurrentIndex+1).ToString())}";
    }

    public void OnColorSelect (Color selectedColor, string shaderProperty) {
        newCharacterMat.SetColor(shaderProperty, selectedColor);

        if (instanciatedColorPicker) Destroy(instanciatedColorPicker);
    }
    public void OnCustomColorSelect (GameObject result, string shaderProperty) {        
        if (instanciatedColorPicker) Destroy(instanciatedColorPicker);
        instanciatedColorPicker = Instantiate(colorPickerPrefab, canvas.transform);

        CUIColorPicker colorPicker = instanciatedColorPicker.GetComponent<CUIColorPicker>();
        RectTransform resultRect = result.GetComponent<RectTransform>();

        RectTransform rect = colorPicker.GetComponent<RectTransform>();
        rect.position = resultRect.position;
        rect.anchoredPosition += resultRect.sizeDelta * Vector2.right;
        
        colorPicker.result = result;
        colorPicker.Color = newCharacterMat.GetColor(shaderProperty);
        colorPicker.confirmButton.onClick.AddListener(delegate{OnColorSelect(colorPicker.Color, shaderProperty);});
    }

    void UpdateLabels () {
        headLabel.text = $"Face: {headCurrentIndex + 1}";
        earsLabel.text = $"Ears style: {earCurrentIndex + 2}";
        hairLabel.text = $"Hair style: {(hairCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (hairCurrentIndex+1).ToString())}";
        facialhairLabel.text = $"Facial hair style: {(facialHairCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (facialHairCurrentIndex+1).ToString())}";
        eyebrowLabel.text = $"Eyebrows style: {(eyebrowCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (eyebrowCurrentIndex+1).ToString())}";
    }

    void InitSkinColors () {
        skinColors = new Color[] {
            //Regular skin
            new Color(255f/255f, 255f/255f, 255f/255f),
            new Color(243f/255f, 215f/255f, 201f/255f),
            new Color(237f/255f, 191f/255f, 167f/255f),
            new Color(234f/255f, 183f/255f, 138f/255f),
            new Color(211f/255f, 142f/255f, 111f/255f),
            new Color(197f/255f, 132f/255f, 92f/255f),
            new Color(88f/255f, 59f/255f, 43f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(0, 0, 0),

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
            GameObject newColorButton = Instantiate(buttonTemplate, skinColorGrid);
            
            if (i == skinColors.Length) {
                SetupCustomColorSwatch(newColorButton, "_Color_Skin");
                continue;
            }

            newColorButton.GetComponent<Image>().color = skinColors[i];
            int c = i;
            newColorButton.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(skinColors[c], "_Color_Skin");});
        }

        Destroy(buttonTemplate);
    }
    void InitHairColors () {
        hairColors = new Color[] {
            new Color(255f/255f, 255f/255f, 255f/255f),
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
        
        GameObject buttonTemplate = hairColorGrid.GetChild(0).gameObject;

        for (int i = 0; i <= hairColors.Length; i++) {
            GameObject newColorButton = Instantiate(buttonTemplate, hairColorGrid);

            if (i == hairColors.Length) {
                SetupCustomColorSwatch(newColorButton, "_Color_Hair");
                continue;
            }

            newColorButton.GetComponent<Image>().color = hairColors[i];
            int c = i;
            newColorButton.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(hairColors[c], "_Color_Hair");});
        }

        Destroy(buttonTemplate);
    }
    void InitEyesColors () {
        eyesColors = new Color[] {
            //Regular skin
            new Color(255f/255f, 255f/255f, 255f/255f),
            new Color(243f/255f, 215f/255f, 201f/255f),
            new Color(237f/255f, 191f/255f, 167f/255f),
            new Color(234f/255f, 183f/255f, 138f/255f),
            new Color(211f/255f, 142f/255f, 111f/255f),
            new Color(197f/255f, 132f/255f, 92f/255f),
            new Color(88f/255f, 59f/255f, 43f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(0, 0, 0),

            //Fantasy skin
            new Color(233f/255f, 208f/255f, 178f/255f),
            new Color(177f/255f, 159f/255f, 139f/255f),
            new Color(164f/255f, 157f/255f, 128f/255f),
            new Color(175f/255f, 141f/255f, 178f/255f),
            new Color(158f/255f, 125f/255f, 154f/255f),
            new Color(204f/255f, 208f/255f, 217f/255f),
            new Color(151f/255f, 150f/255f, 166f/255f),
        };
        
        GameObject buttonTemplate = eyesColorGrid.GetChild(0).gameObject;

        for (int i = 0; i < eyesColors.Length; i++) {
            GameObject newColorButton = Instantiate(buttonTemplate, eyesColorGrid);
            newColorButton.GetComponent<Image>().color = eyesColors[i];
            int c = i;
            newColorButton.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(eyesColors[c], "_Color_Eyes");});
        }

        Destroy(buttonTemplate);
    }
    void InitStubbleColors () {
        stubbleColors = new Color[] {
            //Regular skin
            new Color(255f/255f, 255f/255f, 255f/255f),
            new Color(243f/255f, 215f/255f, 201f/255f),
            new Color(237f/255f, 191f/255f, 167f/255f),
            new Color(234f/255f, 183f/255f, 138f/255f),
            new Color(211f/255f, 142f/255f, 111f/255f),
            new Color(197f/255f, 132f/255f, 92f/255f),
            new Color(88f/255f, 59f/255f, 43f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(0, 0, 0),

            //Fantasy skin
            new Color(233f/255f, 208f/255f, 178f/255f),
            new Color(177f/255f, 159f/255f, 139f/255f),
            new Color(164f/255f, 157f/255f, 128f/255f),
            new Color(175f/255f, 141f/255f, 178f/255f),
            new Color(158f/255f, 125f/255f, 154f/255f),
            new Color(204f/255f, 208f/255f, 217f/255f),
            new Color(151f/255f, 150f/255f, 166f/255f),
        };
        
        GameObject buttonTemplate = stubbleColorGrid.GetChild(0).gameObject;

        for (int i = 0; i < stubbleColors.Length; i++) {
            GameObject newColorButton = Instantiate(buttonTemplate, stubbleColorGrid);
            newColorButton.GetComponent<Image>().color = stubbleColors[i];
            int c = i;
            newColorButton.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(stubbleColors[c], "_Color_Stubble");});
        }

        Destroy(buttonTemplate);
    }
    void InitBodyArtColors () {
        bodyArtColors = new Color[] {
            new Color(255f/255f, 255f/255f, 255f/255f),
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
            GameObject newColorButton = Instantiate(buttonTemplate, bodyArtColorGrid);
            
            if (i == hairColors.Length) {
                SetupCustomColorSwatch(newColorButton, "_Color_BodyArt");
                continue;
            }
            
            newColorButton.GetComponent<Image>().color = bodyArtColors[i];
            int c = i;
            newColorButton.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(bodyArtColors[c], "_Color_BodyArt");});
        }

        Destroy(buttonTemplate);
    }
    void InitScarColors () {
        scarColors = new Color[] {
            new Color(255f/255f, 255f/255f, 255f/255f),
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
        
        GameObject buttonTemplate = scarColorGrid.GetChild(0).gameObject;

        for (int i = 0; i <= scarColors.Length; i++) {
            GameObject newColorButton = Instantiate(buttonTemplate, scarColorGrid);

            if (i == hairColors.Length) {
                SetupCustomColorSwatch(newColorButton, "_Color_Scar");
                continue;
            }

            newColorButton.GetComponent<Image>().color = scarColors[i];
            int c = i;
            newColorButton.GetComponent<Button>().onClick.AddListener(delegate{OnColorSelect(scarColors[c], "_Color_Scar");});
        }

        Destroy(buttonTemplate);
    }

    void SetupCustomColorSwatch(GameObject swatchObject, string shaderProperty) {
        swatchObject.GetComponent<Image>().color = Color.grey;
        swatchObject.GetComponent<Button>().onClick.AddListener(delegate{OnCustomColorSelect(swatchObject, shaderProperty);});

        RectTransform icon = new GameObject().AddComponent<RectTransform>();
        icon.SetParent(swatchObject.transform);
        icon.anchorMin = Vector2.zero;
        icon.anchorMax = Vector2.one;
        icon.offsetMax = -Vector2.one * 5;
        icon.offsetMin = Vector2.one * 5;

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

    IEnumerator PickRandomIdleAnimation () {
        while (true) {
            animator.SetInteger("AnimationID", Random.Range(0,5));
            yield return new WaitForSeconds(3);
        }
    }
}
