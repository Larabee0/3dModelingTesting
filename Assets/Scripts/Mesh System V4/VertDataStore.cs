using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV4
{
    public class VertDataStore : MonoBehaviour
    {
        // ALL Vertices in the verticesArray on the MeshManager which
        // occupy the same position as this.
        public List<int> RelatedVerts = new List<int>();
        public List<EdgeDataStore> InvolvedEdges = new List<EdgeDataStore>();
        public List<TriDataStore> InvolvedTriangles = new List<TriDataStore>();
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
            this.transform.position = MMV5.verticesArray[RelatedVerts[0]];
        }

        public void UpdateScale(Vector3 NewScale)
        {
            this.transform.localScale = NewScale;
        }

        public void DoAction(Vector3 Offset)
        {
            for (int i = 0; i < RelatedVerts.Count; i++)
            {
                MMV5.verticesArray[RelatedVerts[i]] += Offset;
            }

            #region Possibly only need the 1 for loop
            for (int i = 0; i < InvolvedEdges.Count; i++)
            {
                InvolvedEdges[i].UpdateVisuals();
            }

            for (int i = 0; i < InvolvedTriangles.Count; i++)
            {
                InvolvedTriangles[i].UpdateTriangleCollider();
            }
            #endregion
            UpdateVisuals();
            //MMV5.UpdateMesh(); // Update mesh in the mesh manipulator AFTER all modifications this frame have been made.
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