using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV5
{
    public class VertDataStore : MonoBehaviour
    {
        // ALL Vertices in the verticesArray on the MeshManager which
        // occupy the same position as this.
        public List<int> RelatedVerts = new List<int>();
        public List<VertDataStore> ConnectedVerts = new List<VertDataStore>();
        public List<EdgeDataStore> InvolvedEdges = new List<EdgeDataStore>();
        public List<TriDataStore> InvolvedTriangles = new List<TriDataStore>();
        public int SimpliedVertIndex = -1;

        // Reference to teh Mesh Manager.
        public MeshManager MMV5;

        [SerializeField] private MeshRenderer ThisMeshRenderer;
        [SerializeField] private BoxCollider ThisBoxCollider;

        // Start is called before the first frame update
        private void Start()
        {
            ThisMeshRenderer = this.GetComponent<MeshRenderer>();
            ThisBoxCollider = this.GetComponent<BoxCollider>();
            
        }

        // This will Update the position of this visual to the current position of the vertice.
        public void UpdateVisuals()
        {
            this.transform.localPosition = MMV5.LocalVertices[RelatedVerts[0]];
        }

        public void UpdateScale(Vector3 NewScale)
        {
            this.transform.localScale = NewScale;
        }

        public void DoAction(Vector3 Offset)
        {
            
            for (int i = 0; i < RelatedVerts.Count; i++)
            {
                MMV5.GlobalVertices[RelatedVerts[i]] = MMV5.transform.TransformPoint(MMV5.LocalVertices[RelatedVerts[i]] += Offset);
            }

            MMV5.LocalSimplifiedVerts[SimpliedVertIndex] += Offset;
            
            if (InvolvedEdges.Count > 0)
            {
                InvolvedEdges.ForEach(i => i.UpdateVisuals());
            }


            if (InvolvedTriangles.Count > 0)
            {
                InvolvedTriangles.ForEach(i => i.UpdateTriangleCollider());
            }

            UpdateVisuals();
        }

        public void ToggleVisability()
        {
            if (ThisMeshRenderer.enabled)
            {
                ThisMeshRenderer.enabled = false;
            }
            else
            {
                ThisMeshRenderer.enabled = true;
            }
        }

        public void ToggleCollider()
        {
            if (ThisBoxCollider.enabled)
            {
                ThisBoxCollider.enabled = false;
            }
            else
            {
                ThisBoxCollider.enabled = true;
            }
        }
    }
}