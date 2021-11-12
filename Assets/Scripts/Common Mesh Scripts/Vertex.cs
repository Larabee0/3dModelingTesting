using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{

    [Header("Parent References")]
    public Mesh ParentMesh;
    public Transform ParentTransform;
    public MeshManagerV3 ParentMeshManager;

    [Header("Children Vertex Transforms")]
    public Transform Mesh1;
    public Transform Mesh2;
    public Transform Mesh3;

    [Header("Children Transform Data")]
    [SerializeField] private Vector3 Position1;
    [SerializeField] private Vector3 Position2;
    [SerializeField] private Vector3 Position3;

    public bool CollidersEnabled = true; // technically also visible enabled.

    // Start is called before the first frame update
    void Start()
    {
        ParentMesh = transform.parent.GetComponent<MeshFilter>().mesh;
        ParentTransform = transform.parent.transform;
        ParentMeshManager = GetComponentInParent<MeshManagerV3>();
    }

    // Update is called once per frame
    void Update()
    {
        Position1 = (ParentMesh.vertices[0] + ParentTransform.position);

        Position2 = (ParentMesh.vertices[1] + ParentTransform.position);

        Position3 = (ParentMesh.vertices[2] + ParentTransform.position);

        Mesh1.position = Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * Position1;
        Mesh2.position = Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * Position2;
        Mesh3.position = Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * Position3;

        if (!ParentMeshManager.VerticesReady)
        {
            ParentMeshManager.VerticesReady = true;
        }
    }

    public void DisableColliders()
    {
        Mesh1.GetComponent<Collider>().enabled = false;
        Mesh2.GetComponent<Collider>().enabled = false;
        Mesh3.GetComponent<Collider>().enabled = false;
        Mesh1.GetComponent<MeshRenderer>().enabled = false;
        Mesh2.GetComponent<MeshRenderer>().enabled = false;
        Mesh3.GetComponent<MeshRenderer>().enabled = false;
        CollidersEnabled = false;
    }
    public void EnableColliders()
    {
        Mesh1.GetComponent<Collider>().enabled = true;
        Mesh2.GetComponent<Collider>().enabled = true;
        Mesh3.GetComponent<Collider>().enabled = true;
        Mesh1.GetComponent<MeshRenderer>().enabled = true;
        Mesh2.GetComponent<MeshRenderer>().enabled = true;
        Mesh3.GetComponent<MeshRenderer>().enabled = true;
        CollidersEnabled = true;
    }
}
