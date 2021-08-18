using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.HAP
{
    [AddComponentMenu("Malbers/Riding/Rider FPS")]

    public class RiderFPC : MRider
    {
        [Tooltip("If True the FPC will rotate with the Animal")]
        public bool FollowRotation = false;
        [Tooltip("If true, the Rider will keep aligned to Y, while the horse doing Pitch Rotation")]
        public float MountTime = 0.5f;
        public AnimationCurve MountAnim = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
        public AnimationCurve DismountAnim = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
        public Vector2 DismountOffset = new Vector2(2, -1);


        private const float TimeToMountDismount = 0.5f;
        private float CurrentTime;

        public override void MountAnimal()
        {
            if (!CanMount) return;
            if (!MTools.ElapsedTime(TimeToMountDismount, CurrentTime)) return;
            if (debug) Debug.Log($"<b>{name}:<color=cyan> [Mount Animal] </color> </b>");  //Debug

            Start_Mounting();

            CurrentTime = Time.time;
            MainCollider.enabled = false;


            if (MountTime > 0)
            {
                StartCoroutine(C_MountAnim());
            }
            else
            {
                UpdateRiderTransform();
                Vector3 AnimalForward = Vector3.ProjectOnPlane(Montura.transform.forward, Montura.Animal.UpVector);
                transform.rotation = Quaternion.LookRotation(AnimalForward, -Physics.gravity);
            }
        }

        public override void DismountAnimal()
        {
            if (!CanDismount) return;
            if (!MTools.ElapsedTime(TimeToMountDismount, CurrentTime)) return;
            if (debug) Debug.Log($"<b>{name}:<color=cyan> [Dismount Animal] </color> </b>");  //Debug

            MountTrigger = GetDismountTrigger();
            Montura.Mounted = Mounted = false;                                  //Unmount the Animal

            SetMountSide(MountTrigger.DismountID);                               //Update MountSide Parameter In the Animator


            CurrentTime = Time.time;
            Start_Dismounting();
            MainCollider.enabled = false;

            if (MountTime > 0)
            {
                StartCoroutine(C_DismountAnim());
            }
            else
            {
                if (debug) Debug.Log($"<b>{name}:<color=cyan> [Dismount Animal] </color> </b>");  //Debug
                transform.position = new Vector3(MountTrigger.transform.position.x, transform.position.y, MountTrigger.transform.position.z);
                if (RB) RB.velocity = Vector3.zero;
                End_Dismounting();
            }
        }

        private IEnumerator C_MountAnim()
        {
            var rot = Quaternion.FromToRotation(Montura.MountPoint.up, -Gravity.Value) * Montura.MountPoint.rotation;
            yield return MTools.AlignTransform(transform, Montura.MountPoint.position, rot, MountTime, MountAnim);

            End_Mounting();
         
            MainCollider.enabled = true;
        }

        private IEnumerator C_DismountAnim()
        {
            var DismountPos = Vector3.ProjectOnPlane(MountTrigger.transform.position - transform.position, Vector3.up).normalized;
            DismountPos *= DismountOffset.x;
            DismountPos.y = DismountOffset.y;

            Debug.DrawRay(transform.position, DismountPos, Color.yellow, 1);

            yield return MTools.AlignTransform_Position(transform, transform.position + DismountPos, MountTime, DismountAnim);

            End_Dismounting();
            MainCollider.enabled = true;
           
        }



        void Update()
        {
            if ((LinkUpdate & UpdateMode.Update) == UpdateMode.Update) UpdateRiderTransform();
        }

        private void LateUpdate()
        {
            if ((LinkUpdate & UpdateMode.LateUpdate) == UpdateMode.LateUpdate || ForceLateUpdateLink) UpdateRiderTransform();
        }

        private void FixedUpdate()
        {
            if ((LinkUpdate & UpdateMode.FixedUpdate) == UpdateMode.FixedUpdate) UpdateRiderTransform();
        }


        /// <summary>Updates the Rider Position to the Mount Point</summary>
        public override void UpdateRiderTransform()
        {
            if (IsRiding)
            {
                transform.position = Montura.MountPoint.position;

                if (FollowRotation && !Parent.Value)
                {
                    Quaternion Inverse_Rot = Quaternion.Inverse(DeltaRot);
                    Quaternion Delta = Inverse_Rot * Montura.Animal.transform.rotation;
                    transform.rotation *= Delta;
                    transform.rotation = Quaternion.FromToRotation(transform.up, -Gravity.Value) * transform.rotation;
                }
            }
            if (IsMountingDismounting && Montura) DeltaRot = Montura.Animal.transform.rotation;
        }
        Quaternion DeltaRot;
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(RiderFPC), true)]
    public class MRiderFPCEd : MRiderEd
    {

        private SerializedProperty keepStraight, FollowRotation, MountAnim, DismountAnim, MountTime, DismountOffset;
        protected override void OnEnable()
        {
            base.OnEnable();
            // MountOffset = serializedObject.FindProperty("MountOffset");
            FollowRotation = serializedObject.FindProperty("FollowRotation");
            MountAnim = serializedObject.FindProperty("MountAnim");
            DismountAnim = serializedObject.FindProperty("DismountAnim");
            MountTime = serializedObject.FindProperty("MountTime");
            DismountOffset = serializedObject.FindProperty("DismountOffset");
            keepStraight = serializedObject.FindProperty("keepStraight");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("First Person", EditorStyles.boldLabel);

                if (!M.Parent.Value)
                {
                    EditorGUILayout.PropertyField(FollowRotation);
                }
                EditorGUILayout.PropertyField(MountTime);
                EditorGUILayout.PropertyField(MountAnim);
                EditorGUILayout.PropertyField(DismountAnim);
                EditorGUILayout.PropertyField(DismountOffset);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}