using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MeshSystemV8
{
    public class MeshSelection : MeshManipulator
    {
        [Header("Mesh Selection Script References")]
        [Header("Mesh Selection")]
        public ModelManagerV2 ModelManager;
        [SerializeField] private UniversalSelection UniSelect;

        [Header("Settings")]
        public Material DeselectedMat;
        public Material PrimarySelectedMat;
        public Material SecondarySelectedMat;
        public GameObject Handle;
        public Mode Mode = Mode.Vertex;
        [SerializeField]private bool disableUpdate = true;

        [Header("Mesh Selection Debug Info")]
        public List<GameObject> SelectedList = new List<GameObject>();
        public GameObject PrimarySelectedObject;
        public Vector3 PrimarySelectedObjectPosition;
        public GameObject SpawnedHandle;
        private void Start()
        {
            meshSelection = this;
            UniSelect = GetComponentInParent<UniversalSelection>();
            ModelManager = GetComponentInParent<ModelManagerV2>();
        }
        public void OnMSEnable()
        {
            if(Mode == Mode.Object)
            {
                Debug.LogError("Invalid Start up mode");
            }
            disableUpdate = false;
            
            if (UniSelect == null)
            {
                enabled = false;
                Debug.LogError("Could not Find Universal Selection Script");
            }

            if (ModelManager == null)
            {
                enabled = false;
                Debug.LogError("Could not Find Model Manager Script");
            }
            if(Mode == Mode.Vertex)
            {
                UniSelect.SelectableObjects = SpawnedVerts;
                SetAllVerticeColsAndVisToOn();
            }
            if(Mode == Mode.Edge)
            {
                UniSelect.SelectableObjects = SpawnedEdges;
                SetAllEdgeCollidersToOn();
            }
            if(Mode == Mode.Triangle)
            {
                UniSelect.SelectableObjects = SpawnedTris;
                SetAllTriCollidersToOn();
            }
        }

        public void OnMSDisable()
        {
            disableUpdate = true;
            HardDeselect();
        }

        private void Update()
        {
            if (!disableUpdate)
            {
                CheckMouse();
            }
        }

        // Check the mouse buttons.
        private void CheckMouse()
        {
            if (LeftMouseButtonIsPressed)
            {
                if (!FoundCollider)
                    UniSelect.SelectionMouse();
                if (EnableUpdateOnManipulator)
                    LeftMouse();
            }
        }

        public void LeftMouse(InputAction.CallbackContext _context)
        {
            if (_context.started)// OnDown
            {
                LeftMouseButtonIsPressed = true;
                if (!FoundCollider)
                    UniSelect.SelectionMouseDown(PrimarySelectedMat, false);
                if (EnableUpdateOnManipulator)
                    LeftMouseDown();
            }
            if (_context.canceled)// OnUp
            {
                LeftMouseButtonIsPressed = false;
                if (!FoundCollider)
                    ConvertGlobalSelectionToLocal();
                if (EnableUpdateOnManipulator)
                    LeftMouseUp();
            }
        }

        public void LeftControl(InputAction.CallbackContext _context)
        {
            if (_context.started)// OnDown
            {
                LeftControlIsPressed = true;
            }
            if (_context.canceled)
            {
                LeftControlIsPressed = false;
            }
        }
        public void NormalExtrude(InputAction.CallbackContext _context)
        {
            if (_context.performed)
            {
                if (Mode == Mode.Vertex && SelectedList.Count >= 2)
                {
                    NewEdge(SelectedList[SelectedList.Count - 1], SelectedList[SelectedList.Count - 2]);
                }
                else if (Mode == Mode.Edge && PrimarySelectedObject != null)
                {
                    if (SelectedList.Count >= 2)
                    {
                        List<GameObject> NewSelection = new List<GameObject>();
                        for (int i = 0; i < SelectedList.Count; i++)
                        {
                            GameObject NewSpawn = ExtrudeEdge(SelectedList[i]);
                            NewSelection.Add(NewSpawn);
                        }
                        Deselect(false);
                        NewSelection.ForEach(i => SelectionSystemBackend(i, true));
                        SetAllEdgeCollidersToOn();
                        UpdateMeshVerticesAndTriangles();
                    }
                    else
                    {
                        GameObject NewSpawn = ExtrudeEdge(PrimarySelectedObject);
                        SelectionSystemBackend(NewSpawn, false);
                        SetAllEdgeCollidersToOn();
                        UpdateMeshVerticesAndTriangles();
                    }
                }
                else if (Mode == Mode.Triangle && PrimarySelectedObject != null)
                {
                    if (SelectedList.Count >= 1)
                    {
                        List<GameObject> NewSelection = ExtrudeSection(SelectedList);
                        Deselect(false);
                        NewSelection.ForEach(i => SelectionSystemBackend(i, true));
                        SetAllTriCollidersToOn();
                        UpdateMeshVerticesAndTriangles();
                    }
                }
            }
        }

        public void VertexExtrude(InputAction.CallbackContext _context)
        {
            if (_context.performed)
            {
                if (Mode == Mode.Vertex && PrimarySelectedObject != null)
                {
                    if (SelectedList.Count >= 2)
                    {
                        List<GameObject> NewSelection = new List<GameObject>();
                        for (int i = 0; i < SelectedList.Count; i++)
                        {
                            GameObject NewSpawn = ExtrudeVertex(SelectedList[i]);
                            NewEdge(SelectedList[i], NewSpawn);
                            NewSelection.Add(NewSpawn);
                        }
                        Deselect(false);
                        NewSelection.ForEach(i => SelectionSystemBackend(i, true));
                        SetAllVerticeColsAndVisToOn();
                    }
                    else
                    {
                        GameObject NewSpawn = ExtrudeVertex(PrimarySelectedObject);
                        NewEdge(PrimarySelectedObject, NewSpawn);
                        SetAllVerticeColsAndVisToOn();
                        SelectionSystemBackend(NewSpawn, false);
                    }

                }
            }
        }

        public void NewTriangle(InputAction.CallbackContext _context)
        {
            if (_context.performed)
            {
                if (Mode == Mode.Vertex && SelectedList.Count >= 3)
                {
                    NewTriangle(SelectedList[SelectedList.Count - 1], SelectedList[SelectedList.Count - 2], SelectedList[SelectedList.Count - 3]);
                    UpdateMeshVerticesAndTriangles();
                }
            }
        }

        public void VertexMode(InputAction.CallbackContext _context)
        {
            if (_context.performed)
            {
                print("Input recieved");
                if (Mode == Mode.Object)
                {
                    Mode = Mode.Vertex;
                    StartEverything();
                }
                if (Mode != Mode.Vertex)
                {
                    Debug.Log("Request to mode 1 recieved");
                    if (Mode == Mode.Edge)
                    {
                        ToggleEdgeColliders();
                    }
                    else if (Mode == Mode.Triangle)
                    {
                        ToggleTriColliders();
                    }

                    Deselect(true);
                    Mode = Mode.Vertex;
                    SetAllVerticeColsAndVisToOn();
                    UniSelect.SelectableObjects = SpawnedVerts;

                }
            }
        }

        public void EdgeMode(InputAction.CallbackContext _context)
        {
            if (_context.performed)
            {
                if (Mode == Mode.Object)
                {
                    Mode = Mode.Edge;
                    StartEverything();
                }
                if (Mode != Mode.Edge)
                {
                    Deselect(true);
                    if (Mode == Mode.Vertex)
                    {
                        SetAllVerticeColsAndVisToOff();
                    }
                    else if (Mode == Mode.Triangle)
                    {
                        ToggleTriColliders();
                    }

                    ToggleEdgeColliders();
                    Mode = Mode.Edge;
                    UniSelect.SelectableObjects = SpawnedEdges;
                }
            }
        }

        public void TriangleMode(InputAction.CallbackContext _context)
        {
            if (_context.performed)
            {
                if (Mode == Mode.Object)
                {
                    Mode = Mode.Triangle;
                    StartEverything();
                }
                if (Mode != Mode.Triangle)
                {
                    Deselect(true);
                    if (Mode == Mode.Vertex)
                    {
                        SetAllVerticeColsAndVisToOff();
                    }
                    else if (Mode == Mode.Edge)
                    {
                        ToggleEdgeColliders();
                    }

                    ToggleTriColliders();
                    Mode = Mode.Triangle;
                    UniSelect.SelectableObjects = SpawnedTris;
                }
            }
        }

        public void ObjectMode(InputAction.CallbackContext _context)
        {
            if (_context.performed)
            {
                Mode = Mode.Object;
                StopEverything();
            }
        }

        /// <summary>
        /// Deleting bits of the mesh.
        /// </summary>
        public void DoDelete(InputAction.CallbackContext _context)
        {
            if (_context.performed)
            {
                if (Mode != Mode.Object && !FoundCollider)
                {
                    if (Mode == Mode.Vertex)
                    {
                        RemoveVertice(PrimarySelectedObject);
                    }
                    else if (Mode == Mode.Edge)
                    {
                        RemoveEdge(PrimarySelectedObject);
                    }
                    else if (Mode == Mode.Triangle)
                    {
                        RemoveTriangle(PrimarySelectedObject);
                    }
                    UpdateMeshAndOptimizeStartStop();
                }
            }
        }

        #region Selection System
        // When mouse up is triggered, this is.
        private void ConvertGlobalSelectionToLocal()
        {
            List<GameObject> Results = UniSelect.SelectionMouseUp(LeftControlIsPressed);
            //Debug.Log(Results.Count);

            if (Results.Count >= 1)
            {
                if (Results.Count > 1)
                {
                    for (int i = 0; i < Results.Count; i++)
                    {
                        if (!SelectedList.Contains(Results[i]))
                        {
                            SelectionSystemBackend(Results[i], true);
                        }
                    }                    
                }
                else
                {
                    if (!SelectedList.Contains(Results[0]))
                    {
                        SelectionSystemBackend(Results[0], false);
                    }
                }
            }
            else
            {
                Deselect(true);
            }
        }

        // Method chooses what to do with the selection returned.
        private void SelectionSystemBackend(GameObject Item, bool BoxSelect)
        {
            if (!LeftControlIsPressed && !BoxSelect)
            {
                Deselect(false);
            }
            if (Mode == Mode.Vertex)
            {
                VertexSelect(Item);
            }
            else if (Mode == Mode.Edge)
            {
                VertexSelect(Item);
            }
            else if (Mode == Mode.Triangle)
            {
                TriSelect(Item);
            }

            if (PrimarySelectedObject == null)
            {
                if (Mode != Mode.Triangle)
                {
                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];
                    PrimarySelectedObject.GetComponent<MeshRenderer>().material = PrimarySelectedMat;
                }
                else
                {
                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];
                    PrimarySelectedObject.GetComponent<TriDataStore>().InvolvedEdgeDS.ForEach(i => i.ThisMeshRenderer.material = PrimarySelectedMat);
                }

                EnableUpdateOnManipulator = true;
            }
            else
            {
                if (Mode != Mode.Triangle)
                {
                    PrimarySelectedObject.GetComponent<MeshRenderer>().material = SecondarySelectedMat;
                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];
                    PrimarySelectedObject.GetComponent<MeshRenderer>().material = PrimarySelectedMat;
                }
                else
                {
                    PrimarySelectedObject.GetComponent<TriDataStore>().InvolvedEdgeDS.ForEach(i =>i.ThisMeshRenderer.material = SecondarySelectedMat);
                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];
                    PrimarySelectedObject.GetComponent<TriDataStore>().InvolvedEdgeDS.ForEach(i => i.ThisMeshRenderer.material = PrimarySelectedMat);
                }
            }
            DoHandlesStuff();
        }

        // Select a vertex also does edge.
        private void VertexSelect(GameObject Item)
        {
            Item.GetComponent<MeshRenderer>().material = SecondarySelectedMat;
            SelectedList.Add(Item);
        }

        // Select a triangle.
        private void TriSelect(GameObject Item)
        {
            Item.GetComponent<TriDataStore>().InvolvedEdgeDS.ForEach(i =>i.ThisMeshRenderer.material = SecondarySelectedMat);
            SelectedList.Add(Item);
        }

        // Soft Deselect
        private void Deselect(bool DestroyHandles)
        {
            if (Mode != Mode.Triangle)
            {
                SelectedList.ForEach(i => i.GetComponent<MeshRenderer>().material = DeselectedMat);
            }
            else
            {
                SelectedList.ForEach(j => j.GetComponent<TriDataStore>().InvolvedEdgeDS.ForEach(i => i.ThisMeshRenderer.material = DeselectedMat));
            }

            SelectedList.Clear();
            PrimarySelectedObject = null;
            if (DestroyHandles)
            {
                Destroy(SpawnedHandle);
                SpawnedHandle = null;
            }

            EnableUpdateOnManipulator = false;
        }

        // Hard Deselect
        private void HardDeselect()
        {
            SelectedList.Clear();
            PrimarySelectedObject = null;
            EnableUpdateOnManipulator = false;
            if (SpawnedHandle)
            {
                Destroy(SpawnedHandle);
                SpawnedHandle = null;
            }
        }

        // Update the handles
        public void DoHandlesStuff()
        {
            if (SpawnedHandle == null)
            {
                SpawnedHandle = Instantiate(Handle, transform);
                SpawnedHandle.transform.localScale /= transform.localScale.magnitude;
            }

            if (Mode != Mode.Triangle)
            {
                SpawnedHandle.transform.position = PrimarySelectedObject.transform.position;
                PrimarySelectedObjectPosition = PrimarySelectedObject.transform.position;
            }
            else
            {
                PrimarySelectedObjectPosition = SpawnedHandle.transform.position = transform.TransformPoint(PrimarySelectedObject.GetComponent<TriDataStore>().Position);
            }
        }
        #endregion
    }
}