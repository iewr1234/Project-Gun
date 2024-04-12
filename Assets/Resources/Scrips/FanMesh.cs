using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FanMesh : MonoBehaviour
{
    private MeshRenderer meshRdr;
    private readonly int segments = 20;

    public void SetComponents()
    {
        meshRdr = GetComponent<MeshRenderer>();
        meshRdr.material = new Material(Resources.Load<Material>("Materials/Fan"));
    }

    public void CreateMesh(int angle, float outRadius, float inRadius)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int totalVertices = (segments + 2) * 2;
        Vector3[] vertices = new Vector3[totalVertices];
        Vector2[] uv = new Vector2[totalVertices];
        int[] triangles = new int[segments * 6];

        float angleIncrement = (float)angle / segments;
        float currentAngle = -angle / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float x1 = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * outRadius;
            float z1 = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * outRadius;
            float x2 = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * inRadius;
            float z2 = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * inRadius;

            int index = i * 2;
            vertices[index] = new Vector3(x1, 0f, z1);
            vertices[index + 1] = new Vector3(x2, 0f, z2);
            uv[index] = new Vector2((x1 / outRadius + 1) * 0.5f, (z1 / outRadius + 1) * 0.5f);
            uv[index + 1] = new Vector2((x2 / inRadius + 1) * 0.5f, (z2 / inRadius + 1) * 0.5f);

            if (i > 0)
            {
                int triIndex = (i - 1) * 6;
                triangles[triIndex] = index - 2;
                triangles[triIndex + 1] = index - 1;
                triangles[triIndex + 2] = index + 1;
                triangles[triIndex + 3] = index - 2;
                triangles[triIndex + 4] = index + 1;
                triangles[triIndex + 5] = index;
            }

            currentAngle += angleIncrement;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
}
