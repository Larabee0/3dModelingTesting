using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MeshSystemV6
{
    public struct VertDataStruct
    {
        // ALL Vertices in the verticesArray on the MeshManager which
        // occupy the same position as this.
        public List<int> RelatedVerts { get; set; }
        public List<VertDataStruct> ConnectedVerts { get; set; }
        public List<EdgeDataStruct> InvolvedEdges { get; set; }
        public List<TriDataStruct> InvolvedTriangles { get; set; }
        public int SimpliedVertIndex { get; set; }

        // Reference to the Mesh Manager.

        public MeshRenderer ThisMeshRenderer { get; set; }
        public BoxCollider ThisBoxCollider { get; set; }
        public GameObject GameObject { get; set; }

        public Transform transform;

        public VertDataStruct(List<int> RVs, int SVIndex, GameObject TGO)
        {
            RelatedVerts = RVs;
            ConnectedVerts = new List<VertDataStruct> ();
            InvolvedEdges = new List<EdgeDataStruct>();
            InvolvedTriangles = new List<TriDataStruct>();
            SimpliedVertIndex = SVIndex;
            GameObject = TGO;
            transform = GameObject.transform;
            ThisMeshRenderer = GameObject.GetComponent<MeshRenderer>();
            ThisBoxCollider = GameObject.GetComponent<BoxCollider>();
        }
    }
}