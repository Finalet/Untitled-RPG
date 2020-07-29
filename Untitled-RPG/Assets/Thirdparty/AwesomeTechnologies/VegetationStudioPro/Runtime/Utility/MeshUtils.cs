using System.Collections.Generic;
using AwesomeTechnologies.External;
using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies
{
    public enum MeshType
    {
        Normal,
        Billboard
    }

    public class MeshUtils
    {
        public static GameObject SelectMeshObject(GameObject go, LODLevel lodLevel)
        {
            return SelectNormalMesh(go, lodLevel);
        }

        public static Quaternion GetMeshRotation(GameObject go, LODLevel lodLevel)
        {
            GameObject meshGameObject = SelectMeshObject(go, lodLevel);
            if (meshGameObject)
            {
                return Quaternion.Inverse(Quaternion.identity) * meshGameObject.transform.rotation;
            }
            else
            {
                return Quaternion.identity;
            }
        }

        public static int GetLODCount(GameObject go, IShaderController shaderController)
        {
            LODGroup lodGroup = go.GetComponentInChildren<LODGroup>();
            if (lodGroup)
            {
                LOD[] lods = lodGroup.GetLODs();
                int lodCount = lods.Length;

                LOD lastLOD = lods[lods.Length - 1];
                foreach (Renderer renderer in lastLOD.renderers)
                {
                    if (renderer is BillboardRenderer)
                    {
                        lodCount -= 1;
                        break;
                    }
                    else if (renderer is MeshRenderer)
                    {
                        if (shaderController == null) continue;

                        MeshRenderer meshRenderer = renderer as MeshRenderer;
                        if (shaderController.MatchBillboardShader(meshRenderer.sharedMaterials))
                        {
                            lodCount -= 1;
                            break;
                        }
                    }
                }
                return lodCount;
            }
            return 1;
        }

        static GameObject SelectNormalMesh(GameObject go, LODLevel lodLevel)
        {
            LODGroup lodGroup = go.GetComponentInChildren<LODGroup>();
            if (lodGroup)
            {
                LOD[] lods = lodGroup.GetLODs();

                int lodIndex = (int)lodLevel;
                lodIndex = Mathf.Clamp(lodIndex, 0, lods.Length - 1);

                LOD lod = lods[lodIndex];
                if (lod.renderers.Length > 0)
                {
                    if (lod.renderers[0].gameObject.GetComponent<BillboardRenderer>())
                    {
                        if (lodIndex > 0)
                        {
                            lod = lods[lodIndex - 1];
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                return lod.renderers.Length > 0 ? lod.renderers[0].gameObject : null;
            }
            else
            {
                var meshRenderer = go.GetComponent<MeshRenderer>();
                if (meshRenderer) return meshRenderer.gameObject;

                meshRenderer = go.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer) return meshRenderer.gameObject;
                return null;
            }
        }        
        public static Bounds CalculateBoundsInstantiate(GameObject go)
        {
            if (!go)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            GameObject originalObject = Object.Instantiate(go);
            originalObject.transform.localScale = Vector3.one;
            originalObject.hideFlags = HideFlags.DontSave;
            Bounds objectBounds = CalculateBounds(originalObject);

            if (Application.isPlaying)
            {
                Object.Destroy(originalObject);
            }
            else
            {
                Object.DestroyImmediate(originalObject);
            }

            return objectBounds;
        }

        public static Bounds CalculateBounds(GameObject go)
        {
            Bounds combinedbounds = new Bounds(go.transform.position, Vector3.zero);
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer is SkinnedMeshRenderer)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                    Mesh mesh = new Mesh();
                    skinnedMeshRenderer.BakeMesh(mesh);
                    Vector3[] vertices = mesh.vertices;

                    for (int i = 0; i <= vertices.Length - 1; i++)
                    {
                        vertices[i] = skinnedMeshRenderer.transform.TransformPoint(vertices[i]);
                    }
                    mesh.vertices = vertices;
                    mesh.RecalculateBounds();
                    Bounds meshBounds = mesh.bounds;
                    combinedbounds.Encapsulate(meshBounds);
                }
                else
                {
                    combinedbounds.Encapsulate(renderer.bounds);
                }
            }
            return combinedbounds;
        }

        public static Mesh CreateBoxMesh(float length = 1f, float width = 1f, float height = 1f)
        {
            Mesh mesh = new Mesh();
            mesh.Clear();

            //const float length = 1f;
            //const float width = 1f;
            //const float height = 1f;

            #region Vertices

            Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
            Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
            Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
            Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);

            Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
            Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
            Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
            Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

            Vector3[] vertices =
            {
                // Bottom
                p0, p1, p2, p3,

                // Left
                p7, p4, p0, p3,

                // Front
                p4, p5, p1, p0,

                // Back
                p6, p7, p3, p2,

                // Right
                p5, p6, p2, p1,

                // Top
                p7, p6, p5, p4
            };

            #endregion

            #region Normales

            Vector3 up = Vector3.up;
            Vector3 down = Vector3.down;
            Vector3 front = Vector3.forward;
            Vector3 back = Vector3.back;
            Vector3 left = Vector3.left;
            Vector3 right = Vector3.right;

            Vector3[] normales =
            {
                // Bottom
                down, down, down, down,

                // Left
                left, left, left, left,

                // Front
                front, front, front, front,

                // Back
                back, back, back, back,

                // Right
                right, right, right, right,

                // Top
                up, up, up, up
            };

            #endregion

            #region UVs

            UnityEngine.Vector2 _00 = new UnityEngine.Vector2(0f, 0f);
            UnityEngine.Vector2 _10 = new UnityEngine.Vector2(1f, 0f);
            UnityEngine.Vector2 _01 = new UnityEngine.Vector2(0f, 1f);
            UnityEngine.Vector2 _11 = new UnityEngine.Vector2(1f, 1f);

            UnityEngine.Vector2[] uvs =
            {
                // Bottom
                _11, _01, _00, _10,

                // Left
                _11, _01, _00, _10,

                // Front
                _11, _01, _00, _10,

                // Back
                _11, _01, _00, _10,

                // Right
                _11, _01, _00, _10,

                // Top
                _11, _01, _00, _10
            };

            #endregion

            #region Triangles

            int[] triangles =
            {
                // Bottom
                3, 1, 0,
                3, 2, 1,

                // Left
                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

                // Front
                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

                // Back
                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

                // Right
                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

                // Top
                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5
            };

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateCapsuleMesh(float radius, float height)
        {
            int segments = 24;


            if (segments % 2 != 0)
                segments++;

            // extra vertex on the seam
            int points = segments + 1;

            // calculate points around a circle
            float[] pX = new float[points];
            float[] pZ = new float[points];
            float[] pY = new float[points];
            float[] pR = new float[points];

            float calcH = 0f;
            float calcV = 0f;

            for (int i = 0; i < points; i++)
            {
                pX[i] = Mathf.Sin(calcH * Mathf.Deg2Rad);
                pZ[i] = Mathf.Cos(calcH * Mathf.Deg2Rad);
                pY[i] = Mathf.Cos(calcV * Mathf.Deg2Rad);
                pR[i] = Mathf.Sin(calcV * Mathf.Deg2Rad);

                calcH += 360f / segments;
                calcV += 180f / segments;
            }


            // - Vertices and UVs -

            Vector3[] vertices = new Vector3[points * (points + 1)];
            UnityEngine.Vector2[] uvs = new UnityEngine.Vector2[vertices.Length];
            int ind = 0;

            // Y-offset is half the height minus the diameter
            float yOff = (height - radius * 2f) * 0.5f;
            if (yOff < 0)
                yOff = 0;

            // uv calculations
            float stepX = 1f / (points - 1);
            float uvX, uvY;

            // Top Hemisphere
            int top = Mathf.CeilToInt(points * 0.5f);

            for (int y = 0; y < top; y++)
                for (int x = 0; x < points; x++)
                {
                    vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                    vertices[ind].y = yOff + vertices[ind].y;

                    uvX = 1f - stepX * x;
                    uvY = (vertices[ind].y + height * 0.5f) / height;
                    uvs[ind] = new UnityEngine.Vector2(uvX, uvY);

                    ind++;
                }

            // Bottom Hemisphere
            int btm = Mathf.FloorToInt(points * 0.5f);

            for (int y = btm; y < points; y++)
                for (int x = 0; x < points; x++)
                {
                    vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                    vertices[ind].y = -yOff + vertices[ind].y;

                    uvX = 1f - stepX * x;
                    uvY = (vertices[ind].y + height * 0.5f) / height;
                    uvs[ind] = new UnityEngine.Vector2(uvX, uvY);

                    ind++;
                }


            // - Triangles -

            int[] triangles = new int[segments * (segments + 1) * 2 * 3];

            for (int y = 0, t = 0; y < segments + 1; y++)
                for (int x = 0; x < segments; x++, t += 6)
                {
                    triangles[t + 0] = (y + 0) * (segments + 1) + x + 0;
                    triangles[t + 1] = (y + 1) * (segments + 1) + x + 0;
                    triangles[t + 2] = (y + 1) * (segments + 1) + x + 1;

                    triangles[t + 3] = (y + 0) * (segments + 1) + x + 1;
                    triangles[t + 4] = (y + 0) * (segments + 1) + x + 0;
                    triangles[t + 5] = (y + 1) * (segments + 1) + x + 1;
                }

            Mesh mesh = new Mesh();
            mesh.Clear();

            mesh.name = "ProceduralCapsule";

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
            //_capsuleColliderMesh = mesh;
        }

        public static Mesh CreateSphereMesh(float radius = 1f)
        {
            Mesh mesh = new Mesh();
            mesh.Clear();

            //float radius = 1f;
            // Longitude |||
            int nbLong = 24;
            // Latitude ---
            int nbLat = 16;

            #region Vertices

            Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
            float _pi = Mathf.PI;
            var _2Pi = _pi * 2f;

            vertices[0] = Vector3.up * radius;
            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2Pi * (lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
                }
            }
            vertices[vertices.Length - 1] = Vector3.up * -radius;

            #endregion

            #region Normales		

            Vector3[] normales = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
                normales[n] = vertices[n].normalized;

            #endregion

            #region UVs

            UnityEngine.Vector2[] uvs = new UnityEngine.Vector2[vertices.Length];
            uvs[0] = UnityEngine.Vector2.up;
            uvs[uvs.Length - 1] = UnityEngine.Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
                for (int lon = 0; lon <= nbLong; lon++)
                    uvs[lon + lat * (nbLong + 1) + 1] =
                        new UnityEngine.Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));

            #endregion

            #region Triangles

            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            //Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    triangles[i++] = current;
                    triangles[i++] = current + 1;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = next;
                }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh ExtrudeMeshFromPolygon(Vector3[] polygonPoints, float yExtent)
        {
            Vector2[] polyginPoints2D = new Vector2[polygonPoints.Length];
            for (int i = 0; i < polygonPoints.Length; i++)
            {
                polyginPoints2D[i] = new Vector2(polygonPoints[i].x,polygonPoints[i].z);
            }
            Triangulator tr = new Triangulator(polyginPoints2D);
            int[] indices = tr.Triangulate();

            List<int> indexList = new List<int>();

            for (int i = 0; i < indices.Length; i += 3)
            {
                indexList.Add(indices[i + 2]);
                indexList.Add(indices[i + 1]);
                indexList.Add(indices[i]);                               
            }

            int polygonCount = polygonPoints.Length;
            for (int i = 0; i < indices.Length; i+=3)
            {
                indexList.Add(indices[i] + polygonCount);
                indexList.Add(indices[i + 1] + polygonCount);
                indexList.Add(indices[i + 2] + polygonCount);                               
            }

            for (int i = 0; i < polygonPoints.Length -1; i++)
            {
                    indexList.Add(i);
                    indexList.Add(i + polygonCount);
                    indexList.Add(i + 1);

                    indexList.Add(i + polygonCount);
                    indexList.Add(i + 1 + polygonCount);
                    indexList.Add(i + 1);
            }

            indexList.Add(polygonCount -1 );
            indexList.Add(polygonCount -1 + polygonCount);
            indexList.Add(0);

            indexList.Add(polygonCount - 1 + polygonCount);
            indexList.Add(polygonCount);
            indexList.Add(0);

            List<Vector3> verticeList = new List<Vector3>();

            for (int i = 0; i < polygonPoints.Length; i++)
            {
                verticeList.Add(new Vector3(polygonPoints[i].x, polygonPoints[i].y - yExtent, polygonPoints[i].z));
            }

            for (int i = 0; i < polygonPoints.Length; i++)
            {
                verticeList.Add(new Vector3(polygonPoints[i].x, polygonPoints[i].y + yExtent, polygonPoints[i].z));
            }

            Mesh mesh = new Mesh
            {
                vertices = verticeList.ToArray(),
                triangles = indexList.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}


