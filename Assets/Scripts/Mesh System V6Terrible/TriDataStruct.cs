using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MeshSystemV6
{
    public struct TriDataStruct
    {
        public List<int> InvolvedVerts { get; set; }
        public List<int> TriangleArrayInfo { get; set; }
        public List<VertDataStruct> InvolvedVertObjects { get; set; }
        public List<GameObject> InvolvedEdgeObjects { get; set; }
        public List<EdgeDataStruct> InvolvedEdgeDS { get; set; }
        public List<TriDataStruct> InvolvedTriDS { get; set; }
        public int TriIndex { get; set; }

        public GameObject GameObject;
        public Vector3 Position { get; set; }
        public Mesh ThisMesh { get; set; }
        public Vector3[] localVerticesArray { get; set; }
        public int[] localTrianglesArray { get; set; }
        public MeshCollider ThisMeshCollider { get; set; }

        public TriDataStruct(List<int> IVs, List<int> TAI, List<VertDataStruct> IVOs, List<GameObject> IEOs, List<EdgeDataStruct> IEDS,int TI, GameObject GO)
        {
            InvolvedVerts = IVs;
            TriangleArrayInfo = TAI;
            InvolvedVertObjects = IVOs;
            InvolvedEdgeObjects = IEOs;
            InvolvedEdgeDS = IEDS;
            InvolvedTriDS = new List<TriDataStruct>();
            TriIndex = TI;
            GameObject = GO;
            Position = Vector3.zero;
            ThisMesh = null;
            localVerticesArray = null;
            localTrianglesArray = null;
            ThisMeshCollider = GameObject.GetComponent<MeshCollider>();
        }
    }
}