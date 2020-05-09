using UnityEngine;
using MalbersAnimations.Events;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Utilities;

namespace MalbersAnimations
{
    [System.Serializable]
    public class BodyPart
    {
        /// <summary>
        /// Name of the BodyPart
        /// </summary>
        public string name = "member";
        /// <summary>
        /// if True the separated limb will be instantiated... else is already on scene and it will be enabled
        /// </summary>
        public bool Instantiate = true;
        /// <summary>
        /// Life of the limb on the scene
        /// </summary>
        public float life = 10f;

        /// <summary>
        /// The Game Object to Instantiate or show when Dismember is called
        /// </summary>
        public Limb member;
        /// <summary>
        /// this variable gets enabled when the limb is intantiated... that way it will be used one time
        /// </summary>
        public bool dismembered = false;

        /// <summary>
        /// The Attached limb on the Animal.. this gameobject will be hide after is dismemered
        /// </summary>
        public GameObject AttachedLimb;
        /// <summary>
        /// Attached Limb Bones... this ones are use to aling the Dismembered bones
        /// </summary>
        public List<Transform> AttachedLimbBones;

        public UnityEvent OnDismember = new UnityEvent();



        public BodyPart()
        {
            name = "member";
            Instantiate = true;
            dismembered = false;
            life = 10f;
        }
    }

    public class Dismember : MonoBehaviour
    {

        public List<BodyPart> bodyParts;

        /// <summary>
        ///  The Current Material Item on the Animal to Update the Limbs Material Items
        /// </summary>
        public string MaterialItemName;
        
        /// <summary>
        /// Store the Index of the Current Material Item on the Animal to Update on the Limbs Material Items
        /// </summary>
        protected int CurrentMaterialItemIndex;


        public void Awake()
        {
            if (MaterialItemName != string.Empty)
            {
                MaterialChanger AnimalMaterialChanger = GetComponentInChildren<MaterialChanger>();
                MaterialItem MaterialItemLimbs = AnimalMaterialChanger.materialList.Find(mat => mat.Name.ToLower() == MaterialItemName.ToLower()); //Find 

                if (MaterialItemLimbs != null)
                {
                    CurrentMaterialItemIndex = MaterialItemLimbs.current;
                    MaterialItemLimbs.OnMaterialChanged.AddListener(UpdateMaterialItemIndex);
                }
            }
        }

        /// <summary>
        /// Updates the Index of the used Material Item
        /// </summary>
        /// <param name="value"></param>
        private void UpdateMaterialItemIndex(int value)
        {
            CurrentMaterialItemIndex = value;
        }


        /// <summary>
        /// Dismember a random BodyPart
        /// </summary>
        public void _DismemberLimb()
        {
            _DismemberLimb(bodyParts[Random.Range(0, bodyParts.Count)]);
        }

        /// <summary>
        /// Dismember a limb given an Index from the body part list
        /// </summary>
        public void _DismemberLimb(int bodypartIndex)
        {
            if (bodypartIndex < bodyParts.Count && bodypartIndex>=0)
            {
                _DismemberLimb(bodyParts[bodypartIndex]);
            }
            else
            {
                Debug.LogWarning("Wrong index... or the BodyPart is Empty");
            }
        }

        /// <summary>
        /// Dismember a limb given an Name from the body part list
        /// </summary>
        public void _DismemberLimb(string bodypartName)
        {
            BodyPart bodyPart = bodyParts.Find(item => item.name.ToLower() == bodypartName.ToLower()); 

            if (bodyPart != null)
            {
                _DismemberLimb(bodyPart);
            }
            else
            {
                Debug.LogWarning("There's no body part named "+bodypartName);
            }
        }

        /// <summary>
        /// Dismember a limb given an bodypart
        /// </summary>
        public void _DismemberLimb(BodyPart bodypart)
        {
            if (bodypart == null)
            {
                Debug.LogWarning("The Body Part is empty");
                return;
            }

            if (bodypart.dismembered) return; //If the limb has being already dismembered skip

            GameObject Limb = null;

            if (bodypart.member)
            {
                Limb = bodypart.Instantiate ? Instantiate(bodypart.member.gameObject) : bodypart.member.gameObject; //Get the Limb
                Limb.SetActive(true);               //Enable the GameObject

                for (int i = 0; i < bodypart.AttachedLimbBones.Count; i++)        //Set Position of the Dismembered Limb on the Attached Limb
                {
                    bodypart.member.Bones[i].position = bodypart.AttachedLimbBones[i].position;
                    bodypart.member.Bones[i].rotation = bodypart.AttachedLimbBones[i].rotation;
                    bodypart.member.Bones[i].localScale = bodypart.AttachedLimbBones[i].localScale;
                    bodypart.AttachedLimbBones[i].gameObject.SetActive(false);
                }

                UpdateMaterialDismemberLimb(Limb);
            }
           

            bodypart.dismembered = true;

            if (bodypart.AttachedLimb) bodypart.AttachedLimb.SetActive(false);  //Hide the Attached Body Part 

            if (Limb && bodypart.life>0)
            {
                Destroy(Limb, bodypart.life);
            }
        }


        /// <summary>
        /// Updates the Material Changer Component on the Dismembered Limbs
        /// </summary>
        public void UpdateMaterialDismemberLimb(GameObject limb)
        {
            MaterialChanger LimbMaterial = limb.GetComponent<MaterialChanger>();

            if (LimbMaterial != null && MaterialItemName != string.Empty && CurrentMaterialItemIndex != -1)
            {
                LimbMaterial.SetMaterial(MaterialItemName, CurrentMaterialItemIndex);
            }
        }
    }
}