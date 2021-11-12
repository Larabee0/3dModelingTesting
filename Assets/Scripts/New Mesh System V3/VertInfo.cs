using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Tiny script used to store the Triangle component a vertice is a child of, 
/// as well as what index it is in the vertices list of this triangle.
/// </summary>
public class VertInfo : MonoBehaviour
{
    public Triangle2 Parent;
    public int VertIndex;
}

