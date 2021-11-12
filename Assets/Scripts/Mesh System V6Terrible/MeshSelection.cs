using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV6
{
    public class MeshSelection : MonoBehaviour
    {
        [Header("Script References")]
        [SerializeField] private MeshManipulator MeshManipulator;
        [SerializeField] private MeshManager MeshManager;
        [SerializeField] private ModelManagerV2 ModelManager;
        [SerializeField] private UniversalSelection UniSelect;
        [SerializeField] private ThreeWayHandler ThreeWayHandler;

        [Header("Settings")]
        public Material DeselectedMat;
        public Material PrimarySelectedMat;
        public Material SecondarySelectedMat;
        public GameObject Handle;
        public uint Mode = 1; //  Vertices = 1, Edges = 2, Faces = 3,

        [Header("Debug Info")]
        public List<GameObject> SelectedList = new List<GameObject>();
        public GameObject PrimarySelectedObject;
        public Vector3 PrimarySelectedObjectPosition;
        public GameObject SpawnedHandle;

        private void OnEnable()
        {
            UniSelect = GetComponentInParent<UniversalSelection>();
            ModelManager = GetComponentInParent<ModelManagerV2>();

            if (UniSelect == null)
            {
                this.enabled = false;
                Debug.LogError("Could not Find Universal Selection Script");
            }

            if (ModelManager == null)
            {
                this.enabled = false;
                Debug.LogError("Could not Find Model Manager Script");
            }
            if(Mode == 1)
            {
                UniSelect.SelectableObjects = MeshManager.SpawnedVerts;
                MeshManager.SetAllVerticeColsAndVisToOn();
            }
            if(Mode == 2)
            {
                UniSelect.SelectableObjects = MeshManager.SpawnedEdges;
                MeshManager.SetAllEdgeCollidersToOn();
            }
            if(Mode == 3)
            {
                UniSelect.SelectableObjects = MeshManager.SpawnedTris;
                MeshManager.SetAllTriCollidersToOn();
            }

        }

        private void OnDisable()
        {
            HardDeselect();
        }

        private void Update()
        {
            ChangeMode();

            if (MeshManipulator.FoundCollider)
                return;

            CheckMouse();
            DoExtrude();
            DoDelete();
        }

        // This method changes the mode when 1,2 or 3 are pressed.
        private void ChangeMode()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (Mode != 1)
                {
                    Debug.Log("Request to mode 1 recieved");
                    if (Mode == 2)
                    {
                        MeshManager.SetAllEdgeCollidersToOff();
                    }
                    if (Mode == 3)
                    {
                        MeshManager.SetAllTriCollidersToOff();
                    }

                    Deselect(true);
                    Mode = 1;
                    MeshManager.SetAllVerticeColsAndVisToOn();
                    UniSelect.SelectableObjects = MeshManager.SpawnedVerts;

                    return;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (Mode != 2)
                {
                    Deselect(true);
                    if (Mode == 1)
                    {
                        MeshManager.SetAllVerticeColsAndVisToOff();
                    }

                    if (Mode == 3)
                    {
                        MeshManager.SetAllTriCollidersToOff();
                    }

                    MeshManager.SetAllEdgeCollidersToOn();
                    Mode = 2;
                    UniSelect.SelectableObjects = MeshManager.SpawnedEdges;

                    return;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (Mode != 3)
                {
                    Deselect(true);
                    if (Mode == 1)
                    {
                        MeshManager.SetAllVerticeColsAndVisToOff();
                    }
                    if (Mode == 2)
                    {
                        MeshManager.SetAllEdgeCollidersToOff();
                    }

                    MeshManager.SetAllVerticeColsAndVisToOn();
                    Mode = 3;
                    UniSelect.SelectableObjects = MeshManager.SpawnedTris;

                    return;
                }
            }
        }

        // Check the mouse buttons.
        private void CheckMouse()
        {
            if (Input.GetMouseButtonUp(0))
            {
                ConvertGlobalSelectionToLocal();
            }

            if (Input.GetMouseButtonDown(0))
            {
                UniSelect.SelectionMouseDown(PrimarySelectedMat, false);
            }

            if (Input.GetMouseButton(0))
            {
                UniSelect.SelectionMouse();
            }
        }

        /// <summary>
        /// All the Extrude tools.
        /// Plus some extra stuff for Mode 1.
        /// </summary>
        private void DoExtrude()
        {
            if (Mode == 1 && PrimarySelectedObject != null)
            {
                if (Input.GetKeyDown(KeyCode.V))
                {
                    if (SelectedList.Count >= 2)
                    {
                        List<GameObject> NewSelection = new List<GameObject>();                        
                        for (int i = 0; i < SelectedList.Count; i++)
                        {
                            GameObject NewSpawn = MeshManager.ExtrudeVertex(SelectedList[i]);
                            MeshManager.NewEdge(SelectedList[i], NewSpawn);
                            NewSelection.Add(NewSpawn);
                        }
                        Deselect(false);
                        NewSelection.ForEach(i => SelectionSystemBackend(i, true));
                        MeshManager.SetAllVerticeColsAndVisToOn();
                        return;
                    }
                    else
                    {
                        GameObject NewSpawn = MeshManager.ExtrudeVertex(PrimarySelectedObject);
                        MeshManager.NewEdge(PrimarySelectedObject, NewSpawn);
                        MeshManager.SetAllVerticeColsAndVisToOn();
                        SelectionSystemBackend(NewSpawn, false);
                        return;
                    }
                }
            }

            if (Mode == 1 && SelectedList.Count >= 2)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    MeshManager.NewEdge(SelectedList[SelectedList.Count - 1], SelectedList[SelectedList.Count - 2]);
                    return;
                }
            }
            if(Mode == 1 && SelectedList.Count >= 3)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    MeshManager.NewTriangle(SelectedList[SelectedList.Count - 1], SelectedList[SelectedList.Count - 2], SelectedList[SelectedList.Count - 3]);
                    MeshManager.UpdateMeshVerticesAndTriangles();
                    return;
                }
            }


            if (Mode == 2 && PrimarySelectedObject != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (SelectedList.Count >= 2)
                    {
                        List<GameObject> NewSelection = new List<GameObject>();
                        for (int i = 0; i < SelectedList.Count; i++)
                        {
                            GameObject NewSpawn = MeshManager.ExtrudeEdge(SelectedList[i]);
                            NewSelection.Add(NewSpawn);
                        }
                        Deselect(false);
                        NewSelection.ForEach(i => SelectionSystemBackend(i, true));
                        MeshManager.SetAllEdgeCollidersToOn();
                        MeshManager.UpdateMeshVerticesAndTriangles();
                        return;
                    }
                    else
                    {
                        GameObject NewSpawn = MeshManager.ExtrudeEdge(PrimarySelectedObject);
                        SelectionSystemBackend(NewSpawn, false);
                        MeshManager.SetAllEdgeCollidersToOn();
                        MeshManager.UpdateMeshVerticesAndTriangles();
                        return;
                    }
                }
            }

            if(Mode == 3 && PrimarySelectedObject != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (SelectedList.Count >= 1)
                    {
                        List<GameObject> NewSelection = MeshManager.ExtrudeSection(SelectedList);
                        Deselect(false);
                        NewSelection.ForEach(i => SelectionSystemBackend(i, true));
                        MeshManager.SetAllTriCollidersToOn();
                        MeshManager.UpdateMeshVerticesAndTriangles();
                        return;
                    }


                    /// if (SelectedList.Count >= 2)
                    /// {
                    ///     List<GameObject> NewSelection = new List<GameObject>();
                    ///     for (int i = 0; i < SelectedList.Count; i++)
                    ///     {
                    ///         GameObject NewSpawn = MeshManager.ExtrudeTriangle(SelectedList[i]);
                    ///         NewSelection.Add(NewSpawn);
                    ///     }
                    ///     Deselect(false);
                    ///     NewSelection.ForEach(i => SelectionSystemBackend(i, true));
                    ///     MeshManager.SetAllTriCollidersToOn();
                    ///     
                    ///     return;
                    /// }
                    /// else
                    /// {
                    ///     GameObject NewSpawn = MeshManager.ExtrudeTriangle(PrimarySelectedObject);
                    ///     
                    ///     SelectionSystemBackend(NewSpawn, false);
                    ///     MeshManager.SetAllTriCollidersToOn();
                    ///     MeshManager.UpdateMeshVerticesAndTriangles();
                    ///     return;
                    /// }
                }
            }
        }

        /// <summary>
        /// Deleting bits of the mesh.
        /// </summary>
        private void DoDelete()
        {
            if (Mode == 1)
            {
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    MeshManager.RemoveVertice(PrimarySelectedObject);

                    MeshManager.UpdateMeshAndOptimize();
                }
            }
            if (Mode == 2)
            {
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    MeshManager.RemoveEdge(PrimarySelectedObject);

                    MeshManager.UpdateMeshAndOptimize();
                }
            }
            if (Mode == 3)
            {
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    MeshManager.RemoveTriangle(PrimarySelectedObject);

                    MeshManager.UpdateMeshAndOptimize();
                }
            }
        }

        #region Selection System
        // When mouse up is triggered, this is.
        private void ConvertGlobalSelectionToLocal()
        {
            List<GameObject> Results = UniSelect.SelectionMouseUp(Input.GetKey(KeyCode.LeftControl));
            Debug.Log(Results.Count);

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
            if (!Input.GetKey(KeyCode.LeftControl) && !BoxSelect)
            {
                Deselect(false);
            }
            if (Mode == 1)
            {
                VertexSelect(Item);
            }
            if (Mode == 2)
            {
                VertexSelect(Item);
            }
            if (Mode == 3)
            {
                TriSelect(Item);
            }

            if (PrimarySelectedObject == null)
            {
                if (Mode != 3)
                {
                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];
                    PrimarySelectedObject.GetComponent<MeshRenderer>().material = PrimarySelectedMat;
                }
                else
                {
                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];
                    ThreeWayHandler.GOTOTDSs[PrimarySelectedObject].InvolvedEdgeObjects.ForEach(i => i.GetComponent<MeshRenderer>().material = PrimarySelectedMat);
                }

                MeshManipulator.enabled = true;
            }
            else
            {
                if (Mode != 3)
                {
                    PrimarySelectedObject.GetComponent<MeshRenderer>().material = SecondarySelectedMat;
                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];
                    PrimarySelectedObject.GetComponent<MeshRenderer>().material = PrimarySelectedMat;
                }
                else
                {
                    ThreeWayHandler.GOTOTDSs[PrimarySelectedObject].InvolvedEdgeObjects.ForEach(i =>i.GetComponent<MeshRenderer>().material = SecondarySelectedMat);
                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];
                    ThreeWayHandler.GOTOTDSs[PrimarySelectedObject].InvolvedEdgeObjects.ForEach(i => i.GetComponent<MeshRenderer>().material = PrimarySelectedMat);
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
            ThreeWayHandler.GOTOTDSs[Item].InvolvedEdgeObjects.ForEach(i =>i.GetComponent<MeshRenderer>().material = SecondarySelectedMat);
            SelectedList.Add(Item);
        }

        // Soft Deselect
        public void Deselect(bool DestroyHandles)
        {
            if (Mode != 3)
            {
                SelectedList.ForEach(i => i.GetComponent<MeshRenderer>().material = DeselectedMat);
            }
            else
            {
                SelectedList.ForEach(j => ThreeWayHandler.GOTOTDSs[j].InvolvedEdgeObjects.ForEach(i => i.GetComponent<MeshRenderer>().material = DeselectedMat));
            }

            SelectedList.Clear();
            PrimarySelectedObject = null;
            if (DestroyHandles)
            {
                Destroy(SpawnedHandle);
                SpawnedHandle = null;
            }

            MeshManipulator.enabled = false;
        }

        // Hard Deselect
        public void HardDeselect()
        {
            SelectedList.Clear();
            PrimarySelectedObject = null;
            MeshManipulator.enabled = false;
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
                SpawnedHandle = Instantiate(Handle, this.transform);
                SpawnedHandle.transform.localScale /= transform.localScale.magnitude;
            }

            if (Mode != 3)
            {
                SpawnedHandle.transform.position = PrimarySelectedObject.transform.position;
                PrimarySelectedObjectPosition = PrimarySelectedObject.transform.position;
            }
            else
            {
                PrimarySelectedObjectPosition = SpawnedHandle.transform.position = this.transform.TransformPoint(ThreeWayHandler.GOTOTDSs[PrimarySelectedObject].Position);
            }
        }
        #endregion
    }
}