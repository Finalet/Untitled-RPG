using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace MalbersAnimations.Utilities
{
    /// <summary>Redundant functions to be used all over the assets</summary>
    public static class MalbersTools
    {
        public static Camera FindMainCamera()
        {
              var  MainCamera = Camera.main;
                if (MainCamera == null)
                    MainCamera = GameObject.FindObjectOfType<Camera>();

            return MainCamera;
        }


        public static Keyframe[] DefaultCurve = { new Keyframe(0, 0), new Keyframe(1, 1) };
        public static void ResetFloatParameters(Animator animator)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)                          //Set All Float values to their defaut (For all the Float Values on the Controller
            {
                if (animator.IsParameterControlledByCurve(parameter.name)) continue;

                if (parameter.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat(parameter.nameHash, parameter.defaultFloat);
                }
            }
        }

        public static float DeltaAngle(Vector3 rFrom, Vector3 rTo, bool toDegrees = true)
        {
            if (rTo == rFrom) { return 0f; }

            Vector3 lCross = Vector3.Cross(rFrom, rTo);
            float lSign = (lCross.y < -0.0001f ? -1 : 1);

            float lDot = Vector3.Dot(rFrom, rTo);

            return lSign * Mathf.Atan2(lCross.magnitude, lDot) * (toDegrees ? Mathf.Rad2Deg : 1);
        }

        /// <summary>True if the colliders layer is on the layer mask</summary>
        public static bool CollidersLayer(Collider collider, LayerMask layerMask)
        {
            return layerMask == (layerMask | (1 << collider.gameObject.layer));
        }

        public static void DrawTriggers(Transform transform,Collider Trigger, Color DebugColor)
        {
            Gizmos.color = DebugColor;
            var DColorFlat = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1f);

            Gizmos.matrix = transform.localToWorldMatrix;

            if (Trigger && Trigger.enabled)
            {
                if (Trigger is BoxCollider)
                {
                    BoxCollider _C = Trigger as BoxCollider;
                    var sizeX = transform.lossyScale.x * _C.size.x;
                    var sizeY = transform.lossyScale.y * _C.size.y;
                    var sizeZ = transform.lossyScale.z * _C.size.z;
                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(_C.bounds.center, transform.rotation, new Vector3(sizeX, sizeY, sizeZ));

                    Gizmos.matrix = rotationMatrix;
                    if (Application.isPlaying)
                    {
                        Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        Gizmos.color = DColorFlat;
                        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                    }
                }
                else if (Trigger is SphereCollider)
                {
                    SphereCollider _C = Trigger as SphereCollider;

                    Gizmos.matrix = transform.localToWorldMatrix;
                    if (Application.isPlaying)
                    {
                        Gizmos.DrawSphere(Vector3.zero + _C.center, _C.radius);
                        Gizmos.color = DColorFlat;
                        Gizmos.DrawWireSphere(Vector3.zero + _C.center, _C.radius);
                    }

                }
            }
        }


        /// <summary>
        /// Removes the Right and Foward influence from a vector
        /// </summary>
        public static Vector3 CleanUpVector(Vector3 Main, Vector3 Forward, Vector3 Right)
        {
            var CleanVector = Vector3.ProjectOnPlane(Main, Forward);                 //Remove Forward
            CleanVector = Vector3.ProjectOnPlane(CleanVector, Right);                //Remove Horizontal
            return CleanVector;
        }

        /// <summary>True if the colliders layer is on the layer mask</summary>
        public static bool Layer_in_LayerMask(int layer, LayerMask layerMask)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        public static Vector3 NullVector = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        /// <summary>
        /// Returns all the Instances created on the Project for an Scriptable Asset. WORKS ON EDITOR ONLY
        /// </summary>
        public static List<T> GetAllInstances<T>() where T : ScriptableObject
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
                T[] a = new T[guids.Length];

                for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                    a[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                }
                var aA = a.ToList();

                return aA;
            }
#endif
            return null;
        }


        /// <summary>Returns the Instance of an Scriptable Object by its name. WORKS ON EDITOR ONLY</summary>
        public static T GetInstance<T>(string name) where T : ScriptableObject
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var allInstances = GetAllInstances<T>();

                T found = allInstances.Find(x => x.name == name);

                return found;
            }
#endif
            return null;
        }

        public static void DebugCross(Vector3 center, float radius, Color color)
        {
            Debug.DrawLine(center - new Vector3(0, radius, 0), center + new Vector3(0, radius, 0), color);
            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(0, 0, radius), color);
        }

        public static void DebugPlane(Vector3 center, float radius, Color color, bool cross = false)
        {
            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(0, 0, -radius), color);
            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(0, 0, radius), color);
            Debug.DrawLine(center + new Vector3(0, 0, radius), center - new Vector3(-radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(radius, 0, 0), color);

            if (cross)
            {
                Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(radius, 0, 0), color);
                Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(0, 0, radius), color);
            }
        }

        public static void DebugTriangle(Vector3 center, float radius, Color color)
        {
            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(0, 0, radius), color);

            Debug.DrawLine(center - new Vector3(0, -radius, 0), center + new Vector3(radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, -radius, 0), center + new Vector3(-radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, -radius, 0), center + new Vector3(0, 0, radius), color);
            Debug.DrawLine(center - new Vector3(0, -radius, 0), center + new Vector3(0, 0, -radius), color);

            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(0, 0, -radius), color);
            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(0, 0, radius), color);
            Debug.DrawLine(center + new Vector3(0, 0, radius), center - new Vector3(-radius,0, 0), color);
            Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(radius, 0,0), color);
        }

        /// <summary> Set a Layer to the Game Object and all its children</summary>
        public static void  SetLayer(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
                SetLayer(child, layer);
        }

        /// <summary>Calculate a Direction from an origin to a target</summary>
        public static Vector3 DirectionTarget(Transform origin, Transform Target, bool normalized = true)
        {
            return DirectionTarget(origin.position, Target.position, normalized);
        }

        public static Vector3 DirectionTarget(Vector3 origin, Vector3 Target, bool normalized = true)
        {
            if (normalized)
                return (Target - origin).normalized;

            return (Target - origin);
        }

        /// <summary> Serialize a Class to XML</summary>
        public static string Serialize<T>(this T toSerialize)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            StringWriter writer = new StringWriter();
            xml.Serialize(writer, toSerialize);

            return writer.ToString();
        }

        /// <summary>Finds a bit (index) on a Integer</summary>
        public static bool IsBitActive(int IntValue, int index)
        {
            return (IntValue & (1 << index)) != 0;
        }

        /// <summary> DeSerialize a Class with xml</summary>
        public static T Deserialize<T>(this string toDeserialize)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(toDeserialize);

            return (T) xml.Deserialize(reader);
        }

      

        /// <summary>
        /// Gets the horizontal angle between two vectors. The calculation
        /// removes any y components before calculating the angle.
        /// </summary>
        /// <returns>The signed horizontal angle (in degrees).</returns>
        /// <param name="From">Angle representing the starting vector</param>
        /// <param name="To">Angle representing the resulting vector</param>
        public static float HorizontalAngle(Vector3 From, Vector3 To, Vector3 Up)
        {
            float lAngle = Mathf.Atan2(Vector3.Dot(Up, Vector3.Cross(From, To)), Vector3.Dot(From, To));
            lAngle *= Mathf.Rad2Deg;

            if (Mathf.Abs(lAngle) < 0.0001f) { lAngle = 0f; }

            return lAngle;
        }

        /// <summary>
        /// Calculate the direction from the center of the Screen
        /// </summary>
        /// <param name="origin">The start point to calculate the direction</param>
        ///  <param name="hitmask">Just use this layers</param>
        public static Vector3 DirectionFromCamera(Transform origin, float x, float y, out RaycastHit hit, LayerMask hitmask)
        {
            Camera cam = Camera.main;

            hit = new RaycastHit();

            Ray ray = cam.ScreenPointToRay(new Vector2(x * cam.pixelWidth, y * cam.pixelHeight));
            Vector3 dir = ray.direction;

            hit.distance = float.MaxValue;

            RaycastHit[] hits;

            hits = Physics.RaycastAll(ray, 100, hitmask);

            foreach (RaycastHit item in hits)
            {
                if (item.transform.root == origin.transform.root) continue; //Dont Hit anything in this hierarchy
                if (Vector3.Distance(cam.transform.position, item.point) < Vector3.Distance(cam.transform.position, origin.position)) continue; //If I hit something behind me skip
                if (hit.distance > item.distance) hit = item;
            }

            if (hit.distance != float.MaxValue)
            {
                dir = (hit.point - origin.position).normalized;
            }

            return dir;
        }

        /// <summary>
        /// Calculate the direction from the ScreenPoint of the Screen and also saves the RaycastHit Info
        /// </summary>
        /// <param name="origin">The start point to calculate the direction</param>
        ///  <param name="hitmask">Just use this layers</param>
        public static Vector3 DirectionFromCamera(Camera cam,Transform origin, Vector3 ScreenPoint, out RaycastHit hit, LayerMask hitmask, Transform Ignore = null)
        {
            Ray ray = cam.ScreenPointToRay(ScreenPoint);
            Vector3 dir = ray.direction;

            hit = new RaycastHit
            {
                distance = float.MaxValue,
                point = ray.GetPoint(100)
            };
            RaycastHit[] hits;

            hits = Physics.RaycastAll(ray, 100, hitmask);

            foreach (RaycastHit item in hits)
            {
              
                if (item.transform.root == Ignore) continue;                                     //Dont Hit anything the Ingore
                if (item.transform.root == origin.transform.root) continue;                      //Dont Hit anything in this hierarchy
                if (Vector3.Distance(cam.transform.position, item.point) < Vector3.Distance(cam.transform.position, origin.position)) continue; //If I hit something behind me skip
                if (hit.distance > item.distance) hit = item;
            }

            if (hit.distance != float.MaxValue)
            {
                dir = (hit.point - origin.position).normalized;
            }

            return dir;
        }

        /// <summary>
        /// Calculate the direction from the center of the Screen
        /// </summary>
        /// <param name="origin">The start point to calculate the direction</param>
        public static Vector3 DirectionFromCamera(Transform origin)
        {
            RaycastHit p;
            return DirectionFromCamera(origin, 0.5f * Screen.width, 0.5f * Screen.height, out p, -1);
        }


        /// <summary>
        /// Calculate the direction from the center of the Screen
        /// </summary>
        /// <param name="origin">The start point to calculate the direction</param>
        public static Vector3 DirectionFromCamera(Transform origin, LayerMask layerMask)
        {
            RaycastHit p;
            return DirectionFromCamera(origin, 0.5f * Screen.width, 0.5f * Screen.height, out p, layerMask);
        } 


        public static RaycastHit RayCastHitToCenter(Camera cam, Transform origin, Vector3 ScreenCenter, int layerMask = 0)
        {
            RaycastHit hit = new RaycastHit();

            Ray ray = cam.ScreenPointToRay(ScreenCenter);
            Vector3 dir = ray.direction;

            hit.distance = float.MaxValue;

            // RaycastHit[] hits = new RaycastHit[6];

            // int HitsAmmount = Physics.RaycastNonAlloc(ray, hits, 100, layerMask);

            RaycastHit[] hits = Physics.RaycastAll(ray, 100, layerMask);

           // if (HitsAmmount > 0)
            {
                foreach (RaycastHit rayhit in hits)
                {
                    if (rayhit.transform.root == origin.root) continue; //Dont Hit anything in this hierarchy
                    if (Vector3.Distance(cam.transform.position, rayhit.point) < Vector3.Distance(cam.transform.position, origin.position)) continue; //If I hit something behind me skip
                    if (hit.distance > rayhit.distance) hit = rayhit;
                }
            }
            return hit;
        }


        public static Vector3 DirectionFromCameraNoRayCast(Camera cam, Vector3 ScreenCenter)
        {
            Ray ray = cam.ScreenPointToRay(ScreenCenter);

            return ray.direction;
        }

        /// <summary>
        /// Returns a RaycastHit to the center of the screen
        /// </summary>
        public static RaycastHit RayCastHitToCenter(Camera cam, Transform origin)
        {
            var Center = new Vector3(0.5f * Screen.width, 0.5f * Screen.height);

            return RayCastHitToCenter(cam, origin, Center);
        }


        /// <summary>
        /// Returns a RaycastHit to the center of the screen
        /// </summary>
        public static RaycastHit RayCastHitToCenter(Camera cam, Transform origin, LayerMask layerMask)
        {
            var Center = new Vector3(0.5f * Screen.width, 0.5f * Screen.height);

            return RayCastHitToCenter(cam, origin, Center, layerMask);
        }

      

        /// <summary>The angle between dirA and dirB around axis</summary>
        public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
        {
            // Project A and B onto the plane orthogonal target axis
            dirA = dirA - Vector3.Project(dirA, axis);
            dirB = dirB - Vector3.Project(dirB, axis);

            // Find (positive) angle between A and B
            float angle = Vector3.Angle(dirA, dirB);

            // Return angle multiplied with 1 or -1
            return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
        }



       public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
        {
            var vVector1 = vPoint - vA;
            var vVector2 = (vB - vA).normalized;

            var d = Vector3.Distance(vA, vB);
            var t = Vector3.Dot(vVector2, vVector1);

            if (t <= 0)
                return vA;

            if (t >= d)
                return vB;

            var vVector3 = vVector2 * t;

            var vClosestPoint = vA + vVector3;

            return vClosestPoint;
        }



        public static IEnumerator AlignTransform_Position(Transform t1, Vector3 NewPosition, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            Vector3 CurrentPos = t1.position;

            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve
                t1.position = Vector3.LerpUnclamped(CurrentPos, NewPosition, result);
                elapsedTime += Time.deltaTime;

                yield return null;
            }
           t1.position = NewPosition;
        }



        public static Vector3 Quaternion_to_AngularVelocity(Quaternion quaternion)
        {
            float angleInDegrees;
            Vector3 rotationAxis;
            quaternion.ToAngleAxis(out angleInDegrees, out rotationAxis);

            Vector3 angularDisplacement = rotationAxis * angleInDegrees * Mathf.Deg2Rad;
            Vector3 angularVelocity = angularDisplacement / Time.deltaTime;

            return angularVelocity;
        }

        public static IEnumerator AlignTransform_Rotation(Transform t1, Quaternion NewRotation, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            Quaternion CurrentRot = t1.rotation;

            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve
                t1.rotation = Quaternion.LerpUnclamped(CurrentRot, NewRotation, result);
                elapsedTime += Time.deltaTime;

                yield return null;
            }
            t1.rotation = NewRotation;
        }

         

        public static IEnumerator AlignLookAtTransform(Transform t1, Transform t2, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            Quaternion CurrentRot = t1.rotation;
            Vector3 direction = (t2.position - t1.position).normalized;
            direction.y = t1.forward.y;
            Quaternion FinalRot = Quaternion.LookRotation(direction);
            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
            t1.rotation = FinalRot;
        }


        public static IEnumerator AlignLookAtTransform(Transform t1, Vector3 targetPosition, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            Quaternion CurrentRot = t1.rotation;
            Vector3 direction = (targetPosition - t1.position).normalized;

            direction.y = t1.forward.y;

            Quaternion FinalRot = Quaternion.LookRotation(direction);
            
            
            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
            t1.rotation = FinalRot;
        }

        public static IEnumerator AlignTransformRadius(Transform TargetToAlign, Transform AlignOrigin, float time, float radius, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            Vector3 CurrentPos = TargetToAlign.position;

            var Direction = (TargetToAlign.position - AlignOrigin.position).normalized;

            Debug.DrawRay(AlignOrigin.position, Direction, Color.cyan, 2);

            Ray TargetRay = new Ray(AlignOrigin.position, Direction);


            Vector3 TargetPos = TargetRay.GetPoint(radius);
           
            
            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                TargetToAlign.position = Vector3.LerpUnclamped(CurrentPos, TargetPos, result);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
            TargetToAlign.position = TargetPos;
        }

        /// <summary>
        ///  Finds if a parameter exist on a Animator Controller using its name
        /// </summary>
        public static bool FindAnimatorParameter(Animator animator, AnimatorControllerParameterType type, string ParameterName)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)                        
            {
                if (parameter.type == type && parameter.name == ParameterName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds if a parameter exist on a Animator Controller using its nameHash
        /// </summary>
        public static bool FindAnimatorParameter(Animator animator, AnimatorControllerParameterType type, int hash)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == type && parameter.nameHash == hash)
                {
                    return true;
                }
            }
            return false;
        }
    }
}