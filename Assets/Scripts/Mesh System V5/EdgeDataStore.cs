using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV5
{
    public class EdgeDataStore : MonoBehaviour
    {
        // ALL Vertices in the verticesArray on the MeshManager involed with this edge.
        public List<List<int>> InvolvedVerts;
        public List<VertDataStore> InvolvedVertObjects = new List<VertDataStore>();
        public List<GameObject> InvolvedTriangles = new List<GameObject>();
        
        public int EdgePositionsIndex = -1;
        public float ScaleFactor = 0.025f;

        // Reference to the MeshManager.
        public MeshManager MMV5;

        public MeshRenderer ThisMeshRenderer;
        [SerializeField] private BoxCollider ThisBoxCollider;

        // Start is called before the first frame update
        private void Start()
        {
            ThisMeshRenderer = this.GetComponent<MeshRenderer>();
            ThisBoxCollider = this.GetComponent<BoxCollider>();
        }

        // This will Update teh position of this edge Visual to the current position of the edge in the mesh.
        public void UpdateVisuals()
        {
            Vector3 A = MMV5.GlobalVertices[InvolvedVerts[0][0]];
            Vector3 B = MMV5.GlobalVertices[InvolvedVerts[1][0]];
            Vector3 EdgeAveragePositon = ((A + B) / 2);

            float EdgeDistance = Vector3.Distance(A, B);

            this.transform.localScale = new Vector3(ScaleFactor, ScaleFactor, EdgeDistance);
            this.transform.position = Quaternion.Euler(MMV5.transform.localRotation.eulerAngles) * EdgeAveragePositon;
            this.transform.LookAt(Quaternion.Euler(MMV5.transform.localRotation.eulerAngles) * A);
        }

        public void UpdateScale(float NewScale)
        {
            ScaleFactor = NewScale;
            UpdateVisuals();
        }

        public void DoAction(Vector3 Offset)
        {
            InvolvedVertObjects.ForEach(i => i.DoAction(Offset));
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