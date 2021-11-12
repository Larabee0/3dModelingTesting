using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MeshSystemV8
{
    /// <summary>
    /// This script is disabled when nothing is selected in the MeshSelection script.
    /// If this script is running, something is selected.
    /// </summary>
    public class MeshManipulator : MeshManager
    {
        [Header("Tag Settings")]
        [Header("Mesh Manipulator")]
        public string HandleTag = ""; // This tag is for a xyz handle.
        public string FreeMoveTag = ""; // this tag is for the main object being selected, clicking on it allows free movement.

        [Header("Mesh Manipulator Debug Info")]
        public GameObject XYZHandle; // Current Handle (X, Y or Z axis)
        public bool EnableUpdateOnManipulator = false; // [HideInInspector] 
        public bool FoundCollider = false; // Do we currently have *any* handle selected by this script.

        public bool RoundingEnabled = false; // Are we rounding positions to a certain number of decimal Places?
        public int RoundingPrecision = 1; // Current decimal places we are rounding to.

        private Vector3 MouseLastFrame; // Mouse position last frame, important for moving shit.

        /// <summary>
        /// When we let off the left mouse button, 
        /// we no longer have a handle or collider
        /// </summary>
        public void LeftMouseUp()
        {
            //StopAction();
            XYZHandle = null;
            FoundCollider = false;
        }

        /// <summary>
        /// We need to calculate the mouse last frame to stop jumpyness in movement on mouse down.
        /// This is only triggered if we find a collider from GetHandle().
        /// </summary>
        public void LeftMouseDown()
        {
            GetHandle();
            if (FoundCollider)
            {
                // Get the Z component of the distance to the primary Selectred Object.
                UpdateMouseLastFrame(DistanceToScreen());
            }
        }

        private float DistanceToScreen()
        {
            return Camera.main.WorldToScreenPoint(meshSelection.PrimarySelectedObjectPosition).z;
        }
        private void UpdateMouseLastFrame(float distanceToScreen)
        {
            MouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, distanceToScreen));
        }

        public void LeftMouse()
        {
            if (FoundCollider)
            {
                MovePoint();
                float distanceToScreen = DistanceToScreen();
                if (XYZHandle != null)
                {
                    // Get the Z component of the distance to the primary Selectred Object.
                    distanceToScreen = Camera.main.WorldToScreenPoint(XYZHandle.transform.position).z;
                }
                UpdateMouseLastFrame(distanceToScreen);
            }
        }

        private void MovePoint()
        {

            // Get the Z component of the distance to the primary Selectred Object.
            float distanceToScreen = Camera.main.WorldToScreenPoint(meshSelection.PrimarySelectedObjectPosition).z;

            // If Per AxisHandle is NOT equal to null then:
            if (XYZHandle != null)
            {
                // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                distanceToScreen = Camera.main.WorldToScreenPoint(XYZHandle.transform.position).z;
            }

            // Create a new Vector3 called pos equal to the current mouse positon in the camrea to a world position using the distance from the camera to the object as z.
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, distanceToScreen));

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
            if (meshSelection.Mode == Mode.Vertex)
            {
                if (meshSelection.SelectedList.Count > 1)
                {
                    meshSelection.SelectedList.ForEach(i => i.GetComponent<VertDataStore>().DoAction(pos));
                }
                else
                {
                    meshSelection.PrimarySelectedObject.GetComponent<VertDataStore>().DoAction(pos);
                }
            }
            else if (meshSelection.Mode == Mode.Edge)
            {
                if (meshSelection.SelectedList.Count > 1)
                {
                    List<VertDataStore> InvolvedVerts = new List<VertDataStore>();
                    for (int i = 0; i < meshSelection.SelectedList.Count; i++)
                    {
                        List<VertDataStore> Temp = meshSelection.SelectedList[i].GetComponent<EdgeDataStore>().InvolvedVertObjects;
                        for (int j = 0; j < Temp.Count; j++)
                        {
                            if (!InvolvedVerts.Contains(Temp[j]))
                            {
                                InvolvedVerts.Add(Temp[j]);
                            }
                        }
                    }

                    InvolvedVerts.ForEach(i => i.DoAction(pos));
                }
                else
                {
                    meshSelection.PrimarySelectedObject.GetComponent<EdgeDataStore>().InvolvedVertObjects.ForEach(i => i.DoAction(pos));
                }
            }
            else if (meshSelection.Mode == Mode.Triangle)
            {
                if (meshSelection.SelectedList.Count > 1)
                {
                    List<VertDataStore> InvolvedVerts = new List<VertDataStore>();
                    for (int i = 0; i < meshSelection.SelectedList.Count; i++)
                    {
                        List<VertDataStore> Temp = meshSelection.SelectedList[i].GetComponent<TriDataStore>().InvolvedVertObjects;
                        for (int j = 0; j < Temp.Count; j++)
                        {
                            if (!InvolvedVerts.Contains(Temp[j]))
                            {
                                InvolvedVerts.Add(Temp[j]);
                            }
                        }
                    }
                    InvolvedVerts.ForEach(i => i.DoAction(pos));
                }
                else
                {
                    TriDataStore Temp = meshSelection.PrimarySelectedObject.GetComponent<TriDataStore>();
                    Temp.InvolvedVertObjects.ForEach(i => i.DoAction(pos));
                }
            }
            else
            {
                // If we get here, something catastrophic has happened and we need to abort.
                Debug.LogError("Critial Failure, Selection Mode not valid");
                return;
            }
            // Update the actual mesh, and reposition the handles.
            UpdateMeshVertices();
            meshSelection.DoHandlesStuff();

            // This stops the jumpyness of snapping to a vertex in another Mesh.
            //if (Snapped)
            //{
            //    StopAction();
            //}
        }

        /// <summary>
        /// Might be used later for snapping to things in another mesh.
        /// </summary>
        //private void StopAction()
        //{
        //
        //}

        private void GetHandle()
        {
            Collider theCollider = meshSelection.ModelManager.DoRaycast();
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
                    if (theCollider.gameObject == meshSelection.PrimarySelectedObject)
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