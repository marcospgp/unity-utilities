using UnityEngine;

namespace UnityUtilities
{
    [RequireComponent(typeof(MeshFilter))]
    public class ShowVertexNormals : MonoBehaviour
    {
        public Color normalColor = Color.green; // Color of the normals
        public float normalLength = 0.1f; // Length of the normals

        public void OnDrawGizmos()
        {
            MeshFilter meshFilter = this.GetComponentStrict<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    Vector3[] vertices = mesh.vertices;
                    Vector3[] normals = mesh.normals;

                    // Transform each vertex and normal to world space
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 worldVertex = this.transform.TransformPoint(vertices[i]);
                        Vector3 worldNormal = this.transform.TransformDirection(normals[i]);

                        // Draw the normal as a line
                        Gizmos.color = this.normalColor;
                        Gizmos.DrawLine(
                            worldVertex,
                            worldVertex + (worldNormal * this.normalLength)
                        );
                    }
                }
            }
        }
    }
}
