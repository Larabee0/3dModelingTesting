using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshManagerV3 : MonoBehaviour
{
    [Header("Script References")]
    public MeshManipulatorV2 meshVisualizer;
    
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
    public List<Triangle> TriangleComponents = new List<Triangle>();
    public List<MeshFilter> TrianglesMeshFilters = new List<MeshFilter>();
    public List<GameObject> TrianglesObjects = new List<GameObject>();
    public List<Edge> EdgeComponents = new List<Edge>();
    public List<GameObject> Edges = new List<GameObject>();
    public List<Vertex> VertexComponents = new List<Vertex>();
    public List<GameObject> Vertices = new List<GameObject>();
    [SerializeField] private int[] trianglesArray;
    [SerializeField] private Vector3[] verticesArray;


    public List<List<int>> RelatedVertices = new List<List<int>>();
    public List<List<int>> RelatedEdges = new List<List<int>>();


    public List<List<int>> EdgeToVerticeRelation = new List<List<int>>();

    public Dictionary<GameObject, List<int>> TriToVerticeRelation = new Dictionary<GameObject, List<int>>();

    public bool EdgesReady = false;
    public bool VerticesReady = false;

    //public List<int[]> TriangleSuperList = new List<int[]>();
    //public List<Vector3[]> VertexSuperList = new List<Vector3[]>();

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
                for (int i = 0; i < trianglesArray.Length; i += 3)
                {
                    //Create a name formatted like "CubeTriangle0".
                    string name = this.gameObject.name + "Triangle" + i.ToString();
                    CreationVert[0] = verticesArray[trianglesArray[i]];
                    CreationVert[1] = verticesArray[trianglesArray[i + 1]];
                    CreationVert[2] = verticesArray[trianglesArray[i + 2]];
                    CreationVert[3] = verticesArray[trianglesArray[i + 2]];
                    CreationVert[4] = verticesArray[trianglesArray[i + 1]];
                    CreationVert[5] = verticesArray[trianglesArray[i]];

                    TriangleAssembler(name, CreationVert, CreationTriangles);
                }
            }
            else
            {
                for (int i = 0; i < trianglesArray.Length; i += 6)
                {
                    string name = this.gameObject.name + "Triangle" + i.ToString();
                    CreationVert[0] = verticesArray[trianglesArray[i]];
                    CreationVert[1] = verticesArray[trianglesArray[i + 1]];
                    CreationVert[2] = verticesArray[trianglesArray[i + 2]];
                    CreationVert[3] = verticesArray[trianglesArray[i + 3]];
                    CreationVert[4] = verticesArray[trianglesArray[i + 4]];
                    CreationVert[5] = verticesArray[trianglesArray[i + 5]];

                    TriangleAssembler(name, CreationVert, CreationTriangles);
                }
            }
            // Now all the triangle gameObjects are spawned, turn off the this.gameObject's MeshCollider and MeshRenderer.
            this.GetComponent<MeshRenderer>().enabled = false;
            this.GetComponent<MeshCollider>().enabled = false;
            InstantiateEdges();
            InstantiateVertices();
            this.enabled = true;
            Debug.Log("Initilised MeshManagerV3.cs");
            //Debug.Log("Triangle Dict Length: " + (TriangleDictionary.Count - 1));
            //Debug.Log("Vertex Dict Length: " + (VertexDictionary.Count - 1));

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
            verticesArray = new Vector3[TrianglesMeshFilters.Count * 6];
            trianglesArray = new int[TrianglesMeshFilters.Count * 6];

            // B is used to interate through the triangle/vertice position in the vetice and triangle array's.
            int b = 0;
            for (int i = 0; i < TrianglesMeshFilters.Count; i++)
            {
                // Get all the vertices from the triangle at index "i", assign it to the Vertices Array at the equal position.
                verticesArray[b] = TrianglesMeshFilters[i].mesh.vertices[0];
                verticesArray[b + 1] = TrianglesMeshFilters[i].mesh.vertices[1];
                verticesArray[b + 2] = TrianglesMeshFilters[i].mesh.vertices[2];
                verticesArray[b + 3] = TrianglesMeshFilters[i].mesh.vertices[3];
                verticesArray[b + 4] = TrianglesMeshFilters[i].mesh.vertices[4];
                verticesArray[b + 5] = TrianglesMeshFilters[i].mesh.vertices[5];

                // assign the triangles array in order of the vertices we've just assigned, so they match up/
                trianglesArray[b] = b;
                trianglesArray[b + 1] = b + 1;
                trianglesArray[b + 2] = b + 2;
                trianglesArray[b + 3] = b + 3;
                trianglesArray[b + 4] = b + 4;
                trianglesArray[b + 5] = b + 5;
                // Increment b by 6.
                b += 6;
            }

            // If DoubleFaced is false, it is now true, and if it isn't then crash time! (maybe?)
            if (!DoubleFaced)
                DoubleFaced = true;

            // Destroy all the SubMeshes, we have no need for them anymore
            for (int i = 0; i < TrianglesMeshFilters.Count; i++)
            {
                Destroy(TrianglesMeshFilters[i].gameObject);
            }
            // Clear all the lists and dictionaries.
            EdgeToVerticeRelation.Clear();
            RelatedVertices.Clear();
            RelatedEdges.Clear();
            TriangleComponents.Clear();
            TrianglesMeshFilters.Clear();
            TrianglesObjects.Clear();
            TriToVerticeRelation.Clear();
            EdgeComponents.Clear();
            VertexComponents.Clear();
            Vertices.Clear();
            Edges.Clear();
            // Set the meshFilter's mesh to the new vertice and triangle array's we've created based off
            // the TriangleObjects.
            meshFilter.mesh.vertices = verticesArray;
            meshFilter.mesh.triangles = trianglesArray;
            meshFilter.mesh.RecalculateNormals();

            //Enabled renderer, so the new 3d shape can be seen
            // assign the mesh collider, and enable it.
            this.GetComponent<MeshRenderer>().enabled = true;
            this.GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
            this.GetComponent<MeshCollider>().enabled = true;
            isCloned = false;

            // Useful Debug to see if everything gets cleared
            //Debug.Log(EdgeToVerticeRelation.Count.ToString() + RelatedVertices.Count.ToString() + RelatedEdges.Count.ToString() + TriangleComponents.Count.ToString() +
            //    TrianglesMeshFilters.Count.ToString() + TrianglesObjects.Count.ToString() + TriToVerticeRelation.Count.ToString() + EdgeComponents.Count.ToString() + VertexComponents.Count.ToString()
            //    + Vertices.Count.ToString() + Edges.Count.ToString());
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
        for (int i = 0; i < TrianglesMeshFilters.Count; i++)
        {
            GameObject go = (GameObject)Instantiate(Edge, TrianglesMeshFilters[i].transform.position, Quaternion.identity);
            go.transform.SetParent(TrianglesMeshFilters[i].transform);
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
        for (int i = 0; i < TrianglesMeshFilters.Count; i++)
        {
            GameObject go = (GameObject)Instantiate(Vertex, TrianglesMeshFilters[i].transform.position, Quaternion.identity);
            go.transform.SetParent(TrianglesMeshFilters[i].transform);
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


    public void InstantiateHandle(Transform T, Vector3 P)
    {
        GameObject go = (GameObject)Instantiate(handle, new Vector3(0f, 0f, 0f), Quaternion.identity);
        go.transform.position = P;
        go.transform.localScale *= this.transform.localScale.magnitude * 0.75f;
        go.transform.SetParent(T);
        SpawnedHandle = go;
    }

    public void ReTransformHandle(Transform T, Vector3 P)
    {
        SpawnedHandle.transform.SetParent(T);
        SpawnedHandle.transform.position = P;
        //SpawnedHandle.transform.localScale *= this.transform.localScale.magnitude * 0.75f;
    }

    #endregion

    #region Helpers
    // This function creates a gameObject for a single double faced triangle.
    void TriangleAssembler(string name, Vector3[] CreationVert, int[] CreationTriangles)
    {
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
        TriangleComponents.Add(go.GetComponent<Triangle>());
        TriangleComponents[TriangleComponents.Count - 1].InitMesh();
        TrianglesMeshFilters.Add(go.GetComponent<MeshFilter>());
        TrianglesObjects.Add(go);
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
        verticesArray = clonedMesh.vertices;
        trianglesArray = clonedMesh.triangles;
        isCloned = true;
        Debug.Log("Cloned Mesh");
    }

    /// <summary>
    /// This Creates the two list
    /// List 1 Related vertices this list contains a lists of ints which corrispond to vertices in the mesh. 
    /// Each Sublist of ints are vertices that share the same position, aka the same vertice.
    /// The idea is that this will speed up searching and selecting vertices as now the program only has to search through 8 items instead of 36 for a cube.
    /// And instead of it having to search through those 36 items multiple times to get other verts that share the same position, it only has to do it once.
    /// 
    /// List 2 Related Edges this List contains a list of ints which corrispond to edges in the mesh.
    /// Each Sublist of ints are edges that share the same position, aka the same edge.
    /// The idea is that this will speed up searching and selecting edges as now the program only has to search through 18 items instead of 36 for a cube.
    /// And instead of it having to search through those 36 items multiple time to get edges that share the same position, it only has to do it once.
    /// 
    /// Both these lists *should* be scalable with different types of meshes that share different numbers of vertices for one point
    /// and share different numbers of edges per edge.
    /// 
    /// This should also making indavdual triangle selection doable as now selecting a triangle is the same as selecting 3 vertices at once.
    /// There by we just have to select 3 vertices plus shared ones in order to manipulate a triangle.
    /// 
    /// 
    /// Further optimisations:
    /// - If the vertice or edge being clicked on, can tell the program *which* Sublist it is a part of inside the Related vertice/edge list.
    ///   As then no searching at all would be required.
    ///   
    /// - Splitting this function into two functions each on seperate threads, probably something for DOTS.
    ///   This function would scale poorly with mesh complexity, however it only needs to be done once when the mesh is "intitliased".
    /// 
    /// </summary>
    void BuildAssociationLists()
    {
        RelatedVertices.Clear();
        RelatedEdges.Clear();
        List<int> AlreadyAdded = new List<int>();
        Vector3 Main;
        Vector3 Comparer;

        // Builds lists of related vertices.
        for (int i = 0; i < Vertices.Count; i++)
        {
            if (!AlreadyAdded.Contains(i))
            {
                List<int> ListBuilder = new List<int>();
                Main = Vertices[i].transform.localPosition;
                ListBuilder.Add(i);
                Vertices[i].name = (RelatedVertices.Count).ToString();
                //Debug.Log("Adding to list " + i.ToString());
                for (int j = 0; j < Vertices.Count; j++)
                {
                    if (!AlreadyAdded.Contains(j) || (i != j))
                    {
                        Comparer = Vertices[j].transform.localPosition;

                        if (Main == Comparer)
                        {
                            ListBuilder.Add(j);
                            Vertices[j].name = (RelatedVertices.Count).ToString();
                            //Debug.Log("Adding to list "+ j.ToString());
                        }
                    }
                }
                RelatedVertices.Add(ListBuilder);
                for (int k = 0; k < ListBuilder.Count; k++)
                {
                    AlreadyAdded.Add(ListBuilder[k]);
                }
                //Debug.Log((RelatedVertices.Count - 1).ToString() + "Has been Completed");
            }
            //else
            //{
            //    Debug.Log(i.ToString() + "already in list");
            //}
        }
        AlreadyAdded.Clear();

        // Builds lists of related Edges.
        for (int i = 0; i < Edges.Count; i++)
        {
            // If we have already added i to a list then don't check it again.
            if (!AlreadyAdded.Contains(i))
            {
                List<int> ListBuilder = new List<int>();
                Main = Edges[i].transform.localPosition;
                ListBuilder.Add(i);
                Edges[i].name = (RelatedEdges.Count).ToString();
                //Debug.Log("Adding to list " + i.ToString());
                for (int j = 0; j < Edges.Count; j++)
                {
                    if (!AlreadyAdded.Contains(j) || (i != j))
                    {
                        Comparer = Edges[j].transform.localPosition;

                        if (Main == Comparer)
                        {
                            ListBuilder.Add(j);
                            Edges[j].name = (RelatedEdges.Count).ToString();
                            //Debug.Log("Adding to list "+ j.ToString());
                        }
                    }
                }
                RelatedEdges.Add(ListBuilder);
                for (int k = 0; k < ListBuilder.Count; k++)
                {
                    AlreadyAdded.Add(ListBuilder[k]);
                }
                //Debug.Log((RelatedVertices.Count - 1).ToString() + " Has been Completed");
            }
            //else
            //{
            //    Debug.Log(i.ToString() + " already in list");
            //}
        }

        for (int i = 0; i < TrianglesObjects.Count; i++) // Convert.ToInt32(VertexComponents[i].Mesh1.gameObject.name)
        {
            int Vert1 = Convert.ToInt32(VertexComponents[i].Mesh1.gameObject.name);
            int Vert2 = Convert.ToInt32(VertexComponents[i].Mesh2.gameObject.name);
            int Vert3 = Convert.ToInt32(VertexComponents[i].Mesh3.gameObject.name);

            EdgeToVerticeRelation.Add(new List<int> { Vert1, Vert2 });
            EdgeToVerticeRelation.Add(new List<int> { Vert2, Vert3 });
            EdgeToVerticeRelation.Add(new List<int> { Vert1, Vert3 });
            TriToVerticeRelation.Add(TrianglesObjects[i], new List<int> { Vert1, Vert2, Vert3 });
        }

        Debug.Log("Related Verts: " + (RelatedVertices.Count - 1) + " Related Edges: " + (RelatedEdges.Count - 1) + " Edge To Vertice Relation Count: " + EdgeToVerticeRelation.Count +
            " Tri To Vertice Relation Count: " + TriToVerticeRelation.Count);
    }


    #endregion

    // Update is called once per frame
    void Update()
    {
        if(EdgesReady && VerticesReady)
        {
            BuildAssociationLists();
            //DebugRelatedLists();
            this.enabled = false;
        }
    }

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



    #region Unused Potentially Useful Code and Debugging Code
    void GetPointToScreen()
    {
        for (int i = 0; i < TrianglesMeshFilters[0].sharedMesh.vertexCount; i++)
        {
            Debug.Log(Camera.main.WorldToScreenPoint(TrianglesMeshFilters[0].mesh.vertices[i]));
        }
        Vector3 position = (TrianglesMeshFilters[0].mesh.vertices[0] + TrianglesMeshFilters[0].mesh.vertices[1]) / 2;
        Debug.Log(position);
        Debug.Log(Quaternion.FromToRotation(position, (TrianglesMeshFilters[0].mesh.vertices[1])).eulerAngles);
    }

    // Useful for debugging related vertices and edges list
    // Remember to set everything correctly.
    void DebugRelatedLists()
    {
        //Debug.Log("RelatedEdge Count" + (EdgeToVerticeRelation.Count - 1).ToString());
        //Debug.Log("Total Count" + (Edges.Count - 1).ToString());
        for (int i = 0; i < TriToVerticeRelation.Count; i++)
        {
            for (int j = 0; j < TriToVerticeRelation[TrianglesObjects[i]].Count; j++)
            {
                Debug.Log("List: " + i + " Vert: " + TriToVerticeRelation[TrianglesObjects[i]][j]);// + " Edge Count: " + EdgeToVerticeRelation[i].Count);
            }
        }
    }

    #endregion
}
