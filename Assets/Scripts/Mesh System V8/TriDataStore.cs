using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV8
{
    public class TriDataStore : MonoBehaviour
    {
        public List<int> TriangleArrayInfo = new List<int>();
        public List<VertDataStore> InvolvedVertObjects = new List<VertDataStore>();
        public List<EdgeDataStore> InvolvedEdgeDS = new List<EdgeDataStore>();
        public List<TriDataStore> InvolvedTriDS = new List<TriDataStore>();
        public int TriIndex = 1;
        public MeshManager MeshManager;

        public Vector3 Position = Vector3.zero;

        private Mesh ThisMesh;
        [SerializeField] private Vector3[] localVerticesArray = new Vector3[3];
        private readonly int[] localTrianglesArray = new int[] { 0, 1, 2, }; // [SerializeField] 

        private MeshCollider ThisMeshCollider;

        // Start is called before the first frame update
        private void Start()
        {
            for (int i = 0; i < 3; i++)
            {
                InvolvedVertObjects[i].InvolvedTriangles.Add(this);
            }
            StartTriangle();
        }

        public void StartTriangle()
        {
            ThisMeshCollider = GetComponent<MeshCollider>();
            UpdateLocalVerticeArrays();
            if (CalculateArea() > 0.0001f)
            {
                NewTriMesh();
            }
        }

        void NewTriMesh()
        {
            CalculatePosition();

            ThisMesh = new Mesh
            {
                name = name,
                vertices = localVerticesArray,
                triangles = localTrianglesArray
            };
            ThisMeshCollider.sharedMesh = ThisMesh;
        }

        public void UpdateTriangleCollider()
        {
            UpdateLocalVerticeArrays();
            CalculatePosition();

            if (CalculateArea() > 0.0001f)
            {
                if (ThisMesh == null)
                {
                    NewTriMesh();                        
                }
                else
                {
                    ThisMesh.vertices = localVerticesArray;
                    ThisMeshCollider.sharedMesh = ThisMesh;
                }
            }
            else
            {
                ThisMeshCollider.sharedMesh = null;
            }
        }

        public void ToggleCollider()
        {
            if (ThisMeshCollider.enabled)
            {
                ThisMeshCollider.enabled = false;
            }
            else
            {
                ThisMeshCollider.enabled = true;
            }
        }

        private float CalculateArea()
        {
            float A = Vector3.Distance(localVerticesArray[0], localVerticesArray[1]);
            float B = Vector3.Distance(localVerticesArray[1], localVerticesArray[2]);
            float C = Vector3.Distance(localVerticesArray[2], localVerticesArray[0]);
            float temp = (A + B + C) / 2;
            return Mathf.Sqrt(temp * (temp - A) * (temp - B) * (temp - C));
        }
        private void CalculatePosition()
        {
            Position = (localVerticesArray[0] + localVerticesArray[1] + localVerticesArray[2]) / 3;
        }

        private void UpdateLocalVerticeArrays()
        {
            localVerticesArray[0] = InvolvedVertObjects[0].transform.localPosition;
            localVerticesArray[1] = InvolvedVertObjects[1].transform.localPosition;
            localVerticesArray[2] = InvolvedVertObjects[2].transform.localPosition;
        }
    }
}