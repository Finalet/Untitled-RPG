using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Grass
{
    [CustomEditor(typeof(WindBaker))]
    public class WindBakerEditor : VegetationStudioProBaseEditor
    {
        private WindBaker _windBaker;

        public override void OnInspectorGUI()
        {
            _windBaker = (WindBaker) target;
            base.OnInspectorGUI();

            _windBaker.Mesh = EditorGUILayout.ObjectField("Selected mesh", _windBaker.Mesh, typeof(Mesh), true) as Mesh;
            _windBaker.Rotation = EditorGUILayout.Vector3Field("Rotation", _windBaker.Rotation);

            if (GUILayout.Button("rotate mesh"))
            {
                RotateMesh(_windBaker.Mesh, _windBaker.Rotation, Vector3.zero);
            }

            _windBaker.BendCurve = EditorGUILayout.CurveField("Bend curve", _windBaker.BendCurve);

            if (GUILayout.Button("Apply wind"))
            {
                ApplyPhaseAndBend(_windBaker.Mesh, _windBaker.BendCurve);
            }
        }

        void RotateMesh(Mesh mesh, Vector3 eulerRotation, Vector3 pivotPoint)
        {
            Quaternion newRotation = new Quaternion { eulerAngles = eulerRotation };
            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = newRotation * (vertices[i] - pivotPoint) + pivotPoint;
            }
            mesh.vertices = vertices;
        }

        void ApplyPhaseAndBend(Mesh mesh, AnimationCurve bendCurve)
        {
            Color[] meshColors;
            Vector3[] vertex = mesh.vertices;

            float height = 0;
            for (int i = 0; i <= vertex.Length - 1; i++)
            {
                if (vertex[i].y > height) height = vertex[i].y;
            }

            Debug.Log(height);

            if (mesh.colors.Length == 0)
            {
                meshColors = new Color[mesh.vertexCount];
            }
            else
            {
                meshColors = mesh.colors;
            }

            byte bendByte;

            for (int i = 0; i <= meshColors.Length - 1; i++)
            {
                float vertexHeight = (vertex[i].y + height / 2f) / height;
                vertexHeight = Mathf.Clamp(vertexHeight, 0, 1);

                    float bendCurveOutput = bendCurve.Evaluate(vertexHeight);
                    bendCurveOutput = Mathf.Clamp(bendCurveOutput, 0f, 1f);
                    bendByte = (byte)(bendCurveOutput * 255);             

                meshColors[i] = new Color32(255, 255, bendByte, 255);
            }
            mesh.colors = meshColors;
        }
    }
}
