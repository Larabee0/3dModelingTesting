using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MeshManipulatorV2 : MonoBehaviour
{
    [Header("Script References")]
    public MeshManagerV4 MM;
    public MeshSelectionV3 MS;
    public ModelManagerV2 ModelManager;

    [Header("Settings")]
    public LayerMask VertexLayerMask = 8;
    public LayerMask NoneVertexLayerMask = 9;
    public LayerMask VertexLayerMask2 = 9;
    public LayerMask HandleLayerMask;


    [Header("Debug Info")]
    public GameObject XYZHandle;
    public bool roundingEnabled;
    public int roundingPrecision = 1;
    private Vector3 mouseLastFrame;

    public int Selection = -10;

    private bool Snapped = false;
    public bool FoundCollider = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            LeftMouseUp();
        }

        if (Input.GetMouseButtonDown(0))
        {
            LeftMouseDown();
        }

        if (Input.GetMouseButton(0))
        {
            LeftMouse();
        }
    }


    void LeftMouseUp()
    {
        StopAction();
        XYZHandle = null;
        if (MS.PrimarySelectedObjectVertex != null)
        {
            //for (int i = 0; i < MS.VertexSelectionList.Count; i++)
            //{
            //    if (MS.Mode == 2)
            //    {
            //        MS.VertexSelectionList[i].GetComponent<MeshCollider>().sharedMesh = MS.VertexSelectionList[i].GetComponent<MeshFilter>().mesh;
            //    }
            //}
        }
    }

    void LeftMouseDown()
    {
        if (MS.PrimarySelectedObjectVertex != null)
        {
            float distanceToScreen = Camera.main.WorldToScreenPoint(MS.PrimarySelectedObjectVertex.transform.position).z;
            mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
        }
        if (MS.VertexSelectionList.Count - 1 >= 0)
        {
            GetVertexPoint();
        }
    }

    void LeftMouse()
    {
        if (MS.PrimarySelectedObjectVertex != null)
        {
            MovePoint();
            float distanceToScreen = Camera.main.WorldToScreenPoint(MS.PrimarySelectedObjectVertex.transform.position).z;
            if (XYZHandle != null)
            {
                // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                distanceToScreen = Camera.main.WorldToScreenPoint(XYZHandle.transform.position).z;
            }
            mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
        }
    }

    void MovePoint()
    {
        // Get the Z component of the distance to the primary vertexPoint. (item 0 in vertexPosSelection/vertexSelection).
        float distanceToScreen = Camera.main.WorldToScreenPoint(MS.PrimarySelectedObjectVertex.transform.position).z;

        // If Per AxisHandle is NOT equal to null then:
        if (XYZHandle != null)
        {
            // distanceToScreen is equal to the z component of the distance from the handle clicked on.
            distanceToScreen = Camera.main.WorldToScreenPoint(XYZHandle.transform.position).z;
        }

        //create a new vector3 which converts the mouse positon in the camrea to a world position using the distance from the camera to the object as z.
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));

        // If Rounding is Enabled:
        if (roundingEnabled)
        {
            // Round every float in the Vector3 Pos to the spsified decmial places.
            pos = new Vector3((float)Math.Round(pos.x, roundingPrecision), (float)Math.Round(pos.y, roundingPrecision), (float)Math.Round(pos.z, roundingPrecision));
        }

        // pos equals itself minus mouseLastFrame.
        pos -= mouseLastFrame;

        /// If Selection contains more than 0 items AND we arent in TrinagleAdding mode then:
        ///if (Selection > 0)
        ///{
        ///    // Pos is updated to the return of the SnapToVertexInOtherMesh function.
        ///    pos = SnapToVertexInOtherMesh(pos);
        ///}
        /// // If left Control is down
        ///if (Input.GetKey(KeyCode.LeftShift))
        ///{
        ///    // snap to vertex in same mesh
        ///    pos = SnapToVertexInSameMesh(pos);
        ///}
        /// If PerAxisHandle is null:
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
        // Pos is componsenated incase the gameObject is rotated.

        // For each item in vertexPosSelection, update the position to the current positon of the vertices stored in vertexSelection, 
        // to the position stored in vertexPosSelection + pos.
        for (int i = 0; i < MS.VertexSelectionList.Count; i++)
        {
            VertInfo Local = MS.VertexSelectionList[i].GetComponent<VertInfo>();
            Local.Parent.DoMeshAction(Local.VertIndex, pos);
            //DoAction(MS.VertexSelectionList[i], pos);
        }

        // This stops the jumpyness of snapping to a vertex in another Mesh.
        if (Snapped)
        {
            StopAction();
        }
    }

    private Vector3 SnapToVertexInOtherMesh(Vector3 pos)
    {
        // Get a collider, assign it to theCollider var.
        Collider theCollider = ModelManager.DoRaycast();
        // If the collider is not null:
        if (theCollider != null)
        {
            // int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            // See if the collider is the one we want to click ie, is on the default layer.
            int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            if ((maskForThisHitObject & VertexLayerMask2) != 0)
            {
                // If the collider's parent gameObject is not the gameObject the Mesh Visualizer is on:
                if (theCollider.transform.parent.gameObject != gameObject)
                {
                    // Make pos equal to the positon of the vertexPoint in the otherMesh the mouse was over.
                    // Set Snapped to True.
                    pos = theCollider.transform.position - gameObject.transform.position - MS.PrimarySelectedObjectVertex.transform.position;
                    Snapped = true;
                }
            }
        }
        //return's either the position of the vertexPoint in another mesh or the pos the function was given when called.
        return pos;
    }

    private Vector3 SnapToVertexInSameMesh(Vector3 pos)
    {
        // Get a collider, assign it to theCollider var.
        Collider theCollider = ModelManager.DoRaycast();
        // If the collider is not null:
        if (theCollider != null)
        {
            // See if the collider is the one we want to click ie, is on the default layer.
            int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            if ((maskForThisHitObject & VertexLayerMask) != 0)
            {
                // Make pos equal to the positon of the vertexPoint in the otherMesh the mouse was over.
                // Set Snapped to True.
                if (MS.PrimarySelectedObjectVertex != null)
                {
                    pos = theCollider.transform.localPosition - gameObject.transform.position - MS.PrimarySelectedObjectVertex.transform.position;
                    Snapped = true;
                }
            }
        }
        //return's either the position of the vertexPoint in another mesh or the pos the function was given when called.
        return pos;
    }

    void DoAction(GameObject item, Vector3 pos)
    {
        // Pos is componsenated incase the gameObject is rotated.
        pos = (Quaternion.Euler(item.transform.localRotation.eulerAngles) * pos);
        if (MS.Mode == 0) // Edges
        {

        }
        else if (MS.Mode == 1) // Vertices
        {

        }
        else if (MS.Mode == 2) // Faces
        {
            for (int i = 0; i < MS.VertexSelectionList.Count; i++)
            {
                Vector3[] vertices = MS.VertexSelectionList[i].GetComponent<MeshFilter>().mesh.vertices;
                for (int j = 0; j < vertices.Length; j++)
                {
                    vertices[j] += pos;
                }
                MS.VertexSelectionList[i].GetComponent<MeshFilter>().mesh.vertices = vertices;
                MS.VertexSelectionList[i].GetComponent<MeshFilter>().mesh.RecalculateNormals();
            }
            MM.SpawnedHandle.transform.position += pos;
        }
    }

    void StopAction()
    {

    }

    void GetVertexPoint()
    {
        // Set colliderFound to False.
        // Run the RayCast helper, and assign its output to theCollider.
        Collider theCollider = ModelManager.DoRaycast();
        // If we didn't get a collider end function here        
        if (theCollider)
        {
            //if (MS.Mode == 1)
            //{
            //    // If the Collider is on the handle layer mask:
            //    int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            //    if ((maskForThisHitObject & HandleLayerMask) != 0)
            //    {
            //        XYZHandle = theCollider.gameObject;
            //        Debug.Log("Mode 1 handle");
            //        FoundCollider = true;
            //    }
            //    
            //}
            // If the Collider is on the handle layer mask:
            int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            if ((maskForThisHitObject & HandleLayerMask) != 0)
            {
                XYZHandle = theCollider.gameObject;
                Debug.Log("Else handle");
                FoundCollider = true;
            }

            if (MS.Mode == 0)
            {
                if (theCollider.transform.gameObject == MS.PrimarySelectedObjectEdge)
                {
                    XYZHandle = null;
                    FoundCollider = true;
                }
            }
            else if (theCollider.transform.gameObject == MS.PrimarySelectedObjectVertex)
            {
                XYZHandle = null;
                FoundCollider = true;
            }
            else if (MS.Mode == 2)
            {
                if (theCollider.transform.gameObject == MS.PrimarySelectedObjectTri)
                {
                    XYZHandle = null;
                    FoundCollider = true;
                }
            }


        }
    }
}
