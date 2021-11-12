using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MeshSelectionV2 : MonoBehaviour
{
    [Header("Script References")]
    public MeshManipulatorV2 MV;
    public MeshManagerV3 MM;
    private UniversalSelection UniSelect;

    [Header("Settings")]
    public Material Deselected;
    public Material PrimarySelected;
    public Material SecondarySelected;
    public Material DeselectedTriangle;
    public uint Mode = 0; // Edges = 0, Faces = 2, Vertices = 1
    [Header("Debug Info")]
    public List<int> VertexSubListRecord = new List<int>(); // The Index's of all currently selected SubLists in the SuperList.
    public List<GameObject> VertexSelectionList = new List<GameObject>();
    public GameObject PrimarySelectedObjectVertex;

    public List<int> EdgeSubListRecord = new List<int>(); // The Index's of all currently selected SubLists in the SuperList.
    public List<GameObject> EdgeSelectionList = new List<GameObject>();
    public GameObject PrimarySelectedObjectEdge;

    public List<GameObject> TriangleSelectionList = new List<GameObject>();
    public GameObject PrimarySelectedObjectTri;

    // Start is called before the first frame update
    void Start()
    {
        UniSelect = GetComponentInParent<UniversalSelection>();
        if(UniSelect == null)
        {
            this.enabled = false;
            Debug.LogError("Could not Find Universal Selection Script");
        }
    }

    // Update is called once per frame
    /// <summary>
    /// Update needs splitting up into smaller functions, 
    /// theres too much code in it for my liking
    /// </summary>
    void Update()
    {
        // Collider found stops the selection system running when manipulating the mesh.
        if (!MV.FoundCollider)
        {
            if (Input.GetMouseButtonUp(0))
            {
                ConvertGlobalSelectionToLocal();
            }

            if (Input.GetMouseButtonDown(0))
            {
                UniSelect.SelectionMouseDown(PrimarySelected, false);
            }

            if (Input.GetMouseButton(0))
            {
                UniSelect.SelectionMouse();
            }
        }
        // Obviously mouse up inside the above IF statement won't run, and FoundCollider needs to be set to False every mouse up.
        if (Input.GetMouseButtonUp(0))
        {
            MV.FoundCollider = false;
        }

        // Initlise Mesh Mode. (Defaults to Vertice Mode (mode 1))
        if (Input.GetKeyDown(KeyCode.I) && Input.GetKey(KeyCode.LeftShift)) // Shuts down the mesh
        {
            // Merges all the triangles back into the one mesh.
            GeneralDeselect();
            MM.DeinitiMesh();
        }
        else if (Input.GetKeyDown(KeyCode.I)) // Clones splits mesh into seperate triangle objects.
        {
            MM.InitiMesh();

            // Disable colliders in the Faces and Edges, as the default mode is vertex Selection
            if (MM.EdgeComponents[0].CollidersEnabled)
            {
                ToggleEdges();
            }
            if (MM.TriangleComponents[0].CollidersEnabled)
            {
                ToggleFaces();
            }
            // Set selectable objects to all spawned Vertices. Set the mode to 1 aka vertex mode.
            UniSelect.SelectableObjects = MM.Vertices;
            Mode = 1;
        }

        // If the triangle objects are spawned, we do the things.
        if (MM.TrianglesObjects.Count - 1 >= 0)
        {
            // Vertice Mode
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (!MM.VertexComponents[0].CollidersEnabled)
                {
                    if (MM.EdgeComponents[0].CollidersEnabled)
                    {
                        ToggleEdges();
                    }
                    if (MM.TriangleComponents[0].CollidersEnabled)
                    {
                        ToggleFaces();
                    }
                    ToggleVertices();
                    GeneralDeselect();
                    //EdgeDeselection();
                    UniSelect.SelectableObjects = MM.Vertices;
                    Mode = 1;
                }
            }





            // Edge Mode
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (!MM.EdgeComponents[0].CollidersEnabled)
                {
                    if (MM.VertexComponents[0].CollidersEnabled)
                    {
                        ToggleVertices();
                    }
                    if (MM.TriangleComponents[0].CollidersEnabled)
                    {
                        ToggleFaces();
                    }
                    ToggleEdges();
                    GeneralDeselect();
                    UniSelect.SelectableObjects = MM.Edges;
                    Mode = 0;
                }
            }






            // Face Mode
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (MM.VertexComponents[0].CollidersEnabled)
                {
                    ToggleVertices();
                }
                if (MM.EdgeComponents[0].CollidersEnabled)
                {
                    ToggleEdges();
                }
                ToggleFaces();
                GeneralDeselect();
                UniSelect.SelectableObjects = MM.TrianglesObjects;
                Mode = 2;
            }
        }
    }

    #region Component Visability and Collider Toggles
    void ToggleEdges()
    {
        if (MM.EdgeComponents[0].CollidersEnabled)
        {
            for (int i = 0; i < MM.EdgeComponents.Count; i++)
            {
                MM.EdgeComponents[i].DisableColliders();
            }
        }
        else
        {
            for (int i = 0; i < MM.EdgeComponents.Count; i++)
            {
                MM.EdgeComponents[i].EnableColliders();
            }
        }
    }
    void ToggleVertices()
    {
        if (MM.VertexComponents[0].CollidersEnabled)
        {
            for (int i = 0; i < MM.VertexComponents.Count; i++)
            {
                MM.VertexComponents[i].DisableColliders();
            }
        }
        else
        {
            for (int i = 0; i < MM.VertexComponents.Count; i++)
            {
                MM.VertexComponents[i].EnableColliders();
            }
        }
    }

    void ToggleFaces()
    {
        if (MM.TriangleComponents[0].CollidersEnabled)
        {
            Debug.Log("Trying to disable colliders");
            for (int i = 0; i < MM.TriangleComponents.Count; i++)
            {
                MM.TriangleComponents[i].DisableColliders();
            }
        }
        else
        {
            Debug.Log("Trying to enable colliders");
            for (int i = 0; i < MM.TriangleComponents.Count; i++)
            {
                MM.TriangleComponents[i].EnableColliders();
            }
        }
    }
    #endregion

    #region Selection and Deselection Systems
    void ConvertGlobalSelectionToLocal()
    {
        List<GameObject> Results = UniSelect.SelectionMouseUp(Input.GetKey(KeyCode.LeftControl));
        if (Results.Count - 1 >= 0)
        {
            if (Results.Count - 1 > 0)
            {
                for (int i = 0; i < Results.Count; i++)
                {
                    if (!VertexSelectionList.Contains(Results[i]))
                    {
                        SelectionBackEnd(Results[i], true);
                    }
                }
            }
            else
            {
                SelectionBackEnd(Results[0], false);
            }
        }
        else
        {
            GeneralDeselect();
        }
    }

    /// <summary>
    /// New selection backend, will only select vertices,
    /// even if there colliders and renderers are disabled.
    /// This is because it will make updating vertex positions in each triangle object much easier / actually possible.
    /// However, to the user it should still appear as though they have selected e.g. a Face, 
    /// from a material and Handle Placement perspecive.
    /// </summary>
    /// <param name="Item"> Item we are selecting. </param>
    /// <param name="BoxSelect"> Is this a box Selection? True/False. </param>
    void SelectionBackEnd(GameObject Item, bool BoxSelect)
    {
        // Left Control key/ box select check.
        if (!Input.GetKey(KeyCode.LeftControl) && !BoxSelect)
        {
            GeneralDeselect();
        }
        // Selection Block
        if (Mode == 0) // Edge Selection
        {
            EdgeSelection(Item);
        }
        else if (Mode == 1) // Vertex Selection
        {
            int SublistIndex = Convert.ToInt32(Item.name);
            GlobalVertSelection(SublistIndex, true);
        }
        else if (Mode == 2) // Face Selection
        {
            FaceSelection(Item);
        }

        if (PrimarySelectedObjectVertex == null)
        {

            PrimarySelectedObjectVertex = VertexSelectionList[VertexSelectionList.Count - 1];
            PrimarySelectedObjectVertex.SetActive(true);
            if (Mode == 0) // Edge Selection
            {
                PrimarySelectedObjectEdge = EdgeSelectionList[EdgeSelectionList.Count - 1];
                PrimarySelectedObjectEdge.SetActive(true);
                PrimarySelectedObjectEdge.GetComponent<MeshRenderer>().material = PrimarySelected;
                if (PrimarySelectedObjectVertex)
                {
                    if (MM.SpawnedHandle == null)
                    {
                        MM.InstantiateHandle(PrimarySelectedObjectEdge.transform, PrimarySelectedObjectEdge.transform.position);
                    }
                    else
                    {
                        MM.ReTransformHandle(PrimarySelectedObjectEdge.transform, PrimarySelectedObjectEdge.transform.position);
                    }
                }
            }
            else if (Mode == 1) // Vertex Selection
            {
                PrimarySelectedObjectVertex.GetComponent<MeshRenderer>().material = PrimarySelected;
                if (PrimarySelectedObjectVertex)
                {
                    if (MM.SpawnedHandle == null)
                    {
                        MM.InstantiateHandle(PrimarySelectedObjectVertex.transform, PrimarySelectedObjectVertex.transform.position);
                    }
                    else
                    {
                        MM.ReTransformHandle(PrimarySelectedObjectVertex.transform, PrimarySelectedObjectVertex.transform.position);
                    }
                }
            }
            else if (Mode == 2) // Face Selection
            {
                PrimarySelectedObjectTri = TriangleSelectionList[TriangleSelectionList.Count - 1];
                PrimarySelectedObjectTri.GetComponent<MeshRenderer>().material = PrimarySelected;
                if (MM.SpawnedHandle == null)
                {
                    MM.InstantiateHandle(PrimarySelectedObjectTri.transform, PrimarySelectedObjectTri.GetComponentInChildren<Edge>().AveragePositon);
                }
                else
                {
                    MM.ReTransformHandle(PrimarySelectedObjectTri.transform, PrimarySelectedObjectTri.GetComponentInChildren<Edge>().AveragePositon);
                }
            }
        }
        else
        {
            if(Mode == 1)
            {
                PrimarySelectedObjectVertex.GetComponent<MeshRenderer>().material = SecondarySelected;
            }

            PrimarySelectedObjectVertex = VertexSelectionList[VertexSelectionList.Count - 1];
            PrimarySelectedObjectVertex.SetActive(true);
            if (Mode == 0) // Edge Selection
            {
                PrimarySelectedObjectEdge.GetComponent<MeshRenderer>().material = SecondarySelected;
                PrimarySelectedObjectEdge = EdgeSelectionList[EdgeSelectionList.Count - 1];
                PrimarySelectedObjectEdge.SetActive(true);
                PrimarySelectedObjectEdge.GetComponent<MeshRenderer>().material = PrimarySelected;
                if (PrimarySelectedObjectVertex)
                {
                    if (MM.SpawnedHandle == null)
                    {
                        MM.InstantiateHandle(PrimarySelectedObjectEdge.transform, PrimarySelectedObjectEdge.transform.position);
                    }
                    else
                    {
                        MM.ReTransformHandle(PrimarySelectedObjectEdge.transform, PrimarySelectedObjectEdge.transform.position);
                    }
                }
            }
            else if (Mode == 1) // Vertex Selection
            {
                PrimarySelectedObjectVertex.GetComponent<MeshRenderer>().material = PrimarySelected;
                if (PrimarySelectedObjectVertex)
                {
                    if (MM.SpawnedHandle == null)
                    {
                        MM.InstantiateHandle(PrimarySelectedObjectVertex.transform, PrimarySelectedObjectVertex.transform.position);
                    }
                    else
                    {
                        MM.ReTransformHandle(PrimarySelectedObjectVertex.transform, PrimarySelectedObjectVertex.transform.position);
                    }
                }
            }
            else if (Mode == 2) // Face Selection
            {
                PrimarySelectedObjectTri.GetComponent<MeshRenderer>().material = SecondarySelected;
                PrimarySelectedObjectTri = TriangleSelectionList[TriangleSelectionList.Count - 1];
                PrimarySelectedObjectTri.GetComponent<MeshRenderer>().material = PrimarySelected;
                if (MM.SpawnedHandle == null)
                {
                    MM.InstantiateHandle(PrimarySelectedObjectTri.transform, PrimarySelectedObjectTri.GetComponentInChildren<Edge>().AveragePositon);
                }
                else
                {
                    MM.ReTransformHandle(PrimarySelectedObjectTri.transform, PrimarySelectedObjectTri.GetComponentInChildren<Edge>().AveragePositon);
                }
            }
        }
    }

   

    void GlobalVertSelection(int SublistIndex, bool SetTexture)
    {
        VertexSubListRecord.Add(SublistIndex);
        for (int i = 0; i < MM.RelatedVertices[SublistIndex].Count; i++)
        {
            GameObject Temp = MM.Vertices[MM.RelatedVertices[SublistIndex][i]];
            Debug.Log("Vertex: " + Temp.name);
            if (!VertexSelectionList.Contains(Temp))
            {
                VertexSelectionList.Add(Temp);
            }
            if (i != MM.RelatedVertices[SublistIndex].Count - 1 && SetTexture)
            {
                Temp.SetActive(false);
                
                Temp.GetComponent<MeshRenderer>().material = SecondarySelected;
            }
        }
    }

    void EdgeSelection(GameObject Item)
    {
        // All vertices along the edges we are selecting must be selected, THEN all vertices occpying the same
        // point as those vertices we are already selecting MUST ALSO be selected.
        // Handles needs setting to the actual edge object position/average position.

        int SublistIndex = Convert.ToInt32(Item.name);
        EdgeSubListRecord.Add(SublistIndex);
        for (int i = 0; i < MM.RelatedEdges[SublistIndex].Count; i++)
        {
            GameObject Temp = MM.Edges[MM.RelatedEdges[SublistIndex][i]];
            //Debug.Log("Edge: " + Temp.name);
            if (!EdgeSelectionList.Contains(Temp))
            {
                EdgeSelectionList.Add(Temp);
                if (i != MM.RelatedEdges[SublistIndex].Count - 1)
                {
                    Temp.SetActive(false);
                    Temp.GetComponent<MeshRenderer>().material = SecondarySelected;
                }
            }

        }
        //Debug.Log(MM.EdgeToVerticeRelation[MM.Edges.IndexOf(Item)].Count);

        for (int i = 0; i < MM.EdgeToVerticeRelation[MM.Edges.IndexOf(Item)].Count; i++)
        {
            GlobalVertSelection(MM.EdgeToVerticeRelation[MM.Edges.IndexOf(Item)][i], false);
        }
    }


    void FaceSelection(GameObject Item)
    {
        Debug.Log(Item.name);
        // All vertices in the face we are selecting must be selected, THEN all vertices occpying the same
        // point as those vertices we are already selecting MUST ALSO be selected.
        // Handles needs setting to the actual face object position/average position.
        for (int i = 0; i < MM.TriToVerticeRelation[Item].Count; i++)
        {
            GlobalVertSelection(MM.TriToVerticeRelation[Item][i], false);
        }
        if (!TriangleSelectionList.Contains(Item))
        {
            TriangleSelectionList.Add(Item);
            Item.GetComponent<MeshRenderer>().material = SecondarySelected;
        }

    }

    // Lol delesection gonna be unfunny.
    void GeneralDeselect()
    {
        Debug.Log("Deselecting");
        if (Mode == 0)
        {
            EdgeDeselection();
        }
        else if (Mode == 1)
        {
            for (int i = 0; i < VertexSelectionList.Count; i++)
            {
                VertexDeselection(VertexSelectionList[i]);
            }
        }
        else if (Mode == 2)
        {
            FaceDeselection();
        }

        VertexSubListRecord.Clear();
        VertexSelectionList.Clear();
        PrimarySelectedObjectVertex = null;
        Destroy(MM.SpawnedHandle);
        MM.SpawnedHandle = null;
    }

    void VertexDeselection(GameObject Item)
    {
        Item.SetActive(true);
        Item.GetComponent<MeshRenderer>().material = Deselected;
    }

    void EdgeDeselection()
    {
        for (int i = 0; i < EdgeSelectionList.Count; i++)
        {
            EdgeSelectionList[i].SetActive(true);
            EdgeSelectionList[i].GetComponent<MeshRenderer>().material = Deselected;
        }

        EdgeSubListRecord.Clear();
        EdgeSelectionList.Clear();
        PrimarySelectedObjectEdge = null;
    }

    void FaceDeselection()
    {
        for (int i = 0; i < TriangleSelectionList.Count; i++)
        {
            TriangleSelectionList[i].GetComponent<MeshRenderer>().material = DeselectedTriangle;
        }

        TriangleSelectionList.Clear();
        PrimarySelectedObjectTri = null;
    }
    #endregion
}
