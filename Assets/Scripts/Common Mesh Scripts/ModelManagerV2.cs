using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModelManagerV2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // RayCast function, returns a Collider Component.
    public Collider DoRaycast()
    {
        // theCamera is assigned to the currently active camera.
        //create a ray using the "ScreenPointToRay" helper which takes the mouse positon, this returns a ray cast from the mouse's positon in the camera view.
        Camera theCamera = Camera.main;
        Ray ray = theCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // If ray cast is true, ie we hit a thing:
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // Return the hit infomation.
            return hitInfo.collider;
        }
        // No collider was hit, return null.
        return null;
    }
}
