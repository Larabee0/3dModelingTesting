using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshSystemV6
{
    [RequireComponent(typeof(MeshSelection), typeof(MeshManipulator), typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider),typeof(ThreeWayHandler))]
    public class MeshManager : MonoBehaviour
    {
        [Header("Script References")]
        [SerializeField] private MeshManipulator MeshManipulator;
        public MeshSelection MeshSelection;
        [SerializeField] private ThreeWayHandler ThreeWayHandler;

        [Header("Mesh stuff")]
        public MeshFilter meshFilter; // Active MeshFilter
        private Mesh originalMesh; // Mesh in filter before starting this script
        public Mesh clonedMesh; // Current Mesh.
        public bool isCloned = false; // Has the mesh been cloned?
        public MeshCollider Collider; // Active MeshCollider (technically its disabled but whatever)

        [Header("Visual Objects")]
        [SerializeField] private GameObject VertVisualElement;
        [SerializeField] private GameObject EdgeVisualElement;
        [SerializeField] private GameObject TriVisualElement;

        // Visual Object containers
        private GameObject vertContainer;
        private GameObject edgeContainer;
        private GameObject triContainer;

        [Header("Raw Verts & Tris")]
        public List<Vector3> GlobalVertices = new List<Vector3>(); // converstion of the vertices arrive into world coordinates.
        public List<Vector3> LocalVertices = new List<Vector3>(); // Copy of the vertices array
        public List<int> Triangles = new List<int>();
        //public Vector3[] verticesArray;
        //1public int[] trianglesArray;

        [Header("Simplified Verts & Tris")]
        // Contains a list of position of al unquie vertices in verticesArray. For a cube that is 8, instead of 24 like verticesArray contains.
        public List<Vector3> LocalSimplifiedVerts = new List<Vector3>();
        // Contains a list of all vertices that occupy the same position as SimplifiedVerts[i], ordered the same as SimplifiedVerts.
        public List<List<int>> RelatedVertices = new List<List<int>>(); // Always useful.

        // Contains the full list of triangles (trianglesArray) using the simplied vertices instead of the full verticesArray.
        public List<int> SimplifiedTriangles = new List<int>(); // Probably only useful on start up.

        // Contains the list of edges with no duplicate edges using SimplifiedVerts instead of the full verticesArray.
        public List<List<int>> SimplifiedEdges = new List<List<int>>(); // Probably only useful on start up.
        public List<Vector3> EdgePositions = new List<Vector3>();

        [Header("Spawned Visuals & Data Components")]
        public List<GameObject> SpawnedVerts = new List<GameObject>();
        public List<VertDataStruct> SpawnedVertsDS = new List<VertDataStruct>();
        public List<GameObject> SpawnedEdges = new List<GameObject>();
        public List<EdgeDataStruct> SpawnedEdgesDS = new List<EdgeDataStruct>();
        public List<GameObject> SpawnedTris = new List<GameObject>();
        public List<TriDataStruct> SpawnedTriDS = new List<TriDataStruct>();

        private void Start()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = false;
        }
        #region Association and Relation Lists
        private void CreateSimplifiedVertices()
        {
            RelatedVertices.Clear();
            HashSet<Vector3> TempHasSet = new HashSet<Vector3>();
            // Builds list of simplified vertices - Unity requires each triangle to have its own 3 vertices in the vertices array.
            // This creates a list of all unqie vertices.
            for (int i = 0; i < LocalVertices.Count; i++)
            {
                TempHasSet.Add(LocalVertices[i]);
            }
            LocalSimplifiedVerts = TempHasSet.ToList();
            // Builds lists of related vertices.
            // This is an index map of the vertices array to the simplied vertices list.
            // Basically if we change the vertex at index 0 of the SimpliedVerts list. We want to change all the vertices in the verticesArray, so that the mesh remains intact.
            // This list is indexed to match SimpliedVertices.
            // Each index of this list contains a list of all the indexes in the vertices array that occupy the same position. In other words, all Related Vertices.
            for (int i = 0; i < LocalSimplifiedVerts.Count; i++)
            {
                List<int> Temp = new List<int>();
                for (int j = 0; j < LocalVertices.Count; j++)
                {
                    if (LocalSimplifiedVerts[i] == LocalVertices[j])
                    {
                        Temp.Add(j);
                    }
                }
                RelatedVertices.Add(Temp);
            }
        }

        private void CreateSimpliedTriangles()
        {
            SimplifiedTriangles.Clear();

            // This creates the triangles as they are in the triangles array, except index referencing to SimplifiedVertices list, instead of the verticesArray.
            for (int i = 0; i < Triangles.Count; i++)
            {
                for (int j = 0; j < LocalSimplifiedVerts.Count; j++)
                {
                    if (LocalSimplifiedVerts[j] == LocalVertices[Triangles[i]])
                    {
                        SimplifiedTriangles.Add(j);
                        break;
                    }
                }
            }
        }

        private void CreateSimpliedEdges()
        {
            EdgePositions.Clear();
            SimplifiedEdges.Clear();

            // Thiscreates two lists.
            // The first creates a 2d List of ints (indexes of SimplifiedVerts) called SimplifiedEdges.
            // the SimplifiedTriangles is ordered 0,1,2 to create a triangle, each edge is stored here as index 0 {0,1}, index 1 {1, 2}, index 2 {2,0}.
            // Basically it stores how to create a straight line between each vertice in the triangle in the order it is rendered, to find its edges.
            // However it triangles have overlapping edges, to stop ducplicates being added, the EdgePositions List is used, which also has other uses.
            // the second list, is called EdgePositions and is a list of Vector3s, this contains the average position between the 2 vertices that make up an edge.
            // To stop duplicates being added to either list, when attempting to add a new edge to both lists,
            // a check is made to see if EdgePositons contains the average position just calculated, and if it does we know to skip this edge.

            // This should be quick to run as we advance through SimplifiedTriangles at 3 indexes per loop.
            for (int i = 0; i < SimplifiedTriangles.Count; i += 3)
            {
                List<int> Temp = new List<int> { SimplifiedTriangles[i], SimplifiedTriangles[i + 1] };
                Vector3 TempV3 = (LocalSimplifiedVerts[Temp[0]] + LocalSimplifiedVerts[Temp[1]]) / 2;
                if (!EdgePositions.Contains(TempV3))
                {
                    EdgePositions.Add(TempV3);
                    SimplifiedEdges.Add(Temp);
                }

                Temp = new List<int> { SimplifiedTriangles[i + 1], SimplifiedTriangles[i + 2] };
                TempV3 = (LocalSimplifiedVerts[Temp[0]] + LocalSimplifiedVerts[Temp[1]]) / 2;
                if (!EdgePositions.Contains(TempV3))
                {
                    EdgePositions.Add(TempV3);
                    SimplifiedEdges.Add(Temp);
                }

                Temp = new List<int> { SimplifiedTriangles[i + 2], SimplifiedTriangles[i] };
                TempV3 = (LocalSimplifiedVerts[Temp[0]] + LocalSimplifiedVerts[Temp[1]]) / 2;
                if (!EdgePositions.Contains(TempV3))
                {
                    EdgePositions.Add(TempV3);
                    SimplifiedEdges.Add(Temp);
                }
            }
        }
        #endregion

        #region Setup
        private void SpawnVerticeVisuals()
        {
            SpawnedVerts.Clear();
            SpawnedVertsDS.Clear();

            vertContainer = new GameObject { name = "vertContainer" };
            vertContainer.transform.parent = this.transform;
            vertContainer.transform.localPosition = Vector3.zero;
            vertContainer.transform.localScale = Vector3.one;

            for (int i = 0; i < LocalSimplifiedVerts.Count; i++)
            {
                GameObject VertexMesh = Instantiate(VertVisualElement, this.transform.TransformPoint(LocalSimplifiedVerts[i]), Quaternion.identity, vertContainer.transform);
                VertexMesh.name = i.ToString();
                VertDataStruct VDStore = new VertDataStruct(new List<int>(RelatedVertices[i]), i,VertexMesh);
                VDStore.ConnectedVerts.Add(VDStore);
                ThreeWayHandler.VDSs.Add(i, VDStore);
                ThreeWayHandler.VDOs.Add(i, VertexMesh);
                ThreeWayHandler.GOTOVDSs.Add(VertexMesh, VDStore);

                SpawnedVerts.Add(VertexMesh);
                SpawnedVertsDS.Add(VDStore);
            }
        }

        private void SpawnEdgeVisuals()
        {
            SpawnedEdges.Clear();
            SpawnedEdgesDS.Clear();

            edgeContainer = new GameObject { name = "edgeContainer" };
            edgeContainer.transform.parent = this.transform;
            edgeContainer.transform.localPosition = Vector3.zero;

            for (int i = 0; i < SimplifiedEdges.Count; i++)
            {
                VertDataStruct SpawnedVertsDS1 = SpawnedVertsDS[SimplifiedEdges[i][0]];
                VertDataStruct SpawnedVertsDS2 = SpawnedVertsDS[SimplifiedEdges[i][1]];

                if (!SpawnedVertsDS1.ConnectedVerts.Contains(SpawnedVertsDS2))
                {
                    SpawnedVertsDS1.ConnectedVerts.Add(SpawnedVertsDS2);
                }
                if (!SpawnedVertsDS2.ConnectedVerts.Contains(SpawnedVertsDS1))
                {
                    SpawnedVertsDS2.ConnectedVerts.Add(SpawnedVertsDS1);
                }
                Vector3 SpawnedVerts1 = SpawnedVertsDS[SpawnedVertsDS1.SimpliedVertIndex].transform.position;
                Vector3 SpawnedVerts2 = SpawnedVertsDS[SpawnedVertsDS2.SimpliedVertIndex].transform.position;
                Vector3 EdgeAveragePositon = (SpawnedVerts1 + SpawnedVerts2) / 2;

                GameObject EdgeMesh = Instantiate(EdgeVisualElement, EdgeAveragePositon, Quaternion.identity, edgeContainer.transform);

                float EdgeDistance = Vector3.Distance(SpawnedVerts1, SpawnedVerts2);

                EdgeMesh.name = i.ToString();
                EdgeMesh.transform.localScale = new Vector3(0.025f, 0.025f, EdgeDistance);
                Quaternion QuaterEuler = Quaternion.Euler(this.transform.localRotation.eulerAngles);
                EdgeMesh.transform.position = QuaterEuler * EdgeAveragePositon;
                EdgeMesh.transform.LookAt(QuaterEuler * SpawnedVerts1);
                
                EdgeDataStruct EDStore = new EdgeDataStruct(new List<List<int>> { RelatedVertices[SpawnedVertsDS1.SimpliedVertIndex], RelatedVertices[SpawnedVertsDS2.SimpliedVertIndex] }, new List<VertDataStruct> { SpawnedVertsDS1, SpawnedVertsDS2 }, i, EdgeMesh);

                SpawnedVertsDS[SpawnedVertsDS1.SimpliedVertIndex].InvolvedEdges.Add(EDStore);
                SpawnedVertsDS[SpawnedVertsDS2.SimpliedVertIndex].InvolvedEdges.Add(EDStore);

                ThreeWayHandler.EDOs.Add(i, EdgeMesh);
                ThreeWayHandler.EDSs.Add(i, EDStore);
                ThreeWayHandler.GOTOEDSs.Add(EdgeMesh, EDStore);

                SpawnedEdges.Add(EdgeMesh);
                SpawnedEdgesDS.Add(EDStore);
            }
        }

        private void SpawnTriangleVisuals()
        {
            SpawnedTris.Clear();
            SpawnedTriDS.Clear();

            triContainer = new GameObject { name = "triContainer" };
            triContainer.transform.parent = this.transform;
            triContainer.transform.localPosition = Vector3.zero;
            triContainer.transform.localScale = Vector3.one;
            int j = 0;
            for (int i = 0; i < SimplifiedTriangles.Count; i += 3)
            {
                GameObject TriangleMesh = Instantiate(TriVisualElement, this.transform.position, Quaternion.identity, triContainer.transform);
                TriangleMesh.name = j.ToString();

                List<GameObject> InvolvedEdges = new List<GameObject>();
                List<EdgeDataStruct> InvolvedEdgeDataStore = new List<EdgeDataStruct>();
                VertDataStruct[] VDS = new VertDataStruct[]
                {
                    SpawnedVertsDS[SimplifiedTriangles[i]],
                    SpawnedVertsDS[SimplifiedTriangles[i + 1]],
                    SpawnedVertsDS[SimplifiedTriangles[i + 2]]
                };
                int secondaryK = 1;
                for (int k = 0; k < 3; k++)
                {
                    if (secondaryK == 3)
                    {
                        secondaryK = 0;
                    }
                    for (int h = 0; h < VDS[k].InvolvedEdges.Count; h++)
                    {
                        for (int l = 0; l < VDS[secondaryK].InvolvedEdges.Count; l++)
                        {

                            if (VDS[k].InvolvedEdges[h].EdgePositionsIndex == VDS[secondaryK].InvolvedEdges[l].EdgePositionsIndex)
                            {
                                InvolvedEdges.Add(VDS[k].InvolvedEdges[h].GameObject);
                                InvolvedEdgeDataStore.Add(VDS[k].InvolvedEdges[h]);
                                goto Next;
                            }
                        }
                    }
                Next:
                    secondaryK++;
                }

                TriDataStruct TriDStore = new TriDataStruct(new List<int> { SimplifiedTriangles[i], SimplifiedTriangles[i + 1], SimplifiedTriangles[i + 2] }, new List<int> { Triangles[i], Triangles[i + 1], Triangles[i + 2] }, new List<VertDataStruct>
                    (VDS),new List<GameObject>(InvolvedEdges),new List<EdgeDataStruct>(InvolvedEdgeDataStore), j,TriangleMesh);

                InvolvedEdgeDataStore.ForEach(k => k.InvolvedTriangles.Add(TriDStore));
                ThreeWayHandler.TDSs.Add(j, TriDStore);
                ThreeWayHandler.TDOs.Add(j, TriangleMesh);
                ThreeWayHandler.GOTOTDSs.Add(TriangleMesh, TriDStore);
                SpawnedTris.Add(TriangleMesh);
                SpawnedTriDS.Add(TriDStore);

                j++;
            }
            GetAdjacentTris();
        }
        private void GetAdjacentTris()
        {
            for (int i = 0; i < SpawnedTriDS.Count; i++)
            {
                HashSet<TriDataStruct> InvolvedTrisHash = new HashSet<TriDataStruct>();
                InvolvedTrisHash.Add(SpawnedTriDS[i]);
                for (int j = 0; j < SpawnedTriDS[i].InvolvedEdgeDS.Count; j++)
                {
                    InvolvedTrisHash.UnionWith(SpawnedTriDS[i].InvolvedEdgeDS[j].InvolvedTriangles);
                }
                foreach (TriDataStruct j in InvolvedTrisHash)
                {
                    SpawnedTriDS[i].InvolvedTriDS.Add(j);
                }
            }
        }

        private void ProvideColliders()
        {
            for (int i = 0; i < SpawnedVertsDS.Count; i++)
            {
                ThreeWayHandler.VerticeColliders.Add(SpawnedVertsDS[i].ThisBoxCollider);
                ThreeWayHandler.VerticeVisuals.Add(SpawnedVertsDS[i].ThisMeshRenderer);
            }
            SpawnedEdgesDS.ForEach(i => ThreeWayHandler.EdgeColliders.Add(i.ThisBoxCollider));
            SpawnedTriDS.ForEach(i => ThreeWayHandler.TriangleColliders.Add(i.ThisMeshCollider));
        }
        #endregion

        #region Extrude
        public GameObject ExtrudeVertex(GameObject Origin)
        {
            LocalSimplifiedVerts.Add(Origin.transform.localPosition);
            LocalVertices.Add(Origin.transform.localPosition);
            GlobalVertices.Add(Origin.transform.position);

            GameObject VertexMesh = Instantiate(VertVisualElement, Origin.transform.position, Quaternion.identity, vertContainer.transform);
            VertexMesh.name = SpawnedVerts.Count.ToString();

            VertDataStruct VDStore = new VertDataStruct(new List<int> { LocalVertices.Count - 1 }, SpawnedVerts.Count, VertexMesh);
            VDStore.ConnectedVerts.Add(VDStore);

            ThreeWayHandler.VDSs.Add(SpawnedVerts.Count, VDStore);
            ThreeWayHandler.VDOs.Add(SpawnedVerts.Count, VertexMesh);
            ThreeWayHandler.GOTOVDSs.Add(VertexMesh, VDStore);

            SpawnedVerts.Add(VertexMesh);
            SpawnedVertsDS.Add(VDStore);

            return VertexMesh;
        }

        public GameObject ExtrudeEdge(GameObject Origin)
        {
            EdgeDataStruct OriginEDS = ThreeWayHandler.GOTOEDSs[Origin];
            GameObject InvolvedVertObject1 = OriginEDS.InvolvedVertObjects[0].GameObject;
            GameObject InvolvedVertObject2 = OriginEDS.InvolvedVertObjects[1].GameObject;
            GameObject VertexA = ExtrudeVertex(InvolvedVertObject1);
            GameObject VertexB = ExtrudeVertex(InvolvedVertObject2);

            NewEdge(InvolvedVertObject1, VertexA);
            NewEdge(InvolvedVertObject2, VertexB);
            NewEdge(InvolvedVertObject2, VertexA);
            NewEdge(VertexA, VertexB);

            NewTriangle(InvolvedVertObject1, VertexA, InvolvedVertObject2);
            NewTriangle(InvolvedVertObject2, VertexA, VertexB);

            return SpawnedEdges[SpawnedEdges.Count - 1];
        }

        public List<GameObject> ExtrudeSection(List<GameObject> OriginTris)
        {
            OriginTris.ForEach(i => RemoveTriangle(i));

            HashSet<VertDataStruct> MasterVertHashSet = new HashSet<VertDataStruct>();
            HashSet<GameObject> EdgeHasSet = new HashSet<GameObject>();


            List<VertDataStruct> MasterVerts = new List<VertDataStruct>();
            List<VertDataStruct> MasterVertsInOrder = new List<VertDataStruct>();
            List<VertDataStruct> DirectVertexEdgeMap = new List<VertDataStruct>();


            List<TriDataStruct> MastersTDS = new List<TriDataStruct>();
           

            List<GameObject> NewVerts = new List<GameObject>();
            List<GameObject> NewTris = new List<GameObject>();

            for (int i = 0; i < OriginTris.Count; i++)
            {
                MastersTDS.Add(ThreeWayHandler.GOTOTDSs[OriginTris[i]]);
                List<VertDataStruct> Temp = new List<VertDataStruct>(MastersTDS[i].InvolvedVertObjects);
                Temp.Sort(SortByName);
                MasterVertsInOrder.AddRange(Temp);
                MasterVerts.AddRange(MastersTDS[i].InvolvedVertObjects);
                MasterVertHashSet.UnionWith(MastersTDS[i].InvolvedVertObjects);
                EdgeHasSet.UnionWith(MastersTDS[i].InvolvedEdgeObjects);
            }

            List<GameObject> UnquieEdgesObj = EdgeHasSet.ToList();
            UnquieEdgesObj.ForEach(i => DirectVertexEdgeMap.AddRange(ThreeWayHandler.GOTOEDSs[i].InvolvedVertObjects));

            List<VertDataStruct> UnquieVertOrigin = MasterVertHashSet.ToList();
            UnityEngine.Debug.Log("Extruding " + UnquieVertOrigin.Count + " vertices");
            for (int i = 0; i < UnquieVertOrigin.Count; i++)
            {
                NewVerts.Add(ExtrudeVertex(UnquieVertOrigin[i].GameObject));
                NewEdge(UnquieVertOrigin[i].GameObject, NewVerts[i]);
            }


            List<List<VertDataStruct>> Pairs = new List<List<VertDataStruct>>();
            List<VertDataStruct> NewVDSs = new List<VertDataStruct>();
            NewVerts.ForEach(i => NewVDSs.Add(ThreeWayHandler.GOTOVDSs[i]));
            for (int i = 0; i < MasterVertsInOrder.Count; i+=3)
            {
                Pairs.Add(new List<VertDataStruct> { NewVDSs[UnquieVertOrigin.IndexOf(MasterVertsInOrder[i + 1])], NewVDSs[UnquieVertOrigin.IndexOf(MasterVertsInOrder[i])] });
                Pairs.Add(new List<VertDataStruct> { NewVDSs[UnquieVertOrigin.IndexOf(MasterVertsInOrder[i + 1])], NewVDSs[UnquieVertOrigin.IndexOf(MasterVertsInOrder[i + 2])] });
                Pairs.Add(new List<VertDataStruct> { NewVDSs[UnquieVertOrigin.IndexOf(MasterVertsInOrder[i + 2])], NewVDSs[UnquieVertOrigin.IndexOf(MasterVertsInOrder[i])] });
            }

            UnityEngine.Debug.Log("Pairs: " + Pairs.Count);

            List<int> FlaggedForRemoval = new List<int>();
            for (int i = 0; i < Pairs.Count; i++)
            {
                for (int j = 0; j < Pairs.Count; j++)
                {
                    if(i == j)
                    {
                        j++;
                    }
                    if(j == Pairs.Count)
                    {
                        break;
                    }
                    if(Pairs[i].Contains(Pairs[j][0]) && Pairs[i].Contains(Pairs[j][1]))
                    {
                        if(!(FlaggedForRemoval.Contains(i) && FlaggedForRemoval.Contains(j)))
                        {
                            FlaggedForRemoval.Add(i);
                            FlaggedForRemoval.Add(j);
                        }
                    }
                }
            }
            FlaggedForRemoval.Sort(); FlaggedForRemoval.Reverse(); FlaggedForRemoval.ForEach(i => Pairs.RemoveAt(i));

            UnityEngine.Debug.Log("Pairs Post Clean-up: " + Pairs.Count);
            for (int i = 0; i < Pairs.Count; i++)
            {
                if (!Pairs[i][1].ConnectedVerts.Contains(UnquieVertOrigin[NewVDSs.IndexOf(Pairs[i][0])]) && !Pairs[i][0].ConnectedVerts.Contains(UnquieVertOrigin[NewVDSs.IndexOf(Pairs[i][1])]))
                {
                    UnityEngine.Debug.Log("A: " + Pairs[i][0].SimpliedVertIndex + " B: " + Pairs[i][1].SimpliedVertIndex+"\nNew Edge Between " + UnquieVertOrigin[NewVDSs.IndexOf(Pairs[i][0])].SimpliedVertIndex + " and " + Pairs[i][1].SimpliedVertIndex);
                    NewEdge(UnquieVertOrigin[NewVDSs.IndexOf(Pairs[i][0])].GameObject, Pairs[i][1].GameObject);
                }
                else
                {
                    UnityEngine.Debug.Log(UnquieVertOrigin[NewVDSs.IndexOf(Pairs[i][0])].SimpliedVertIndex + " and " + Pairs[i][1].SimpliedVertIndex+ " are already connected");
                }
            }
            

            for (int i = 0; i < DirectVertexEdgeMap.Count; i += 2)
            {
                NewEdge(NewVerts[UnquieVertOrigin.IndexOf(DirectVertexEdgeMap[i])], NewVerts[UnquieVertOrigin.IndexOf(DirectVertexEdgeMap[i + 1])]);
            }


            List<VertDataStruct> SlaveTriangles = CalculatePossibleTriangles(NewVDSs);
            UnityEngine.Debug.Log("Creating " + SlaveTriangles.Count/3 + " 'slave' triangles");
            for (int i = 0; i < SlaveTriangles.Count; i += 3)
            {
                //UnityEngine.Debug.Log("new Tri: " + SlaveTriangles[i].gameObject.name + " " + SlaveTriangles[i + 1].gameObject.name + " " + SlaveTriangles[i + 2].gameObject.name);
                NewTriangle(SlaveTriangles[i].GameObject, SlaveTriangles[i + 1].GameObject, SlaveTriangles[i + 2].GameObject);
            }

            UnityEngine.Debug.Log("Creating " + MastersTDS.Count/3 + " 'master' triangles");
            for (int i = 0; i < MasterVerts.Count; i += 3)
            {
                NewTris.Add(NewTriangle(NewVerts[UnquieVertOrigin.IndexOf(MasterVerts[i])], NewVerts[UnquieVertOrigin.IndexOf(MasterVerts[i + 1])], NewVerts[UnquieVertOrigin.IndexOf(MasterVerts[i + 2])]));
            }

            return NewTris;
        }

        public List<VertDataStruct> CalculatePossibleTriangles(List<VertDataStruct> Input)
        {
            List<VertDataStruct> PossibleTriangles = new List<VertDataStruct>();
            HashSet<VertDataStruct> AllInvolvedVDSHash = new HashSet<VertDataStruct>();
            List<int> TemporyTriangles = new List<int>(SimplifiedTriangles);
            List<List<int>> TemporyTrianglesIDs = new List<List<int>>();
            for (int k = 0; k < TemporyTriangles.Count; k+=3)
            {
                TemporyTrianglesIDs.Add(new List<int> { TemporyTriangles[k], TemporyTriangles[k + 1], TemporyTriangles[k + 2] });
            }
            Input.ForEach(i => AllInvolvedVDSHash.UnionWith(i.ConnectedVerts));
            List<VertDataStruct> AllInvolvedVDS = AllInvolvedVDSHash.ToList();
            for (int i = 0; i < Input.Count; i++)
            {
                VertDataStruct PrimaryVert = Input[i];
                List<VertDataStruct> PrimaryConnectedVerts = new List<VertDataStruct>(PrimaryVert.ConnectedVerts);
                for (int j = 1; j < PrimaryConnectedVerts.Count; j++)
                {
                    VertDataStruct SecondaryVert = PrimaryConnectedVerts[j];
                    List<VertDataStruct> SecondaryConnectedVerts = new List<VertDataStruct>(SecondaryVert.ConnectedVerts);
                    for (int k = 2; k < PrimaryConnectedVerts.Count; k++)
                    {
                        if (SecondaryConnectedVerts.Contains(PrimaryConnectedVerts[k]) && SecondaryVert.SimpliedVertIndex != PrimaryConnectedVerts[k].SimpliedVertIndex)
                        {
                            bool okToAdd = true;
                            for (int o = 0; o < TemporyTrianglesIDs.Count; o++)
                            {
                                if (TemporyTrianglesIDs[o].Contains(PrimaryVert.SimpliedVertIndex) && TemporyTrianglesIDs[o].Contains(SecondaryVert.SimpliedVertIndex) && TemporyTrianglesIDs[o].Contains(PrimaryConnectedVerts[k].SimpliedVertIndex))
                                {
                                    okToAdd = false;
                                    break;
                                }
                            }
                            if (okToAdd)
                            {
                                PossibleTriangles.AddRange(new List<VertDataStruct> { PrimaryVert, SecondaryVert, PrimaryConnectedVerts[k] });
                                TemporyTrianglesIDs.Add(new List<int> { PrimaryVert.SimpliedVertIndex, SecondaryVert.SimpliedVertIndex, PrimaryConnectedVerts[k].SimpliedVertIndex });
                            }
                        }
                    }
                }
            }
            UnityEngine.Debug.Log(PossibleTriangles.Count);
            //for (int i = 0; i < PossibleTriangles.Count; i+=3)
            //{
            //    UnityEngine.Debug.Log(PossibleTriangles[i].SimpliedVertIndex + " " + PossibleTriangles[i + 1].SimpliedVertIndex + " " + PossibleTriangles[i + 2].SimpliedVertIndex);
            //}
            return PossibleTriangles;
        }


        private int SortByName(VertDataStruct A, VertDataStruct B)
        {
            int x = A.SimpliedVertIndex;
            int y = B.SimpliedVertIndex;

            int retval = x.CompareTo(y);
            if (retval != 0)
            {
                return retval;
            }
            else
            {
                return x.CompareTo(y);
            }
        }

        #endregion

        #region NewEdge and Triangle
        public void NewEdge(GameObject origin, GameObject newVert)
        {
            Vector3 originPos = origin.transform.position;
            Vector3 newVertPos = newVert.transform.position;
            VertDataStruct originVDS = origin.GetComponent<VertDataStruct>();
            VertDataStruct newVertVDS = newVert.GetComponent<VertDataStruct>();

            if (!originVDS.ConnectedVerts.Contains(newVertVDS))
            {
                originVDS.ConnectedVerts.Add(newVertVDS);
            }
            if (!newVertVDS.ConnectedVerts.Contains(originVDS))
            {
                newVertVDS.ConnectedVerts.Add(originVDS);
            }
            Vector3 EdgeAveragePositon = ((originPos + newVertPos) / 2);

            GameObject EdgeMesh = Instantiate(EdgeVisualElement, EdgeAveragePositon, Quaternion.identity, edgeContainer.transform);

            float EdgeDistance = Vector3.Distance(originPos, newVertPos);

            EdgeMesh.name = SpawnedEdges.Count.ToString();
            EdgeMesh.transform.localScale = new Vector3(0.025f, 0.025f, EdgeDistance);
            Quaternion QuaterEuler = Quaternion.Euler(this.transform.localRotation.eulerAngles);
            EdgeMesh.transform.position = QuaterEuler * EdgeAveragePositon;
            EdgeMesh.transform.LookAt(QuaterEuler * originPos);

            EdgeDataStruct EDStore = new EdgeDataStruct(new List<List<int>> { originVDS.RelatedVerts, newVertVDS.RelatedVerts }, new List<VertDataStruct> { originVDS, newVertVDS }, SpawnedEdges.Count, EdgeMesh);
            //EDStore.InvolvedVerts = new List<List<int>> { originVDS.RelatedVerts, newVertVDS.RelatedVerts };

            originVDS.InvolvedEdges.Add(EDStore);
            newVertVDS.InvolvedEdges.Add(EDStore);

            //EDStore.InvolvedVertObjects.AddRange(new List<VertDataStruct> { originVDS, newVertVDS });
            //EDStore.EdgePositionsIndex = SpawnedEdges.Count;

            ThreeWayHandler.EDOs.Add(SpawnedEdges.Count, EdgeMesh);
            ThreeWayHandler.EDSs.Add(SpawnedEdges.Count, EDStore);
            ThreeWayHandler.GOTOEDSs.Add(EdgeMesh, EDStore);

            SpawnedEdges.Add(EdgeMesh);
            SpawnedEdgesDS.Add(EDStore);
        }

        public GameObject NewEdgeReturn(GameObject origin, GameObject newVert)
        {
            Vector3 originPos = origin.transform.position;
            Vector3 newVertPos = newVert.transform.position;
            VertDataStruct originVDS = ThreeWayHandler.GOTOVDSs[origin];// origin.GetComponent<VertDataStore>();
            VertDataStruct newVertVDS = ThreeWayHandler.GOTOVDSs[newVert]; //newVert.GetComponent<VertDataStore>();

            if (!originVDS.ConnectedVerts.Contains(newVertVDS))
            {
                originVDS.ConnectedVerts.Add(newVertVDS);
            }
            if (!newVertVDS.ConnectedVerts.Contains(originVDS))
            {
                newVertVDS.ConnectedVerts.Add(originVDS);
            }
            Vector3 EdgeAveragePositon = ((originPos + newVertPos) / 2);

            GameObject EdgeMesh = Instantiate(EdgeVisualElement, EdgeAveragePositon, Quaternion.identity, edgeContainer.transform);

            float EdgeDistance = Vector3.Distance(originPos, newVertPos);

            EdgeMesh.name = SpawnedEdges.Count.ToString();
            EdgeMesh.transform.localScale = new Vector3(0.025f, 0.025f, EdgeDistance);
            EdgeMesh.transform.position = Quaternion.Euler(this.transform.localRotation.eulerAngles) * EdgeAveragePositon;
            EdgeMesh.transform.LookAt(Quaternion.Euler(this.transform.localRotation.eulerAngles) * originPos);

            EdgeDataStruct EDStore = new EdgeDataStruct(new List<List<int>> { originVDS.RelatedVerts, newVertVDS.RelatedVerts }, new List<VertDataStruct> { originVDS, newVertVDS }, SpawnedEdges.Count, EdgeMesh);
            //EDStore.InvolvedVerts = new List<List<int>> { originVDS.RelatedVerts, newVertVDS.RelatedVerts };

            originVDS.InvolvedEdges.Add(EDStore);
            newVertVDS.InvolvedEdges.Add(EDStore);

            //EDStore.InvolvedVertObjects.AddRange(new List<VertDataStruct> { originVDS, newVertVDS });
            //EDStore.EdgePositionsIndex = SpawnedEdges.Count;

            ThreeWayHandler.EDOs.Add(SpawnedEdges.Count, EdgeMesh);
            ThreeWayHandler.EDSs.Add(SpawnedEdges.Count, EDStore);
            ThreeWayHandler.GOTOEDSs.Add(EdgeMesh, EDStore);

            SpawnedEdges.Add(EdgeMesh);
            SpawnedEdgesDS.Add(EDStore);

            return EdgeMesh;
        }

        public GameObject NewTriangle(GameObject V1, GameObject V2,GameObject V3)
        {
            VertDataStruct[] VertDS = new VertDataStruct[] { ThreeWayHandler.GOTOVDSs[V1], ThreeWayHandler.GOTOVDSs[V2], ThreeWayHandler.GOTOVDSs[V3] };
            Vector3[] VertLocalPositions = new Vector3[] { V1.transform.localPosition, V2.transform.localPosition, V3.transform.localPosition };
            SimplifiedTriangles.AddRange(new List<int> { VertDS[0].SimpliedVertIndex, VertDS[1].SimpliedVertIndex, VertDS[2].SimpliedVertIndex });
            // idk if transform point is actually required here, need to try not having it.
            Vector3[] VertGlobalPositions = new Vector3[] { this.transform.TransformPoint(V1.transform.position), this.transform.TransformPoint(V2.transform.position), this.transform.TransformPoint(V3.transform.position) };
            
            List<Vector3> NewLocalVerts = new List<Vector3>();
            List<Vector3> NewGlobalVerts = new List<Vector3>();
            List<int> NewTriBuilder = new List<int>();

            for (int i = 0; i < VertDS.Length; i++)
            {
                if (VertDS[i].InvolvedEdges.Count == 2)
                {
                    NewTriBuilder.Add(VertDS[i].RelatedVerts[0]);
                }
                else
                {
                    NewLocalVerts.Add(VertLocalPositions[i]);
                    NewGlobalVerts.Add(VertGlobalPositions[i]);
                    NewTriBuilder.Add(NewLocalVerts.Count + LocalVertices.Count - 1);
                    VertDS[i].RelatedVerts.Add(NewLocalVerts.Count + LocalVertices.Count - 1);
                }
            }

            Triangles.AddRange(NewTriBuilder);

            if (NewLocalVerts.Count > 0)
            {
                LocalVertices.AddRange(NewLocalVerts);
                GlobalVertices.AddRange(NewGlobalVerts);
            }

            GameObject TriangleMesh = Instantiate(TriVisualElement, this.transform.position, Quaternion.identity, triContainer.transform);
            TriangleMesh.name = SpawnedTris.Count.ToString();

            List<int> Edges = new List<int>();
            List<GameObject> InvolvedEdges = new List<GameObject>();
            List<EdgeDataStruct> InvolvedEdgesDS = new List<EdgeDataStruct>();

            int secondaryK = 1;
            for (int k = 0; k < 3; k++)
            {
                if (secondaryK == 3)
                {
                    secondaryK = 0;
                }
                for (int i = 0; i < VertDS[k].InvolvedEdges.Count; i++)
                {
                    for (int j = 0; j < VertDS[secondaryK].InvolvedEdges.Count; j++)
                    {

                        if (VertDS[k].InvolvedEdges[i].EdgePositionsIndex == VertDS[secondaryK].InvolvedEdges[j].EdgePositionsIndex)
                        {
                            Edges.Add(VertDS[k].InvolvedEdges[i].EdgePositionsIndex);
                            InvolvedEdges.Add(VertDS[k].InvolvedEdges[i].GameObject);
                            InvolvedEdgesDS.Add(VertDS[k].InvolvedEdges[i]);
                        }
                    }
                }
                secondaryK++;
            }

            TriDataStruct TriDStore = new TriDataStruct(new List<int> { VertDS[0].SimpliedVertIndex, VertDS[1].SimpliedVertIndex, VertDS[2].SimpliedVertIndex }, NewTriBuilder, new List<VertDataStruct>(VertDS), InvolvedEdges, InvolvedEdgesDS, SpawnedTris.Count, TriangleMesh);
            TriDStore.InvolvedVerts.AddRange(new List<int> { VertDS[0].SimpliedVertIndex, VertDS[1].SimpliedVertIndex, VertDS[2].SimpliedVertIndex });
            TriDStore.Position = (VertLocalPositions[0] + VertLocalPositions[1] + VertLocalPositions[2]) / 3;

            Edges.ForEach(i => SpawnedEdgesDS[i].InvolvedTriangles.Add(TriDStore));
            ThreeWayHandler.TDSs.Add(SpawnedTris.Count, TriDStore);
            ThreeWayHandler.TDOs.Add(SpawnedTris.Count, TriangleMesh);
            ThreeWayHandler.GOTOTDSs.Add(TriangleMesh, TriDStore);
            SpawnedTris.Add(TriangleMesh);
            SpawnedTriDS.Add(TriDStore);
            return TriangleMesh;
        }
        #endregion

        #region Remove
        public void RemoveVertice(GameObject ToRemove)
        {
            VertDataStruct ToRemoveVDS = ThreeWayHandler.GOTOVDSs[ToRemove];
            List<TriDataStruct> InvolvedTriangles = new List<TriDataStruct>(ToRemoveVDS.InvolvedTriangles);
            for (int i = 0; i < InvolvedTriangles.Count; i++)
            {
                RemoveTriangle(InvolvedTriangles[i].GameObject);
            }
        }

        public void RemoveEdge(GameObject ToRemove)
        {
            EdgeDataStruct ToRemoveEDS = ThreeWayHandler.GOTOEDSs[ToRemove];
            List<GameObject> InvolvedTriangles = new List<GameObject>();
            ToRemoveEDS.InvolvedTriangles.ForEach(i => InvolvedTriangles.Add(i.GameObject));
            for (int i = 0; i < InvolvedTriangles.Count; i++)
            {
                RemoveTriangle(InvolvedTriangles[i]);
            }
        }

        public void RemoveTriangle(GameObject ToRemove)
        {
            TriDataStruct ToRemoveTDS = ThreeWayHandler.GOTOTDSs[ToRemove];

            int cutPoint = -1;
            int cutsToMake = 3;

            // Searches through the triangle list to find the triangle to delete.
            for (int i = 0; i < Triangles.Count; i += 3)
            {
                if (Triangles[i] == ToRemoveTDS.TriangleArrayInfo[0])
                {
                    if (Triangles[i + 1] == ToRemoveTDS.TriangleArrayInfo[1])
                    {
                        if (Triangles[i + 2] == ToRemoveTDS.TriangleArrayInfo[2])
                        {
                            // Once it finds it, set the cut point and break the loop.
                            cutPoint = i;
                            break;
                        }
                    }
                }
            }
            
            // Remove the values from the list at position i, do this 3 timess as the vertices are in order,
            // and there are 3 vertices to a triangle.
            for (int i = 0; i < cutsToMake; i++)
            {
                Triangles.RemoveAt(cutPoint);
                SimplifiedTriangles.RemoveAt(cutPoint);
            }
            for (int i = 0; i < ToRemoveTDS.InvolvedEdgeDS.Count; i++)
            {
                ToRemoveTDS.InvolvedEdgeDS[i].InvolvedTriangles.Remove(ThreeWayHandler.GOTOTDSs[ToRemove]);
                if(ToRemoveTDS.InvolvedEdgeDS[i].InvolvedTriangles.Count == 0)
                {
                    for (int j = 0; j < ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects.Count; j++)
                    {
                        ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects[j].InvolvedEdges.Remove(ToRemoveTDS.InvolvedEdgeDS[i]);
                        for (int k = 0; k < ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects.Count; k++)
                        {
                            if(j == k)
                            {
                                k++;
                            }
                            if(k == ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects.Count)
                            {
                                break;
                            }
                            ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects[j].ConnectedVerts.Remove(ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects[k]);
                        }
                    }
                    Destroy(ToRemoveTDS.InvolvedEdgeObjects[i]);
                }
            }

            for (int i = 0; i < ToRemoveTDS.InvolvedVertObjects.Count; i++)
            {
                ToRemoveTDS.InvolvedVertObjects[i].InvolvedTriangles.Remove(ToRemoveTDS);
            }

            for (int i = 0; i < ToRemoveTDS.InvolvedTriDS.Count; i++)
            {
                ToRemoveTDS.InvolvedTriDS[i].InvolvedTriDS.Remove(ToRemoveTDS);
            }
            SpawnedTriDS.Remove(ToRemoveTDS);
            SpawnedTris.Remove(ToRemove);
            Destroy(ToRemove);
        }
        #endregion

        #region Start/Stop
        void StartEverything()
        {
            Stopwatch StopWatch = new Stopwatch();
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
            Collider.sharedMesh = clonedMesh;
            LocalVertices = new List<Vector3>(clonedMesh.vertices);
            Triangles = new List<int>(clonedMesh.triangles);
            isCloned = true;

            RebuildVisualControls();

            StopWatch.Stop();
            TimeSpan ts = StopWatch.Elapsed;
            UnityEngine.Debug.Log("Related Verts Execution time: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10));
            UnityEngine.Debug.Log("Cloned Mesh");
        }
        private void StopEverything()
        {
            MeshSelection.enabled = false;
            Destroy(vertContainer);
            Destroy(edgeContainer);
            Destroy(triContainer);

            vertContainer = null;
            edgeContainer = null;
            triContainer = null;

            SpawnedVerts.Clear();
            SpawnedEdges.Clear();
            SpawnedTris.Clear();

            SpawnedVertsDS.Clear();
            SpawnedEdgesDS.Clear();
            SpawnedTriDS.Clear();

            EdgePositions.Clear();
            SimplifiedEdges.Clear();
            SimplifiedTriangles.Clear();
            LocalSimplifiedVerts.Clear();

            Triangles.Clear();
            GlobalVertices.Clear();
            LocalVertices.Clear();
            
            isCloned = false;
            clonedMesh = null;
        }

        private void RebuildVisualControls()
        {
            ConvertLocalToGlobal();
            CreateSimplifiedVertices();
            CreateSimpliedTriangles();
            CreateSimpliedEdges();

            SpawnVerticeVisuals();
            SpawnEdgeVisuals();
            SpawnTriangleVisuals();
            ProvideColliders();
            MeshSelection.enabled = true;
        }
        #endregion

        #region Update Mesh
        public void UpdateMeshVertices()
        {
            clonedMesh.vertices = LocalVertices.ToArray();            
            clonedMesh.RecalculateNormals();
        }
        public void UpdateMeshVerticesAndTriangles()
        {
            clonedMesh.vertices = LocalVertices.ToArray();
            clonedMesh.triangles = Triangles.ToArray();
            clonedMesh.RecalculateNormals();
        }
        public void UpdateMeshAndOptimize()
        {
            clonedMesh.vertices = LocalVertices.ToArray();
            clonedMesh.triangles = Triangles.ToArray();
            clonedMesh.RecalculateNormals();
            clonedMesh.Optimize();
            StopEverything();
            StartEverything();
        }

        public void ConvertLocalToGlobal()
        {
            if(GlobalVertices.Count  == 0)
            {
                for (int i = 0; i < LocalVertices.Count; i++)
                {
                    GlobalVertices.Add(this.transform.TransformPoint(LocalVertices[i]));
                }
            }
            else
            {
                for (int i = 0; i < LocalVertices.Count; i++)
                {
                    GlobalVertices[i] = this.transform.TransformPoint(LocalVertices[i]);
                }
            }
        }

        #endregion


        #region Set ALL of 'x' to On or off
        public void SetAllVerticeColsAndVisToOn()
        {
            ThreeWayHandler.ToggleVerticeCollidersAndVisualsOn();
        }
        public void SetAllVerticeColsAndVisToOff()
        {
            ThreeWayHandler.ToggleVerticeCollidersAndVisualsOff();
        }
        public void SetAllEdgeCollidersToOn()
        {
            ThreeWayHandler.ToggleEdgeCollidersOn();
        }
        public void SetAllEdgeCollidersToOff()
        {
            ThreeWayHandler.ToggleEdgeCollidersOff();
        }

        public void SetAllTriCollidersToOn()
        {
            ThreeWayHandler.ToggleTriangleCollidersOn();
        }
        public void SetAllTriCollidersToOff()
        {
            ThreeWayHandler.ToggleTriangleCollidersOff();
        }
        #endregion

        #region Debug Stuff
        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 0, 120, 30), "Start Everything"))
            {
                StartEverything();
            }

            if (GUI.Button(new Rect(10, 35, 120, 30), "Stop Everything"))
            {
                StopEverything();
            }

            if (isCloned)
            {
                if (GUI.Button(new Rect(10, 70, 120, 30), "Optimize Mesh"))
                {
                    UpdateMeshAndOptimize();
                }
                if (GUI.Button(new Rect(10, 105, 120, 30), "Log Relation Lists"))
                {
                    LogRelationLists();
                }
            }
        }

        private void LogRelationLists()
        {
            for (int i = 0; i < SimplifiedEdges.Count; i++)
            {
                for (int j = 0; j < SimplifiedEdges[i].Count; j++)
                {
                    UnityEngine.Debug.Log("Master List: " + i + " Sublist Result: " + SimplifiedEdges[i][j]);
                }
            }
        }
        #endregion
    }
}
