using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV5
{
    public class QuadDataStore : MonoBehaviour
    {
        public List<int> InvolvedVerts = new List<int>();
        //public List<int> TriangleArrayInfo = new List<int>();
        public List<VertDataStore> InvolvedVertObjects = new List<VertDataStore>();
        public List<GameObject> InvolvedEdgeObjects = new List<GameObject>();
        public List<GameObject> InvolvedTriObjects = new List<GameObject>();
        public int QuadIndex = 1;
        public MeshManager MMV5;

        public Vector3 Position = Vector3.zero;

        public Mesh ThisMesh;
        [SerializeField] Vector3[] localVerticesArray;
        [SerializeField] int[] localTrianglesArray;

        private MeshCollider ThisMeshCollider;
        // Start is called before the first frame update
        void Start()
        {
            StartTriangle();
        }

        public void StartTriangle()
        {
            ThisMeshCollider = this.GetComponent<MeshCollider>();
            localVerticesArray = new Vector3[]
            {
                InvolvedVertObjects[0].transform.localPosition,
                InvolvedVertObjects[1].transform.localPosition,
                InvolvedVertObjects[2].transform.localPosition,
                InvolvedVertObjects[0].transform.localPosition,
                InvolvedVertObjects[2].transform.localPosition,
                InvolvedVertObjects[3].transform.localPosition
            };

            if (Vector3.Distance(localVerticesArray[0], localVerticesArray[1]) > 0f && Vector3.Distance(localVerticesArray[1], localVerticesArray[2]) > 0f && Vector3.Distance(localVerticesArray[2], localVerticesArray[0]) > 0f)
            {
                NewTriMesh();
            }
        }

        void NewTriMesh()
        {
            localTrianglesArray = new int[] { 0, 1, 2, 3, 4, 5 };

            Position = (localVerticesArray[0] + localVerticesArray[1] + localVerticesArray[2]+ localVerticesArray[5]) / 4;

            ThisMesh = new Mesh
            {
                name = this.name,
                vertices = localVerticesArray,
                triangles = localTrianglesArray
            };
            ThisMeshCollider.sharedMesh = ThisMesh;
        }

        public void UpdateTriangleCollider()
        {
            localVerticesArray[0] = InvolvedVertObjects[0].transform.localPosition;
            localVerticesArray[1] = InvolvedVertObjects[1].transform.localPosition;
            localVerticesArray[2] = InvolvedVertObjects[2].transform.localPosition;
            localVerticesArray[3] = InvolvedVertObjects[0].transform.localPosition;
            localVerticesArray[4] = InvolvedVertObjects[2].transform.localPosition;
            localVerticesArray[5] = InvolvedVertObjects[3].transform.localPosition;

            Position = (localVerticesArray[0] + localVerticesArray[1] + localVerticesArray[2] + localVerticesArray[5]) / 4;
            if (ThisMesh == null)
            {
                if (Vector3.Distance(localVerticesArray[0], localVerticesArray[1]) > 0f && Vector3.Distance(localVerticesArray[1], localVerticesArray[2]) > 0f && Vector3.Distance(localVerticesArray[2], localVerticesArray[0]) > 0f)
                {
                    NewTriMesh();
                    return;
                }
            }

            if (Vector3.Distance(localVerticesArray[0], localVerticesArray[1]) > 0f && Vector3.Distance(localVerticesArray[1], localVerticesArray[2]) > 0f && Vector3.Distance(localVerticesArray[2], localVerticesArray[0]) > 0f)
            {
                ThisMesh.vertices = localVerticesArray;
                ThisMeshCollider.sharedMesh = ThisMesh;
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
    }
}