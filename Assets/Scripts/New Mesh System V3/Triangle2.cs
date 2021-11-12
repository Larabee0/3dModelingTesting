using UnityEngine;
/// <summary>
/// This component handles updating the position of all the visual aspects of the triangle, the vertices and edges.
/// This component is also what changes the mesh, called by the MeshManipulator.
/// This component also controls the colliders and turning visuals off when changing between vertex, edge, and triangle selection modes.
/// </summary>
public class Triangle2 : MonoBehaviour
{
    [Header("References")] // Main Script and componenet References
    public MeshManagerV4 ParentMeshManager;
    public MeshCollider TriCollider;
    public MeshFilter TriMeshFilter;
    [SerializeField] private Mesh TriMesh;

    [Header("Mesh Detials")] // Details about the triangle mesh
    [SerializeField] private Vector3[] Verts;
    [SerializeField] private int[] Tris;
    [SerializeField] private int[] VertexRemap = { 5, 4, 3, 2, 1, 0 };

    [Header("Tri Information")]// Infomations and settings reguarding the triangle for the purposes of the selection script.
    public bool TriCollidersEnabled = false;


    [Header("Vertex Information")] // Infomations and settings reguarding the vertices for the purposes of the selection script.
    public bool VertCollidersEnabled = true; // Also Visability enabled/disable.

    [Header("Vertex Children Transforms")] // The transforms of each Vertice visual component.
    public Transform VertMesh1;
    public Transform VertMesh2;
    public Transform VertMesh3;

    [Header("Vertex Children Transform Data")] // Mostly for debugging.
    [SerializeField] private Vector3 VertPosition1;
    [SerializeField] private Vector3 VertPosition2;
    [SerializeField] private Vector3 VertPosition3;


    [Header("Edge Information")] //  Infomations and settings reguarding the triangle for the purposes of the selection script.
    public bool EdgeCollidersEnabled = true;

    [Header("Edge Children Transforms")] // The transforms of each Edge visual component.
    public Transform EdgeMesh1;
    public Transform EdgeMesh2;
    public Transform EdgeMesh3;

    [Header("Edge Children Transform Data")]  // Mostly for debugging.
    public Vector3 EdgeAveragePositon;
    [SerializeField] private Vector3 EdgeAveragePositon1;
    [SerializeField] private Vector3 EdgeAveragePositon2;
    [SerializeField] private Vector3 EdgeAveragePositon3;
    [SerializeField] private float EdgeDistance1;
    [SerializeField] private float EdgeDistance2;
    [SerializeField] private float EdgeDistance3;

    /// <summary>
    /// This is supposed to be calle after the Triangle has been spawned.
    /// </summary>
    public void InitMesh()
    {
        TriMesh = TriMeshFilter.mesh;
        Verts = TriMesh.vertices;
        Tris = TriMesh.triangles;
        TriMesh.RecalculateNormals();
        VertUpdate();
        EdgeUpdate();
    }

    /// <summary>
    /// Modify's the vertices of the triangle
    /// </summary>
    /// <param name="VertIndex"> the vertice to be modified </param>
    /// <param name="Offset">The offset Vector3, how much the vertice is being moved </param>
    public void DoMeshAction(int VertIndex, Vector3 Offset)
    {
        // Every triangle actually has two triangles so it can be seen from both sides, both needs changing.
        // So there exits a vertex remap array which contains the index of the mirrored vertice.
        Verts[VertIndex] += Offset;
        Verts[VertexRemap[VertIndex]] += Offset;

        // assign the vertices back to the mesh, recalculate its normals and reassign the mesh to the collider.
        TriMesh.vertices = Verts;
        TriMesh.RecalculateNormals();
        TriCollider.sharedMesh = TriMesh;

        VertUpdate();
        EdgeUpdate();
    }

    /// <summary>
    /// Disables the colliders relating to the Triangle shape itself.
    /// </summary>
    public void DisableTriColliders()
    {
        TriCollider.enabled = false;
        TriCollidersEnabled = false;
    }

    /// <summary>
    /// Enables the colliders relating to the Triangle shape itself.
    /// </summary>
    public void EnableTriColliders()
    {
        TriCollider.enabled = true;
        TriCollidersEnabled = true;
    }
    /// <summary>
    /// Disables the colliders relating to the Edge visuals.
    /// </summary>
    public void DisableEdgeColliders()
    {
        EdgeMesh1.GetComponent<Collider>().enabled = false;
        EdgeMesh2.GetComponent<Collider>().enabled = false;
        EdgeMesh3.GetComponent<Collider>().enabled = false;
        EdgeCollidersEnabled = false;
    }
    /// <summary>
    /// Enables the colliders relating to the Edge visuals.
    /// </summary>
    public void EnableEdgeColliders()
    {
        EdgeMesh1.GetComponent<Collider>().enabled = true;
        EdgeMesh2.GetComponent<Collider>().enabled = true;
        EdgeMesh3.GetComponent<Collider>().enabled = true;
        EdgeCollidersEnabled = true;
    }
    /// <summary>
    /// disalbes the colliders relating to the vertex visuals.
    /// Also disables the actual visuals.
    /// </summary>
    public void DisableVertColliders()
    {
        VertMesh1.GetComponent<Collider>().enabled = false;
        VertMesh2.GetComponent<Collider>().enabled = false;
        VertMesh3.GetComponent<Collider>().enabled = false;
        VertMesh1.GetComponent<MeshRenderer>().enabled = false;
        VertMesh2.GetComponent<MeshRenderer>().enabled = false;
        VertMesh3.GetComponent<MeshRenderer>().enabled = false;
        VertCollidersEnabled = false;
    }
    /// <summary>
    /// Enables the colliders relating to the vertex visuals.
    /// Also enables the actual visuals.
    /// </summary>
    public void EnableVertColliders()
    {
        VertMesh1.GetComponent<Collider>().enabled = true;
        VertMesh2.GetComponent<Collider>().enabled = true;
        VertMesh3.GetComponent<Collider>().enabled = true;
        VertMesh1.GetComponent<MeshRenderer>().enabled = true;
        VertMesh2.GetComponent<MeshRenderer>().enabled = true;
        VertMesh3.GetComponent<MeshRenderer>().enabled = true;
        VertCollidersEnabled = true;
    }


    /// <summary>
    /// Updates the visuals of all the vertices
    /// </summary>
    void VertUpdate()
    {
        VertPosition1 = (Verts[0] + this.transform.position);
        VertPosition2 = (Verts[1] + this.transform.position);
        VertPosition3 = (Verts[2] + this.transform.position);

        VertMesh1.position = Quaternion.Euler(this.transform.parent.localRotation.eulerAngles) * VertPosition1;
        VertMesh2.position = Quaternion.Euler(this.transform.parent.localRotation.eulerAngles) * VertPosition2;
        VertMesh3.position = Quaternion.Euler(this.transform.parent.localRotation.eulerAngles) * VertPosition3;
    }

    /// <summary>
    /// Updates the visuals of all the edges, and calculates important information about the edges
    /// for the selection system and handles.
    /// </summary>
    void EdgeUpdate()
    {
        EdgeAveragePositon1 = ((Verts[0] + Verts[1]) / 2) + this.transform.position;
        EdgeDistance1 = Vector3.Distance(Verts[0], Verts[1]);
        EdgeAveragePositon2 = ((Verts[1] + Verts[2]) / 2) + this.transform.position;
        EdgeDistance2 = Vector3.Distance(Verts[1], Verts[2]);
        EdgeAveragePositon3 = ((Verts[0] + Verts[2]) / 2) + this.transform.position;
        EdgeDistance3 = Vector3.Distance(Verts[0], Verts[2]);
        EdgeAveragePositon = ((EdgeAveragePositon1 + EdgeAveragePositon2 + EdgeAveragePositon3) / 3) + this.transform.position;
        EdgeMesh1.position = Quaternion.Euler(this.transform.transform.parent.localRotation.eulerAngles) * EdgeAveragePositon1;
        EdgeMesh2.position = Quaternion.Euler(this.transform.transform.parent.localRotation.eulerAngles) * EdgeAveragePositon2;
        EdgeMesh3.position = Quaternion.Euler(this.transform.transform.parent.localRotation.eulerAngles) * EdgeAveragePositon3;

        EdgeMesh1.localScale = new Vector3(0.025f, 0.025f, EdgeDistance1);
        EdgeMesh2.localScale = new Vector3(0.025f, 0.025f, EdgeDistance2);
        EdgeMesh3.localScale = new Vector3(0.025f, 0.025f, EdgeDistance3);

        EdgeMesh1.LookAt(Quaternion.Euler(this.transform.transform.parent.localRotation.eulerAngles) * Verts[0]);
        EdgeMesh2.LookAt(Quaternion.Euler(this.transform.transform.parent.localRotation.eulerAngles) * Verts[1]);
        EdgeMesh3.LookAt(Quaternion.Euler(this.transform.transform.parent.localRotation.eulerAngles) * Verts[2]);
    }
}
