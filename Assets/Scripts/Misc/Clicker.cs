using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clicker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Right Mouse went down at: "+ Time.time);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Left Mouse went down at: " + Time.time);
        }
    }
}
