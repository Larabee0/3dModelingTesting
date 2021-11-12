using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshManagerV2 : MonoBehaviour
{
    [Header("Script References")]
    public MeshManipulator meshVisualizer;
    [Header("Settings")]

    public bool isCloned = false;
    public GameObject PreparedObject;
    public GameObject Edge;
    public GameObject Vertex;
    public GameObject handle;
    [Header("Debug Info")]
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] Mesh originalMesh;
    public GameObject SpawnedHandle;
    public Mesh clonedMesh;
    [SerializeField] private bool DoubleFaced = false;
    public List<MeshFilter> TrianglesObjects = new List<MeshFilter>();
    public List<Edge> EdgeComponents = new List<Edge>();
    public List<GameObject> Edges = new List<GameObject>();
    public List<Vertex> VertexComponents = new List<Vertex>();
    public List<GameObject> Vertices = new List<GameObject>();
    public int[] triangles;
    public Vector3[] vertices;

    //public List<int[]> TriangleSuperList = new List<int[]>();
    //public List<Vector3[]> VertexSuperList = new List<Vector3[]>();


    // Key = Spawned Triangle Object Number. Value = list of triangles for that object.
    public Dictionary<int, List<int>> TriangleDictionary = new Dictionary<int, List<int>>();
    // Key = Spawned Triangle Object Number, Value is equal to the list of vertices for that object.
    public Dictionary<int, List<Vector3>> VertexDictionary = new Dictionary<int, List<Vector3>>();

    #region Public Methods
    /// <summary>
    /// This splits a mesh into triangles as seperate gameObjects as children of this.gameObject
    /// It also converts Meshes that are back faced culled to double the verts and triangles so they aren't.
    /// This is achived with the DoubleFaced Flag which MUST always be accruate, 
    /// otherwise uncessary Vertices and Triangles will be added to the mesh.
    /// </summary>    
    public void InitiMesh()
    {
        // Don't do anything if we are already cloned.
        if (!isCloned)
        {
            // Clone the Mesh
            CloneMesh();
            // Set up Triangle and Vertice Arry's for a double faced Triangle (two triangles)
            int[] CreationTriangles = { 0, 1, 2, 3, 4, 5 };
            Vector3[] CreationVert = new Vector3[6];
            
            // Depending on if the mesh is doubled faced or not, we want to do different things here.
            // If it is not double faced, we need to make it double faced so the user can see the triangle from both sides.
            // However, if it is already double faced, we really do not want to add more triangles for performance reasons,
            // and also for simplifying the handling modifying the mesh.
            // Other than that, the building of the triangle game object is exactly the same, same number of
            // verts and triangles so this is done in a helper function, once the Vertices are calculated from
            // this.meshFilter.
            if (!DoubleFaced)
            {
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    //Create a name formatted like "CubeTriangle0".
                    string name = this.gameObject.name + "Triangle" + i.ToString();
                    CreationVert[0] = vertices[triangles[i]];
                    CreationVert[1] = vertices[triangles[i + 1]];
                    CreationVert[2] = vertices[triangles[i + 2]];
                    CreationVert[3] = vertices[triangles[i + 2]];
                    CreationVert[4] = vertices[triangles[i + 1]];
                    CreationVert[5] = vertices[triangles[i]];

                    TriangleAssembler(name, CreationVert, CreationTriangles);
                }
            }
            else
            {
                for (int i = 0; i < triangles.Length; i += 6)
                {
                    string name = this.gameObject.name + "Triangle" + i.ToString();
                    CreationVert[0] = vertices[triangles[i]];
                    CreationVert[1] = vertices[triangles[i + 1]];
                    CreationVert[2] = vertices[triangles[i + 2]];
                    CreationVert[3] = vertices[triangles[i + 3]];
                    CreationVert[4] = vertices[triangles[i + 4]];
                    CreationVert[5] = vertices[triangles[i + 5]];

                    TriangleAssembler(name, CreationVert, CreationTriangles);
                }
            }
            // Now all the triangle gameObjects are spawned, turn off the this.gameObject's MeshCollider and MeshRenderer.
            this.GetComponent<MeshRenderer>().enabled = false;
            this.GetComponent<MeshCollider>().enabled = false;
            InstantiateEdges();
            Debug.Log("Initilised MeshManagerV2.cs");
            Debug.Log("Triangle Dict Length: " + (TriangleDictionary.Count - 1));
            Debug.Log("Vertex Dict Length: " + (VertexDictionary.Count - 1));
        }
    }

    // This converts all the triangle gameObjects back into vertices and triangles for this.gameObject's MeshFilter.
    // This also destroy's the Edges as the triangles are their parents.
    public void DeinitiMesh()
    {
        // Don't do anything if the mesh is not cloned.
        if (isCloned)
        {
            // Set the size of the vertices and triangles array's. These should be 6x the number of SubMeshes.
            vertices = new Vector3[TrianglesObjects.Count * 6];
            triangles = new int[TrianglesObjects.Count * 6];

            // B is used to interate through the triangle/vertice position in the vetice and triangle array's.
            int b = 0;
            for (int i = 0; i < TrianglesObjects.Count; i++)
            {
                vertices[b] = TrianglesObjects[i].mesh.vertices[0];
                vertices[b + 1] = TrianglesObjects[i].mesh.vertices[1];
                vertices[b + 2] = TrianglesObjects[i].mesh.vertices[2];
                vertices[b + 3] = TrianglesObjects[i].mesh.vertices[3];
                vertices[b + 4] = TrianglesObjects[i].mesh.vertices[4];
                vertices[b + 5] = TrianglesObjects[i].mesh.vertices[5];

                triangles[b] = b;
                triangles[b + 1] = b + 1;
                triangles[b + 2] = b + 2;
                triangles[b + 3] = b + 3;
                triangles[b + 4] = b + 4;
                triangles[b + 5] = b + 5;

                b += 6;
            }

            // If DoubleFaced is false, it is now true, and if it isn't then crash time.
            if (!DoubleFaced)
                DoubleFaced = true;

            // Destroy all the SubMeshes, we have no need for them anymore
            for (int i = 0; i < TrianglesObjects.Count; i++)
            {
                Destroy(TrianglesObjects[i].gameObject);
            }
            TrianglesObjects.Clear();
            TriangleDictionary.Clear();
            VertexDictionary.Clear();
            EdgeComponents.Clear();
            VertexComponents.Clear();
            Vertices.Clear();
            Edges.Clear();
            // Set the meshFilter's mesh to the new vertice and triangle array's we've created based off
            // the SubMeshes.
            meshFilter.mesh.vertices = vertices;
            meshFilter.mesh.triangles = triangles;
            meshFilter.mesh.RecalculateNormals();

            //Enabled the things and set isCloned to false.
            this.GetComponent<MeshRenderer>().enabled = true;
            this.GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
            this.GetComponent<MeshCollider>().enabled = true;
            isCloned = false;

            Debug.Log("Triangle Dict Length: " + (TriangleDictionary.Count - 1));
            Debug.Log("Vertex Dict Length: " + (VertexDictionary.Count - 1));
        }
    }

    // Spawns Edge Components, this places a cube around a the edge of each triangle.
    public void InstantiateEdges()
    {
        Edges.Clear();
        if (EdgeComponents.Count - 1 >= 0)
        {
            return;
        }
        for (int i = 0; i < TrianglesObjects.Count; i++)
        {
            GameObject go = (GameObject)Instantiate(Edge, TrianglesObjects[i].transform.position,Quaternion.identity);
            go.transform.SetParent(TrianglesObjects[i].transform);
            EdgeComponents.Add(go.GetComponent<Edge>());
            Edges.Add(EdgeComponents[EdgeComponents.Count - 1].Mesh1.gameObject);
            Edges.Add(EdgeComponents[EdgeComponents.Count - 1].Mesh2.gameObject);
            Edges.Add(EdgeComponents[EdgeComponents.Count - 1].Mesh3.gameObject);
        }
    }

    // Only destroys Edge Components.
    public void DestroyEdges()
    {
        if (EdgeComponents.Count - 1 < 0)
        {
            return;
        }
        for (int i = 0; i < EdgeComponents.Count; i++)
        {
            Destroy(EdgeComponents[i].gameObject);
        }
        EdgeComponents.Clear();
        Edges.Clear();
    }


    public void InstantiateVertices()
    {
        Vertices.Clear();
        if (VertexComponents.Count - 1 >= 0)
        {
            return;
        }
        for (int i = 0; i < TrianglesObjects.Count; i++)
        {
            GameObject go = (GameObject)Instantiate(Vertex, TrianglesObjects[i].transform.position,Quaternion.identity);
            go.transform.SetParent(TrianglesObjects[i].transform);
            VertexComponents.Add(go.GetComponent<Vertex>());
            Vertices.Add(VertexComponents[VertexComponents.Count - 1].Mesh1.gameObject);
            Vertices.Add(VertexComponents[VertexComponents.Count - 1].Mesh2.gameObject);
            Vertices.Add(VertexComponents[VertexComponents.Count - 1].Mesh3.gameObject);
        }
    }

    public void DestroyVertices()
    {
        if (VertexComponents.Count - 1 < 0)
        {
            return;
        }
        for (int i = 0; i < VertexComponents.Count; i++)
        {
            Destroy(VertexComponents[i].gameObject);
        }
        VertexComponents.Clear();
        Vertices.Clear();
    }

    #endregion

    #region Helpers
    // This function creates a gameObject for a single double faced triangle.
    void TriangleAssembler(string name, Vector3[] CreationVert, int[] CreationTriangles)
    {
        List<int> TrianglesListTemp =  CreationTriangles.ToList();
        List<Vector3> VerticesListTemp = CreationVert.ToList();
        
        Mesh CreationMesh = new Mesh
        {
            name = name,
            vertices = CreationVert,
            triangles = CreationTriangles
        };
        CreationMesh.RecalculateNormals();
        GameObject go = (GameObject)Instantiate(PreparedObject, this.transform);
        go.GetComponent<MeshFilter>().mesh = CreationMesh;
        go.GetComponent<MeshCollider>().sharedMesh = CreationMesh;
        go.name = name;

        TrianglesObjects.Add(go.GetComponent<MeshFilter>());
        TriangleDictionary.Add(TrianglesObjects.Count - 1, TrianglesListTemp);
        VertexDictionary.Add(TrianglesObjects.Count - 1, VerticesListTemp);
    }


    // This clones the mesh
    void CloneMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh;
        clonedMesh = new Mesh
        {
            name = this.gameObject.name + "clone",
            vertices = originalMesh.vertices,
            triangles = originalMesh.triangles,
            normals = originalMesh.normals,
            uv = originalMesh.uv
        };
        meshFilter.mesh = clonedMesh;
        vertices = clonedMesh.vertices;
        triangles = clonedMesh.triangles;
        isCloned = true;
        Debug.Log("Cloned Mesh");
    }

    public void InstantiateHandle(Transform T, Vector3 point)
    {
        GameObject go = (GameObject)Instantiate(handle, new Vector3(0f, 0f, 0f), Quaternion.identity);
        go.transform.position = point;
        go.transform.localScale *= this.transform.localScale.magnitude * 0.75f;
        go.transform.SetParent(T);
        SpawnedHandle = go;
    }

    public void ReTransformHandle(Transform T, Vector3 point)
    {
        SpawnedHandle.transform.SetParent(T);
        SpawnedHandle.transform.position = point;
        SpawnedHandle.transform.localScale *= this.transform.localScale.magnitude * 0.75f;
    }
    #endregion

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        
    //        DeinitiMesh();
    //    }
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        InitiMesh();
    //    }
    //
    //    if (SubMeshes.Count - 1 >= 0)
    //    {
    //        if (Input.GetKeyDown(KeyCode.E) && Input.GetKey(KeyCode.LeftShift))
    //        {
    //            DestroyEdges();
    //        }
    //        else if (Input.GetKeyDown(KeyCode.E))
    //        {
    //            InstantiateEdges();
    //        }
    //    }
    //}

    // We need a way to aggragate all the Vertices from all the SubMeshes so we can modify their positions
    // and dynamically show this to the user, so we get the current functionality of MeshManagerV1.

    // Given we can now, theortically, pull the enire triangle, we need a way to do that too, this means we'd have to move all the vertices in the triangle,
    // rather than moving the gameObject as this wouldn't change the vertice's position when being merged up into the main mesh.

    // Extruding vertice's like you can in blender would be a really nice feature, however when a face is created from this it MUST be trianglualted so unity can handle it.
    // This will be extremely complicated in all likely hood, the easy way would be to only let a single triangle be created at once, the best way would be to allow
    // all vertices to be turned into a face and let the code work it out, but that sounds like a pain to code.
    // Whatever happens, it will require a way to display JUST the edges between vertices, which I have no idea about right now, possibly quill18's selection indication
    // video can help with this, I somehow doubt it tho. 
    // Edge display and selection would be super helpful cos modifying mesh normals is important.



    #region Unused Potentially Useful Code
    void GetPointToScreen()
    {
        for (int i = 0; i < TrianglesObjects[0].sharedMesh.vertexCount; i++)
        {
            Debug.Log(Camera.main.WorldToScreenPoint(TrianglesObjects[0].mesh.vertices[i]));
        }
        Vector3 position = (TrianglesObjects[0].mesh.vertices[0] + TrianglesObjects[0].mesh.vertices[1]) / 2;
        Debug.Log(position);
        Debug.Log(Quaternion.FromToRotation(position, (TrianglesObjects[0].mesh.vertices[1])).eulerAngles);
    }
    #endregion
}
