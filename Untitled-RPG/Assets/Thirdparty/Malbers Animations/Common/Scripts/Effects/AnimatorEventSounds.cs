using System.Collections.Generic;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Animator Event Sound")]

    public class AnimatorEventSounds : MonoBehaviour, IAnimatorListener
    {
        public List<EventSound> m_EventSound;
        public AudioSource _audioSource;
        protected Animator anim;

        void Start()
        {
            anim = GetComponent<Animator>();                    //Get the reference for the animator

            if (_audioSource == null)                           //if there's no audio source add one..
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.volume = 0;
        }


        public virtual void DisableSoundEvent(string SoundName)
        {
            EventSound SoundEvent = m_EventSound.Find(item => item.name == SoundName);
            if (SoundEvent != null) SoundEvent.active = false;
        }

        public virtual void EnableSoundEvent(string SoundName)
        {
            EventSound SoundEvent = m_EventSound.Find(item => item.name == SoundName);
            if (SoundEvent != null) SoundEvent.active = true;
        }

        public virtual void PlaySound(AnimationEvent e)
        {
            if (e.animatorClipInfo.weight < 0.1) return; // if is too small the weight of the animation clip do nothing

            var SoundEvent = m_EventSound.Find(item => item.name == e.stringParameter && item.active == true);

            if (SoundEvent != null)
            {
                SoundEvent.VolumeWeight = e.animatorClipInfo.weight;

                if (anim) _audioSource.pitch = anim.speed;                     //Match the AnimatorSpeed with the Sound Pitch

                if (_audioSource.isPlaying)                                         //If the Audio is already Playing play the one that has more weight
                {
                    if (SoundEvent.VolumeWeight * SoundEvent.volume > _audioSource.volume)
                    {
                        SoundEvent.PlayAudio(_audioSource);
                    }
                }
                else
                {
                    SoundEvent.PlayAudio(_audioSource);
                }
            }
        }

        public virtual void PlaySound(string sound)
        {
            var SoundEvent = m_EventSound.Find(item => item.name == sound && item.active == true);

            if (SoundEvent != null)
            {
                SoundEvent.VolumeWeight = 1;

                if (_audioSource.isPlaying)                                         //If the Audio is already Playing play the one that has more weight
                {
                    if (SoundEvent.volume > _audioSource.volume)
                        SoundEvent.PlayAudio(_audioSource);
                }
                else
                {
                    SoundEvent.PlayAudio(_audioSource);
                }
            }
        }

        public virtual void PlaySoundForever(string sound)
        {
            var SoundEvent = m_EventSound.Find(item => item.name == sound && item.active == true);

            if (SoundEvent != null)
            {
                SoundEvent.VolumeWeight = 1;

                StartCoroutine(C_Playforever(SoundEvent));
            }
        }

        public virtual void StopPlaying(string sound)
        {
            var SoundEvent = m_EventSound.Find(item => item.name == sound && item.active == true);

            if (SoundEvent != null && SoundEvent.source != null)
            {
                if (SoundEvent.source.isPlaying)
                    SoundEvent.source.Stop();
            }

            StopAllCoroutines();
        }

        IEnumerator C_Playforever(EventSound E_sound)
        {
            if (E_sound.interval <= 0)
            {
                yield return null;
            }
            else
            {
                var timeInterval = new WaitForSeconds(E_sound.interval);
                while (true)
                {
                    E_sound.PlayAudio(_audioSource);
                    yield return timeInterval;
                }
            }

            yield return null;
        }


        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);
    }

    [System.Serializable]
    public class EventSound
    {
        [Tooltip("Name of the Sound you will call on the Animation Event")]
        public string name = "Name Here";

        public float volume = 1;

        public float pitch = 1;
        public bool active = true;
        [Tooltip("Interval Time to play a sound forever. Remember to call [PlaySoundForever]")]
        public float interval = 0f;
        public AudioSource source;
        public AudioClip[] Clips;

        /// <summary>Weight of the sound depeding of the Animation Weight</summary>
        public float VolumeWeight { get; internal set; }

        public void PlayAudio(AudioSource audio)
        {
            if (source == null) source = audio;
            if (source == null) return;                            //Do nothing if the audio is empty
            if (Clips == null || Clips.Length == 0) return;         //Do nothing if there's no clips 

            source.spatialBlend = 1;                                 //Set the sound to 3D

            source.clip = Clips[Random.Range(0, Clips.Length)];     //Set a random clip to the audio Source
            source.pitch = pitch;                                   //Depending the animator speed modify the pitch
            source.volume = Mathf.Clamp01(volume * VolumeWeight);    //Depending the weight of the animation clip modify the volume
            source.Play();                                           //Play the Audio
        }
    }





#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(AnimatorEventSounds))]
    public class AnimEventSoundEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty m_EventSound;
        private AnimatorEventSounds M;

        private void OnEnable()
        {
            M = ((AnimatorEventSounds)target);

            m_EventSound = serializedObject.FindProperty("m_EventSound");

            list = new ReorderableList(serializedObject, m_EventSound, true, true, true, true);
            list.drawElementCallback = DrawElementCallback;
            list.drawHeaderCallback = HeaderCallbackDelegate;
            list.onAddCallback = OnAddCallBack;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Receive animations events from the animations clips to play sounds using the method: PlaySound(string Name)");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MTools.StyleGray);
                {

                    list.DoLayoutList();

                    if (list.index != -1)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            SerializedProperty Element = m_EventSound.GetArrayElementAtIndex(list.index);
                            EditorGUILayout.LabelField("[" + M.m_EventSound[list.index].name + "]", EditorStyles.boldLabel);
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("interval"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("Clips"), new GUIContent("Clips", "AudioClips"), true);
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("source"), new GUIContent("Internal Source", "AudioSource"));
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioSource"), new GUIContent("Global Source", "AudioSource"), true);
                    }
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Animation Event Sound");
                //  EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_1 = new Rect(rect.x + 28, rect.y, (rect.width) / 3 + 25, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Name");

            Rect R_2 = new Rect(rect.x + (rect.width) / 3 + 65, rect.y, (rect.width) / 3, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_2, "Volume");

            Rect R_3 = new Rect(rect.x + ((rect.width) / 3) * 2 + 40, rect.y, ((rect.width) / 3), EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Pitch");

        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_EventSound.GetArrayElementAtIndex(index);

            var name = element.FindPropertyRelative("name");
            var active = element.FindPropertyRelative("active");
            var volume = element.FindPropertyRelative("volume");
            var pitch = element.FindPropertyRelative("pitch");
            rect.y += 2;

            Rect R_0 = new Rect(rect.x - 3, rect.y, 10, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 15, rect.y, (rect.width) / 3 + 55 - 15, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + (rect.width) / 3 + 61, rect.y, (rect.width) / 3 - 30, EditorGUIUtility.singleLineHeight);
            Rect R_3 = new Rect(rect.x + ((rect.width) / 3) * 2 + 35, rect.y, ((rect.width) / 3) - 35, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(R_0, active, GUIContent.none);
            EditorGUI.PropertyField(R_1, name, GUIContent.none);
            EditorGUI.PropertyField(R_2, volume, GUIContent.none);
            EditorGUI.PropertyField(R_3, pitch, GUIContent.none);
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (M.m_EventSound == null)
            {
                M.m_EventSound = new System.Collections.Generic.List<EventSound>();
            }
            M.m_EventSound.Add(new EventSound());
        }
    }
#endif

}