using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshSystemV4
{
    /// <summary>
    /// This script is disabled when nothing is selected in the MeshSelection script.
    /// If this script is running, something is selected.
    /// </summary>
    public class MeshManipulator : MonoBehaviour
    {
        [Header("Script References")]
        [SerializeField] private ModelManagerV2 ModelManager; // We need the Raycast method from the Model Manager.
        [SerializeField] private MeshManager MeshManager; // We need to update the mesh every frame.
        [SerializeField] private MeshSelection MeshSelection; // We need access to the selection.

        [Header("Tag Settings")]
        public string HandleTag = ""; // This tag is for a xyz handle.
        public string FreeMoveTag = ""; // this tag is for the main object being selected, clicking on it allows free movement.

        [Header("Debug Info")]
        public GameObject XYZHandle; // Current Handle (X, Y or Z axis)

        public bool FoundCollider = false; // Do we currently have *any* handle selected by this script.

        public bool RoundingEnabled = false; // Are we rounding positions to a certain number of decimal Places?
        public int RoundingPrecision = 1; // Current decimal places we are rounding to.

        private Vector3 MouseLastFrame; // Mouse position last frame, important for moving shit.


        // Start is called before the first frame update
        private void Start()
        {
            // On start get all the scripts we reference.
            ModelManager = GetComponentInParent<ModelManagerV2>();
            MeshManager = this.GetComponent<MeshManager>();
            MeshSelection = this.GetComponent<MeshSelection>();
        }

        // Update is called once per frame
        private void Update()
        {
            // When the left mouse button is released
            if (Input.GetMouseButtonUp(0)) 
            {
                LeftMouseUp();
            }
            // When the left mouse button is pressed down
            if (Input.GetMouseButtonDown(0))
            {
                LeftMouseDown();
            }
            // If the left mouse button is currently pressed down
            if (Input.GetMouseButton(0)) 
            {
                LeftMouse();
            }
        }

        /// <summary>
        /// When we let off the left mouse button, 
        /// we no longer have a handle or collider
        /// </summary>
        private void LeftMouseUp()
        {
            //StopAction();
            XYZHandle = null;
            FoundCollider = false;
        }

        /// <summary>
        /// We need to calculate the mouse last frame to stop jumpyness in movement on mouse down.
        /// This is only triggered if we find a collider from GetHandle().
        /// </summary>
        private void LeftMouseDown()
        {
            GetHandle();
            if (FoundCollider)
            {
                // Get the Z component of the distance to the primary Selectred Object.
                float distanceToScreen = Camera.main.WorldToScreenPoint(MeshSelection.PrimarySelectedObjectPosition).z;
                // Set MouseLastFrame to the current mouse positon in the camrea to a world position using the distance from the camera to the object as z.
                MouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
            }
        }

        private void LeftMouse()
        {
            MovePoint();
            float distanceToScreen = Camera.main.WorldToScreenPoint(MeshSelection.PrimarySelectedObjectPosition).z;
            if (XYZHandle != null)
            {
                // Get the Z component of the distance to the primary Selectred Object.
                distanceToScreen = Camera.main.WorldToScreenPoint(XYZHandle.transform.position).z;
            }
            
            // Set MouseLastFrame to the current mouse positon in the camrea to a world position using the distance from the camera to the object as z.
            MouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
        }

        private void MovePoint()
        {
            // Get the Z component of the distance to the primary Selectred Object.
            float distanceToScreen = Camera.main.WorldToScreenPoint(MeshSelection.PrimarySelectedObjectPosition).z;

            // If Per AxisHandle is NOT equal to null then:
            if (XYZHandle != null)
            {
                // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                distanceToScreen = Camera.main.WorldToScreenPoint(XYZHandle.transform.position).z;
            }

            // Create a new Vector3 called pos equal to the current mouse positon in the camrea to a world position using the distance from the camera to the object as z.
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));

            // If Rounding is Enabled:
            if (RoundingEnabled)
            {
                // Round every float in the Vector3 Pos to the spsified decmial places.
                pos = new Vector3((float)Math.Round(pos.x, RoundingPrecision), (float)Math.Round(pos.y, RoundingPrecision), (float)Math.Round(pos.z, RoundingPrecision));
            }

            // pos equals itself minus MouseLastFrame.
            pos -= MouseLastFrame;

            // If we have gotten a handle...
            if (XYZHandle != null)
            {
                // If the Name of the GameObject is "XAxis":
                if (XYZHandle.name == "XAxis")
                {
                    // Set these two components to 0.
                    pos.y = 0f;
                    pos.z = 0f;
                }
                // If the Name of the GameObject is "YAxis":
                else if (XYZHandle.name == "YAxis")
                {
                    // Set these two components to 0.
                    pos.x = 0f;
                    pos.z = 0f;
                }
                // If the Name of the GameObject is "ZAxis":
                else if (XYZHandle.name == "ZAxis")
                {
                    // Set these two components to 0.
                    pos.x = 0f;
                    pos.y = 0f;
                }
            }

            
            // Depending on the current mode, a different component is selected.
            // these all work the same though, they get the applicable DataStore and run the 
            // DoAction method, passing in pos.
            if(MeshSelection.Mode == 1)
            {
                for (int i = 0; i < MeshSelection.SelectedList.Count; i++)
                {
                    VertDataStore Local = MeshSelection.SelectedList[i].GetComponent<VertDataStore>();
                    Local.DoAction(pos);                    
                }
            }
            else if(MeshSelection.Mode == 2)
            {
                for (int i = 0; i < MeshSelection.SelectedList.Count; i++)
                {
                    EdgeDataStore Local = MeshSelection.SelectedList[i].GetComponent<EdgeDataStore>();
                    Local.DoAction(pos);
                }
            }
            else if(MeshSelection.Mode == 3)
            {
                for (int i = 0; i < MeshSelection.SelectedList.Count; i++)
                {
                    TriDataStore Local = MeshSelection.SelectedList[i].GetComponent<TriDataStore>();
                    Local.DoAction(pos);
                }
            }
            else
            {
                // If we get here, something catastrophic has happened and we need to abort.
                Debug.LogError("Critial Failure Mode not valid");
                return;
            }
            // Update the actual mesh, and reposition the handles.
            MeshManager.UpdateMesh();
            MeshSelection.DoHandlesStuff();

            // This stops the jumpyness of snapping to a vertex in another Mesh.
            //if (Snapped)
            //{
            //    StopAction();
            //}
        }

        /// <summary>
        /// Might be used later for snapping to things in another mesh.
        /// </summary>
        private void StopAction()
        {

        }

        private void GetHandle()
        {
            Collider theCollider = ModelManager.DoRaycast();            
            // If we get *a* collider...
            if (theCollider)
            {
                // If the Collider is tagged with the HandleTag...
                if (theCollider.CompareTag(HandleTag))
                {
                    // Set teh XYZHandle to theCollider's GameObject
                    // and set FoundCollider to true.
                    XYZHandle = theCollider.gameObject;
                    FoundCollider = true;
                }
                // If the Collider is tagged with the FreeMoveTag tag...
                else if (theCollider.CompareTag(FreeMoveTag))
                {
                    // If theCollider's GameObject is the same as the PrimarySelectedObject... 
                    if (theCollider.gameObject == MeshSelection.PrimarySelectedObject)
                    {
                        // Set the XYZHandle to null.
                        // and set FoundCollider to true.
                        XYZHandle = null;
                        FoundCollider = true;
                    }
                }
            }
        }
    }
}