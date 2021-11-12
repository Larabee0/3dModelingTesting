using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV4
{
    public class TriDataStore : MonoBehaviour
    {
        public List<int> InvolvedVerts = new List<int>();
        public List<VertDataStore> InvolvedVertObjects = new List<VertDataStore>();
        public List<GameObject> InvolvedEdgeObjects = new List<GameObject>();
        public MeshManager MMV5;

        public Vector3 Position = Vector3.zero;

        private Mesh ThisMesh;
        [SerializeField] Vector3[] localVerticesArray;
        [SerializeField] int[] localTrianglesArray;

        private MeshCollider ThisMeshCollider;

        // Start is called before the first frame update
        private void Start()
        {
            for (int i = 0; i < InvolvedVerts.Count; i++)
            {
                InvolvedVertObjects.Add(MMV5.SpawnedVertsDS[InvolvedVerts[i]]);
                InvolvedVertObjects[i].InvolvedTriangles.Add(this);
            }

            ThisMeshCollider = this.GetComponent<MeshCollider>();
            StartTriangle();
        }

        public void StartTriangle()
        {
            localVerticesArray = new Vector3[]
            {
                MMV5.SimplifiedVerts[InvolvedVerts[0]], MMV5.SimplifiedVerts[InvolvedVerts[1]], MMV5.SimplifiedVerts[InvolvedVerts[2]],
                MMV5.SimplifiedVerts[InvolvedVerts[2]], MMV5.SimplifiedVerts[InvolvedVerts[1]], MMV5.SimplifiedVerts[InvolvedVerts[0]]
            };

            //ThisMesh.vertices = localVerticesArray;

            localTrianglesArray = new int[] { 0, 1, 2, 2, 1, 0 };

            ThisMesh = new Mesh
            {
                name = this.name,
                vertices = localVerticesArray,
                triangles = localTrianglesArray
            };
            ThisMesh.RecalculateNormals();
            ThisMeshCollider.sharedMesh = ThisMesh;

            Position = (localVerticesArray[0] + localVerticesArray[1] + localVerticesArray[2]) / 3;
        }

        public void UpdateTriangleCollider()
        {
            localVerticesArray[0] = MMV5.verticesArray[InvolvedVerts[0]];
            localVerticesArray[1] = MMV5.verticesArray[InvolvedVerts[1]];
            localVerticesArray[2] = MMV5.verticesArray[InvolvedVerts[2]];
                                  
            localVerticesArray[2] = MMV5.verticesArray[InvolvedVerts[2]];
            localVerticesArray[1] = MMV5.verticesArray[InvolvedVerts[1]];
            localVerticesArray[0] = MMV5.verticesArray[InvolvedVerts[0]];
            ThisMesh.vertices = localVerticesArray;
            ThisMesh.RecalculateNormals();
            ThisMeshCollider.sharedMesh = ThisMesh;
            Position = (localVerticesArray[0] + localVerticesArray[1] + localVerticesArray[2]) / 3;
        }

        public void DoAction(Vector3 Offset)
        {
            for (int i = 0; i < InvolvedVerts.Count; i++)
            {
                InvolvedVertObjects[i].DoAction(Offset);
            }

            //UpdateTriangleCollider();
            //MMV5.UpdateMesh();
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