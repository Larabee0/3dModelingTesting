using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EclipisedByCollider : MonoBehaviour
{

    public GameObject CubeContainer;
    public GameObject CubesToSpawn;
    public List<MeshFilter> SubMeshes = new List<MeshFilter>();
    public List<GameObject> SubObjects = new List<GameObject>();
    public List<List<Vector3>> GlobalVerts = new List<List<Vector3>>();

    public List<Vector3> Cube = new List<Vector3>();

    public List<Vector3> Cylinder = new List<Vector3>();

    public List<bool> EclipisedBy = new List<bool>();

    // Start is called before the first frame update
    void Start()
    {
        SubMeshes.AddRange(GetComponentsInChildren<MeshFilter>());
        for (int i = 0; i < SubMeshes.Count; i++)
        {
            SubObjects.Add(SubMeshes[i].gameObject);
        }

        ConvertVertsToGlobal();
        Cube = GlobalVerts[0];
        Cylinder = GlobalVerts[1];
        int pointsNotInSide = 0;
        for (int i = 0; i < Cube.Count; i++)
        {
            bool Inside = IsInCollider(SubMeshes[1].GetComponent<MeshCollider>(), Cube[i]);

            if(!Inside)
            {
                pointsNotInSide += 1;
                //Debug.Log(Cube[i] + " was not Inside.");
            }
        }
        if(pointsNotInSide < 2)
        {
            Debug.Log(SubMeshes[0].name + " is inside " + SubMeshes[1].name);
        }
        else
        {
            Debug.Log(SubMeshes[0].name + " is outside " + SubMeshes[1].name);
        }


    }
    void Update()
    {
        
    }

    void ConvertVertsToGlobal()
    {
        for (int i = 0; i < SubMeshes.Count; i++)
        {
            List<Vector3> Temp1 = new List<Vector3>();
            for (int j = 0; j < SubMeshes[i].sharedMesh.vertexCount; j++)
            {
                if (!Temp1.Contains(SubObjects[i].transform.TransformPoint(SubMeshes[i].sharedMesh.vertices[j])))
                {
                    Temp1.Add(SubObjects[i].transform.TransformPoint(SubMeshes[i].sharedMesh.vertices[j]));

                    GameObject go = (GameObject)Instantiate(CubesToSpawn, Temp1[Temp1.Count - 1], Quaternion.identity, CubeContainer.transform);
                    go.name = Temp1[Temp1.Count - 1].ToString();
                }
            }
            
            GlobalVerts.Add(Temp1);
        }

    }


    public bool IsInCollider(MeshCollider other, Vector3 point)
    {
        Vector3 from = (Vector3.up * 5000f);
        Vector3 dir = (point - from).normalized;
        float dist = Vector3.Distance(from, point);
        //fwd      
        int hit_count = Cast_Till(from, point, other);
        //back
        dir = (from - point).normalized;
        hit_count += Cast_Till(point, point + (dir * dist), other);

        if (hit_count % 2 == 1)
        {
            return (true);
        }
        return (false);
    }

    int Cast_Till(Vector3 from, Vector3 to, MeshCollider other)
    {
        int counter = 0;
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);
        bool Break = false;
        while (!Break)
        {
            Break = true;
            RaycastHit[] hit = Physics.RaycastAll(from, dir, dist);
            for (int tt = 0; tt < hit.Length; tt++)
            {
                if (hit[tt].collider == other)
                {
                    counter++;
                    from = hit[tt].point + dir.normalized * .001f;
                    dist = Vector3.Distance(from, to);
                    Break = false;
                    break;
                }
            }
        }
        return (counter);
    }

}
