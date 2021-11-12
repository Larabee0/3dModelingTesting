using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV4
{
    public class EdgeDataStore : MonoBehaviour
    {
        // ALL Vertices in the verticesArray on the MeshManager involed with this edge.
        public List<List<int>> InvolvedVerts = new List<List<int>>();
        public List<VertDataStore> InvolvedVertObjects = new List<VertDataStore>();
        public float ScaleFactor = 0.025f;

        // Reference to the MeshManager.
        public MeshManager MMV5;

        [SerializeField] private MeshRenderer ThisMeshRenderer;
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
            Vector3 A = MMV5.verticesArray[InvolvedVerts[0][0]];
            Vector3 B = MMV5.verticesArray[InvolvedVerts[1][0]];
            Vector3 EdgeAveragePositon = ((A + B) / 2) + MMV5.transform.position;
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
            for (int i = 0; i < InvolvedVertObjects.Count; i++)
            {
                InvolvedVertObjects[i].DoAction(Offset);
            }

            //UpdateVisuals();
            //MMV5.UpdateMesh();
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