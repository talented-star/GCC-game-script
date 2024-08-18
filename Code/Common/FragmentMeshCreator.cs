using UnityEngine;
using System.Collections.Generic;

namespace NibiruLibrary
{
    public class FragmentMeshCreator
    {
        public static void Create(MeshFilter myMeshFilter, float angle, float distance, float step = 10f)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            Vector3 right = GetRotation(Vector3.forward, angle) * distance;
            Vector3 left = GetRotation(Vector3.forward, angle) * distance;
            Vector3 from = left;

            vertices.Add(Vector3.zero);
            vertices.Add(from);
            uvs.Add(Vector2.one * 0.5f);
            uvs.Add(Vector2.one);
            int triangleIdx = 3;

            for (float angleStep = -angle; angleStep < angle; angleStep += step)
            {
                Vector3 to = GetRotation(Vector3.forward, angleStep) * distance; // метод ниже
                from = to;
                vertices.Add(from);
                uvs.Add(Vector2.one);
                triangles.Add(triangleIdx - 1);
                triangles.Add(triangleIdx);
                triangles.Add(0);

                triangleIdx++;
            }
            vertices.Add(right);

            uvs.Add(Vector2.one);

            Mesh mesh = new Mesh();
            mesh.name = "FragmentArea";
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            myMeshFilter.mesh = mesh;
        }
        
        public static Vector3 GetRotation(Vector3 forward, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 result = new Vector3(forward.x * Mathf.Cos(rad) + forward.z * Mathf.Sin(rad), 0,
                                            forward.z * Mathf.Cos(rad) - forward.x * Mathf.Sin(rad));
            return result;
        }
    }
}
