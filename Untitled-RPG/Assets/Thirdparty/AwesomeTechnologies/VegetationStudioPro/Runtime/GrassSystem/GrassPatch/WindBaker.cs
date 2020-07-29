using UnityEngine;

namespace AwesomeTechnologies.Grass
{
    public class WindBaker : MonoBehaviour
    {
        public Mesh Mesh;
        public Vector3 Rotation;
        public AnimationCurve BendCurve = new AnimationCurve();

        // ReSharper disable once UnusedMember.Local
        void Reset()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter)
            {
                Mesh = meshFilter.sharedMesh;
            }
        }
    }
}
