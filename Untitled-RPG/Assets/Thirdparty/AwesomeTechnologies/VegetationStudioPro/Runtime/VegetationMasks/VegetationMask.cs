using System.Collections.Generic;
using System.Linq;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies
{
    [System.Serializable]
    public class Node
    {
        public bool Selected;
        public Vector3 Position;
        public bool OverrideWidth;
        public float CustomWidth = 2f;
        public bool Active = true;
    }

    [ExecuteInEditMode]
    public class VegetationMask : MonoBehaviour
    {
        public bool RemoveGrass = true;
        public bool RemovePlants = true;
        public bool RemoveTrees = true;
        public bool RemoveObjects = true;
        public bool RemoveLargeObjects = true;
        public float AdditionalGrassPerimiter;
        public float AdditionalPlantPerimiter;
        public float AdditionalTreePerimiter;
        public float AdditionalObjectPerimiter;
        public float AdditionalLargeObjectPerimiter;

        public float AdditionalGrassPerimiterMax;
        public float AdditionalPlantPerimiterMax;
        public float AdditionalTreePerimiterMax;
        public float AdditionalObjectPerimiterMax;
        public float AdditionalLargeObjectPerimiterMax;

        public float NoiseScaleGrass = 5;
        public float NoiseScalePlant = 5;
        public float NoiseScaleTree = 5;
        public float NoiseScaleObject = 5;
        public float NoiseScaleLargeObject = 5;

        public string Id;

        public bool IncludeVegetationType;

        public List<Node> Nodes = new List<Node>();
        public bool ClosedArea = true;
        public bool ShowArea = true;
        public bool ShowHandles = true;
        public string MaskName = "";
        public LayerMask GroundLayerMask;

        public List<VegetationTypeSettings> VegetationTypeList = new List<VegetationTypeSettings>();

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private bool _needInit;
        // ReSharper disable once UnusedMember.Local
        void Start()
        {
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;

            if (Nodes.Count == 0)
            {
                CreateDefaultNodes();
            }
        }

        // ReSharper disable once Unity.RedundantEventFunction
        public virtual void Awake()
        {
            
        }

        public void AddVegetationTypes(BaseMaskArea maskArea)
        {
            for (int i = 0; i <= VegetationTypeList.Count - 1; i++)
            {
                maskArea.VegetationTypeList.Add(new VegetationTypeSettings(VegetationTypeList[i]));
            }
        }

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            if (Id == "") Id = System.Guid.NewGuid().ToString();
            _needInit = true;
        }

        // ReSharper disable once UnusedMember.Local
        void Update()
        {
            if (!Application.isPlaying)
            {
                if (_lastPosition != transform.position || _lastRotation != transform.rotation)
                {
                    PositionNodes();
                    _lastPosition = transform.position;
                    _lastRotation = transform.rotation;
                }
            }
            
            if (_needInit)
            {
                _needInit = false;
                PositionNodes();
            }
        }

        public virtual void Reset()
        {
            if (Id == "") Id = System.Guid.NewGuid().ToString();
            ClearNodes();
            CreateDefaultNodes();
        }

        void CreateDefaultNodes()
        {
            Bounds tempBounds = new Bounds(new Vector3(0,0,0), new Vector3(6f, 1f, 6f));
            ClearNodes();
            for (int i = 0; i <= 3; i++)
            {
                Node node = new Node();
                switch (i)
                {
                    case 0:
                        node.Position = new Vector3(tempBounds.extents.x, tempBounds.extents.y, tempBounds.extents.z);
                        break;
                    case 1:
                        node.Position = new Vector3(-tempBounds.extents.x, tempBounds.extents.y, tempBounds.extents.z);
                        break;
                    case 2:
                        node.Position = new Vector3(-tempBounds.extents.x, tempBounds.extents.y, -tempBounds.extents.z);
                        break;
                    case 3:
                        node.Position = new Vector3(tempBounds.extents.x, tempBounds.extents.y, -tempBounds.extents.z);
                        break;
                }
                Nodes.Add(node);
            }
            PositionNodes();
        }

        public void ClearNodes()
        {
            Nodes.Clear();
        }

        public void PositionNodes()
        {
            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                Ray ray = new Ray(transform.TransformPoint(Nodes[i].Position) + new Vector3(0, 2000f, 0), Vector3.down);
                var hits = Physics.RaycastAll(ray).OrderBy(h => h.distance).ToArray();
                for (int j = 0; j <= hits.Length - 1; j++)
                {
                    if (hits[j].collider is TerrainCollider || GroundLayerMask.Contains(hits[j].collider.gameObject.layer))
                    {
                        Nodes[i].Position = transform.InverseTransformPoint(hits[j].point);// + new Vector3(0, 0.5f, 0));
                        break;
                    }
                }
            }

            UpdateVegetationMask();
        }

        public List<Vector3> GetWorldSpaceNodePositions()
        {
            List<Vector3> worldSpaceNodeList = new List<Vector3>();

            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                worldSpaceNodeList.Add(transform.TransformPoint(Nodes[i].Position));
            }
            return worldSpaceNodeList;
        }
        public virtual void UpdateVegetationMask()
        {

        }

        public void DeleteNode(Node node)
        {
            Nodes.Remove(node);
        }

        public void AddNodesToEnd(Vector3[] worldPositions)
        {
            for (int i = 0; i <= worldPositions.Length - 1; i++)
            {
                AddNodeToEnd(worldPositions[i]);
            }
        }

        public void AddNodesToEnd(Vector3[] worldPositions, float[] customWidth, bool[] active)
        {
            for (int i = 0; i <= worldPositions.Length - 1; i++)
            {
                AddNodeToEnd(worldPositions[i], customWidth[i], active[i]);
            }
        }

        public void AddNodeToEnd(Vector3 worldPosition)
        {
            Node node = new Node {Position = transform.InverseTransformPoint(worldPosition)};
            Nodes.Add(node);
        }

        public void AddNodeToEnd(Vector3 worldPosition, float customWidth, bool active)
        {
            Node node = new Node
            {
                Position = transform.InverseTransformPoint(worldPosition),
                CustomWidth = customWidth,
                OverrideWidth = true,
                Active = active
            };
            Nodes.Add(node);
        }

        public void AddNode(Vector3 worldPosition)
        {
            if (Nodes.Count == 0)
            {
                AddNodeToEnd(worldPosition);
                return;
            }

            Node closestNode = FindClosestNode(worldPosition);
            Node nextNode = GetNextNode(closestNode);
            Node previousNode = GetPreviousNode(closestNode);

            LineSegment3D nextSegment = new LineSegment3D(transform.TransformPoint(closestNode.Position), transform.TransformPoint(nextNode.Position));
            LineSegment3D previousSegment = new LineSegment3D(transform.TransformPoint(closestNode.Position), transform.TransformPoint(previousNode.Position));

            float nextSegmentDistance = nextSegment.DistanceTo(worldPosition);
            float previousSegmentDistance = previousSegment.DistanceTo(worldPosition);

            Node node = new Node {Position = transform.InverseTransformPoint(worldPosition)};

            int currentNodeIndex = GetNodeIndex(closestNode);

            if (nextSegmentDistance < previousSegmentDistance)
            {
                if (currentNodeIndex == Nodes.Count - 1)
                {
                    Nodes.Add(node);
                }
                else
                {
                    Nodes.Insert(currentNodeIndex + 1, node);
                }
            }
            else
            {
                Nodes.Insert(currentNodeIndex, node);
            }
        }

        public int GetNodeIndex(Node node)
        {
            int nodeIndex = 0;
            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                if (Nodes[i] == node)
                {
                    nodeIndex = i;
                    break;
                }
            }
            return nodeIndex;
        }
        public Node GetNextNode(Node node)
        {
            int nodeIndex = 0;
            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                if (Nodes[i] == node)
                {
                    nodeIndex = i;
                    break;
                }
            }

            if (nodeIndex == Nodes.Count - 1)
            {
                return Nodes[0];
            }
            else
            {
                return Nodes[nodeIndex + 1];
            }
        }

        public Node GetPreviousNode(Node node)
        {
            int nodeIndex = 0;
            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                if (Nodes[i] == node)
                {
                    nodeIndex = i;
                    break;
                }
            }

            if (nodeIndex == 0)
            {
                return Nodes[Nodes.Count - 1];
            }
            else
            {
                return Nodes[nodeIndex - 1];
            }
        }

        public Node FindClosestNode(Vector3 worldPosition)
        {
            Node returnNode = null;
            float smallestDistance = float.MaxValue;

            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                float distance = Vector3.Distance(worldPosition, transform.TransformPoint(Nodes[i].Position));
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    returnNode = Nodes[i];
                }
            }

            return returnNode;
        }


        void DrawGizmos()
        {
#if UNITY_EDITOR
            if (MaskName != "")
            {
                GUIStyle stLabel = new GUIStyle(EditorStyles.whiteLabel);
                Handles.Label(GetMaskCenter(), MaskName, stLabel);
            }
#endif
            if (ShowArea)
            {


#if UNITY_EDITOR
                Gizmos.color = new Color(1f, 1f, 0, 1f);
                Camera sceneviewCamera = SceneViewDetector.GetCurrentSceneViewCamera();
                if (!sceneviewCamera) return;

                for (int i = 0; i <= Nodes.Count - 1; i++)
                {
                    var distance = Vector3.Distance(sceneviewCamera.transform.position, transform.TransformPoint(Nodes[i].Position));

                    if (distance < 200)
                    {

                        Gizmos.color = new Color(1f, 1f, 1f, 1f);
                        if (Nodes[i].Selected) Gizmos.color = new Color(0, 1f, 0f, 1f);

                        Gizmos.DrawSphere(transform.TransformPoint(Nodes[i].Position), 0.015f * distance);
                    }
                }

                if (Nodes.Count > 1)
                {
                    for (int i = 0; i <= Nodes.Count - 1; i++)
                    {
                        if (i == Nodes.Count - 1)
                        {
                            if (ClosedArea)
                            {
                                Gizmos.DrawLine(transform.TransformPoint(Nodes[0].Position), transform.TransformPoint(Nodes[i].Position));
                            }
                        }
                        else
                        {
                            Gizmos.DrawLine(transform.TransformPoint(Nodes[i].Position), transform.TransformPoint(Nodes[i + 1].Position));
                        }
                    }
                }
#endif
            }
        }

        public virtual void OnDrawGizmosSelected()
        {
            DrawGizmos();           
        }

        Vector3 GetMaskCenter()
        {
            List<Vector3> worldPositions = GetWorldSpaceNodePositions();
            return (GetMeanVector(worldPositions.ToArray()));
        }

        private Vector3 GetMeanVector(Vector3[] positions)
        {
            if (positions.Length == 0)
                return Vector3.zero;
            float x = 0f;
            float y = 0f;
            float z = 0f;
            foreach (Vector3 pos in positions)
            {
                x += pos.x;
                y += pos.y;
                z += pos.z;
            }
            return new Vector3(x / positions.Length, y / positions.Length, z / positions.Length);
        }

//        // ReSharper disable once UnusedMember.Local
//        void Update()
//        {
//           if (_needInit)
//           {
//               _needInit = false;
//               PositionNodes();
//           }
//        }
    }
}
