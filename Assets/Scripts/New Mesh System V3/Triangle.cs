using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    public MeshCollider Collider;
    public bool CollidersEnabled = false;
    public MeshFilter ThisTriangle;
    [SerializeField] private Mesh ThisMesh;
    [SerializeField] private Vector3[] Verts;
    [SerializeField] private int[] Tris;


    public void InitMesh()
    {
        ThisMesh = ThisTriangle.mesh;
        Verts = ThisMesh.vertices;
        Tris = ThisMesh.triangles;
    }

    public void DoMeshAction(int VertIndex, Vector3 Offset)
    {
        Verts[VertIndex] += Offset;
        ThisMesh.vertices = Verts;
        ThisMesh.RecalculateNormals();
    }

    public void DisableColliders()
    {
        Collider.enabled = false;
        CollidersEnabled = false;
    }

    public void EnableColliders()
    {
        Collider.enabled = true;
        CollidersEnabled = true;
    }
}
