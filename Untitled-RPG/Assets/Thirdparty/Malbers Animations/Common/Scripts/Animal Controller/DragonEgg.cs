using UnityEngine;
using System.Collections;
using MalbersAnimations.Events;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Controller
{
    /// <summary> Script to manage the logic of the Egg  </summary>
    [AddComponentMenu("Malbers/Animal Controller/Dragon Egg")]
    public class DragonEgg : MonoBehaviour
    {
        public enum HatchType { None, Input, Time };         //Type of Hatch you want to do with the little dragon
        protected Animator anim;

        protected MAnimal animal;

        public Vector3 preHatchOffset;

        public GameObject Dragon;                               //The Dragon to Come out of the egg
        public float removeShells = 10f;
        bool crack_egg;

        [HideInInspector]
        public InputRow input = new InputRow("CrackEgg", KeyCode.Alpha0, InputButton.Down);

        [HideInInspector] public float seconds;

        public HatchType hatchtype;

        public UnityEvent OnEggCrack = new UnityEvent();

        void Start()
        {
            anim = GetComponent<Animator>();

            if (Dragon)
            {
                if (Dragon.IsPrefab()) Dragon = Instantiate(Dragon);

                animal = Dragon.GetComponent<MAnimal>();

                if (animal)
                {
                    animal.transform.position = transform.position;
                    animal.Anim.Play("Hatch");                                       //Set the egg State (This set on the animator INT -10 which is the transition for the EggHatching Start Animation
                    animal.LockInput = true;
                    animal.LockMovement = true;
                    animal.EnableColliders(false);
                    animal.transform.localPosition += preHatchOffset;
                }

                var skinnedMeshes = Dragon.GetComponentsInChildren<Renderer>();

                foreach (var item in skinnedMeshes)
                    item.enabled = false;
            }


            if (hatchtype == HatchType.Time)
            {
                StartCoroutine(TimeCrackEgg());
            }
        }




        void Update()
        {
            switch (hatchtype)
            {
                case HatchType.Input:
                    if (input.GetValue) crack_egg = true;
                    break;
                default:
                    break;
            }

            if (crack_egg)
            {
                CrackEgg();
            }
        }

        IEnumerator TimeCrackEgg()
        {
            yield return new WaitForSeconds(seconds);
            CrackEgg();
        }

        public void CrackEgg()
        {
            var col = GetComponent<Collider>();
            anim.SetInteger("State", 1); //Crak the Egg Animation
            if (col)
                Destroy(col);

            if (animal)
            {
                animal.State_Force(StateEnum.Idle);

                var skinnedMeshes = animal.GetComponentsInChildren<Renderer>();
                foreach (var item in skinnedMeshes) item.enabled = true;

                animal.SetModeStatus(Random.Range(1, 4)); //Set a random Out of the Egg animation
            }

            OnEggCrack.Invoke();

            StartCoroutine(EggDisapear(removeShells));
        }

        void EnableAnimalScript()
        {
            if (animal) animal.enabled = true;
        }

        //Destroy the Game Object
        IEnumerator EggDisapear(float seconds)
        {
            yield return null;
            yield return null;
            if (Dragon) Dragon.transform.position = transform.position; //Restore the position to the egg
            yield return new WaitForSeconds(seconds);
            anim.SetInteger("State", 2);
            yield return new WaitForSeconds(1f);
            Destroy(transform.gameObject);
        }

        //void OnMouseDown()
        //{
        //    if (hatchtype == HatchType.Click && !crack_egg)
        //    {
        //        CrackEgg();
        //    }
        //}
    }


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(DragonEgg))]
    public class DragonEggEditor : Editor
    {
        DragonEgg DE;
        private SerializedProperty input, time, hatchtype;

        private void OnEnable()
        {
            DE = (DragonEgg)target;

            hatchtype = serializedObject.FindProperty("hatchtype");
            input = serializedObject.FindProperty("input");
            time = serializedObject.FindProperty("seconds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Egg Logic");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MTools.StyleGray);
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.HelpBox("Use Baby Dragons or Scale Up the egg", MessageType.None, true);


                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Dragon"), new GUIContent("Dragon", "the Prefab or Gameobject that contains the little dragon"));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("preHatchOffset"), new GUIContent("Pre-Hatch Offset"));

                        EditorGUILayout.PropertyField(hatchtype, new GUIContent("Hatch Type"));

                        DragonEgg.HatchType ht = (DragonEgg.HatchType)hatchtype.enumValueIndex;

                        switch (ht)
                        {
                            case DragonEgg.HatchType.None:
                                EditorGUILayout.HelpBox("Just Call the Method CrackEgg() to activate it", MessageType.Info, true);
                                break;
                            case DragonEgg.HatchType.Time:
                                EditorGUILayout.PropertyField(time, new GUIContent("Time", "ammount of Seconds to Hatch"));
                                break;
                            case DragonEgg.HatchType.Input:
                                EditorGUILayout.PropertyField(input, new GUIContent("Input", "Input assigned in the InputManager to Hatch"));
                                break;
                            default:
                                break;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEggCrack"), new GUIContent("On Egg Crack", "Invoked When the Egg Crack"));
                }
                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Dragon Egg Values Changed");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
