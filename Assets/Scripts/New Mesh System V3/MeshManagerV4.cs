using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UnityEngine;

public class MeshManagerV4 : MonoBehaviour
{
    [Header("Script References")]
    public MeshManipulatorV2 meshVisualizer;

    [Header("Settings")]
    public bool isCloned = false;
    public GameObject PreparedObject;
    public GameObject handle;

    [Header("UnityEngine.Debug Info")]
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] Mesh originalMesh;
    public GameObject SpawnedHandle;
    public Mesh clonedMesh;
    [SerializeField] private bool DoubleFaced = false;
    public List<Triangle2> TriangleComponents = new List<Triangle2>();
    public List<MeshFilter> TrianglesMeshFilters = new List<MeshFilter>();
    public List<GameObject> TrianglesObjects = new List<GameObject>();
    public List<GameObject> Edges = new List<GameObject>();
    public List<GameObject> Vertices = new List<GameObject>();
    [SerializeField] private int[] trianglesArray;
    [SerializeField] private Vector3[] verticesArray;


    public List<List<int>> RelatedVertices = new List<List<int>>();
    public List<List<int>> RelatedEdges = new List<List<int>>();


    public List<List<int>> EdgeToVerticeRelation = new List<List<int>>();

    public Dictionary<GameObject, List<int>> TriToVerticeRelation = new Dictionary<GameObject, List<int>>();


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
            // Set up Triangle and Vertice Arry's for a double faced Triangle2 (two triangles)
            int[] CreationTriangles = { 0, 1, 2, 3, 4, 5 };
            Vector3[] CreationVert = new Vector3[6];

            /// Depending on if the mesh is doubled faced or not, we want to do different things here.
            /// If it is not double faced, we need to make it double faced so the user can see the Triangle from both sides.
            /// However, if it is already double faced, we really do not want to add more triangles for performance reasons,
            /// and also for simplifying the handling modifying the mesh.
            /// Other than that, the building of the Triangle game object is exactly the same, same number of
            /// verts and triangles so this is done in a helper function, once the Vertices are calculated from
            /// this.meshFilter.
            
            if (!DoubleFaced)
            {
                for (int i = 0; i < trianglesArray.Length; i += 3)
                {
                    //Create a name formatted like "CubeTriangle0".
                    string name = this.gameObject.name + "Triangle";
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
                    string name = this.gameObject.name + "Triangle";
                    CreationVert[0] = verticesArray[trianglesArray[i]];
                    CreationVert[1] = verticesArray[trianglesArray[i + 1]];
                    CreationVert[2] = verticesArray[trianglesArray[i + 2]];
                    CreationVert[3] = verticesArray[trianglesArray[i + 3]];
                    CreationVert[4] = verticesArray[trianglesArray[i + 4]];
                    CreationVert[5] = verticesArray[trianglesArray[i + 5]];

                    TriangleAssembler(name, CreationVert, CreationTriangles);
                }
            }
            //TriReady = true;
            // Now all the Triangle2 gameObjects are spawned, turn off the this.gameObject's MeshCollider and MeshRenderer.
            this.GetComponent<MeshRenderer>().enabled = false;
            this.GetComponent<MeshCollider>().enabled = false;
            this.enabled = true;
            UnityEngine.Debug.Log("Initilised MeshManagerV4.cs");


        }
    }

    // This converts all the Triangle gameObjects back into vertices and triangles for this.gameObject's MeshFilter.
    // This also destroy's the Edges as the triangles are their parents.
    public void DeinitiMesh()
    {
        // Don't do anything if the mesh is not cloned.
        if (isCloned)
        {
            // Set the size of the vertices and triangles array's. These should be 6x the number of SubMeshes.
            verticesArray = new Vector3[TrianglesMeshFilters.Count * 6];
            trianglesArray = new int[TrianglesMeshFilters.Count * 6];

            // B is used to interate through the Triangle/vertice position in the vetice and Triangle array's.
            int b = 0;
            for (int i = 0; i < TrianglesMeshFilters.Count; i++)
            {
                // Get all the vertices from the Triangle at index "i", assign it to the Vertices Array at the equal position.
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
            Vertices.Clear();
            Edges.Clear();
            // Set the meshFilter's mesh to the new vertice and Triangle array's we've created based off
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

            // Useful UnityEngine.Debug to see if everything gets cleared
            //UnityEngine.Debug.Log(EdgeToVerticeRelation.Count.ToString() + RelatedVertices.Count.ToString() + RelatedEdges.Count.ToString() + TriangleComponents.Count.ToString() +
            //    TrianglesMeshFilters.Count.ToString() + TrianglesObjects.Count.ToString() + TriToVerticeRelation.Count.ToString() + EdgeComponents.Count.ToString() + VertexComponents.Count.ToString()
            //    + Vertices.Count.ToString() + Edges.Count.ToString());
        }
    }

    public void InstantiateHandle(Transform T, Vector3 P)
    {
        GameObject go = (GameObject)Instantiate(handle, Vector3.zero, Quaternion.identity);
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
    // This function creates a gameObject for a single double faced Triangle2.
    void TriangleAssembler(string name, Vector3[] CreationVert, int[] CreationTriangles)
    {

        name += (TriangleComponents.Count).ToString();
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
        TriangleComponents.Add(go.GetComponent<Triangle2>());
        TriangleComponents[TriangleComponents.Count - 1].InitMesh();
        TriangleComponents[TriangleComponents.Count - 1].ParentMeshManager = this;
        TrianglesMeshFilters.Add(go.GetComponent<MeshFilter>());
        TrianglesObjects.Add(go);

        Vertices.Add(TriangleComponents[TriangleComponents.Count - 1].VertMesh1.gameObject);
        Vertices.Add(TriangleComponents[TriangleComponents.Count - 1].VertMesh2.gameObject);
        Vertices.Add(TriangleComponents[TriangleComponents.Count - 1].VertMesh3.gameObject);

        Edges.Add(TriangleComponents[TriangleComponents.Count - 1].EdgeMesh1.gameObject);
        Edges.Add(TriangleComponents[TriangleComponents.Count - 1].EdgeMesh2.gameObject);
        Edges.Add(TriangleComponents[TriangleComponents.Count - 1].EdgeMesh3.gameObject);
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
        UnityEngine.Debug.Log("Cloned Mesh");
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
    /// This should also making indavdual Triangle2 selection doable as now selecting a Triangle is the same as selecting 3 vertices at once.
    /// There by we just have to select 3 vertices plus shared ones in order to manipulate a Triangle.
    /// 
    /// 
    /// Further optimisations:
    /// - If the vertice or edge being clicked on, can tell the program *which* Sublist it is a part of inside the Related vertice/edge list.
    ///   As then no searching at all would be required.
    ///   
    /// - Splitting this function into two functions each on seperate threads, probably something for DOTS.
    ///   This function scales poorly with mesh complexity, however it only needs to be done once when the mesh is "intitliased".
    /// 
    /// - Investigate sorting algoritms and searching algorithms.
    /// </summary>
    void BuildAssociationLists()
    {
        Stopwatch StopWatch = new Stopwatch();
        StopWatch.Start();
        RelatedVertices.Clear();
        RelatedEdges.Clear();
        Vector3 Main;
        Vector3 Comparer;
        List<GameObject> LocalVertices = new List<GameObject>(Vertices);
        List<GameObject> LocalEdges = new List<GameObject>(Edges);

        List<int> AlreadyAdded = new List<int>();
        // Builds lists of related vertices.
        for (int i = 0; i < Vertices.Count; i++)
        {
            if (!AlreadyAdded.Contains(i))
            {
                List<int> ListBuilder = new List<int>();
                Main = Vertices[i].transform.position;
                ListBuilder.Add(i);
                Vertices[i].name = (RelatedVertices.Count).ToString();
                for (int j = 0; j < Vertices.Count; j++)
                {
                    if (!AlreadyAdded.Contains(j) && (i != j))
                    {
                        Comparer = Vertices[j].transform.position;

                        if (Main == Comparer)
                        {
                            ListBuilder.Add(j);
                            Vertices[j].name = (RelatedVertices.Count).ToString();
                        }
                    }
                }
                RelatedVertices.Add(ListBuilder);
                AlreadyAdded.AddRange(ListBuilder);
            }
        }
        AlreadyAdded.Clear();
        StopWatch.Stop();
        TimeSpan ts = StopWatch.Elapsed;
        UnityEngine.Debug.Log("Related Verts Execution time: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
        ts.Hours, ts.Minutes, ts.Seconds,
        ts.Milliseconds / 10));
        StopWatch = new Stopwatch();
        StopWatch.Start();

        // Builds lists of related Edges.
        for (int i = 0; i < Edges.Count; i++)
        {
            if (!AlreadyAdded.Contains(i))
            {
                List<int> ListBuilder = new List<int>();
                Main = Edges[i].transform.position;
                ListBuilder.Add(i);
                Edges[i].name = (RelatedEdges.Count).ToString();
                for (int j = 0; j < Edges.Count; j++)
                {
                    if (!AlreadyAdded.Contains(j) && (i != j))
                    {
                        Comparer = Edges[j].transform.position;

                        if (Main == Comparer)
                        {
                            ListBuilder.Add(j);
                            Edges[j].name = (RelatedEdges.Count).ToString();
                        }
                    }
                }
                RelatedEdges.Add(ListBuilder);
                AlreadyAdded.AddRange(ListBuilder);
            }
        }

        StopWatch.Stop();
        ts = StopWatch.Elapsed;
        UnityEngine.Debug.Log("Related Edges Execution time: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
        ts.Hours, ts.Minutes, ts.Seconds,
        ts.Milliseconds / 10));

        StopWatch = new Stopwatch();
        StopWatch.Start();
        for (int i = 0; i < TriangleComponents.Count; i++) // Convert.ToInt32(VertexComponents[i].Mesh1.gameObject.name)
        {
            int Vert1 = Convert.ToInt32(TriangleComponents[i].VertMesh1.gameObject.name);
            int Vert2 = Convert.ToInt32(TriangleComponents[i].VertMesh2.gameObject.name);
            int Vert3 = Convert.ToInt32(TriangleComponents[i].VertMesh3.gameObject.name);

            EdgeToVerticeRelation.Add(new List<int> { Vert1, Vert2 });
            EdgeToVerticeRelation.Add(new List<int> { Vert2, Vert3 });
            EdgeToVerticeRelation.Add(new List<int> { Vert1, Vert3 });
            TriToVerticeRelation.Add(TrianglesObjects[i], new List<int> { Vert1, Vert2, Vert3 });
        }
        StopWatch.Stop();
        ts = StopWatch.Elapsed;
        UnityEngine.Debug.Log("Edge To Vertice & Tri To Vertice Execution time: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
        ts.Hours, ts.Minutes, ts.Seconds,
        ts.Milliseconds / 10));

        UnityEngine.Debug.Log("Related Verts: " + (RelatedVertices.Count - 1) + " Related Edges: " + (RelatedEdges.Count - 1) + " Edge To Vertice Relation Count: " + EdgeToVerticeRelation.Count +
            " Tri To Vertice Relation Count: " + TriToVerticeRelation.Count);
    }


    #endregion

    // Update is called once per frame
    void Update()
    {
        this.enabled = false;
        BuildAssociationLists();
        //DebugRelatedLists();
    }

    // We need a way to aggragate all the Vertices from all the SubMeshes so we can modify their positions
    // and dynamically show this to the user, so we get the current functionality of MeshManagerV1.

    // Given we can now, theortically, pull the enire Triangle, we need a way to do that too, this means we'd have to move all the vertices in the Triangle,
    // rather than moving the gameObject as this wouldn't change the vertice's position when being merged up into the main mesh.

    // Extruding vertice's like you can in blender would be a really nice feature, however when a face is created from this it MUST be trianglualted so unity can handle it.
    // This will be extremely complicated in all likely hood, the easy way would be to only let a single Triangle be created at once, the best way would be to allow
    // all vertices to be turned into a face and let the code work it out, but that sounds like a pain to code.
    // Whatever happens, it will require a way to display JUST the edges between vertices, which I have no idea about right now, possibly quill18's selection indication
    // video can help with this, I somehow doubt it tho. 
    // Edge display and selection would be super helpful cos modifying mesh normals is important.



    #region Unused Potentially Useful Code and Debugging Code

    void GetPointToScreen()
    {
        for (int i = 0; i < TrianglesMeshFilters[0].sharedMesh.vertexCount; i++)
        {
            UnityEngine.Debug.Log(Camera.main.WorldToScreenPoint(TrianglesMeshFilters[0].mesh.vertices[i]));
        }
        Vector3 position = (TrianglesMeshFilters[0].mesh.vertices[0] + TrianglesMeshFilters[0].mesh.vertices[1]) / 2;
        UnityEngine.Debug.Log(position);
        UnityEngine.Debug.Log(Quaternion.FromToRotation(position, (TrianglesMeshFilters[0].mesh.vertices[1])).eulerAngles);
    }

    // Useful for debugging related vertices and edges list
    // Remember to set everything correctly.
    void DebugRelatedLists()
    {
        UnityEngine.Debug.Log("RelatedVertices Count " + (RelatedVertices.Count - 1).ToString());
        //UnityEngine.Debug.Log("Total Count" + (Ve.Count - 1).ToString());
        for (int i = 0; i < RelatedVertices.Count; i++)
        {
            for (int j = 0; j < RelatedVertices[i].Count; j++)
            {
                UnityEngine.Debug.Log("List: " + i + " Vert: " + RelatedVertices[i][j]);// + " Edge Count: " + EdgeToVerticeRelation[i].Count);
            }
        }
    }
    #endregion
}
