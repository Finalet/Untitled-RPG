using AwesomeTechnologies.BillboardSystem;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.Billboards
{
    public class BillboardGenerator
    {
        public static Mesh CreateMeshFromBillboardInstance(BillboardInstance billboardInstance)
        {           
            NativeArray<Vector3> verticeArray = billboardInstance.VerticeList;
            NativeArray<int> indexArray = billboardInstance.IndexList;
            NativeArray<Vector2> uvArray = billboardInstance.UvList;
            NativeArray<Vector2> uv2Array = billboardInstance.Uv2List;
            NativeArray<Vector2> uv3Array = billboardInstance.Uv3List;
            NativeArray<Vector3> normalArray = billboardInstance.NormalList;

            Vector3[] vertices = new Vector3[billboardInstance.VerticeList.Length];
            int[] indices = new int[billboardInstance.IndexList.Length];
            Vector2[] uvs = new Vector2[billboardInstance.UvList.Length];
            Vector2[] uv2S = new Vector2[billboardInstance.Uv2List.Length];
            Vector2[] uv3S = new Vector2[billboardInstance.Uv3List.Length];
            Vector3[] normals = new Vector3[billboardInstance.NormalList.Length];

            verticeArray.CopyToFast(vertices);
            indexArray.CopyToFast(indices);
            uvArray.CopyToFast(uvs);
            uv2Array.CopyToFast(uv2S);
            uv3Array.CopyToFast(uv3S);
            normalArray.CopyToFast(normals);

            var newMesh = new Mesh
            {
                hideFlags = HideFlags.DontSave,
                subMeshCount = 1,
                indexFormat = IndexFormat.UInt32,
                vertices = vertices
            };
            newMesh.SetIndices(indices, MeshTopology.Triangles, 0, false);                     
            newMesh.uv = uvs;
            newMesh.uv2 = uv2S;
            newMesh.uv3 = uv3S;
            newMesh.normals = normals;

            //TODO add bounds manually from billboard cell bounds
            //TODO replace with nativearray direct when unity adds interface.
            newMesh.RecalculateBounds();
            return newMesh;
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct CreateBillboardMeshJob : IJob
        {
            [ReadOnly]
            public NativeList<MatrixInstance> InstanceList;

            public NativeList<Vector3> VerticeList;
            public NativeList<int> IndexList;
            public NativeList<Vector2> UvList;
            public NativeList<Vector2> Uv2List;
            public NativeList<Vector2> Uv3List;
            public NativeList<Vector3> NormalList;

            public float BoundsYExtent;
            public float VegetationItemSize;
 
 
            private Vector3 _srcVert0;
            private Vector3 _srcVert1;
            private Vector3 _srcVert2;
            private Vector3 _srcVert3;

            private Vector2 _srcUVs0;
            private Vector2 _srcUVs1;
            private Vector2 _srcUVs2;
            private Vector2 _srcUVs3;

            private int _srcIndex0;
            private int _srcIndex1;
            private int _srcIndex2;
            private int _srcIndex3;
            private int _srcIndex4;
            private int _srcIndex5;

            public void Execute()
            {
                SetupData();
                int vertIndex = 0;
            
                for (int i = 0; i <= InstanceList.Length - 1; i++)
                {
                    MatrixInstance matrixInstance = InstanceList[i];

                    Vector3 scale = ExtractScaleFromMatrix(matrixInstance.Matrix) / 2f;
                    Vector3 position = ExtractTranslationFromMatrix(matrixInstance.Matrix) + new Vector3(0, BoundsYExtent * scale.y * 2, 0);
                    Quaternion rotation = ExtractRotationFromMatrix(matrixInstance.Matrix);

                    VerticeList.Add(position);
                    VerticeList.Add(position);
                    VerticeList.Add(position);
                    VerticeList.Add(position);

                    NormalList.Add(_srcVert0);
                    NormalList.Add(_srcVert1);
                    NormalList.Add(_srcVert2);
                    NormalList.Add(_srcVert3);

                    UvList.Add(_srcUVs0);
                    UvList.Add(_srcUVs1);
                    UvList.Add(_srcUVs2);
                    UvList.Add(_srcUVs3);

                    var rotationVector = new Vector2((360f - rotation.eulerAngles.y) / 360f, 1f);
                    Uv2List.Add(rotationVector);
                    Uv2List.Add(rotationVector);
                    Uv2List.Add(rotationVector);
                    Uv2List.Add(rotationVector);

                    Vector2 scaleAndVFix;

                    scaleAndVFix.x = VegetationItemSize * scale.x * 2f;
                    scaleAndVFix.y = -(BoundsYExtent * scale.y * 2);

                    Uv3List.Add(scaleAndVFix);
                    Uv3List.Add(scaleAndVFix);
                    Uv3List.Add(scaleAndVFix);
                    Uv3List.Add(scaleAndVFix);

                    IndexList.Add(_srcIndex0 + vertIndex);
                    IndexList.Add(_srcIndex1 + vertIndex);
                    IndexList.Add(_srcIndex2 + vertIndex);
                    IndexList.Add(_srcIndex3 + vertIndex);
                    IndexList.Add(_srcIndex4 + vertIndex);
                    IndexList.Add(_srcIndex5 + vertIndex);
                    vertIndex += 4;
                }
            }

            void SetupData()
            {
                _srcVert0 = new Vector3(-0.5f, -0.5f, 0);
                _srcVert1 = new Vector3(0.5f, 0.5f, 0);
                _srcVert2 = new Vector3(0.5f, -0.5f, 0);
                _srcVert3 = new Vector3(-0.5f, 0.5f, 0);

                _srcUVs0 = new Vector2(0f, 0f);
                _srcUVs1 = new Vector2(1f, 1f);
                _srcUVs2 = new Vector2(1f, 0f);
                _srcUVs3 = new Vector2(0f, 1);

                _srcIndex0 = 0;
                _srcIndex1 = 1;
                _srcIndex2 = 2;
                _srcIndex3 = 1;
                _srcIndex4 = 0;
                _srcIndex5 = 3;
            }

            Vector3 ExtractTranslationFromMatrix(Matrix4x4 matrix)
            {
                Vector3 translate;
                translate.x = matrix.m03;
                translate.y = matrix.m13;
                translate.z = matrix.m23;
                return translate;
            }

            public static Vector3 ExtractScaleFromMatrix(Matrix4x4 matrix)
            {
                return new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
            }

            public static Quaternion ExtractRotationFromMatrix(Matrix4x4 matrix)
            {
                Vector3 forward;
                forward.x = matrix.m02;
                forward.y = matrix.m12;
                forward.z = matrix.m22;

                if (forward == Vector3.zero) return Quaternion.identity;

                Vector3 upwards;
                upwards.x = matrix.m01;
                upwards.y = matrix.m11;
                upwards.z = matrix.m21;

                return Quaternion.LookRotation(forward, upwards);
            }
        }
    }
}
