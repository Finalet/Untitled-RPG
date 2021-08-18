using UnityEngine;
using MalbersAnimations.Scriptables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    /// <summary>  Make the Calculation for Strafing with Camera or a Target </summary>
    [AddComponentMenu("Malbers/Animal Controller/Strafe")]
    public class MStrafe : MonoBehaviour
    {
    //    public enum StrafeType { Camera, Target, }

    //    public StrafeType SType = StrafeType.Camera;

    //    #region Camera Stuff
    //    /// <summary>Camera Side to use on Strafing</summary>
    //    private float strafeAngle;
      
    //    ///  /// <summary>Main Camera </summary>
    //    [SerializeField] private TransformReference mainCamera = new TransformReference();
    //    [SerializeField] private TransformReference target = new TransformReference();
    //    #endregion

    //    private MAnimal animal;


    //    protected Vector3 Direction;
    //    public bool LeftSide { get; set; }
    //    public float DeltaValue { get; private set; }
    //    public Transform MainCamera { get => mainCamera.Value; set => mainCamera.Value = value; }
    //    public Transform Target { get => target.Value; set => target.Value = value; }

    //    public string m_StrafeAngle = "StrafeAngle";
    //    private int hash_StrafeAngle;

    //    void OnEnable()
    //    {
    //        animal = gameObject.FindComponent<MAnimal>();                     //Catche the MainCamera
    //        MainCamera = MTools.FindMainCamera().transform;
    //        hash_StrafeAngle = Animator.StringToHash(m_StrafeAngle);
    //    }

    //    #region Strafing
    //    /// <summary>Calculate the Strafe Angle using the Camera or a Target</summary>
    //    protected virtual void LookDirection()
    //    {
    //        Direction = transform.forward;

    //        if (Target && SType == StrafeType.Target)         //Check if we have Target First
    //        {
    //            Direction = (Target.position - transform.position);
    //        }
    //        else if (MainCamera && SType == StrafeType.Camera)   //if we do not have Target then use the Main Camera
    //        {
    //            Direction = MainCamera.forward;
    //        }

    //        Direction = Vector3.ProjectOnPlane(Direction, animal.UpVector).normalized;
          
    //        var ForwardNormalized = Vector3.ProjectOnPlane(transform.forward, animal.UpVector).normalized;
           
    //        float NewHorizontalAngle = -(Vector3.SignedAngle(Direction, ForwardNormalized, animal.UpVector));     //Get the Normalized value for the look direction

    //        strafeAngle = NewHorizontalAngle;

    //        if (animal.StrafeNormalize) strafeAngle /= 180;
    //    }

    //    #endregion


    //void OnAnimatorMove()
    //    {
    //        LookDirection();

    //        if (animal.Strafe)
    //        {
    //            DeltaValue = Mathf.Lerp(DeltaValue,
    //                animal.MovementDetected ? animal.ActiveState.MovementStrafe : animal.ActiveState.IdleStrafe,
    //                animal.DeltaTime * 5);

    //            var DesiredRot = transform.rotation * Quaternion.Euler(0, strafeAngle, 0);
    //            transform.rotation = Quaternion.Lerp(transform.rotation, DesiredRot, DeltaValue);
    //        }
    //        else
    //        {
    //            DeltaValue = 0;
    //        }
    //    }
    }


#if UNITY_EDITOR
     [CustomEditor(typeof(MStrafe)), CanEditMultipleObjects]
    public class MStrafeEd : Editor
    {
        //private MStrafe M;

       // SerializedProperty updateAnimator, m_StrafeAngle, Target, MainCamera, SType, Normalize;

        //private void OnEnable()
        //{
        //    M = ((MStrafe)target);
        //    updateAnimator = serializedObject.FindProperty("UpdateAnimator");
        //    Normalize = serializedObject.FindProperty("Normalize");
        //    SType = serializedObject.FindProperty("SType");
        //    MainCamera = serializedObject.FindProperty("mainCamera");
        //    Target = serializedObject.FindProperty("target");
        //    m_StrafeAngle = serializedObject.FindProperty("m_StrafeAngle");
        //}


        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Strafing (Obsolete) Now its inside the Animal Controller");
            //serializedObject.Update();

            //MalbersEditor.DrawDescription("Strafing Logic");

            //EditorGUI.BeginChangeCheck();

            //EditorGUILayout.BeginVertical(MTools.StyleGray);


            //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //EditorGUILayout.PropertyField(SType, new GUIContent("Use", "Use Camera or a Target to Calculate the Strafing"));
            //EditorGUILayout.PropertyField(MainCamera, new GUIContent("Main Camera", "Use the Main Camera for the Strafe Logic"));
            //EditorGUILayout.PropertyField(Target, new GUIContent("Target", "Use a Target for the Strafe Logic"));
            //EditorGUILayout.EndVertical();


            //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //EditorGUILayout.PropertyField(updateAnimator, new GUIContent("Update Animator", "Update Animator with the Parameter"));

            //if (M.UpdateAnimator.Value)
            //{
            //    EditorGUILayout.PropertyField(Normalize, new GUIContent("Normalize", "Normalize the Angle Value. Instead of -180 to 180 it will go from -1 to 1"));
            //    EditorGUILayout.PropertyField(m_StrafeAngle, new GUIContent("Param Name", "Name of the Parameter on The Animator for the Strafing"));
            //}
            //EditorGUILayout.EndVertical();


            //EditorGUILayout.EndVertical();
            //if (EditorGUI.EndChangeCheck())
            //{
            //    Undo.RecordObject(target, "Strafe Inspector");
            //}

            //serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}