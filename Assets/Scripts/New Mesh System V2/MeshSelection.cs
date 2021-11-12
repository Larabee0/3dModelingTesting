using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MeshSelection : MonoBehaviour
{
    [Header("Script References")]
    public MeshManipulator MV;
    public MeshManagerV2 MM;
    private UniversalSelection UniSelect;

    [Header("Settings")]
    public Material Deselected;
    public Material PrimarySelected;
    public Material SecondarySelected;
    public Material Deselectedtriangle;
    public uint Mode = 0; // Edges = 0, Faces = 2, Vertices = 1
    [Header("Debug Info")]
    public List<GameObject> Selection = new List<GameObject>();
    public GameObject PrimarySelectedObject;
    public List<int> SelectionIndex = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        UniSelect = GetComponentInParent<UniversalSelection>();
    }

    // Update is called once per frame
    void Update()
    {
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
        if (Input.GetMouseButtonUp(0))
        {
            MV.FoundCollider = false;
        }
        // Edit Mesh Mode. (Defaults to Face Mode)
        if (Input.GetKeyDown(KeyCode.I) && Input.GetKey(KeyCode.LeftShift))
        {
            MM.DeinitiMesh();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            MM.InitiMesh();
        }

        if (MM.TrianglesObjects.Count - 1 >= 0)
        {
            // Vertice Mode
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Deselect();
                for (int i = 0; i < MM.Edges.Count; i++)
                {
                    MM.Edges[i].GetComponent<BoxCollider>().enabled = false;
                }
                MM.InstantiateVertices();
                UniSelect.SelectableObjects = MM.Vertices;
                Mode = 1;
            }





            // Edge Mode
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Deselect();
                MM.DestroyVertices();
                MM.DestroyEdges();
                MM.InstantiateEdges();
                for (int i = 0; i < MM.Edges.Count; i++)
                {
                    MM.Edges[i].GetComponent<BoxCollider>().enabled = true;
                }
                UniSelect.SelectableObjects = MM.Edges;
                Mode = 0;
            }
            





            // Face Mode
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Deselect();
                for (int i = 0; i < MM.Edges.Count; i++)
                {
                    MM.Edges[i].GetComponent<BoxCollider>().enabled = false;
                }
                MM.DestroyVertices();
                UniSelect.SelectableObjects.Clear();
                for (int i = 0; i < MM.TrianglesObjects.Count; i++)
                {
                    UniSelect.SelectableObjects.Add(MM.TrianglesObjects[i].gameObject);
                }
                Mode = 2;
            }
        }
    }

    void ConvertGlobalSelectionToLocal()
    {
        List<GameObject> Results = UniSelect.SelectionMouseUp(Input.GetKey(KeyCode.LeftControl));
        if (Results.Count - 1 >= 0)
        {
            if (Results.Count - 1 > 0)
            {
                for (int i = 0; i < Results.Count; i++)
                {
                    SelectionBackEnd(Results[i], true);
                }
            }
            else
            {
                SelectionBackEnd(Results[0], false);
            }
        }
        else
        {
            Deselect();
        }
    }

    void SelectionBackEnd(GameObject Item, bool BoxSelect)
    {
        Vector3 Temp = Vector3.zero;
        if (Mode == 0) // Edge Selection
        {
            EdgeSelection(Item, BoxSelect);
        }
        else if (Mode == 1) // Vertex Selection
        {
            if (!Input.GetKey(KeyCode.LeftControl) && !BoxSelect)
            {
                Deselect();
            }

            List<GameObject> SelectionAggrigator = new List<GameObject>();
            SelectionAggrigator.Add(Item);

            Item.GetComponent<MeshRenderer>().material = PrimarySelected;

            for (int i = 0; i < UniSelect.SelectableObjects.Count; i++)
            {
                if (UniSelect.SelectableObjects[i] != Item && UniSelect.SelectableObjects[i].transform.position == Item.transform.position)
                {
                    SelectionAggrigator.Add(UniSelect.SelectableObjects[i]);

                    if (SelectionAggrigator[SelectionAggrigator.Count - 1].GetComponent<SelectionComponent>() == null)
                    {
                        SelectionAggrigator[SelectionAggrigator.Count - 1].AddComponent<SelectionComponent>();
                        SelectionAggrigator[SelectionAggrigator.Count - 1].GetComponent<SelectionComponent>().Selection = PrimarySelected;
                        SelectionAggrigator[SelectionAggrigator.Count - 1].GetComponent<SelectionComponent>().SetMat = false;
                    }
                }
            }

            if (PrimarySelectedObject != null)
            {
                PrimarySelectedObject.GetComponent<MeshRenderer>().material = SecondarySelected;
            }

            PrimarySelectedObject = SelectionAggrigator[0];
            PrimarySelectedObject.GetComponent<MeshRenderer>().enabled = true;
            Selection.Remove(PrimarySelectedObject);
            Selection.Insert(0, PrimarySelectedObject);
            for (int i = 1; i < SelectionAggrigator.Count; i++)
            {
                SelectionAggrigator[i].GetComponent<MeshRenderer>().enabled = false;
                if (!Selection.Contains(SelectionAggrigator[i]))
                {
                    Selection.Add(SelectionAggrigator[i]);
                    //Debug.Log("Adding: " + SelectionAggrigator[i].name);
                }
            }
            Temp = PrimarySelectedObject.transform.position;
        }
        else if (Mode == 2) // Face Selection
        {
            // We need to get connected Faces.
            // Any face that shares the same vertice as the selected item must also be selected.
            // Spawn Vertex points just with disabled colliders and renderes?
            if (!Input.GetKey(KeyCode.LeftControl) && !BoxSelect)
            {
                Deselect();
            }
            Item.GetComponent<MeshRenderer>().material = PrimarySelected;
            if (PrimarySelectedObject != null)
            {
                PrimarySelectedObject.GetComponent<MeshRenderer>().material = SecondarySelected;
            }
            PrimarySelectedObject = Item;
            if (Selection.Contains(PrimarySelectedObject))
            {
                Selection.Remove(PrimarySelectedObject);
            }            
            Selection.Insert(0, PrimarySelectedObject);
            Temp = PrimarySelectedObject.GetComponentInChildren<Edge>().AveragePositon;
        }

        if (MM.SpawnedHandle == null)
        {
            MM.InstantiateHandle(PrimarySelectedObject.transform, Temp);
        }
        else
        {
            MM.ReTransformHandle(PrimarySelectedObject.transform, Temp);
        }
    }

    void EdgeSelection(GameObject Item, bool BoxSelect)
    {
        if (!Input.GetKey(KeyCode.LeftControl) && !BoxSelect)
        {
            Deselect();
        }

        List<GameObject> SelectionAggrigator = new List<GameObject>();
        SelectionAggrigator.Add(Item);

        Item.GetComponent<MeshRenderer>().material = PrimarySelected;

        for (int i = 0; i < MM.Vertices.Count; i++)
        {
            if (MM.Vertices[i] != Item && MM.Vertices[i].transform.position == Item.transform.position)
            {
                SelectionAggrigator.Add(MM.Vertices[i]);

                if (SelectionAggrigator[SelectionAggrigator.Count - 1].GetComponent<SelectionComponent>() == null)
                {
                    SelectionAggrigator[SelectionAggrigator.Count - 1].AddComponent<SelectionComponent>();
                    SelectionAggrigator[SelectionAggrigator.Count - 1].GetComponent<SelectionComponent>().Selection = PrimarySelected;
                    SelectionAggrigator[SelectionAggrigator.Count - 1].GetComponent<SelectionComponent>().SetMat = false;
                }
                break;
            }
        }

        if (PrimarySelectedObject != null)
        {
            PrimarySelectedObject.GetComponent<MeshRenderer>().material = SecondarySelected;
        }

        PrimarySelectedObject = SelectionAggrigator[0];
        PrimarySelectedObject.GetComponent<MeshRenderer>().enabled = true;
        Selection.Remove(PrimarySelectedObject);
        Selection.Insert(0, PrimarySelectedObject);

        for (int i = 1; i < SelectionAggrigator.Count; i++)
        {
            SelectionAggrigator[1].GetComponent<MeshRenderer>().enabled = false;
            if (!Selection.Contains(SelectionAggrigator[i]))
            {
                Selection.Add(SelectionAggrigator[i]);
                //Debug.Log("Adding: " + SelectionAggrigator[i].name);
            }
        }
    }

    void Deselect()
    {

        Debug.Log("Deselecting");
        if (Mode == 0)
        {
            VertexEdgeDeselection();
        }
        else if (Mode == 1)
        {
            VertexEdgeDeselection();
        }
        else if (Mode == 2)
        {
            FaceDeselection();
        }
        Selection.Clear();
        PrimarySelectedObject = null;
        Destroy(MM.SpawnedHandle);
        MM.SpawnedHandle = null;
    }

    void VertexEdgeDeselection()
    {
        for (int i = 0; i < Selection.Count; i++)
        {
            Destroy(Selection[i].GetComponent<SelectionComponent>());
            Selection[i].GetComponent<MeshRenderer>().material = Deselected;
            Selection[i].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    void FaceDeselection()
    {
        for (int i = 0; i < Selection.Count; i++)
        {
            Destroy(Selection[i].GetComponent<SelectionComponent>());
            Selection[i].GetComponent<MeshRenderer>().material = Deselectedtriangle;
            Selection[i].GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
