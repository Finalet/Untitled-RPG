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
    Material newCharacterMat;

    [Space]
    public CanvasGroup maleButton;
    public CanvasGroup femaleButton;
    public TextMeshProUGUI headLabel;
    public TextMeshProUGUI hairLabel;
    public TextMeshProUGUI facialhairLabel;
    public TextMeshProUGUI eyebrowLabel;
    public TextMeshProUGUI earsLabel;
    public Transform skinColorGrid;

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

    private Color skinColor;
    private Color eyeColor;
    private Color bodyArtColor;
    private Color scarColor;
    private Color hairColor;

    Color[] skinColors;

    void Awake() {
        animator = modularCharacterManager.GetComponent<Animator>();
        
        StartCoroutine(PickRandomIdleAnimation());

        InitSkilColors();
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

    public void OnSkinColorSelect (Color selectedColor) {
        newCharacterMat.SetColor("_Color_Skin", selectedColor);
    }

    void UpdateLabels () {
        headLabel.text = $"Face: {headCurrentIndex + 1}";
        earsLabel.text = $"Ears style: {earCurrentIndex + 2}";
        hairLabel.text = $"Hair style: {(hairCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (hairCurrentIndex+1).ToString())}";
        facialhairLabel.text = $"Facial hair style: {(facialHairCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (facialHairCurrentIndex+1).ToString())}";
        eyebrowLabel.text = $"Eyebrows style: {(eyebrowCurrentIndex + 1 == 0 ? "<alpha=#66>none" : (eyebrowCurrentIndex+1).ToString())}";
    }

    void InitSkilColors () {
        skinColors = new Color[] {
            new Color(255f/255f, 255f/255f, 255f/255f),
            new Color(243f/255f, 215f/255f, 201f/255f),
            new Color(237f/255f, 191f/255f, 167f/255f),
            new Color(234f/255f, 183f/255f, 138f/255f),
            new Color(211f/255f, 142f/255f, 111f/255f),
            new Color(197f/255f, 132f/255f, 92f/255f),
            new Color(88f/255f, 59f/255f, 43f/255f),
            new Color(44f/255f, 29f/255f, 21f/255f),
            new Color(0, 0, 0),
        };
        
        GameObject buttonTemplate = skinColorGrid.GetChild(0).gameObject;

        for (int i = 0; i < skinColors.Length; i++) {
            GameObject newColorButton = Instantiate(buttonTemplate, skinColorGrid);
            newColorButton.GetComponent<Image>().color = skinColors[i];
            int c = i;
            newColorButton.GetComponent<Button>().onClick.AddListener(delegate{OnSkinColorSelect(skinColors[c]);});
        }

        Destroy(buttonTemplate);
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
