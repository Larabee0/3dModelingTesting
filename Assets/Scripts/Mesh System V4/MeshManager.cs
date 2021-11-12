using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV4
{
    [RequireComponent(typeof(MeshSelection), typeof(MeshManipulator),typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public class MeshManager : MonoBehaviour
    {
        [Header("Script References")]
        [SerializeField] private MeshManipulator MeshManipulator;
        [SerializeField] private MeshSelection MeshSelection;



        [Header("Mesh stuff")]
        public MeshFilter meshFilter;
        private Mesh originalMesh;
        public Mesh clonedMesh;
        public bool isCloned = false;
        public MeshCollider Collider;

        [Header("Visual Objects")]
        [SerializeField] private GameObject VertVisualElement;
        [SerializeField] private GameObject EdgeVisualElement;
        [SerializeField] private GameObject TriVisualElement;

        private GameObject vertContainer;
        private GameObject edgeContainer;
        private GameObject triContainer;

        [Header("Raw Verts & Tris")]
        public Vector3[] verticesArray;
        public int[] trianglesArray;

        [Header("Vertice Index Lookup Lists")]
        // Contains a list of all vertices that occupy the same position as SimplifiedVerts[i], ordered the same as SimplifiedVerts.
        public List<List<int>> RelatedVertices = new List<List<int>>(); // Always useful.

        // Contains a list of position of al unquie vertices in verticesArray. For a cube that is 8, instead of 24 like verticesArray contains.
        public List<Vector3> SimplifiedVerts = new List<Vector3>(); // Probably only useful on start up.

        // Contains the full list of triangles (trianglesArray) using the simplied vertices instead of the full verticesArray.
        public List<int> SimplifiedTriangles = new List<int>(); // Probably only useful on start up.

        // Contains the list of edges with no duplicate edges using SimplifiedVerts instead of the full verticesArray.
        public List<List<int>> SimplifiedEdges = new List<List<int>>(); // Probably only useful on start up.

        [Header("Spawned Visuals & Data Components")]
        public List<GameObject> SpawnedVerts = new List<GameObject>();
        public List<VertDataStore> SpawnedVertsDS = new List<VertDataStore>();
        public List<GameObject> SpawnedEdges = new List<GameObject>();
        public List<EdgeDataStore> SpawnedEdgesDS = new List<EdgeDataStore>();
        public List<GameObject> SpawnedTris = new List<GameObject>();
        public List<TriDataStore> SpawnedTriDS = new List<TriDataStore>();

        public List<Vector3> EdgePositions = new List<Vector3>();

        // Start is called before the first frame update
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
            verticesArray = clonedMesh.vertices;
            trianglesArray = clonedMesh.triangles;
            isCloned = true;
            RebuildVisualControls();

            StopWatch.Stop();
            TimeSpan ts = StopWatch.Elapsed;
            UnityEngine.Debug.Log("Related Verts Execution time: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10));
            UnityEngine.Debug.Log("Cloned Mesh");
        }

        private void RebuildVisualControls()
        {
            CreateSimplifiedVertices();
            CreateSimpliedTriangles();
            CreateSimpliedEdges();

            SpawnVerticeVisuals();
            SpawnEdgeVisuals();
            SpawnTriangleVisuals();
            MeshSelection.enabled = true;
            //LogRelationLists();
        }

        private void CreateSimplifiedVertices()
        {
            SimplifiedVerts.Clear();
            RelatedVertices.Clear();

            // Builds list of simplified vertices
            for (int i = 0; i < verticesArray.Length; i++)
            {
                if (!SimplifiedVerts.Contains(verticesArray[i]))
                {
                    SimplifiedVerts.Add(verticesArray[i]);
                }
            }

            // Builds lists of related vertices.
            for (int i = 0; i < SimplifiedVerts.Count; i++)
            {
                List<int> Temp = new List<int>();
                for (int j = 0; j < verticesArray.Length; j++)
                {
                    if (SimplifiedVerts[i] == verticesArray[j])
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

            for (int i = 0; i < trianglesArray.Length; i++)
            {
                for (int j = 0; j < SimplifiedVerts.Count; j++)
                {
                    if (SimplifiedVerts[j] == verticesArray[trianglesArray[i]])
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

            //List<Vector3> EdgeRecord = new List<Vector3>();

            for (int i = 0; i < SimplifiedTriangles.Count; i += 3)
            {

                List<int> Temp = new List<int> { SimplifiedTriangles[i], SimplifiedTriangles[i + 1] };
                Vector3 TempV3 = (SimplifiedVerts[Temp[0]] + SimplifiedVerts[Temp[1]]) / 2;
                if (!EdgePositions.Contains(TempV3))
                {
                    EdgePositions.Add(TempV3);
                    SimplifiedEdges.Add(Temp);
                }


                Temp = new List<int> { SimplifiedTriangles[i + 1], SimplifiedTriangles[i + 2] };
                TempV3 = (SimplifiedVerts[Temp[0]] + SimplifiedVerts[Temp[1]]) / 2;
                if (!EdgePositions.Contains(TempV3))
                {
                    EdgePositions.Add(TempV3);
                    SimplifiedEdges.Add(Temp);
                    
                }

                Temp = new List<int> { SimplifiedTriangles[i + 2], SimplifiedTriangles[i] };
                TempV3 = (SimplifiedVerts[Temp[0]] + SimplifiedVerts[Temp[1]]) / 2;
                if (!EdgePositions.Contains(TempV3))
                {
                    EdgePositions.Add(TempV3);
                    SimplifiedEdges.Add(Temp);
                }
            }
        }


        private void SpawnVerticeVisuals()
        {
            SpawnedVerts.Clear();
            SpawnedVertsDS.Clear();

            vertContainer = (GameObject)Instantiate(new GameObject { name = "vertContainer" }, this.transform);

            for (int i = 0; i < SimplifiedVerts.Count; i++)
            {
                GameObject VertexMesh = (GameObject)Instantiate(VertVisualElement, SimplifiedVerts[i], Quaternion.identity, vertContainer.transform);
                VertexMesh.name = i.ToString();

                VertDataStore VDStore = VertexMesh.AddComponent<VertDataStore>();
                VDStore.RelatedVerts = RelatedVertices[i];
                VDStore.MMV5 = this;

                SpawnedVerts.Add(VertexMesh);
                SpawnedVertsDS.Add(VDStore);
            }
        }

        private void SpawnEdgeVisuals()
        {
            SpawnedEdges.Clear();
            SpawnedEdgesDS.Clear();

            edgeContainer = (GameObject)Instantiate(new GameObject { name = "edgeContainer" }, this.transform);

            for (int i = 0; i < SimplifiedEdges.Count; i++)
            {
                Vector3 EdgeAveragePositon = ((SimplifiedVerts[SimplifiedEdges[i][0]] + SimplifiedVerts[SimplifiedEdges[i][1]]) / 2) + this.transform.position;

                GameObject EdgeMesh = (GameObject)Instantiate(EdgeVisualElement, EdgeAveragePositon, Quaternion.identity, edgeContainer.transform);

                float EdgeDistance = Vector3.Distance(SimplifiedVerts[SimplifiedEdges[i][0]], SimplifiedVerts[SimplifiedEdges[i][1]]);

                EdgeMesh.name = i.ToString();
                EdgeMesh.transform.localScale = new Vector3(0.025f, 0.025f, EdgeDistance);
                EdgeMesh.transform.position = Quaternion.Euler(this.transform.localRotation.eulerAngles) * EdgeAveragePositon;
                EdgeMesh.transform.LookAt(Quaternion.Euler(this.transform.localRotation.eulerAngles) * SimplifiedVerts[SimplifiedEdges[i][0]]);

                EdgeDataStore EDStore = EdgeMesh.AddComponent<EdgeDataStore>();
                EDStore.InvolvedVerts.AddRange(new List<List<int>> { RelatedVertices[SimplifiedEdges[i][0]], RelatedVertices[SimplifiedEdges[i][1]] });

                VertDataStore[] TempVDS = new VertDataStore[] { SpawnedVertsDS[SimplifiedEdges[i][0]], SpawnedVertsDS[SimplifiedEdges[i][1]] };
                TempVDS[0].InvolvedEdges.Add(EDStore);
                TempVDS[1].InvolvedEdges.Add(EDStore);

                EDStore.InvolvedVertObjects.AddRange(TempVDS);
                EDStore.MMV5 = this;

                SpawnedEdges.Add(EdgeMesh);
                SpawnedEdgesDS.Add(EDStore);
            }
        }

        private void SpawnTriangleVisuals()
        {
            SpawnedTris.Clear();
            SpawnedTriDS.Clear();

            triContainer = (GameObject)Instantiate(new GameObject { name = "triContainer" }, this.transform);

            int j = 0;
            for (int i = 0; i < SimplifiedTriangles.Count; i += 3)
            {
                GameObject TriangleMesh = (GameObject)Instantiate(TriVisualElement, this.transform.position, Quaternion.identity, triContainer.transform);
                TriangleMesh.name = j.ToString();

                Vector3 EdgeAveragePositon1 = ((SimplifiedVerts[SimplifiedTriangles[i]] + SimplifiedVerts[SimplifiedTriangles[i + 1]]) / 2) + this.transform.position;
                Vector3 EdgeAveragePositon2 = ((SimplifiedVerts[SimplifiedTriangles[i + 1]] + SimplifiedVerts[SimplifiedTriangles[i + 2]]) / 2) + this.transform.position;
                Vector3 EdgeAveragePositon3 = ((SimplifiedVerts[SimplifiedTriangles[i + 2]] + SimplifiedVerts[SimplifiedTriangles[i]]) / 2) + this.transform.position;
                List<GameObject> InvolvedEdges = new List<GameObject>();
                for (int k = 0; k < EdgePositions.Count; k++)
                {
                    if(EdgePositions[k] == EdgeAveragePositon1)
                    {
                        InvolvedEdges.Add(SpawnedEdges[k]);
                    }
                    if (EdgePositions[k] == EdgeAveragePositon2)
                    {
                        InvolvedEdges.Add(SpawnedEdges[k]);
                    }
                    if (EdgePositions[k] == EdgeAveragePositon3)
                    {
                        InvolvedEdges.Add(SpawnedEdges[k]);
                    }
                }


                TriDataStore TriDStore = TriangleMesh.AddComponent<TriDataStore>();
                TriDStore.InvolvedVerts.AddRange(new List<int> { SimplifiedTriangles[i], SimplifiedTriangles[i + 1], SimplifiedTriangles[i + 2] });
                TriDStore.InvolvedEdgeObjects.AddRange(InvolvedEdges);
                TriDStore.MMV5 = this;
                

                SpawnedTris.Add(TriangleMesh);
                SpawnedTriDS.Add(TriDStore);

                j++;
            }
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
            EdgePositions.Clear();
            SpawnedTris.Clear();
            SpawnedVertsDS.Clear();
            SpawnedEdgesDS.Clear();
            SpawnedTriDS.Clear();
            verticesArray = null;
            trianglesArray = null;
            isCloned = false;
            clonedMesh = null;
        }

        private void LogRelationLists()
        {
            for (int i = 0; i < SimplifiedEdges.Count; i++)
            {
                for (int j = 0; j < SimplifiedEdges[i].Count; j++)
                {
                    UnityEngine.Debug.Log("Master List: " + i + " Sublist Result: " + SimplifiedEdges[i][j]);// + " Position: " + verticesArray[RelatedVertices[i][j]]);
                }
            }
        }

        public void UpdateMesh()
        {
            clonedMesh.vertices = verticesArray;
            clonedMesh.RecalculateNormals();
        }


        public void ToggleVerticeColliders()
        {
            for (int i = 0; i < SpawnedVertsDS.Count; i++)
            {
                SpawnedVertsDS[i].ToggleCollider();
            }
        }
        public void ToggleVerticeVisuals()
        {

            for (int i = 0; i < SpawnedVertsDS.Count; i++)
            {
                SpawnedVertsDS[i].ToggleVisability();
            }
        }
        public void ToggleEdgeColliders()
        {

            for (int i = 0; i < SpawnedEdgesDS.Count; i++)
            {
                SpawnedEdgesDS[i].ToggleCollider();
            }
        }
        public void ToggleEdgeVisuals()
        {

            for (int i = 0; i < SpawnedVertsDS.Count; i++)
            {
                SpawnedEdgesDS[i].ToggleVisability();
            }
        }

        public void ToggleTriColliders()
        {

            for (int i = 0; i < SpawnedTriDS.Count; i++)
            {
                SpawnedTriDS[i].ToggleCollider();
            }
        }

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
        }
    }
}
