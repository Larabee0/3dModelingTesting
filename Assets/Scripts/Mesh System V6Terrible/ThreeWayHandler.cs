using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV6 {
    public class ThreeWayHandler : MonoBehaviour
    {
        [SerializeField] private MeshManager MeshManager;
        [SerializeField] private MeshManipulator MeshManipulator;
        [SerializeField] private MeshSelection MeshSelection;

        public Dictionary<int, VertDataStruct> VDSs = new Dictionary<int, VertDataStruct>();
        public Dictionary<int, GameObject> VDOs = new Dictionary<int, GameObject>();
        public Dictionary<GameObject, VertDataStruct> GOTOVDSs = new Dictionary<GameObject, VertDataStruct>();

        public Dictionary<int, EdgeDataStruct> EDSs = new Dictionary<int, EdgeDataStruct>();
        public Dictionary<int, GameObject> EDOs = new Dictionary<int, GameObject>();
        public Dictionary<GameObject, EdgeDataStruct> GOTOEDSs = new Dictionary<GameObject, EdgeDataStruct>();

        public Dictionary<int, TriDataStruct> TDSs = new Dictionary<int, TriDataStruct>();
        public Dictionary<int, GameObject> TDOs = new Dictionary<int, GameObject>();
        public Dictionary<GameObject, TriDataStruct> GOTOTDSs = new Dictionary<GameObject, TriDataStruct>();


        public List<BoxCollider> VerticeColliders = new List<BoxCollider>();
        public List<MeshRenderer> VerticeVisuals = new List<MeshRenderer>();

        public List<BoxCollider> EdgeColliders = new List<BoxCollider>();
        public List<MeshRenderer> EdgeVisuals = new List<MeshRenderer>();

        public List<MeshCollider> TriangleColliders = new List<MeshCollider>();

        public Vector3 VerticeScaleFactor = Vector3.one;
        public float EdgeScaleFactor = 0.025f;
        public void UpdateVerticeVisuals(int Vertex)
        {
            VDOs[Vertex].transform.localPosition = MeshManager.LocalVertices[VDSs[Vertex].RelatedVerts[0]];
        }
        public void UpdateVerticeScale(Vector3 NewScale)
        {
            VerticeScaleFactor = NewScale;
            for (int i = 0; i < MeshManager.LocalSimplifiedVerts.Count; i++)
            {
                VDOs[i].transform.localScale = VerticeScaleFactor;
            }
        }

        public void UpdateEdgeVisuals(int Edge)
        {

            Vector3 A = MeshManager.GlobalVertices[EDSs[Edge].InvolvedVerts[0][0]];
            Vector3 B = MeshManager.GlobalVertices[EDSs[Edge].InvolvedVerts[1][0]];
            Vector3 EdgeAveragePositon = ((A + B) / 2);

            float EdgeDistance = Vector3.Distance(A, B);

            VDOs[Edge].transform.localScale = new Vector3(EdgeScaleFactor, EdgeScaleFactor, EdgeDistance);
            VDOs[Edge].transform.position = Quaternion.Euler(MeshManager.transform.localRotation.eulerAngles) * EdgeAveragePositon;
            VDOs[Edge].transform.LookAt(Quaternion.Euler(MeshManager.transform.localRotation.eulerAngles) * A);
        }

        public void UpdateEdgeScale(float NewScale)
        {
            EdgeScaleFactor = NewScale;
            for (int i = 0; i < MeshManager.SpawnedEdgesDS.Count; i++)
            {
                UpdateEdgeVisuals(i);
            }
        }

        public void UpdateTriangleCollider(int Triangle)
        {
            TriDataStruct TempTDS = TDSs[Triangle];
            TempTDS.localVerticesArray[0] = TempTDS.InvolvedVertObjects[0].transform.localPosition;
            TempTDS.localVerticesArray[1] = TempTDS.InvolvedVertObjects[1].transform.localPosition;
            TempTDS.localVerticesArray[2] = TempTDS.InvolvedVertObjects[2].transform.localPosition;

            TempTDS.Position = (TempTDS.localVerticesArray[0] + TempTDS.localVerticesArray[1] + TempTDS.localVerticesArray[2]) / 3;
            if (TDSs[Triangle].ThisMesh == null)
            {
                if (Vector3.Distance(TempTDS.localVerticesArray[0], TempTDS.localVerticesArray[1]) > 0f && Vector3.Distance(TempTDS.localVerticesArray[1], TempTDS.localVerticesArray[2]) > 0f && Vector3.Distance(TempTDS.localVerticesArray[2], TempTDS.localVerticesArray[0]) > 0f)
                {
                    NewTriMesh(TempTDS);
                    return;
                }
            }

            if (Vector3.Distance(TempTDS.localVerticesArray[0], TempTDS.localVerticesArray[1]) > 0f && Vector3.Distance(TempTDS.localVerticesArray[1], TempTDS.localVerticesArray[2]) > 0f && Vector3.Distance(TempTDS.localVerticesArray[2], TempTDS.localVerticesArray[0]) > 0f)
            {
                TempTDS.ThisMesh.vertices = TempTDS.localVerticesArray;
                TempTDS.ThisMeshCollider.sharedMesh = TempTDS.ThisMesh;
            }
            else
            {
                TempTDS.ThisMeshCollider.sharedMesh = null;
            }
        }

        public void TrianglePreStarter(TriDataStruct Triangle)
        {
            for (int i = 0; i < Triangle.InvolvedVerts.Count; i++)
            {
                Triangle.InvolvedVertObjects[i].InvolvedTriangles.Add(Triangle);
            }
            StartTriangle(Triangle);
        }

        public void StartTriangle(TriDataStruct Triangle)
        {
            Vector3[] localVerticesArray = Triangle.localVerticesArray = new Vector3[]
            {
                Triangle.InvolvedVertObjects[0].transform.localPosition,
                Triangle.InvolvedVertObjects[1].transform.localPosition,
                Triangle.InvolvedVertObjects[2].transform.localPosition
            };

            if (Vector3.Distance(localVerticesArray[0], localVerticesArray[1]) > 0f && Vector3.Distance(localVerticesArray[1], localVerticesArray[2]) > 0f && Vector3.Distance(localVerticesArray[2], localVerticesArray[0]) > 0f)
            {
                NewTriMesh(Triangle);
            }
        }

        void NewTriMesh(TriDataStruct Triangle)
        {
            Triangle.localTrianglesArray = new int[] { 0, 1, 2, };

            Triangle.Position = (Triangle.localVerticesArray[0] + Triangle.localVerticesArray[1] + Triangle.localVerticesArray[2]) / 3;

            Triangle.ThisMesh = new Mesh
            {
                name = this.name,
                vertices = Triangle.localVerticesArray,
                triangles = Triangle.localTrianglesArray
            };
            Triangle.ThisMeshCollider.sharedMesh = Triangle.ThisMesh;
        }

        public void DoVertexAction(GameObject Vertex, Vector3 Offset)
        {
            VertDataStruct VDSsTemp = GOTOVDSs[Vertex];
            for (int i = 0; i < VDSsTemp.RelatedVerts.Count; i++)
            {
                MeshManager.GlobalVertices[VDSsTemp.RelatedVerts[i]] = MeshManager.transform.TransformPoint(MeshManager.LocalVertices[VDSsTemp.RelatedVerts[i]] += Offset);
            }

            MeshManager.LocalSimplifiedVerts[VDSsTemp.SimpliedVertIndex] += Offset;

            if (VDSsTemp.InvolvedEdges.Count > 0)
            {
                VDSsTemp.InvolvedEdges.ForEach(i => UpdateEdgeVisuals(i.EdgePositionsIndex));
            }


            if (VDSsTemp.InvolvedTriangles.Count > 0)
            {
                VDSsTemp.InvolvedTriangles.ForEach(i => UpdateTriangleCollider(i.TriIndex));
            }

            UpdateVerticeVisuals(VDSsTemp.SimpliedVertIndex);
        }

        public void ToggleVerticeCollidersAndVisualsOff()
        {
            for (int i = 0; i < VerticeColliders.Count; i++)
            {
                VerticeColliders[i].enabled = false;
                VerticeVisuals[i].enabled = false;
            }
        }

        public void ToggleVerticeCollidersAndVisualsOn()
        {
            for (int i = 0; i < VerticeColliders.Count; i++)
            {
                VerticeColliders[i].enabled = true;
                VerticeVisuals[i].enabled = true;
            }
        }

        public void ToggleEdgeCollidersOff()
        {
            EdgeColliders.ForEach(i => i.enabled = false);
        }

        public void ToggleEdgeCollidersOn()
        {
            EdgeColliders.ForEach(i => i.enabled = false);
        }

        public void ToggleTriangleCollidersOff()
        {
            TriangleColliders.ForEach(i => i.enabled = false);
        }

        public void ToggleTriangleCollidersOn()
        {
            TriangleColliders.ForEach(i => i.enabled = false);
        }
    }
}