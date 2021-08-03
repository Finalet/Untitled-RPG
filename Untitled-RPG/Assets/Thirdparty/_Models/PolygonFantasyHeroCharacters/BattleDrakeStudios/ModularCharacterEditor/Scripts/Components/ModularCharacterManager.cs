using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeStudios.ModularCharacters {

    [ExecuteInEditMode]
    public class ModularCharacterManager : MonoBehaviour, ISavable {
        
        string characterName = "Unnamed";
        Gender baseGender = Gender.Male;

        int baseHeadID = 1;
        int baseHairID = -1;
        int baseEyebrowID = -7;
        int baseEarID = -1;
        int baseFacialHairID = -1;

        Color baseSkinColor = new Color(255f/255, 204f/255, 174f/255);
        Color baseHairColor = new Color(96f/255, 33f/255, 0/255);
        Color baseEyesColor = new Color(0, 0, 0);
        Color baseStubbleColor = new Color(205f/255, 179f/255, 161f/255);
        Color baseBodyArtColor = new Color(79f/255, 96f/255, 165f/255);
        Color baseScarColor = new Color(237f/255, 175f/255, 151f/255);

        [Header("Male Base")]
        [SerializeField] private List<BodyPartLinker> maleBaseBody = new List<BodyPartLinker>();

        [Header("Female Base")]
        [SerializeField] private List<BodyPartLinker> femaleBaseBody = new List<BodyPartLinker>();

        [Header("Character Material")]
        [SerializeField] private Material characterMaterial;
        Material runtimeMaterial;

        [Header("MaleParts Arrays")]
        [SerializeField] private GameObject[] maleHelmetParts;
        [SerializeField] private GameObject[] maleHeadParts;
        [SerializeField] private GameObject[] maleEyebrowParts;
        [SerializeField] private GameObject[] maleFacialHairParts;
        [SerializeField] private GameObject[] maleTorsoParts;
        [SerializeField] private GameObject[] maleArmUpperRightParts;
        [SerializeField] private GameObject[] maleArmUpperLeftParts;
        [SerializeField] private GameObject[] maleArmLowerRightParts;
        [SerializeField] private GameObject[] maleArmLowerLeftParts;
        [SerializeField] private GameObject[] maleHandRightParts;
        [SerializeField] private GameObject[] maleHandLeftParts;
        [SerializeField] private GameObject[] maleHipsParts;
        [SerializeField] private GameObject[] maleLegRightParts;
        [SerializeField] private GameObject[] maleLegLeftParts;

        [Header("FemaleParts Arrays")]
        [SerializeField] private GameObject[] femaleHelmetParts;
        [SerializeField] private GameObject[] femaleHeadParts;
        [SerializeField] private GameObject[] femaleEyebrowParts;
        [SerializeField] private GameObject[] femaleFacialHairParts;
        [SerializeField] private GameObject[] femaleTorsoParts;
        [SerializeField] private GameObject[] femaleArmUpperRightParts;
        [SerializeField] private GameObject[] femaleArmUpperLeftParts;
        [SerializeField] private GameObject[] femaleArmLowerRightParts;
        [SerializeField] private GameObject[] femaleArmLowerLeftParts;
        [SerializeField] private GameObject[] femaleHandRightParts;
        [SerializeField] private GameObject[] femaleHandLeftParts;
        [SerializeField] private GameObject[] femaleHipsParts;
        [SerializeField] private GameObject[] femaleLegRightParts;
        [SerializeField] private GameObject[] femaleLegLeftParts;

        [Header("UniversalParts Arrays")]
        [SerializeField] private GameObject[] hatParts;
        [SerializeField] private GameObject[] maskParts;
        [SerializeField] private GameObject[] headCoveringParts;
        [SerializeField] private GameObject[] hairParts;
        [SerializeField] private GameObject[] earParts;
        [SerializeField] private GameObject[] headAttachmentParts;
        [SerializeField] private GameObject[] backAttachmentParts;
        [SerializeField] private GameObject[] shoulderAttachmentRightParts;
        [SerializeField] private GameObject[] shoulderAttachmentLeftParts;
        [SerializeField] private GameObject[] elbowAttachmentRightParts;
        [SerializeField] private GameObject[] elbowAttachmentLeftParts;
        [SerializeField] private GameObject[] hipsAttachmentParts;
        [SerializeField] private GameObject[] kneeAttachmentRightParts;
        [SerializeField] private GameObject[] kneeAttachmentLeftParts;

        [HideInInspector]
        [SerializeField] private List<GameObject> allParts = new List<GameObject>();

        [HideInInspector]
        [SerializeField] private bool isInitialized;
        [HideInInspector]
        [SerializeField] private Gender characterGender;

        public bool IsInitialized => isInitialized;
        public Gender CharacterGender => characterGender;
        public Material CharacterMaterial => characterMaterial;

        private Dictionary<ModularBodyPart, GameObject[]> characterBody = new Dictionary<ModularBodyPart, GameObject[]>();
        private Dictionary<ModularBodyPart, int> activeParts = new Dictionary<ModularBodyPart, int>();

        void Start() {
            if (SaveManager.instance && PlayerControlls.instance) SaveManager.instance.saveObjects.Add(this);
        }

        private void OnEnable() {
            if (isInitialized) {
                InitializeDictionaries();
            }
        }

        public void ActivatePart(ModularBodyPart bodyPart, int partID) {
            GameObject partToActivate = GetPartFromID(bodyPart, partID);
            if (partToActivate != null) {
                if (activeParts.TryGetValue(bodyPart, out int activePartID)) {
                    GetPartFromID(bodyPart, activePartID).SetActive(false);
                }
                activeParts[bodyPart] = partID;
                partToActivate.SetActive(true);
            }
        }

        public void DeactivatePart(ModularBodyPart bodyPart) {
            if (activeParts.ContainsKey(bodyPart)) {
                GetPartFromID(bodyPart, activeParts[bodyPart]).SetActive(false);
                activeParts.Remove(bodyPart);
            }
        }
        public void ReturnToBaseBodypart (ModularBodyPart bodyPart) {
            int partID;
            switch (bodyPart) {
                case ModularBodyPart.Head:
                    partID = baseHeadID;
                    break;
                case ModularBodyPart.Hair:
                    partID = baseHairID;
                    break;
                case ModularBodyPart.Eyebrow:
                    partID = baseEyebrowID;
                    break;
                case ModularBodyPart.Ear:
                    partID = baseEarID;
                    break;
                case ModularBodyPart.FacialHair:
                    partID = baseFacialHairID;
                    break;
                default:
                    partID = -1;
                    Debug.LogError("bodyPart unsupported. You fucked something up, Grant");
                    return;
            }
            if (partID == -1) return;
            ActivatePart(bodyPart, partID);
        }

        public void SetPartColor(ModularBodyPart bodyPart, int partID, string colorProperty, Color newColor) {
            GameObject part = GetPartFromID(bodyPart, partID);
            if (part != null) {
                Material tempMaterial = new Material(part.GetComponent<SkinnedMeshRenderer>().sharedMaterial);
                tempMaterial.SetColor(colorProperty, newColor);
                part.GetComponent<SkinnedMeshRenderer>().material = tempMaterial;

            }

        }
        
        public void SetAllPartsMaterial(Material material) {
            foreach (var part in allParts) {
                part.GetComponent<SkinnedMeshRenderer>().material = material;
            }
        }

        public void SwapGender(Gender bodyGender) {
            this.characterGender = bodyGender;
            foreach (var part in activeParts.ToList()) {
                GetPartFromID(part.Key, part.Value).SetActive(false);
                if (part.Key == ModularBodyPart.Eyebrow)
                    activeParts.Remove(part.Key);
            }
            SetupCharacterBodyDictionary();
            foreach (var part in activeParts) {
                GetPartFromID(part.Key, part.Value).SetActive(true);
            }
        }

        public void ToggleBaseBodyDisplay(bool isVisible) {
            switch (characterGender) {
                case Gender.Male:
                    foreach (var part in maleBaseBody) {
                        if (activeParts.ContainsKey(part.bodyType))
                            if (activeParts[part.bodyType] == 0)
                                GetPartFromID(part.bodyType, part.partID).SetActive(isVisible);
                    }
                    break;
                case Gender.Female:
                    foreach (var part in femaleBaseBody) {
                        if (activeParts.ContainsKey(part.bodyType))
                            if (activeParts[part.bodyType] == 0)
                                GetPartFromID(part.bodyType, part.partID).SetActive(isVisible);
                    }
                    break;
            }
        }

        public GameObject GetPartFromID(ModularBodyPart partType, int partID) {
            if (characterBody.TryGetValue(partType, out GameObject[] parts)) {
                return parts[partID];
            }
            Debug.LogError(partType.ToString() + " returned null in ModularManager.GetBodyPart()");
            return null;
        }

        public Dictionary<ModularBodyPart, GameObject[]> GetCharacterBody() {
            return characterBody;
        }

        [ContextMenu("ClearAll")]
        private void ClearAll() {
            SetupNewCharacter(characterGender, characterMaterial);
        }

        public void SetupNewCharacter(Gender characterGender, Material characterMaterial) {
            this.characterMaterial = characterMaterial;

            SetupBodyArrays(false);

            this.characterGender = characterGender;

            InitializeDictionaries();

            List<BodyPartLinker> activeBody = this.characterGender == Gender.Male ? maleBaseBody : femaleBaseBody;
            foreach (var part in activeBody) {
                GetPartFromID(part.bodyType, part.partID).SetActive(true);
                activeParts[part.bodyType] = 0;
            }
        }

        public void SetupExistingCharacter(Gender characterGender, Material characterMaterial) {
            this.characterMaterial = characterMaterial;
            this.characterGender = characterGender;
            SetupBodyArrays(true);
            InitializeDictionaries();
        }

        private void InitializeDictionaries() {
            SetupCharacterBodyDictionary();
            SetupActivePartsDictionary();
        }

        #region ArraySetups
        private void SetupBodyArrays(bool isExisting) {
            SetupPartArray(ref maleHelmetParts, ModularCharacterStatics.MaleHelmetPath, isExisting);
            SetupPartArray(ref maleHeadParts, ModularCharacterStatics.MaleHeadPath, isExisting);
            SetupPartArray(ref maleEyebrowParts, ModularCharacterStatics.MaleEyebrowPath, isExisting);
            SetupPartArray(ref maleFacialHairParts, ModularCharacterStatics.MaleFacialHairPath, isExisting);
            SetupPartArray(ref maleTorsoParts, ModularCharacterStatics.MaleTorsoPath, isExisting);
            SetupPartArray(ref maleArmUpperRightParts, ModularCharacterStatics.MaleArmUpperRightPath, isExisting);
            SetupPartArray(ref maleArmUpperLeftParts, ModularCharacterStatics.MaleArmUpperLeftPath, isExisting);
            SetupPartArray(ref maleArmLowerRightParts, ModularCharacterStatics.MaleArmLowerRightPath, isExisting);
            SetupPartArray(ref maleArmLowerLeftParts, ModularCharacterStatics.MaleArmLowerLeftPath, isExisting);
            SetupPartArray(ref maleHandRightParts, ModularCharacterStatics.MaleHandRightPath, isExisting);
            SetupPartArray(ref maleHandLeftParts, ModularCharacterStatics.MaleHandLeftPath, isExisting);
            SetupPartArray(ref maleHipsParts, ModularCharacterStatics.MaleHipsPath, isExisting);
            SetupPartArray(ref maleLegRightParts, ModularCharacterStatics.MaleLegRightPath, isExisting);
            SetupPartArray(ref maleLegLeftParts, ModularCharacterStatics.MaleLegLeftPath, isExisting);

            SetupPartArray(ref femaleHelmetParts, ModularCharacterStatics.FemaleHelmetPath, isExisting);
            SetupPartArray(ref femaleHeadParts, ModularCharacterStatics.FemaleHeadPath, isExisting);
            SetupPartArray(ref femaleEyebrowParts, ModularCharacterStatics.FemaleEyebrowPath, isExisting);
            SetupPartArray(ref femaleFacialHairParts, ModularCharacterStatics.FemaleFacialHairPath, isExisting);
            SetupPartArray(ref femaleTorsoParts, ModularCharacterStatics.FemaleTorsoPath, isExisting);
            SetupPartArray(ref femaleArmUpperRightParts, ModularCharacterStatics.FemaleArmUpperRightPath, isExisting);
            SetupPartArray(ref femaleArmUpperLeftParts, ModularCharacterStatics.FemaleArmUpperLeftPath, isExisting);
            SetupPartArray(ref femaleArmLowerRightParts, ModularCharacterStatics.FemaleArmLowerRightPath, isExisting);
            SetupPartArray(ref femaleArmLowerLeftParts, ModularCharacterStatics.FemaleArmLowerLeftPath, isExisting);
            SetupPartArray(ref femaleHandRightParts, ModularCharacterStatics.FemaleHandRightPath, isExisting);
            SetupPartArray(ref femaleHandLeftParts, ModularCharacterStatics.FemaleHandLeftPath, isExisting);
            SetupPartArray(ref femaleHipsParts, ModularCharacterStatics.FemaleHipsPath, isExisting);
            SetupPartArray(ref femaleLegRightParts, ModularCharacterStatics.FemaleLegRightPath, isExisting);
            SetupPartArray(ref femaleLegLeftParts, ModularCharacterStatics.FemaleLegLeftPath, isExisting);

            SetupPartArray(ref hatParts, ModularCharacterStatics.HatPath, isExisting);
            SetupPartArray(ref maskParts, ModularCharacterStatics.MaskPath, isExisting);
            SetupPartArray(ref headCoveringParts, ModularCharacterStatics.HeadCoveringPath, isExisting);
            SetupPartArray(ref hairParts, ModularCharacterStatics.HairPath, isExisting);
            SetupPartArray(ref earParts, ModularCharacterStatics.EarPath, isExisting);
            SetupPartArray(ref headAttachmentParts, ModularCharacterStatics.HeadAttachmentPath, isExisting);
            SetupPartArray(ref backAttachmentParts, ModularCharacterStatics.BackAttachmentPath, isExisting);
            SetupPartArray(ref shoulderAttachmentRightParts, ModularCharacterStatics.ShoulderAttachmentRightPath, isExisting);
            SetupPartArray(ref shoulderAttachmentLeftParts, ModularCharacterStatics.ShoulderAttachmentLeftPath, isExisting);
            SetupPartArray(ref elbowAttachmentRightParts, ModularCharacterStatics.ElbowAttachmentRightPath, isExisting);
            SetupPartArray(ref elbowAttachmentLeftParts, ModularCharacterStatics.ElbowAttachmentLeftPath, isExisting);
            SetupPartArray(ref hipsAttachmentParts, ModularCharacterStatics.HipsAttachmentPath, isExisting);
            SetupPartArray(ref kneeAttachmentRightParts, ModularCharacterStatics.KneeAttachmentRightPath, isExisting);
            SetupPartArray(ref kneeAttachmentLeftParts, ModularCharacterStatics.KneeAttachmentLeftPath, isExisting);

            isInitialized = true;
        }

        private void SetupPartArray(ref GameObject[] partsArray, string path, bool isExisting) {
            Transform[] gameObjectTransforms = GetComponentsInChildren<Transform>();

            Transform parentRoot = null;

            foreach (Transform transform in gameObjectTransforms) {
                if (transform.gameObject.name.Equals(path)) {
                    parentRoot = transform;
                    break;
                }
            }

            partsArray = new GameObject[parentRoot.childCount];
            for (int i = 0; i < partsArray.Length; i++) {
                partsArray[i] = parentRoot.GetChild(i).gameObject;
                if (characterMaterial != null)
                    partsArray[i].GetComponent<Renderer>().sharedMaterial = characterMaterial;

                if (partsArray[i].activeSelf)
                    partsArray[i].SetActive(isExisting);

                allParts.Add(partsArray[i]);
            }
        }
        #endregion

        #region DictionarySetups
        private void SetupCharacterBodyDictionary() {
            characterBody.Clear();

            characterBody[ModularBodyPart.Hat] = hatParts;
            characterBody[ModularBodyPart.Mask] = maskParts;
            characterBody[ModularBodyPart.HeadCovering] = headCoveringParts;
            characterBody[ModularBodyPart.Hair] = hairParts;
            characterBody[ModularBodyPart.Ear] = earParts;
            characterBody[ModularBodyPart.HeadAttachment] = headAttachmentParts;
            characterBody[ModularBodyPart.BackAttachment] = backAttachmentParts;
            characterBody[ModularBodyPart.ShoulderAttachmentRight] = shoulderAttachmentRightParts;
            characterBody[ModularBodyPart.ShoulderAttachmentLeft] = shoulderAttachmentLeftParts;
            characterBody[ModularBodyPart.ElbowAttachmentRight] = elbowAttachmentRightParts;
            characterBody[ModularBodyPart.ElbowAttachmentLeft] = elbowAttachmentLeftParts;
            characterBody[ModularBodyPart.HipsAttachment] = hipsAttachmentParts;
            characterBody[ModularBodyPart.KneeAttachmentRight] = kneeAttachmentRightParts;
            characterBody[ModularBodyPart.KneeAttachmentLeft] = kneeAttachmentLeftParts;

            if (characterGender == Gender.Male) {
                characterBody[ModularBodyPart.Helmet] = maleHelmetParts;
                characterBody[ModularBodyPart.Head] = maleHeadParts;
                characterBody[ModularBodyPart.Eyebrow] = maleEyebrowParts;
                characterBody[ModularBodyPart.FacialHair] = maleFacialHairParts;
                characterBody[ModularBodyPart.Torso] = maleTorsoParts;
                characterBody[ModularBodyPart.ArmUpperRight] = maleArmUpperRightParts;
                characterBody[ModularBodyPart.ArmUpperLeft] = maleArmUpperLeftParts;
                characterBody[ModularBodyPart.ArmLowerRight] = maleArmLowerRightParts;
                characterBody[ModularBodyPart.ArmLowerLeft] = maleArmLowerLeftParts;
                characterBody[ModularBodyPart.HandLeft] = maleHandRightParts;
                characterBody[ModularBodyPart.HandRight] = maleHandLeftParts;
                characterBody[ModularBodyPart.Hips] = maleHipsParts;
                characterBody[ModularBodyPart.LegRight] = maleLegRightParts;
                characterBody[ModularBodyPart.LegLeft] = maleLegLeftParts;
            } else if (characterGender == Gender.Female) {
                characterBody[ModularBodyPart.Helmet] = femaleHelmetParts;
                characterBody[ModularBodyPart.Head] = femaleHeadParts;
                characterBody[ModularBodyPart.Eyebrow] = femaleEyebrowParts;
                characterBody[ModularBodyPart.FacialHair] = femaleFacialHairParts;
                characterBody[ModularBodyPart.Torso] = femaleTorsoParts;
                characterBody[ModularBodyPart.ArmUpperRight] = femaleArmUpperRightParts;
                characterBody[ModularBodyPart.ArmUpperLeft] = femaleArmUpperLeftParts;
                characterBody[ModularBodyPart.ArmLowerRight] = femaleArmLowerRightParts;
                characterBody[ModularBodyPart.ArmLowerLeft] = femaleArmLowerLeftParts;
                characterBody[ModularBodyPart.HandLeft] = femaleHandRightParts;
                characterBody[ModularBodyPart.HandRight] = femaleHandLeftParts;
                characterBody[ModularBodyPart.Hips] = femaleHipsParts;
                characterBody[ModularBodyPart.LegRight] = femaleLegRightParts;
                characterBody[ModularBodyPart.LegLeft] = femaleLegLeftParts;
            }
        }

        private void SetupActivePartsDictionary() {
            maleBaseBody.Clear();
            femaleBaseBody.Clear();

            foreach (var bodyPart in characterBody) {
                int activeID = 0;
                foreach (var part in bodyPart.Value) {
                    if (part.activeSelf) {
                        activeParts[bodyPart.Key] = Array.IndexOf(bodyPart.Value, part);
                        activeID = Array.IndexOf(bodyPart.Value, part);
                    }
                }
                if (bodyPart.Key.IsBaseBodyPart()) {
                    SetupBodyLists(bodyPart.Key, activeID);
                }
            }
        }

        void UpdatePlayerAppearance () {
            Characteristics.instance.playerName = characterName;
            
            SetupNewCharacter(baseGender, runtimeMaterial);
            
            if (baseHeadID != -1) ActivatePart(ModularBodyPart.Head, baseHeadID);
            else DeactivatePart(ModularBodyPart.Head);
            if (baseEarID != -1) ActivatePart(ModularBodyPart.Ear, baseEarID);
            else DeactivatePart(ModularBodyPart.Ear);
            if (baseEyebrowID != -1) ActivatePart(ModularBodyPart.Eyebrow, baseEyebrowID);
            else DeactivatePart(ModularBodyPart.Eyebrow);
            if (baseFacialHairID != -1) ActivatePart(ModularBodyPart.FacialHair, baseFacialHairID);
            else DeactivatePart(ModularBodyPart.FacialHair);
            if (baseHairID != -1) ActivatePart(ModularBodyPart.Hair, baseHairID);
            else DeactivatePart(ModularBodyPart.Hair);

            characterMaterial.SetColor("_Color_Skin", baseSkinColor);
            characterMaterial.SetColor("_Color_Hair", baseHairColor);
            characterMaterial.SetColor("_Color_Eyes", baseEyesColor);
            characterMaterial.SetColor("_Color_Stubble", baseStubbleColor);
            characterMaterial.SetColor("_Color_BodyArt", baseBodyArtColor);
            characterMaterial.SetColor("_Color_Scar", baseScarColor);
        }
        #endregion

        #region ListSetup
        private void SetupBodyLists(ModularBodyPart bodyPart, int activeID) {
            if (bodyPart.IsHeadPart()) {
                maleBaseBody.Add(new BodyPartLinker(bodyPart, activeID));
                femaleBaseBody.Add(new BodyPartLinker(bodyPart, activeID));
            } else {
                maleBaseBody.Add(new BodyPartLinker(bodyPart, 0));
                femaleBaseBody.Add(new BodyPartLinker(bodyPart, 0));
            }
        }
        #endregion

        #region ISavable
        
        public LoadPriority loadPriority {
            get {
                return LoadPriority.First;
            }
        }

        public void Save()
        {
            //
        }

        public void Load()
        {
            string saveFilePath = SaveManager.instance.getCurrentCharacterFolderPath("characterAppearance");
            
            characterName = ES3.Load<string>("name", saveFilePath, "Unnamed");
            baseGender = ES3.Load<Gender>("gender", saveFilePath, Gender.Male);
            
            baseHeadID = ES3.Load<int>("headID", saveFilePath, 0);
            baseHairID = ES3.Load<int>("hairID", saveFilePath, -1);
            baseEyebrowID = ES3.Load<int>("eyebrowID", saveFilePath, -1);
            baseEarID = ES3.Load<int>("earID", saveFilePath, -1);
            baseFacialHairID = ES3.Load<int>("facialHairID", saveFilePath, -1);
            
            baseSkinColor = ES3.Load<Color>("skinColor", saveFilePath, new Color(255f/255, 204f/255, 174f/255));
            baseHairColor = ES3.Load<Color>("hairColor", saveFilePath, new Color(96f/255, 33f/255, 0/255));
            baseEyesColor = ES3.Load<Color>("eyesColor", saveFilePath, new Color(0, 0, 0));
            baseStubbleColor = ES3.Load<Color>("stubbleColor", saveFilePath, new Color(205f/255, 179f/255, 161f/255));
            baseBodyArtColor = ES3.Load<Color>("bodyArtColor", saveFilePath, new Color(79f/255, 96f/255, 165f/255)); 
            baseScarColor = ES3.Load<Color>("scarColor", saveFilePath, new Color(237f/255, 175f/255, 151f/255));

            runtimeMaterial = new Material(characterMaterial);
            runtimeMaterial.name = "RuntimeCharMaterial";

            UpdatePlayerAppearance();
        }
        
        #endregion
    }
}
