using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Edge : MonoBehaviour
{

    [Header("Parent References")]
    public Mesh ParentMesh;
    public Transform ParentTransform;
    public MeshManagerV3 ParentMeshManager;

    [Header("Children Edge Transforms")]
    public Transform Mesh1;
    public Transform Mesh2;
    public Transform Mesh3;

    [Header("Children Transform Data")]
    [SerializeField] private Vector3 averagePositon1;
    [SerializeField] private Vector3 averagePositon2;
    [SerializeField] private Vector3 averagePositon3;
    public Vector3 AveragePositon;

    [SerializeField] private float distance1;
    [SerializeField] private float distance2;
    [SerializeField] private float distance3;

    public bool CollidersEnabled = true;

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
        averagePositon1 = ((ParentMesh.vertices[0] + ParentMesh.vertices[1]) / 2) + ParentTransform.position;
        distance1 = Vector3.Distance(ParentMesh.vertices[0], ParentMesh.vertices[1]);
        averagePositon2 = ((ParentMesh.vertices[1] + ParentMesh.vertices[2]) / 2) + ParentTransform.position;
        distance2 = Vector3.Distance(ParentMesh.vertices[1], ParentMesh.vertices[2]);
        averagePositon3 = ((ParentMesh.vertices[0] + ParentMesh.vertices[2]) / 2) + ParentTransform.position;
        distance3 = Vector3.Distance(ParentMesh.vertices[0], ParentMesh.vertices[2]);
        AveragePositon = ((averagePositon1 + averagePositon2 + averagePositon3) / 3) + ParentTransform.position;
        Mesh1.position = Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * averagePositon1;
        Mesh2.position = Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * averagePositon2;
        Mesh3.position = Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * averagePositon3;

        Mesh1.localScale = new Vector3(0.025f, 0.025f, distance1);
        Mesh2.localScale = new Vector3(0.025f, 0.025f, distance2);
        Mesh3.localScale = new Vector3(0.025f, 0.025f, distance3);

        Mesh1.LookAt(Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * ParentMesh.vertices[0]);
        Mesh2.LookAt(Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * ParentMesh.vertices[1]);
        Mesh3.LookAt(Quaternion.Euler(ParentTransform.transform.parent.localRotation.eulerAngles) * ParentMesh.vertices[2]);
        if (!ParentMeshManager.EdgesReady)
        {
            ParentMeshManager.EdgesReady = true;
        }
    }

    public void DisableColliders()
    {
        Mesh1.GetComponent<Collider>().enabled = false;
        Mesh2.GetComponent<Collider>().enabled = false;
        Mesh3.GetComponent<Collider>().enabled = false;
        CollidersEnabled = false;
    }
    public void EnableColliders()
    {
        Mesh1.GetComponent<Collider>().enabled = true;
        Mesh2.GetComponent<Collider>().enabled = true;
        Mesh3.GetComponent<Collider>().enabled = true;
        CollidersEnabled = true;
    }
}
