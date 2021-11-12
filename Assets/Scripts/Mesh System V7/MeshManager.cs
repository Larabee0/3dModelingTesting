using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;


namespace MeshSystemV7
{
    [RequireComponent(typeof(MeshSelection), typeof(MeshManipulator), typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public class MeshManager : MonoBehaviour
    {
        [Header("Script References")]
        [SerializeField] private MeshManipulator MeshManipulator;
        [SerializeField] private MeshSelection MeshSelection;

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
        public List<VertDataStore> SpawnedVertsDS = new List<VertDataStore>();
        public List<GameObject> SpawnedEdges = new List<GameObject>();
        public List<EdgeDataStore> SpawnedEdgesDS = new List<EdgeDataStore>();
        public List<GameObject> SpawnedTris = new List<GameObject>();
        public List<TriDataStore> SpawnedTriDS = new List<TriDataStore>();



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
            vertContainer.transform.parent = transform;
            vertContainer.transform.localPosition = Vector3.zero;
            vertContainer.transform.localScale = Vector3.one;

            for (int i = 0; i < LocalSimplifiedVerts.Count; i++)
            {
                GameObject VertexMesh = Instantiate(VertVisualElement, transform.TransformPoint(LocalSimplifiedVerts[i]), Quaternion.identity, vertContainer.transform);
                VertexMesh.name = i.ToString();
                VertDataStore VDStore = VertexMesh.AddComponent<VertDataStore>();
                VDStore.RelatedVerts = RelatedVertices[i];
                VDStore.ConnectedVerts.Add(VDStore);
                VDStore.SimpliedVertIndex = i;
                VDStore.MeshManager = this;

                SpawnedVerts.Add(VertexMesh);
                SpawnedVertsDS.Add(VDStore);
            }
        }

        private void SpawnEdgeVisuals()
        {
            SpawnedEdges.Clear();
            SpawnedEdgesDS.Clear();

            edgeContainer = new GameObject { name = "edgeContainer" };
            edgeContainer.transform.parent = transform;
            edgeContainer.transform.localPosition = Vector3.zero;

            for (int i = 0; i < SimplifiedEdges.Count; i++)
            {
                VertDataStore SpawnedVertsDS1 = SpawnedVertsDS[SimplifiedEdges[i][0]];
                VertDataStore SpawnedVertsDS2 = SpawnedVertsDS[SimplifiedEdges[i][1]];

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
                Quaternion QuaterEuler = Quaternion.Euler(transform.localRotation.eulerAngles);
                EdgeMesh.transform.position = QuaterEuler * EdgeAveragePositon;
                EdgeMesh.transform.LookAt(QuaterEuler * SpawnedVerts1);

                EdgeDataStore EDStore = EdgeMesh.AddComponent<EdgeDataStore>();
                EDStore.InvolvedVerts = new List<List<int>> { RelatedVertices[SpawnedVertsDS1.SimpliedVertIndex], RelatedVertices[SpawnedVertsDS2.SimpliedVertIndex] };

                SpawnedVertsDS[SpawnedVertsDS1.SimpliedVertIndex].InvolvedEdges.Add(EDStore);
                SpawnedVertsDS[SpawnedVertsDS2.SimpliedVertIndex].InvolvedEdges.Add(EDStore);

                EDStore.InvolvedVertObjects.AddRange(new List<VertDataStore> { SpawnedVertsDS1, SpawnedVertsDS2 });

                EDStore.EdgePositionsIndex = i;
                EDStore.MeshManager = this;

                SpawnedEdges.Add(EdgeMesh);
                SpawnedEdgesDS.Add(EDStore);
            }
        }

        private void SpawnTriangleVisuals()
        {
            SpawnedTris.Clear();
            SpawnedTriDS.Clear();

            triContainer = new GameObject { name = "triContainer" };
            triContainer.transform.parent = transform;
            triContainer.transform.localPosition = Vector3.zero;
            triContainer.transform.localScale = Vector3.one;
            int j = 0;
            for (int i = 0; i < SimplifiedTriangles.Count; i += 3)
            {
                GameObject TriangleMesh = Instantiate(TriVisualElement, transform.position, Quaternion.identity, triContainer.transform);
                TriangleMesh.name = j.ToString();

                //List<GameObject> InvolvedEdges = new List<GameObject>();
                List<EdgeDataStore> InvolvedEdgeDataStore = new List<EdgeDataStore>();
                VertDataStore[] VDS = new VertDataStore[]
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

                            if (VDS[k].InvolvedEdges[h] == VDS[secondaryK].InvolvedEdges[l])
                            {
                                //InvolvedEdges.Add(VDS[k].InvolvedEdges[h].gameObject);
                                InvolvedEdgeDataStore.Add(VDS[k].InvolvedEdges[h]);
                                goto Next;
                            }
                        }
                    }
                Next:
                    secondaryK++;
                }

                TriDataStore TriDStore = TriangleMesh.AddComponent<TriDataStore>();

                TriDStore.InvolvedVerts.AddRange(new List<int> { SimplifiedTriangles[i], SimplifiedTriangles[i + 1], SimplifiedTriangles[i + 2] });
                TriDStore.InvolvedVertObjects.AddRange(VDS);
                TriDStore.TriangleArrayInfo.AddRange(new List<int> { Triangles[i], Triangles[i + 1], Triangles[i + 2] });
                //TriDStore.InvolvedEdgeObjects.AddRange(InvolvedEdges);
                TriDStore.InvolvedEdgeDS.AddRange(InvolvedEdgeDataStore);
                TriDStore.TriIndex = j;
                TriDStore.MeshManager = this;

                InvolvedEdgeDataStore.ForEach(k => k.InvolvedTriangles.Add(TriDStore));
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
                HashSet<TriDataStore> InvolvedTrisHash = new HashSet<TriDataStore> { SpawnedTriDS[i] };
                for (int j = 0; j < SpawnedTriDS[i].InvolvedEdgeDS.Count; j++)
                {
                    InvolvedTrisHash.UnionWith(SpawnedTriDS[i].InvolvedEdgeDS[j].InvolvedTriangles);
                }
                foreach (TriDataStore j in InvolvedTrisHash)
                {
                    SpawnedTriDS[i].InvolvedTriDS.Add(j);
                }
            }
        }
        #endregion

        #region Extrude
        public GameObject ExtrudeVertex(GameObject Origin)
        {
            return NewVertex(Origin.GetComponent<VertDataStore>()).gameObject;
        }
        public VertDataStore ExtrudeVertex(VertDataStore Origin)
        {
            return NewVertex(Origin);
        }

        private VertDataStore NewVertex(VertDataStore Origin)
        {
            LocalSimplifiedVerts.Add(Origin.transform.localPosition);
            LocalVertices.Add(Origin.transform.localPosition);
            GlobalVertices.Add(Origin.transform.position);

            GameObject VertexMesh = Instantiate(VertVisualElement, Origin.transform.position, Quaternion.identity, vertContainer.transform);
            VertexMesh.name = SpawnedVerts.Count.ToString();

            VertDataStore VDStore = VertexMesh.AddComponent<VertDataStore>();
            VDStore.RelatedVerts = new List<int> { LocalVertices.Count - 1 };
            VDStore.ConnectedVerts.Add(VDStore);
            VDStore.SimpliedVertIndex = SpawnedVerts.Count;
            VDStore.MeshManager = this;

            SpawnedVerts.Add(VertexMesh);
            SpawnedVertsDS.Add(VDStore);
            return VDStore;
        }

        public GameObject ExtrudeEdge(GameObject Origin)
        {
            EdgeDataStore OriginEDS = Origin.GetComponent<EdgeDataStore>();
            VertDataStore InvolvedVertObject1 = OriginEDS.InvolvedVertObjects[0];
            VertDataStore InvolvedVertObject2 = OriginEDS.InvolvedVertObjects[1];
            VertDataStore VertexA = ExtrudeVertex(InvolvedVertObject1);
            VertDataStore VertexB = ExtrudeVertex(InvolvedVertObject2);

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
            float TotalExtrudeTime = Time.realtimeSinceStartup;
            HashSet<VertDataStore> MasterVertHashSet = new HashSet<VertDataStore>();
            List<VertDataStore> MasterVerts = new List<VertDataStore>();
            List<VertDataStore> DirectVertexEdgeMap = new List<VertDataStore>();
            HashSet<EdgeDataStore> EdgeHasSet = new HashSet<EdgeDataStore>();

            List<TriDataStore> MastersTDS = new List<TriDataStore>();
            for (int i = 0; i < OriginTris.Count; i++)
            {
                MastersTDS.Add(OriginTris[i].GetComponent<TriDataStore>());
                MasterVertHashSet.UnionWith(MastersTDS[i].InvolvedVertObjects);
                MasterVerts.AddRange(MastersTDS[i].InvolvedVertObjects);
                EdgeHasSet.UnionWith(MastersTDS[i].InvolvedEdgeDS);
            }
            List<VertDataStore> UnquieVertOrigin = MasterVertHashSet.ToList();

            List<VertDataStore> NewVerts = new List<VertDataStore>();
            for (int i = 0; i < UnquieVertOrigin.Count; i++)
            {
                NewVerts.Add(ExtrudeVertex(UnquieVertOrigin[i]));
                NewEdge(UnquieVertOrigin[i], NewVerts[i]);
            }

            List<EdgeDataStore> UnquieEdgesObj = EdgeHasSet.ToList();
            UnquieEdgesObj.ForEach(i => DirectVertexEdgeMap.AddRange(i.InvolvedVertObjects));
            for (int i = 0; i < DirectVertexEdgeMap.Count; i += 2)
            {
                NewEdge(NewVerts[UnquieVertOrigin.IndexOf(DirectVertexEdgeMap[i])], NewVerts[UnquieVertOrigin.IndexOf(DirectVertexEdgeMap[i + 1])]);
            }

            List<EdgeDataStore> MatchingEdges = new List<EdgeDataStore>();

            for (int i = 0; i < MastersTDS.Count; i++)
            {
                for (int k = 0; i < MastersTDS.Count; k++)
                {
                    if(k == i)
                    {
                        k++;
                    }
                    if (k >= MastersTDS.Count)
                    {
                        break;
                    }
                    else
                    {
                        for (int j = 0; j < MastersTDS[i].InvolvedEdgeDS.Count; j++)
                        {
                            for (int ii = 0; ii < MastersTDS[k].InvolvedEdgeDS.Count; ii++)
                            {
                                if (MastersTDS[i].InvolvedEdgeDS[j] == MastersTDS[k].InvolvedEdgeDS[ii])
                                {
                                    MatchingEdges.Add(MastersTDS[i].InvolvedEdgeDS[j]);
                                }
                            }
                        }
                    }
                }
            }
            
            List<List<VertDataStore>> ExcudeEdgesBetween = new List<List<VertDataStore>>();
            for (int i = 0; i < MatchingEdges.Count; i++)
            {
                ExcudeEdgesBetween.AddRange(new List<List<VertDataStore>> { new List<VertDataStore> { MatchingEdges[i].InvolvedVertObjects[0], MatchingEdges[i].InvolvedVertObjects[1] }, new List<VertDataStore> { MatchingEdges[i].InvolvedVertObjects[1], MatchingEdges[i].InvolvedVertObjects[0] } });
            }
            List<List<VertDataStore>> TenativeFoldOuts = new List<List<VertDataStore>>();
            if(MasterVerts.Count % 3 == 0)
            {
                for (int i = 0; i < MasterVerts.Count; i += 3)
                {
                    TenativeFoldOuts.Add(new List<VertDataStore> { MasterVerts[i], MasterVerts[i + 1] });
                    TenativeFoldOuts.Add(new List<VertDataStore> { MasterVerts[i + 1], MasterVerts[i + 2] });
                    TenativeFoldOuts.Add(new List<VertDataStore> { MasterVerts[i + 2], MasterVerts[i] });
                }
            }
            else
            {
                for (int i = 0; i < MasterVerts.Count; i += 2)
                {
                    TenativeFoldOuts.Add(new List<VertDataStore> { MasterVerts[i], MasterVerts[i + 1] });
                }
            }
            for (int i = 0; i < TenativeFoldOuts.Count; i++)
            {
                for (int k = 0; k < ExcudeEdgesBetween.Count; k++)
                {
                    if (ExcudeEdgesBetween[k].Contains(TenativeFoldOuts[i][0]) && ExcudeEdgesBetween[k].Contains(TenativeFoldOuts[i][1]))
                    {
                        TenativeFoldOuts.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
            
            List<VertDataStore> newVDS = new List<VertDataStore>();
            NewVerts.ForEach(i => newVDS.Add(i.GetComponent<VertDataStore>()));
            List<VertDataStore[]> VertRemap = new List<VertDataStore[]>();
            for (int i = 0; i < TenativeFoldOuts.Count; i++)
            {
                VertRemap.Add(new VertDataStore[2]);
                for (int k = 0; k < newVDS.Count; k++)
                {
                    if (newVDS[k].ConnectedVerts.Contains(TenativeFoldOuts[i][0]))
                    {
                        VertRemap[i][0] = newVDS[k];
                    }
                    else if (newVDS[k].ConnectedVerts.Contains(TenativeFoldOuts[i][1]))
                    {
                        VertRemap[i][1] = newVDS[k];
                    }
                }
            }

            List<VertDataStore> QuadMaps = new List<VertDataStore>();
            for (int i = 0; i < TenativeFoldOuts.Count; i++)
            {
                QuadMaps.AddRange(new List<VertDataStore> 
                { 
                    VertRemap[i][0], 
                    TenativeFoldOuts[i][0], 
                    TenativeFoldOuts[i][1], 

                    VertRemap[i][0], 
                    VertRemap[i][1], 
                    TenativeFoldOuts[i][1]
                });
                NewEdge(VertRemap[i][0], TenativeFoldOuts[i][1]);
            }


            for (int i = 0; i < QuadMaps.Count; i+=3)
            {
                NewTriangle(QuadMaps[i], QuadMaps[i + 1], QuadMaps[i + 2]);
            }
            List<GameObject> NewTris = new List<GameObject>();
            for (int i = 0; i < MasterVerts.Count; i += 3)
            {
                NewTris.Add(NewTriangle(NewVerts[UnquieVertOrigin.IndexOf(MasterVerts[i])], NewVerts[UnquieVertOrigin.IndexOf(MasterVerts[i + 1])], NewVerts[UnquieVertOrigin.IndexOf(MasterVerts[i + 2])]).gameObject);
            }
            OriginTris.ForEach(i => RemoveTriangle(i));

            print("Time to extrude: " + (Time.realtimeSinceStartup - TotalExtrudeTime) * 1000f + "ms");
            return NewTris;
        }

        public List<VertDataStore> CalculatePossibleTriangles(List<VertDataStore> Input)
        {
            List<VertDataStore> PossibleTriangles = new List<VertDataStore>();
            HashSet<VertDataStore> AllInvolvedVDSHash = new HashSet<VertDataStore>();
            List<int> TemporyTriangles = new List<int>(SimplifiedTriangles);
            List<List<int>> TemporyTrianglesIDs = new List<List<int>>();
            for (int k = 0; k < TemporyTriangles.Count; k += 3)
            {
                TemporyTrianglesIDs.Add(new List<int> { TemporyTriangles[k], TemporyTriangles[k + 1], TemporyTriangles[k + 2] });
            }
            Input.ForEach(i => AllInvolvedVDSHash.UnionWith(i.ConnectedVerts));
            List<VertDataStore> AllInvolvedVDS = AllInvolvedVDSHash.ToList();
            for (int i = 0; i < Input.Count; i++)
            {
                VertDataStore PrimaryVert = Input[i];
                List<VertDataStore> PrimaryConnectedVerts = new List<VertDataStore>(PrimaryVert.ConnectedVerts);
                for (int j = 1; j < PrimaryConnectedVerts.Count; j++)
                {
                    VertDataStore SecondaryVert = PrimaryConnectedVerts[j];
                    List<VertDataStore> SecondaryConnectedVerts = new List<VertDataStore>(SecondaryVert.ConnectedVerts);
                    for (int k = 2; k < PrimaryConnectedVerts.Count; k++)
                    {
                        if (SecondaryConnectedVerts.Contains(PrimaryConnectedVerts[k]) && SecondaryVert != PrimaryConnectedVerts[k])
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
                                PossibleTriangles.AddRange(new List<VertDataStore> { PrimaryVert, SecondaryVert, PrimaryConnectedVerts[k] });
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

        #endregion

        #region NewEdge and Triangle
        
        public GameObject NewEdge(GameObject origin, GameObject newVert)
        {
            return SpawnEdge(origin.GetComponent<VertDataStore>(), newVert.GetComponent<VertDataStore>()).gameObject;
        }
        public EdgeDataStore NewEdge(VertDataStore originVDS, VertDataStore newVertVDS)
        {
            return SpawnEdge(originVDS, newVertVDS);
        }

        private EdgeDataStore SpawnEdge(VertDataStore originVDS, VertDataStore newVertVDS)
        {

            Vector3 originPos = originVDS.transform.position;
            Vector3 newVertPos = newVertVDS.transform.position;
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


            EdgeDataStore EDStore = EdgeMesh.AddComponent<EdgeDataStore>();
            float EdgeDistance = Vector3.Distance(originPos, newVertPos);

            EdgeMesh.name = SpawnedEdges.Count.ToString();
            EdgeMesh.transform.localScale = new Vector3(0.025f, 0.025f, EdgeDistance);
            Quaternion QuaterEuler = Quaternion.Euler(transform.localRotation.eulerAngles);
            EdgeMesh.transform.position = QuaterEuler * EdgeAveragePositon;
            EdgeMesh.transform.LookAt(QuaterEuler * originPos);

            EDStore.InvolvedVerts = new List<List<int>> { originVDS.RelatedVerts, newVertVDS.RelatedVerts };

            originVDS.InvolvedEdges.Add(EDStore);
            newVertVDS.InvolvedEdges.Add(EDStore);

            EDStore.InvolvedVertObjects.AddRange(new List<VertDataStore> { originVDS, newVertVDS });
            EDStore.EdgePositionsIndex = SpawnedEdges.Count;
            EDStore.MeshManager = this;
            SpawnedEdges.Add(EdgeMesh);
            SpawnedEdgesDS.Add(EDStore);
            return EDStore;
        }


        public GameObject NewTriangle(GameObject V1, GameObject V2, GameObject V3)
        {
            return NewTri(new VertDataStore[] { V1.GetComponent<VertDataStore>(), V2.GetComponent<VertDataStore>(), V3.GetComponent<VertDataStore>() }).gameObject;
        }

        public TriDataStore NewTriangle(VertDataStore V1, VertDataStore V2, VertDataStore V3)
        {
            return NewTri(new VertDataStore[] {V1,V2,V3 });
        }

        private TriDataStore NewTri(VertDataStore[] VertDS)
        {
            Vector3[] VertLocalPositions = new Vector3[] { VertDS[0].transform.localPosition, VertDS[1].transform.localPosition, VertDS[2].transform.localPosition };
            SimplifiedTriangles.AddRange(new List<int> { VertDS[0].SimpliedVertIndex, VertDS[1].SimpliedVertIndex, VertDS[2].SimpliedVertIndex });
            // idk if transform point is actually required here, need to try not having it.
            Vector3[] VertGlobalPositions = new Vector3[] { transform.TransformPoint(VertDS[0].transform.position), transform.TransformPoint(VertDS[1].transform.position), transform.TransformPoint(VertDS[2].transform.position) };

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

            GameObject TriangleMesh = Instantiate(TriVisualElement, transform.position, Quaternion.identity, triContainer.transform);
            TriangleMesh.name = SpawnedTris.Count.ToString();

            List<int> Edges = new List<int>();
            //List<GameObject> InvolvedEdges = new List<GameObject>();
            List<EdgeDataStore> InvolvedEdgesDS = new List<EdgeDataStore>();

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

                        if (VertDS[k].InvolvedEdges[i] == VertDS[secondaryK].InvolvedEdges[j])
                        {
                            Edges.Add(VertDS[k].InvolvedEdges[i].EdgePositionsIndex);
                            //InvolvedEdges.Add(VertDS[k].InvolvedEdges[i].gameObject);
                            InvolvedEdgesDS.Add(VertDS[k].InvolvedEdges[i]);
                        }
                    }
                }
                secondaryK++;
            }


            TriDataStore TriDStore = TriangleMesh.AddComponent<TriDataStore>();
            TriDStore.InvolvedVerts.AddRange(new List<int> { VertDS[0].SimpliedVertIndex, VertDS[1].SimpliedVertIndex, VertDS[2].SimpliedVertIndex });
            TriDStore.InvolvedVertObjects.AddRange(new List<VertDataStore>(VertDS));
            //TriDStore.InvolvedEdgeObjects.AddRange(InvolvedEdges);

            TriDStore.TriangleArrayInfo.AddRange(NewTriBuilder);
            TriDStore.InvolvedEdgeDS.AddRange(InvolvedEdgesDS);
            TriDStore.TriIndex = SpawnedTris.Count;
            TriDStore.MeshManager = this;
            TriDStore.Position = (VertLocalPositions[0] + VertLocalPositions[1] + VertLocalPositions[2]) / 3;
            Edges.ForEach(i => SpawnedEdgesDS[i].InvolvedTriangles.Add(TriDStore));

            SpawnedTris.Add(TriangleMesh);
            SpawnedTriDS.Add(TriDStore);
            return TriDStore;
        }
        #endregion

        #region Remove
        public void RemoveVertice(GameObject ToRemove)
        {
            VertDataStore ToRemoveVDS = ToRemove.GetComponent<VertDataStore>();
            List<TriDataStore> InvolvedTriangles = new List<TriDataStore>(ToRemoveVDS.InvolvedTriangles);
            for (int i = 0; i < InvolvedTriangles.Count; i++)
            {
                RemoveTriangle(InvolvedTriangles[i]);
            }
        }

        public void RemoveEdge(GameObject ToRemove)
        {
            EdgeDataStore ToRemoveEDS = ToRemove.GetComponent<EdgeDataStore>();
            List<TriDataStore> InvolvedTriangles = new List<TriDataStore>(ToRemoveEDS.InvolvedTriangles);
            for (int i = 0; i < InvolvedTriangles.Count; i++)
            {
                RemoveTriangle(InvolvedTriangles[i]);
            }
        }

        public void RemoveTriangle(TriDataStore ToRemove)
        {
            DeleteTri(ToRemove);
        }
        public void RemoveTriangle(GameObject ToRemove)
        {
            DeleteTri(ToRemove.GetComponent<TriDataStore>());
        }
        private void DeleteTri(TriDataStore ToRemoveTDS)
        {
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
                ToRemoveTDS.InvolvedEdgeDS[i].InvolvedTriangles.Remove(ToRemoveTDS);
                if (ToRemoveTDS.InvolvedEdgeDS[i].InvolvedTriangles.Count == 0)
                {
                    for (int j = 0; j < ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects.Count; j++)
                    {
                        ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects[j].InvolvedEdges.Remove(ToRemoveTDS.InvolvedEdgeDS[i]);
                        for (int k = 0; k < ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects.Count; k++)
                        {
                            if (j == k)
                            {
                                k++;
                            }
                            if (k == ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects.Count)
                            {
                                break;
                            }
                            ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects[j].ConnectedVerts.Remove(ToRemoveTDS.InvolvedEdgeDS[i].InvolvedVertObjects[k]);
                        }
                    }
                    Destroy(ToRemoveTDS.InvolvedEdgeDS[i].gameObject);
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
            SpawnedTris.Remove(ToRemoveTDS.gameObject);
            Destroy(ToRemoveTDS.gameObject);
        }

        #endregion

        #region Start/Stop
        void StartEverything()
        {
            float startTime = Time.realtimeSinceStartup;
            originalMesh = meshFilter.sharedMesh;

            clonedMesh = new Mesh
            {
                name = name + "clone",
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

            print("Start up Time: "+(Time.realtimeSinceStartup - startTime) * 1000f + "ms");

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
            if (GlobalVertices.Count == 0)
            {
                for (int i = 0; i < LocalVertices.Count; i++)
                {
                    GlobalVertices.Add(transform.TransformPoint(LocalVertices[i]));
                }
            }
            else
            {
                for (int i = 0; i < LocalVertices.Count; i++)
                {
                    GlobalVertices[i] = transform.TransformPoint(LocalVertices[i]);
                }
            }
        }

        #endregion

        #region Collider and Visual Togglers
        public void ToggleVerticeColliders()
        {
            SpawnedVertsDS.ForEach(i => i.ToggleCollider());
        }
        public void ToggleVerticeVisuals()
        {
            SpawnedVertsDS.ForEach(i => i.ToggleVisability());
        }
        public void ToggleEdgeColliders()
        {
            SpawnedEdgesDS.ForEach(i => i.ToggleCollider());
        }
        public void ToggleEdgeVisuals()
        {
            SpawnedEdgesDS.ForEach(i => i.ToggleVisability());
        }
        public void ToggleTriColliders()
        {
            SpawnedTriDS.ForEach(i => i.ToggleCollider());
        }
        #endregion

        #region Set ALL of 'x' to On or off
        public void SetAllVerticeVisualsToOn()
        {
            SpawnedVerts.ForEach(i => i.GetComponent<MeshRenderer>().enabled = true);
        }
        public void SetAllVerticeCollidersToOn()
        {
            SpawnedVerts.ForEach(i => i.GetComponent<BoxCollider>().enabled = true);
        }
        public void SetAllVerticeColsAndVisToOn()
        {
            for (int i = 0; i < SpawnedVerts.Count; i++)
            {
                SpawnedVerts[i].GetComponent<MeshRenderer>().enabled = true;
                SpawnedVerts[i].GetComponent<BoxCollider>().enabled = true;
            }
        }
        public void SetAllVerticeColsAndVisToOff()
        {
            for (int i = 0; i < SpawnedVerts.Count; i++)
            {
                SpawnedVerts[i].GetComponent<MeshRenderer>().enabled = false;
                SpawnedVerts[i].GetComponent<BoxCollider>().enabled = false;
            }
        }

        public void SetAllEdgeVisualsToOn()
        {
            SpawnedEdges.ForEach(i => i.GetComponent<MeshRenderer>().enabled = true);
        }
        public void SetAllEdgeCollidersToOn()
        {
            SpawnedEdges.ForEach(i => i.GetComponent<BoxCollider>().enabled = true);
        }

        public void SetAllTriCollidersToOn()
        {
            SpawnedTris.ForEach(i => i.GetComponent<MeshCollider>().enabled = true);
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
