using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.Grass
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [System.Serializable]
    public class ProceduralGrassPlane : MonoBehaviour
    {
        public enum AnchorPoint
        {
            TopLeft,
            TopHalf,
            TopRight,
            RightHalf,
            BottomRight,
            BottomHalf,
            BottomLeft,
            LeftHalf,
            Center
        }


        public int widthSegments = 5;
        public int heightSegments = 4;
        public float width = 1.0f;
        public float height = 0.5f;
        public AnchorPoint anchor = AnchorPoint.Center;
        private UnityEngine.Vector2 anchorOffset;
        //private string anchorId;


        public float Offset1 = 0.3f;
        public float Offset2 = 0.15f;
        public float MinimumBendHeight = 0.25f;
        public float CurveOffset = 0.25f;
        public int LODLevel = 0;
        public Material Material;

        public bool BakePhase;
        public bool BakeBend;
        public bool BakeAO;
        public float Phase;
        public AnimationCurve BendCurve;
        public AnimationCurve AmbientOcclusionCurve;
        public bool GenerateBackside = true;

        void Start()
        {
          

        }

        void SetAncorPoints()
        {
            switch (anchor)
            {
                case AnchorPoint.TopLeft:
                    anchorOffset = new UnityEngine.Vector2(-width / 2.0f, height / 2.0f);
                    //anchorId = "TL";
                    break;
                case AnchorPoint.TopHalf:
                    anchorOffset = new UnityEngine.Vector2(0.0f, height / 2.0f);
                    //anchorId = "TH";
                    break;
                case AnchorPoint.TopRight:
                    anchorOffset = new UnityEngine.Vector2(width / 2.0f, height / 2.0f);
                    //anchorId = "TR";
                    break;
                case AnchorPoint.RightHalf:
                    anchorOffset = new UnityEngine.Vector2(width / 2.0f, 0.0f);
                    //anchorId = "RH";
                    break;
                case AnchorPoint.BottomRight:
                    anchorOffset = new UnityEngine.Vector2(width / 2.0f, -height / 2.0f);
                    //anchorId = "BR";
                    break;
                case AnchorPoint.BottomHalf:
                    anchorOffset = new UnityEngine.Vector2(0.0f, -height / 2.0f);
                    //anchorId = "BH";
                    break;
                case AnchorPoint.BottomLeft:
                    anchorOffset = new UnityEngine.Vector2(-width / 2.0f, -height / 2.0f);
                    //anchorId = "BL";
                    break;
                case AnchorPoint.LeftHalf:
                    anchorOffset = new UnityEngine.Vector2(-width / 2.0f, 0.0f);
                    //anchorId = "LH";
                    break;
                case AnchorPoint.Center:
                default:
                    anchorOffset = UnityEngine.Vector2.zero;
                    //anchorId = "C";
                    break;
            }
        }

        void ApplyPhaseAndBend(Mesh mesh)
        {
            Color[] meshColors;
            Vector3[] vertex = mesh.vertices;

            if (mesh.colors.Length == 0)
            {
                meshColors = new Color[mesh.vertexCount];
            }
            else
            {
                meshColors = mesh.colors;
            }

            byte phaseByte = 255;
            byte bendByte = 255;
            byte ambientByte = 255;
            if (BakePhase)
            {
                phaseByte = (byte)(Phase * 255);
            }

            for (int i = 0; i <= meshColors.Length -1; i++)
            {
                float vertexHeight = (vertex[i].y + height / 2f) / height;
                vertexHeight = Mathf.Clamp(vertexHeight, 0, 1);

                if (BakeBend)
                {                   
                    float bendCurveOutput = BendCurve.Evaluate(vertexHeight);                    
                    bendCurveOutput = Mathf.Clamp(bendCurveOutput,0f,1f);                   
                    bendByte = (byte)(bendCurveOutput * 255);
                }

                if (BakeAO)
                {
                    float ambientOcculsionCurveOutput = AmbientOcclusionCurve.Evaluate(vertexHeight);
                    ambientOcculsionCurveOutput = Mathf.Clamp(ambientOcculsionCurveOutput, 0f, 1f);
                    ambientByte = (byte)(ambientOcculsionCurveOutput * 255);
                }

                meshColors[i] = new Color32(ambientByte, phaseByte, bendByte, bendByte);
            }
            mesh.colors = meshColors;
        }
        public void CreateGrassPlane()
        {
            SetAncorPoints();

            // You can change that line to provide another MeshFilter


            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            if (filter == null)
            {

                filter = gameObject.AddComponent<MeshFilter>();
            }

            Mesh m = filter.sharedMesh;
            if (m == null) m = new Mesh();
            m.Clear();


            int hCount2 = widthSegments + 1;
            int vCount2 = heightSegments + 1;
            int numTriangles = widthSegments * heightSegments * 6;
            int numVertices = hCount2 * vCount2;

            Vector3[] vertices = new Vector3[numVertices];
            UnityEngine.Vector2[] uvs = new UnityEngine.Vector2[numVertices];
            int[] triangles = new int[numTriangles];
            Vector4[] tangents = new Vector4[numVertices];
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

            int index = 0;
            float uvFactorX = 1.0f / widthSegments;
            float uvFactorY = 1.0f / heightSegments;
            float scaleX = width / widthSegments;
            float scaleY = height / heightSegments;
            for (float y = 0.0f; y < vCount2; y++)
            {
                for (float x = 0.0f; x < hCount2; x++)
                {

                    float NormalizedX = x * uvFactorX;
                    float CurrentOffset = Mathf.Lerp(Offset1, Offset2, x * uvFactorX);
                    float ZOffset = Mathf.Lerp(0, CurrentOffset, y * uvFactorY);
                    if ((y * scaleY) <= MinimumBendHeight)
                    {
                        ZOffset = 0;
                    }

                    float zCurveoffset = 0;
                    if (NormalizedX <= 0.5f)
                    {
                        zCurveoffset = Mathf.Lerp(CurveOffset,0,NormalizedX *2);
                    }
                    else
                    {
                        zCurveoffset = Mathf.Lerp(0, CurveOffset, (NormalizedX*2) - 0.5f);
                    }
                  

                    vertices[index] = new Vector3(x * scaleX - width / 2f - anchorOffset.x, y * scaleY - height / 2f - anchorOffset.y, ZOffset + zCurveoffset);
                    tangents[index] = tangent;
                    uvs[index++] = new UnityEngine.Vector2(x * uvFactorX, y * uvFactorY);
                }
            }

            index = 0;
            for (int y = 0; y < heightSegments; y++)
            {
                for (int x = 0; x < widthSegments; x++)
                {
                    triangles[index] = (y * hCount2) + x;
                    triangles[index + 1] = ((y + 1) * hCount2) + x;
                    triangles[index + 2] = (y * hCount2) + x + 1;

                    triangles[index + 3] = ((y + 1) * hCount2) + x;
                    triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
                    triangles[index + 5] = (y * hCount2) + x + 1;
                    index += 6;
                }               
            }

            m.vertices = vertices;
            m.uv = uvs;
            m.triangles = triangles;
            m.tangents = tangents;
           
            m.RecalculateNormals();
            if (GenerateBackside)
            {
                BuildBackside(m);
                m.RecalculateNormals();
            }

            filter.sharedMesh = m;
            m.RecalculateBounds();

            //if (material == null)
            //{
            //    material = (Material)Resources.Load("GrassPlaneMaterial", typeof(Material));
            //}

            MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                meshRenderer.sharedMaterial = Material;
            }

            if (BakePhase || BakeBend) ApplyPhaseAndBend(m);
        }


        void BuildBackside(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var uv = mesh.uv;
            var normals = mesh.normals;
            var szV = vertices.Length;
            var newVerts = new Vector3[szV * 2];
            var newUv = new UnityEngine.Vector2[szV * 2];
            var newNorms = new Vector3[szV * 2];
            for (var j = 0; j < szV; j++)
            {
                // duplicate vertices and uvs:
                newVerts[j] = newVerts[j + szV] = vertices[j];
                newUv[j] = newUv[j + szV] = uv[j];
                // copy the original normals...
                newNorms[j] = normals[j];
                // and revert the new ones
                newNorms[j + szV] = -normals[j];
            }
            var triangles = mesh.triangles;
            var szT = triangles.Length;
            var newTris = new int[szT * 2]; // double the triangles
            for (var i = 0; i < szT; i += 3)
            {
                // copy the original triangle
                newTris[i] = triangles[i];
                newTris[i + 1] = triangles[i + 1];
                newTris[i + 2] = triangles[i + 2];
                // save the new reversed triangle
                var j = i + szT;
                newTris[j] = triangles[i] + szV;
                newTris[j + 2] = triangles[i + 1] + szV;
                newTris[j + 1] = triangles[i + 2] + szV;
            }
            mesh.vertices = newVerts;
            mesh.uv = newUv;
            mesh.normals = newNorms;
            mesh.triangles = newTris; // assign triangles last!
        }
    }
}
