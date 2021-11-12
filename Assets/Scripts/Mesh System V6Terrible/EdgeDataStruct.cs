using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MeshSystemV6
{
    public struct EdgeDataStruct
    {
        // ALL Vertices in the verticesArray on the MeshManager involed with this edge.
        public List<List<int>> InvolvedVerts { get; set; }
        public List<VertDataStruct> InvolvedVertObjects { get; set; }
        public List<TriDataStruct> InvolvedTriangles { get; set; }

        public int EdgePositionsIndex { get; set; }

        // Reference to the MeshManager.
        public GameObject GameObject;
        public MeshRenderer ThisMeshRenderer { get; set; }
        public BoxCollider ThisBoxCollider { get; set; }
        public EdgeDataStruct(List<List<int>> IVs,List<VertDataStruct> IVOs, int EPI, GameObject GO)
        {
            InvolvedVerts = IVs;
            InvolvedVertObjects = IVOs;
            InvolvedTriangles = new List<TriDataStruct>();
            EdgePositionsIndex = EPI;
            GameObject = GO;
            ThisMeshRenderer = GameObject.GetComponent<MeshRenderer>();
            ThisBoxCollider = GameObject.GetComponent<BoxCollider>();
        }
    }
}