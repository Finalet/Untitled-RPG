using System.Collections.Generic;
using System.Linq;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif


namespace AwesomeTechnologies.VegetationSystem.Biomes
{
    [System.Serializable]
    public class Node
    {
        public bool Selected;
        public Vector3 Position;
        public bool OverrideWidth;
        public float CustomWidth = 2f;
        public bool Active = true;
        public bool DisableEdge;
    }
    [ExecuteInEditMode]
    public class BiomeMaskArea : MonoBehaviour
    {
        public List<Node> Nodes = new List<Node>();
        public bool ClosedArea = true;
        public bool ShowArea = true;
        public bool ShowHandles = true;
        public string MaskName = "";
        private bool _needInit;
        public string Id;
        public BiomeType BiomeType;
        public LayerMask GroundLayerMask;
        public AnimationCurve BlendCurve = new AnimationCurve();
        public AnimationCurve InverseBlendCurve = new AnimationCurve();

        public AnimationCurve TextureBlendCurve = new AnimationCurve();
        public float BlendDistance = 5f;
        public float NoiseScale = 20;
        public bool UseNoise = true;
        private PolygonBiomeMask _currentMaskArea;

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Vector3 _lastLossyScale; 
            

        public void UpdateBiomeMask()
        {
            if (!enabled || !gameObject.activeSelf) return;
            List<Vector3> worldSpaceNodeList = GetWorldSpaceNodePositions();
            List<bool> disableEdgeList = GetDisableEdgeList();
            PolygonBiomeMask maskArea = new PolygonBiomeMask {BiomeType = BiomeType, BlendDistance = BlendDistance, UseNoise = UseNoise, NoiseScale = NoiseScale};
            maskArea.AddPolygon(worldSpaceNodeList,disableEdgeList);

            if (!ValidateAnimationCurve(BlendCurve))
            {
                BlendCurve = CreateResetAnimationCurve();
            }
            
            if (!ValidateAnimationCurve(InverseBlendCurve))
            {
                InverseBlendCurve = CreateResetAnimationCurve();
            }
            
            if (!ValidateAnimationCurve(TextureBlendCurve))
            {
                TextureBlendCurve = CreateResetAnimationCurve();
            }

            maskArea.SetCurve(BlendCurve.GenerateCurveArray(4096));
            maskArea.SetInverseCurve(InverseBlendCurve.GenerateCurveArray(4096));
            maskArea.SetTextureCurve(TextureBlendCurve.GenerateCurveArray(4096));

            if (_currentMaskArea != null)
            {
                VegetationStudioManager.RemoveBiomeMask(_currentMaskArea);
                _currentMaskArea = null;
            }

            _currentMaskArea = maskArea;
            VegetationStudioManager.AddBiomeMask(maskArea);

            RefreshPostProcessVolume();
        }
        
        
        public bool ValidateAnimationCurve(AnimationCurve curve)
        {
            float sample = curve.Evaluate(0.5f);
            if (float.IsNaN(sample))
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Curve error",
                    "There is a problem with one of the biome curves. It will be reset", "OK");
#endif
                return false;
            }
            return true;
        }

        AnimationCurve CreateResetAnimationCurve()
        {           
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0.5f);
            curve.AddKey(1, 0.5f);
            return curve;
        }


        private List<bool> GetDisableEdgeList()
        {
            List<bool> disableEdgeList = new List<bool>();
            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                disableEdgeList.Add(Nodes[i].DisableEdge);
            }

            return disableEdgeList;
        }

        // ReSharper disable once UnusedMember.Local
        void Start()
        {
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
            _lastLossyScale = transform.lossyScale;

            if (Nodes.Count == 0)
            {
                CreateDefaultNodes();
            }
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
            if (_currentMaskArea != null)
            {
                VegetationStudioManager.RemoveBiomeMask(_currentMaskArea);
                _currentMaskArea = null;
            }
        }

        // ReSharper disable once UnusedMember.Local
        void Update()
        {
            if (!Application.isPlaying || _needInit)
            {
                if (_lastPosition != transform.position || _lastRotation != transform.rotation || _needInit || _lastLossyScale != transform.lossyScale)
                {
                    PositionNodes();
                    _lastPosition = transform.position;
                    _lastRotation = transform.rotation;
                    _lastLossyScale = transform.lossyScale;
                    _needInit = false;
                }
            }            
        }

        public virtual void Reset()
        {
            if (Id == "") Id = System.Guid.NewGuid().ToString();
            ClearNodes();
            CreateDefaultNodes();
            BlendCurve.AddKey(0, 0);
            BlendCurve.AddKey(1, 1);

            InverseBlendCurve.AddKey(0, 1);
            InverseBlendCurve.AddKey(1, 0);

            TextureBlendCurve.AddKey(0, 0);
            TextureBlendCurve.AddKey(1, 1);
        }

        public void ClearNodes()
        {
            Nodes.Clear();
        }

        void CreateDefaultNodes()
        {
            Bounds tempBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(6f, 1f, 6f));
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
        
        public void AddNodesToEnd(Vector3[] worldPositions, bool[] disableEdges)
        {
            for (int i = 0; i <= worldPositions.Length - 1; i++)
            {
                AddNodeToEnd(worldPositions[i],disableEdges[i]);
            }
        }

        public void AddNodesToEnd(Vector3[] worldPositions, float[] customWidth, bool[] active)
        {
            for (int i = 0; i <= worldPositions.Length - 1; i++)
            {
                AddNodeToEnd(worldPositions[i], customWidth[i], active[i]);
            }
        }
        
        public void AddNodesToEnd(Vector3[] worldPositions, float[] customWidth, bool[] active, bool[] disableEdges)
        {
            for (int i = 0; i <= worldPositions.Length - 1; i++)
            {
                AddNodeToEnd(worldPositions[i], customWidth[i], active[i],disableEdges[i]);
            }
        }

        public void AddNodeToEnd(Vector3 worldPosition)
        {
            Node node = new Node { Position = transform.InverseTransformPoint(worldPosition) };
            Nodes.Add(node);
        }
        
        public void AddNodeToEnd(Vector3 worldPosition, bool disableEdge)
        {
            Node node = new Node
            {
                Position = transform.InverseTransformPoint(worldPosition),
                DisableEdge = disableEdge
            };
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
        
        public void AddNodeToEnd(Vector3 worldPosition, float customWidth, bool active, bool disableEdge)
        {
            Node node = new Node
            {
                Position = transform.InverseTransformPoint(worldPosition),
                CustomWidth = customWidth,
                OverrideWidth = true,
                Active = active,
                DisableEdge = disableEdge
            };
            Nodes.Add(node);
        }

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            if (Id == "") Id = System.Guid.NewGuid().ToString();
            _needInit = true;
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
                        Nodes[i].Position = transform.InverseTransformPoint(hits[j].point);
                        break;
                    }
                }
            }

            UpdateBiomeMask();
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

            Node node = new Node { Position = transform.InverseTransformPoint(worldPosition) };

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

        public List<Vector3> GetWorldSpaceNodePositions()
        {
            List<Vector3> worldSpaceNodeList = new List<Vector3>();

            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                worldSpaceNodeList.Add(transform.TransformPoint(Nodes[i].Position));
            }
            return worldSpaceNodeList;
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
                Handles.Label(GetMaskCenter(), MaskName + "(" + BiomeType + ")", stLabel);
            }
            else
            {
                GUIStyle stLabel = new GUIStyle(EditorStyles.whiteLabel);
                Handles.Label(GetMaskCenter(), BiomeType.ToString(), stLabel);
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
                        Gizmos.color = !Nodes[i].DisableEdge ? new Color(0f, 1f, 0f, 0.9f) : new Color(1f, 0f, 0f, 0.9f);

                        if (Nodes[i].Selected) Gizmos.color = new Color(1f, 1f, 1f, 1f);

                        Gizmos.DrawSphere(transform.TransformPoint(Nodes[i].Position), 0.02f * distance);
                    }
                }

                Gizmos.color = new Color(1f, 1f, 1f, 1f);

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
            if (!VegetationStudioManager.ShowBiomes) DrawGizmos();
        }

        public virtual void OnDrawGizmos()
        {
            if (VegetationStudioManager.ShowBiomes) DrawGizmos();
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

        // ReSharper disable once UnusedMember.Local

        public void RefreshPostProcessVolume()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessProfileInfo postProcessProfileInfo =
                VegetationStudioManager.GetPostProcessProfileInfo(BiomeType);
            RefreshPostProcessVolume(postProcessProfileInfo, VegetationStudioManager.GetPostProcessingLayer());
#endif
        }
#if UNITY_POST_PROCESSING_STACK_V2
        public void RefreshPostProcessVolume(PostProcessProfileInfo postProcessProfileInfo, LayerMask postProcessLayer)
        {
            gameObject.layer = postProcessLayer;

            if (postProcessProfileInfo == null)
            {
                PostProcessVolume postProcessVolume = gameObject.GetComponent<PostProcessVolume>();
                if (postProcessVolume)
                {
                    DestroyImmediate(postProcessVolume);
                }

                MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                if (meshCollider)
                {
                    DestroyImmediate(meshCollider);
                }
            }
            else
            {
                PostProcessVolume postProcessVolume = gameObject.GetComponent<PostProcessVolume>();
                if (!postProcessVolume)
                {
                    postProcessVolume = gameObject.AddComponent<PostProcessVolume>();
                }

                postProcessVolume.blendDistance = postProcessProfileInfo.BlendDistance;
                postProcessVolume.priority = postProcessProfileInfo.Priority;
                postProcessVolume.weight = postProcessProfileInfo.Weight;
                postProcessVolume.profile = postProcessProfileInfo.PostProcessProfile;
                postProcessVolume.enabled = postProcessProfileInfo.Enabled;              

                MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                if (!meshCollider)
                {
                    meshCollider = gameObject.AddComponent<MeshCollider>();
                }

                meshCollider.convex = true;
                meshCollider.enabled = postProcessProfileInfo.Enabled;
                meshCollider.isTrigger = true;              

                Vector3[] polygonPoints = new Vector3[Nodes.Count];
                for (int i = 0; i <= Nodes.Count - 1; i++)
                {
                    polygonPoints[i] = Nodes[i].Position;
                }
                meshCollider.sharedMesh = MeshUtils.ExtrudeMeshFromPolygon(polygonPoints, postProcessProfileInfo.VolumeHeight);

            }
        }
#endif
    }

    public static class UnityExtensions
    {
        /// <summary>
        /// Extension method to check if a layer is in a layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
    }
}
