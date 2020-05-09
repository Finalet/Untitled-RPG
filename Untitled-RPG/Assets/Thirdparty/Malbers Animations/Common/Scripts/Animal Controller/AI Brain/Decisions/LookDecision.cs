using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Look", order = 5)]
    public class LookDecision : MAIDecision
    {
        public enum LookFor { AnimalPlayer, Tags, UnityTags, Zones, GameObject, ClosestWayPoint, Target }

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
        public LayerMask ObstacleLayer = 1;


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
        public int ZoneModeIndex = -1;



        public Color debugColor = new Color(0, 0, 0.7f, 0.3f);

        public override bool Decide(MAnimalBrain brain, int index)
        {
            return Look_For(brain);
        }

        private bool Look_For(MAnimalBrain brain)
        {
            switch (lookFor)
            {
                case LookFor.AnimalPlayer:
                    return LookForAnimalPlayer(brain);
                case LookFor.Tags:
                    return LookForTags(brain);
                case LookFor.UnityTags:
                    return LookForUnityTags(brain);
                case LookFor.Zones:
                    return LookForZones(brain);
                case LookFor.GameObject:
                    return LookForGameObject(brain);
                case LookFor.ClosestWayPoint:
                    return LookForClosestWaypoint(brain);
                case LookFor.Target:
                    return LookForTarget(brain);
                default:
                    return false;
            }
        }

        public bool LookForTarget(MAnimalBrain brain)
        {
            if (brain.Target == null) return false;

            return IsInFieldOfView(brain, brain.TargetAnimal ? brain.TargetAnimal.Center : brain.Target.position, brain.Target);
        }


        private bool IsInFieldOfView(MAnimalBrain brain, Vector3 TargetCenter, Transform target)
        {
            //if (TargetCenter.y > (brain.AgentHeight + brain.transform.position.y)) return false; //Meaning the Height of the Target is higher than the animal Agent Height

            var Direction_to_Target = (TargetCenter - brain.Eyes.position);
            var Distance_to_Target = Vector3.Distance(TargetCenter, brain.Eyes.position);

            if (Distance_to_Target < LookRange) //Check if whe are inside the Look Radius
            {
                Vector3 EyesForward = Vector3.ProjectOnPlane(brain.Eyes.forward, brain.Animal.UpVector);

                if (Vector3.Dot(Direction_to_Target.normalized, EyesForward) > Mathf.Cos(LookAngle * 0.5f * Mathf.Deg2Rad)) //Mean is in Range:
                {
                    //Need a RayCast to see if there's no obstacle in front of the Animal OBSCACLE LAYER
                    if (ObstacleLayer != 0 && Physics.Raycast(brain.Eyes.position, Direction_to_Target, out RaycastHit hit, Distance_to_Target * 0.9f, ObstacleLayer, QueryTriggerInteraction.Ignore))
                    {
                        if (brain.debug)
                        {
                            Debug.DrawLine(brain.Eyes.position, hit.point, Color.green, 0.5f);
                            Debug.DrawLine(hit.point, TargetCenter, Color.red, 0.5f);
                        }

                        if (RemoveTarget) brain.RemoveTarget();
                        return false; //Meaning there's something between the Eyes of the Animal and the Target Animal
                    }

                    if (brain.debug) Debug.DrawRay(brain.Eyes.position, Direction_to_Target, Color.green, 0.5f);


                    if (brain.AIMovement.Target != target) //If the Target is different
                    {
                        if (AssignTarget) brain.AIMovement.SetTarget(target, MoveToTarget);
                        //if (MoveToTarget) brain.AIMovement.SetDestination(TargetCenter);
                    }
                    return true;
                }
            }

            if (RemoveTarget) brain.RemoveTarget();
            return false;
        } 

        public bool LookForZones(MAnimalBrain brain)
        {
            var AllZones = Zone.Zones;

            float minDistance = float.MaxValue;
            Zone FoundZone = null;

            foreach (var zone in AllZones)
            {
                if (zone.zoneType == zoneType && ZoneID < 1 || zone.GetID == ZoneID)
                {
                    if (zone.zoneType == ZoneType.Mode && ZoneModeIndex != -1 && zone.ModeIndex != ZoneModeIndex) continue;  //IMPORTANT! Search for an Specific Mode Zone like Sleep)
                    

                    var Distance = Vector3.Distance(zone.transform.position, brain.Eyes.position); //Get the Distance between the Eyes and the Zone
                    if (Distance < minDistance && Distance < LookRange)
                    {
                        minDistance = Distance;
                        FoundZone = zone;
                    }
                }
            }

            if (FoundZone)
            {
                return IsInFieldOfView(brain, FoundZone.transform.position, FoundZone.transform);  //Find if is inside the Field of view
            }
            return false;
        }

        public bool LookForTags(MAnimalBrain brain)
        {
            var AllTagsHolders = Tags.TagsHolders;

            float minDistance = float.MaxValue;
            Tags FoundTagHolder = null;

            foreach (var tagsH in AllTagsHolders)
            {
                var Distance = Vector3.Distance(tagsH.transform.position, brain.Eyes.position);

                if (Distance < minDistance && Distance < LookRange)
                {
                    if (tagsH.HasTag(tags))
                    {
                        minDistance = Distance;
                        FoundTagHolder = tagsH;
                    }
                }
            }

            if (FoundTagHolder)
            {
                return IsInFieldOfView(brain, FoundTagHolder.transform.position, FoundTagHolder.transform);  //Find if is inside the Field of view
            }
            return false;
        }

        public bool LookForGameObject(MAnimalBrain brain)
        {
            if (string.IsNullOrEmpty(GameObjectName)) return false;

            var gameObject = GameObject.Find(GameObjectName);
            if (gameObject)
            {
                return IsInFieldOfView(brain, gameObject.transform.position, gameObject.transform);  //Find if is inside the Field of view
            }
            return false;
        }
        public bool LookForClosestWaypoint(MAnimalBrain brain)
        {
            var allWaypoints = MWayPoint.WayPoints;
            float minDistance = float.MaxValue;
            MWayPoint closestWayPoint = null;

            foreach (var way in allWaypoints)
            {
                var Distance = Vector3.Distance(way.GetPosition(), brain.Eyes.position);

                if (Distance < minDistance && Distance < LookRange)
                {
                    minDistance = Distance;
                    closestWayPoint = way;
                }
            }

            if (closestWayPoint)
            {
                return IsInFieldOfView(brain, closestWayPoint.GetPosition(), closestWayPoint.WPTransform);  //Find if is inside the Field of view
            }
            return false;
        }
        private bool LookForAnimalPlayer(MAnimalBrain brain)
        {
            if (MAnimal.MainAnimal == null || MAnimal.MainAnimal.ActiveStateID == StateEnum.Death) return false; //Means the animal is death or Disable

            return IsInFieldOfView(brain, MAnimal.MainAnimal.Center, MAnimal.MainAnimal.transform);
        }
        public bool LookForUnityTags(MAnimalBrain brain)
        {
            if (string.IsNullOrEmpty(UnityTag)) return false;

            var AllTags = GameObject.FindGameObjectsWithTag(UnityTag);

            float minDistance = float.MaxValue;
            GameObject closestTag = null;

            foreach (var way in AllTags)
            {
                var Distance = Vector3.Distance(way.transform.position, brain.Eyes.position);

                if (Distance < minDistance && Distance < LookRange)
                {
                    minDistance = Distance;
                    closestTag = way;
                }
            }

            if (closestTag)
            {
                return IsInFieldOfView(brain, closestTag.transform.position, closestTag.transform);  //Find if is inside the Field of view
            }
            return false;
        }

#if UNITY_EDITOR
        public override void DrawGizmos(MAnimalBrain brain)
        {
            var Eyes = brain.Eyes;

            if (Eyes)
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
}
