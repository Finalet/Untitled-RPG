using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Controller.AI
{
    public enum LookFor { AnimalPlayer, Tags, UnityTags, Zones, GameObject, ClosestWayPoint, CurrentTarget, TransformVar, GameObjectVar }


    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Look", order = -101)]
    public class LookDecision : MAIDecision
    {
        public override string DisplayName => "General/Look";



        /// <summary>Range for Looking forward and Finding something</summary>
        [Space, Tooltip("Range for Looking forward and Finding something")]
        public FloatReference LookRange = new FloatReference(15);
        [Range(0, 360)]
        /// <summary>Angle of Vision of the Animal</summary>
        [Tooltip("Angle of Vision of the Animal")]
        public float LookAngle = 120;

        /// <summary>What to look for?? </summary>
        [Space, Tooltip("What to look for??")]
        public LookFor lookFor = LookFor.AnimalPlayer;
        [Tooltip("Layers that can block the Animal Eyes")]
        public LayerReference ObstacleLayer = new LayerReference(1);


        [Space(20), Tooltip("If the what we are looking for is found then Assign it as a new Target")]
        public bool AssignTarget = true;
        [Tooltip("If the what we are looking for is found then also start moving")]
        public bool MoveToTarget = true;
        [Tooltip("Remove Target when loose sight:\nIf the Target No longer on the Field of View: Set the Target from the AIControl as NULL")]
        public bool RemoveTarget = false;

        [Space]
        [Tooltip("Look for this Unity Tag on an Object")]
        public string UnityTag = string.Empty;
        [Tooltip("Look for an Specific GameObject by its name")]
        public string GameObjectName = string.Empty;


        [RequiredField, Tooltip("Transform Reference value. This value should be set by a Transform Hook Component")]
        public TransformVar transform;
        [RequiredField, Tooltip("GameObject Reference value. This value should be set by a GameObject Hook Component")]
        public GameObjectVar gameObject;

        /// <summary>Custom Tags you want to find</summary>
        [Tooltip("Custom Tags you want to find")]
        public Tag[] tags;
        /// <summary>Type of Zone we want to find</summary>
        [Tooltip("Type of Zone we want to find")]
        public ZoneType zoneType;
        /// <summary>ID value of the Zone we want to find</summary>
        [Tooltip("ID value of the Zone we want to find")]
        public int ZoneID;

        [Tooltip("Mode Zone Index")]
        public int ZoneModeAbility = -1;

        public Color debugColor = new Color(0, 0, 0.7f, 0.3f);

        void Reset() { Description = "The Animal will look for an Object using a cone view"; }


        public override bool Decide(MAnimalBrain brain, int index) => Look_For(brain,false);

        public override void FinishDecision(MAnimalBrain brain, int Index)
        {
            Look_For(brain, true); //This will assign the Target in case its true
        }

        private bool Look_For(MAnimalBrain brain, bool assign)
        {
            switch (lookFor)
            {
                case LookFor.AnimalPlayer: return LookForAnimalPlayer(brain, assign);
                case LookFor.Tags: return LookForTags(brain, assign);
                case LookFor.UnityTags: return LookForUnityTags(brain, assign);
                case LookFor.Zones: return LookForZones(brain, assign);
                case LookFor.GameObject: return LookForGameObject(brain, assign);
                case LookFor.ClosestWayPoint: return LookForClosestWaypoint(brain, assign);
                case LookFor.CurrentTarget: return LookForTarget(brain, assign);
                case LookFor.TransformVar: return LookForTransformVar(brain, assign);
                case LookFor.GameObjectVar: return LookForGoVar(brain, assign);

                default: return false;
            }
        }

        public bool LookForTarget(MAnimalBrain brain, bool assign)
        {
            if (brain.Target == null) return false;

            return IsInFieldOfView(brain, brain.TargetAnimal ? brain.TargetAnimal.Center : brain.Target.position, brain.Target, assign);
        }

        public bool LookForTransformVar(MAnimalBrain brain, bool assign)
        {
            if (transform == null || transform.Value == null) return false;
            return IsInFieldOfView(brain, transform.Value.position, transform.Value, assign);
        }

        public bool LookForGoVar(MAnimalBrain brain, bool assign)
        {
            if (gameObject == null && gameObject.Value && !gameObject.Value.IsPrefab()) return false;
            return IsInFieldOfView(brain, gameObject.Value.transform.position, gameObject.Value.transform, assign);
        }


        private bool IsInFieldOfView(MAnimalBrain brain, Vector3 TargetCenter, Transform target, bool assign)
        {
            var result = LookAngle <= 0 && LookRange <= 0 && ObstacleLayer.Value == 0;

            if (result) //Fast Result
            {
                AssignMoveTarget(brain, target, assign);
                return result;
            }

            var Direction_to_Target = (TargetCenter + new Vector3(0, 0.001f, 0) - brain.Eyes.position); //Put the Sight a bit higher

            var Distance_to_Target = Vector3.Distance(TargetCenter, brain.Eyes.position) * 0.999f; //Important, otherwise it will find the ground for Objects to close to it


            if (LookRange == 0 || Distance_to_Target < LookRange) //Check if whe are inside the Look Radius
            {
                Vector3 EyesForward = Vector3.ProjectOnPlane(brain.Eyes.forward, brain.Animal.UpVector);

                if (LookAngle == 0 || Vector3.Dot(Direction_to_Target.normalized, EyesForward) > Mathf.Cos(LookAngle * 0.5f * Mathf.Deg2Rad)) //Mean is in Range:
                {
                    //Need a RayCast to see if there's no obstacle in front of the Animal OBSTACLE LAYER
                    if (ObstacleLayer != 0 && 
                        Physics.Raycast(brain.Eyes.position, Direction_to_Target, out RaycastHit hit, Distance_to_Target, ObstacleLayer, QueryTriggerInteraction.Ignore))
                    {
                        if (brain.debug)
                        {
                           
                            Debug.DrawLine(brain.Eyes.position, hit.point, Color.green, 0.5f);
                            Debug.DrawLine(hit.point, TargetCenter, Color.red, 0.5f);
                        }
                        result = false; //Meaning there's something between the Eyes of the Animal and the Target Animal
                    }
                    else
                    {
                        result = true;
                        if (brain.debug) Debug.DrawRay(brain.Eyes.position, Direction_to_Target, Color.green, 0.5f);
                    }
                }
            }

            if (result) AssignMoveTarget(brain, target, assign);

            return result;
        }

        private void AssignMoveTarget(MAnimalBrain brain, Transform target, bool assign)
        {
            if (assign)
            {
                if (AssignTarget) brain.AIMovement.SetTarget(target, MoveToTarget);
                else if (RemoveTarget) brain.AIMovement.ClearTarget();
            } 
        }

        public bool LookForZones(MAnimalBrain brain, bool assign)
        {
            var AllZones = Zone.Zones;

            float minDistance = float.MaxValue;
            Zone FoundZone = null; 

            foreach (var zone in AllZones)
            {
                if (zone.zoneType == zoneType && ZoneID > 0 && zone.ZoneID == ZoneID)
                {
                    if (zone.zoneType == ZoneType.Mode)
                        if (ZoneModeAbility == -1 || zone.ModeAbilityIndex == ZoneModeAbility)
                        {
                            if (MinimumDistance(zone.transform.position, brain.Eyes.position, out float Distance, minDistance))
                            {
                                minDistance = Distance;
                                FoundZone = zone;
                            }
                        }
                }
            }

            if (FoundZone)
                return IsInFieldOfView(brain, FoundZone.transform.position, FoundZone.transform, assign);  //Find if is inside the Field of view

            return false;
        }

        public bool LookForTags(MAnimalBrain brain, bool assign)
        {
            var AllTagsHolders = Tags.TagsHolders;

            float minDistance = float.MaxValue;
            Tags FoundTagHolder = null;

            foreach (var tagsH in AllTagsHolders)
            {
                if (MinimumDistance(tagsH.transform.position, brain.Eyes.position, out float Distance, minDistance) && tagsH.HasTag(tags))
                {
                    minDistance = Distance;
                    FoundTagHolder = tagsH;
                }
            }

            if (FoundTagHolder)
            {
                return IsInFieldOfView(brain, FoundTagHolder.transform.position, FoundTagHolder.transform, assign);  //Find if is inside the Field of view
            }
            return false;
        }


        private bool MinimumDistance(Vector3 Pos1, Vector3 Pos2, out float Distance, float MinDist)
        {
            Distance = float.MaxValue;
            if (LookRange <= 0) return true;
            Distance = Vector3.Distance(Pos1, Pos2);
            return (Distance < MinDist && Distance < LookRange);
        }

        public bool LookForGameObject(MAnimalBrain brain, bool assign)
        {
            if (string.IsNullOrEmpty(GameObjectName)) return false;

            var gameObject = GameObject.Find(GameObjectName);
            if (gameObject)
            {
                return IsInFieldOfView(brain, gameObject.transform.position, gameObject.transform, assign);   //Find if is inside the Field of view
            }
            return false;
        }
        public bool LookForClosestWaypoint(MAnimalBrain brain, bool assign)
        {
            var allWaypoints = MWayPoint.WayPoints;
            float minDistance = float.MaxValue;
            MWayPoint closestWayPoint = null;

            foreach (var way in allWaypoints)
            {
                if (MinimumDistance(way.GetPosition(), brain.Eyes.position, out float Distance, minDistance))
                {
                    minDistance = Distance;
                    closestWayPoint = way;
                }
            }

            if (closestWayPoint)
            {
                return IsInFieldOfView(brain, closestWayPoint.GetPosition(), closestWayPoint.WPTransform, assign); //Find if is inside the Field of view
            }
            return false;
        }
        private bool LookForAnimalPlayer(MAnimalBrain brain, bool assign)
        {
            if (MAnimal.MainAnimal == null || MAnimal.MainAnimal.ActiveStateID == StateEnum.Death) return false; //Means the animal is death or Disable

            return IsInFieldOfView(brain, MAnimal.MainAnimal.Center, MAnimal.MainAnimal.transform, assign);
        }
        public bool LookForUnityTags(MAnimalBrain brain, bool assign)
        {
            if (string.IsNullOrEmpty(UnityTag)) return false;

            var AllTags = GameObject.FindGameObjectsWithTag(UnityTag);

            float minDistance = float.MaxValue;
            GameObject closestTag = null;

            foreach (var way in AllTags)
            {
                if (MinimumDistance(way.transform.position, brain.Eyes.position, out float Distance, minDistance))
                {
                    minDistance = Distance;
                    closestTag = way;
                }
            }

            if (closestTag)
                return IsInFieldOfView(brain, closestTag.transform.position, closestTag.transform, assign);  //Find if is inside the Field of view


            return false;
        }


        



#if UNITY_EDITOR
        public override void DrawGizmos(MAnimalBrain brain)
        {
            var Eyes = brain.Eyes;

            if (Eyes && brain.AIMovement)
            {
                Color c = debugColor;
                c.a = 1f;

                Vector3 EyesForward = Vector3.ProjectOnPlane(brain.Eyes.forward, brain.Animal.UpVector);

                Vector3 rotatedForward = Quaternion.Euler(0, -LookAngle * 0.5f, 0) * EyesForward;
                UnityEditor.Handles.color = c;
                UnityEditor.Handles.DrawWireArc(Eyes.position, Vector3.up, rotatedForward, LookAngle, LookRange);
                UnityEditor.Handles.color = debugColor;
                UnityEditor.Handles.DrawSolidArc(Eyes.position, Vector3.up, rotatedForward, LookAngle, LookRange);
            }
        }
#endif
    }



    /// <summary>  Inspector!!!  </summary>

#if UNITY_EDITOR

    [CustomEditor(typeof(LookDecision))]
    [CanEditMultipleObjects]
    public class LookDecisionEditor : Editor
    {
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));

        SerializedProperty
            Description, UnityTag, debugColor, zoneType, ZoneID, tags, LookRange, LookAngle, lookFor, transform, gameobject,
            MessageID, send, interval, ObstacleLayer, MoveToTarget, AssignTarget, GameObjectName, RemoveTarget, ZoneModeIndex;



        MonoScript script;
        private void OnEnable()
        {
            script = MonoScript.FromScriptableObject((ScriptableObject)target);

            Description = serializedObject.FindProperty("Description");
            tags = serializedObject.FindProperty("tags");
            RemoveTarget = serializedObject.FindProperty("RemoveTarget");
            GameObjectName = serializedObject.FindProperty("GameObjectName");
            UnityTag = serializedObject.FindProperty("UnityTag");
            LookRange = serializedObject.FindProperty("LookRange");
            zoneType = serializedObject.FindProperty("zoneType");
            lookFor = serializedObject.FindProperty("lookFor");
            MessageID = serializedObject.FindProperty("DecisionID");
            send = serializedObject.FindProperty("send");
            interval = serializedObject.FindProperty("interval");
            LookAngle = serializedObject.FindProperty("LookAngle");
            ObstacleLayer = serializedObject.FindProperty("ObstacleLayer");
            AssignTarget = serializedObject.FindProperty("AssignTarget");
            MoveToTarget = serializedObject.FindProperty("MoveToTarget");
            debugColor = serializedObject.FindProperty("debugColor");
            ZoneID = serializedObject.FindProperty("ZoneID");
            ZoneModeIndex = serializedObject.FindProperty("ZoneModeAbility");
            transform = serializedObject.FindProperty("transform");
            gameobject = serializedObject.FindProperty("gameObject");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //EditorGUILayout.BeginVertical(StyleBlue);
            //EditorGUILayout.HelpBox("Look Decision for the AI Brain", MessageType.None);
            //EditorGUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();

            //   EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            {
                EditorGUILayout.PropertyField(Description);
                EditorGUILayout.PropertyField(MessageID);
                EditorGUILayout.PropertyField(send);
                EditorGUILayout.PropertyField(interval);
                EditorGUILayout.PropertyField(LookRange);
                EditorGUILayout.PropertyField(LookAngle);

                EditorGUILayout.PropertyField(lookFor);
                EditorGUILayout.PropertyField(ObstacleLayer);



                LookFor lookforval = (LookFor)lookFor.intValue;

                switch (lookforval)
                {
                    case LookFor.AnimalPlayer:
                        break;
                    case LookFor.Tags:
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(tags, true);
                        EditorGUI.indentLevel--;
                        break;
                    case LookFor.UnityTags:
                        EditorGUILayout.PropertyField(UnityTag);
                        break;
                    case LookFor.Zones:
                        EditorGUILayout.PropertyField(zoneType, new GUIContent("Zone Type", "Choose between a Mode or a State for the Zone"));
                        EditorGUILayout.PropertyField(ZoneID);

                        if (ZoneID.intValue < 1)
                            EditorGUILayout.HelpBox("If ID is set to Zero, it will search for any " + ((ZoneType)zoneType.intValue).ToString() + " Zone.", MessageType.None);

                        if (zoneType.intValue == 0)
                        {
                            EditorGUILayout.PropertyField(ZoneModeIndex);

                            if (ZoneModeIndex.intValue == -1)
                                EditorGUILayout.HelpBox("When [ID = -1], it will search for any " + ((ZoneType)zoneType.intValue).ToString() + " Zone", MessageType.None);
                            else
                                EditorGUILayout.HelpBox("It will search for a " + ((ZoneType)zoneType.intValue).ToString() + " Zone with the ID equals to " + ZoneModeIndex.intValue.ToString(), MessageType.None);
                        }
                        break;
                    case LookFor.GameObject:
                        EditorGUILayout.PropertyField(GameObjectName, new GUIContent("GameObject"));
                        break;
                    case LookFor.ClosestWayPoint:
                        break;
                    case LookFor.CurrentTarget:
                        break;
                    case LookFor.TransformVar:
                        EditorGUILayout.PropertyField(transform);
                        break;
                    case LookFor.GameObjectVar:
                        EditorGUILayout.PropertyField(gameobject);
                        break;
                    default:
                        break;
                }

                EditorGUILayout.PropertyField(AssignTarget);
                EditorGUILayout.PropertyField(MoveToTarget);

                if (!AssignTarget.boolValue)
                {
                    EditorGUILayout.PropertyField(RemoveTarget);
                }
                else
                {
                    RemoveTarget.boolValue = false;
                }


                EditorGUILayout.PropertyField(debugColor);

            }
            // EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Look Decision Inspector");
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
