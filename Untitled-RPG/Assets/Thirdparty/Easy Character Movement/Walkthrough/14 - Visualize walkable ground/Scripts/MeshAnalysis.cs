using UnityEngine;

public class MeshAnalysis : MonoBehaviour
{
    public float maxWalkableAngle = 30.0f;
    
    public Color walkableColor = Color.green;
    public Color nonWalkableColor = Color.red;

    public bool drawGizmos;
    public float normalLength = 0.5f;

    [ContextMenu("Clear Mesh")]
    private void ClearVertices()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;

        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var colors = new Color[vertices.Length];

        for (var i = vertices.Length - 1; i >= 0; i--)
            colors[i] = Color.white;

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors = colors;

        GetComponent<MeshFilter>().mesh = mesh;
    }

    [ContextMenu("Paint Mesh")]
    private void PaintVertices()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;

        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var colors = new Color[vertices.Length];

        for (var i = vertices.Length - 1; i >= 0; i--)
        {
            var n = normals[i];

            var color = nonWalkableColor;

            var angle = Vector3.Angle(transform.TransformDirection(n).normalized, Vector3.up);
            if (angle <= maxWalkableAngle)
                color = Color.Lerp(walkableColor, Color.blue, Mathf.InverseLerp(0.0f, maxWalkableAngle, angle));
            else
                color = Color.Lerp(Color.blue, nonWalkableColor, Mathf.InverseLerp(maxWalkableAngle, 90.0f, angle));

            colors[i] = color;
        }
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors = colors;

        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        var sharedMesh = GetComponent<MeshCollider>().sharedMesh;

        var vertices = sharedMesh.vertices;

        var triangles = sharedMesh.triangles;
        
        for (var i = 0; i < triangles.Length; i += 3)
        {
            var idx0 = triangles[ i + 0 ];
            var idx1 = triangles[ i + 1 ];
            var idx2 = triangles[ i + 2 ];

            var a = vertices[ idx0 ];
            var b = vertices[ idx1 ];
            var c = vertices[ idx2 ];

            var side1 = b - a;
            var side2 = c - a;

            var n = Vector3.Cross(side1, side2).normalized;
            n = transform.TransformDirection(n);

            var angle = Vector3.Angle(n, Vector3.up);
            if (angle <= maxWalkableAngle)
                continue;

            var p = (a + b + c) / 3;
            Debug.DrawRay(transform.TransformPoint(p), n * normalLength, nonWalkableColor);
        }
    }
}
