using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV4
{
    public class MeshSelection : MonoBehaviour
    {
        [Header("Script References")]
        [SerializeField] private MeshManipulator MeshManipulator;
        [SerializeField] private MeshManager MeshManager;
        [SerializeField] private ModelManagerV2 ModelManager;
        [SerializeField] private UniversalSelection UniSelect;

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

        // Start is called before the first frame update
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
            }
            else if(Mode == 2)
            {
                UniSelect.SelectableObjects = MeshManager.SpawnedEdges;
            }
            else if(Mode == 3)
            {
                UniSelect.SelectableObjects = MeshManager.SpawnedTris;
            }
            
        }

        private void OnDisable()
        {
            Deselect(true);
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (Mode != 1)
                {
                    if (Mode == 2)
                    {
                        MeshManager.ToggleEdgeColliders();
                    }
                    if (Mode == 3)
                    {
                        MeshManager.ToggleTriColliders();
                    }

                    Deselect(true);
                    Mode = 1;
                    MeshManager.ToggleVerticeColliders();
                    MeshManager.ToggleVerticeVisuals();
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
                        MeshManager.ToggleVerticeColliders();
                        MeshManager.ToggleVerticeVisuals();
                    }

                    if (Mode == 3)
                    {
                        MeshManager.ToggleTriColliders();
                    }

                    MeshManager.ToggleEdgeColliders();
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
                        MeshManager.ToggleVerticeColliders();
                        MeshManager.ToggleVerticeVisuals();
                    }
                    if (Mode == 2)
                    {
                        MeshManager.ToggleEdgeColliders();
                    }

                    MeshManager.ToggleTriColliders();
                    Mode = 3;
                    UniSelect.SelectableObjects = MeshManager.SpawnedTris;

                    return;
                }
            }

            if (MeshManipulator.FoundCollider)
                return;

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
                            SelectionSystem(Results[i], true);
                        }
                    }
                }
                else
                {
                    if (!SelectedList.Contains(Results[0]))
                    {
                        SelectionSystem(Results[0], false);
                    }
                }
            }
            else
            {
                Deselect(true);
            }
        }

        private void SelectionSystem(GameObject Item, bool BoxSelect)
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

                    TriDataStore TDS = PrimarySelectedObject.GetComponent<TriDataStore>();
                    for (int i = 0; i < TDS.InvolvedEdgeObjects.Count; i++)
                    {
                        TDS.InvolvedEdgeObjects[i].GetComponent<MeshRenderer>().material = PrimarySelectedMat;
                    }
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
                    TriDataStore TDS = PrimarySelectedObject.GetComponent<TriDataStore>();
                    for (int i = 0; i < TDS.InvolvedEdgeObjects.Count; i++)
                    {
                        TDS.InvolvedEdgeObjects[i].GetComponent<MeshRenderer>().material = SecondarySelectedMat;
                    }

                    PrimarySelectedObject = SelectedList[SelectedList.Count - 1];

                    TDS = PrimarySelectedObject.GetComponent<TriDataStore>();
                    for (int i = 0; i < TDS.InvolvedEdgeObjects.Count; i++)
                    {
                        TDS.InvolvedEdgeObjects[i].GetComponent<MeshRenderer>().material = PrimarySelectedMat;
                    }
                }
            }
            DoHandlesStuff();
        }

        private void VertexSelect(GameObject Item)
        {
            Item.GetComponent<MeshRenderer>().material = SecondarySelectedMat;
            SelectedList.Add(Item);
        }

        private void TriSelect(GameObject Item)
        {
            TriDataStore TDS = Item.GetComponent<TriDataStore>();
            for (int i = 0; i < TDS.InvolvedEdgeObjects.Count; i++)
            {
                TDS.InvolvedEdgeObjects[i].GetComponent<MeshRenderer>().material = SecondarySelectedMat;
            }

            SelectedList.Add(Item);
        }

        private void Deselect(bool DestroyHandles)
        {
            if (Mode != 3)
            {
                for (int i = 0; i < SelectedList.Count; i++)
                {
                    SelectedList[i].GetComponent<MeshRenderer>().material = DeselectedMat;
                }
            }
            else
            {
                for (int i = 0; i < SelectedList.Count; i++)
                {
                    TriDataStore TDS = SelectedList[i].GetComponent<TriDataStore>();
                    for (int j = 0; j < TDS.InvolvedEdgeObjects.Count; j++)
                    {
                        TDS.InvolvedEdgeObjects[j].GetComponent<MeshRenderer>().material = DeselectedMat;
                    }
                }
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
        public void DoHandlesStuff()
        {
            if (SpawnedHandle == null)
            {
                SpawnedHandle = (GameObject)Instantiate(Handle, this.transform);
            }

            if (Mode != 3)
            {
                SpawnedHandle.transform.position = PrimarySelectedObject.transform.position;
                PrimarySelectedObjectPosition = PrimarySelectedObject.transform.position;
            }
            else
            {
                SpawnedHandle.transform.position = PrimarySelectedObject.GetComponent<TriDataStore>().Position;
                PrimarySelectedObjectPosition = SpawnedHandle.transform.position;
            }
        }
    }
}