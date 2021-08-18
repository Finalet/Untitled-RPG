using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    /// <summary> This class is for using Physic materials for sound effects  </summary>
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Sound By Material")]

    public class SoundByMaterial : MonoBehaviour
    {
        public AudioClip DefaultSound;
        public List<MaterialSound> materialSounds;

        [SerializeField] private AudioSource audioSource;

        protected AudioSource Audio_Source
        {
            get
            {
                if (!audioSource)
                {
                    audioSource = GetComponent<AudioSource>();
                }
                return audioSource;
            }
            set { audioSource = value; }
        }

        public virtual void PlayMaterialSound(RaycastHit hitSurface)
        {
            var hits = hitSurface.collider;
            if (hits)
            {
                PlayMaterialSound(hits.sharedMaterial);
            }
        }

        public virtual void PlayMaterialSound(GameObject hitSurface)
        {
            var hits = hitSurface.GetComponent<Collider>();
            if (hits)
            {
                PlayMaterialSound(hits.sharedMaterial);
            }
        }

        public virtual void PlayMaterialSound(Component hitSurface)
        {
            PlayMaterialSound(hitSurface.gameObject);
        }

        public virtual void PlayMaterialSound(Collider hitSurface)
        {
            PlayMaterialSound(hitSurface.sharedMaterial);
        }

        public virtual void PlayMaterialSound(PhysicMaterial hitSurface)
        {
            if (!Audio_Source)
            {
                Audio_Source = gameObject.AddComponent<AudioSource>();
                Audio_Source.spatialBlend = 1;
            }

            MaterialSound mat = materialSounds.Find(item => item.material == hitSurface);

            if (!Audio_Source.isPlaying)
            {



                if (mat != null)
                {
                    var sound = mat.Sounds[Random.Range(0, mat.Sounds.Length)];
                    Audio_Source.clip = sound;
                    audioSource.Play();
                }
                else
                {
                    if (DefaultSound && Audio_Source.isActiveAndEnabled)
                    {
                        Audio_Source.clip = DefaultSound;
                        audioSource.Play();
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class MaterialSound
    {
        public AudioClip[] Sounds;
        public PhysicMaterial material;
    }


    //INSPECTOR

#if UNITY_EDITOR
    [CustomEditor(typeof(SoundByMaterial))]
    public class SoundByMaterialEd : Editor
    {
        private ReorderableList list;
        SerializedProperty soundbymaterial;
        private SoundByMaterial M;

        private void OnEnable()
        {
            M = (SoundByMaterial)target;

            soundbymaterial = serializedObject.FindProperty("materialSounds");

            list = new ReorderableList(serializedObject, soundbymaterial, true, true, true, true);

            list.drawElementCallback = DrawElementCallback;
            list.drawHeaderCallback = HeaderCallbackDelegate;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Plays the sound matching the physic material on the hit object\nInvoke the method 'PlayMaterialSound'");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MTools.StyleGray);
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("audioSource"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultSound"));
                    EditorGUILayout.EndVertical();

                    list.DoLayoutList();

                    if (list.index != -1)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            SerializedProperty Element = soundbymaterial.GetArrayElementAtIndex(list.index);
                            SerializedProperty SoundElement = Element.FindPropertyRelative("Sounds");

                            MalbersEditor.Arrays(SoundElement);
                        }
                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "SoundByMat Inspector");
            }
            serializedObject.ApplyModifiedProperties();

        }
        void HeaderCallbackDelegate(Rect rect)
        {
            //Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 14, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Sound by Material List ", EditorStyles.miniLabel);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = M.materialSounds[index];
            rect.y += 2;

            Rect R_1 = new Rect(rect.x, rect.y, (rect.width), EditorGUIUtility.singleLineHeight);
            //Rect R_2 = new Rect(rect.x + 25 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 14, EditorGUIUtility.singleLineHeight);

            element.material = (PhysicMaterial)EditorGUI.ObjectField(R_1, element.material, typeof(PhysicMaterial), false);
        }


    }
#endif
}