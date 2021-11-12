using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSelection : MonoBehaviour
{
    SelectedDirectionary selectedtable;
    RaycastHit hit;
    bool dragSelect;

    Vector3 p1;
    Vector3 p2;

    MeshCollider selectionBox;
    Mesh selectionMesh;
    Vector2[] corners;
    Vector3[] verts;

    // Start is called before the first frame update
    void Start()
    {
        selectedtable = GetComponent<SelectedDirectionary>();
        dragSelect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            p1 = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if((p1 - Input.mousePosition).magnitude > 40)
            {
                dragSelect = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (!dragSelect)
            {
                Ray ray = Camera.main.ScreenPointToRay(p1);
                if (Physics.Raycast(ray, out hit, 50000.0f))
                {
                    Debug.Log("Hit " + hit.transform.gameObject.name);
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        selectedtable.AddSelected(hit.transform.gameObject);
                    }
                    else
                    {
                        selectedtable.DeselectAll();
                        selectedtable.AddSelected(hit.transform.gameObject);
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        return;
                    }
                    else
                    {
                        selectedtable.DeselectAll();
                    }
                }
            }
            else
            {
                /// //verts = new Vector3[4];
                /// //int i = 0;
                ///
                /// //corners = GetBoudingBox(p1, p2);
                /// //for (int j = 0; j < 3; j++)
                /// //{
                /// //    Ray ray = Camera.main.ScreenPointToRay(corners[j]);
                /// //    if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
                /// //    {
                /// //        verts[i] = new Vector3(hit.point.x, 0, hit.point.z);
                /// //        Debug.DrawLine(Camera.main.ScreenToWorldPoint(corners[j]), hit.point, Color.red, 1.0f);
                /// //    }
                /// //    i++;
                /// //}
                /// //selectionMesh = GenerateSelectionMesh(verts);
                /// //selectionBox = gameObject.AddComponent<MeshCollider>();
                /// //selectionBox.sharedMesh = selectionMesh;
                /// //selectionBox.convex = true;
                /// //selectionBox.isTrigger = true;
                /// //if (!Input.GetKey(KeyCode.LeftControl))
                /// //{
                /// //    selectedtable.DeselectAll();
                /// //}
                /// //
                /// //Destroy(selectionBox, 0.02f);

               //p2 = Input.mousePosition;
               //dragSelect = false;
               //SelectionBox.gameObject.SetActive(false);
               //List<GameObject> BoxSelection = new List<GameObject>();
               //Quaternion lookRotation = Quaternion.LookRotation(-Camera.main.transform.localPosition);
               //Vector3 Size = new Vector3(SelectionBox.rect.size.x, SelectionBox.rect.size.y, 50f);
               //
               //RaycastHit[] hits = Physics.BoxCastAll(SelectionBox.rect.center, Size, transform.forward, lookRotation);
               //for (int i = 0; i < hits.Length; i++)
               //{
               //    BoxSelection.Add(hits[i].collider.gameObject);
               //    Debug.Log(BoxSelection[i].name);
               //}

            }
        }
        
        
    }

    void OnGUI()
    {
        if (dragSelect)
        {
            
            var rect = Utils.GetScreenRect(p1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect,2 ,new Color(0.8f, 0.8f, 0.95f));
        }
    }
    Vector2[] GetBoudingBox(Vector2 p1, Vector2 p2)
    {
        Vector2 newP1;
        Vector2 newP2;
        Vector2 newP3;
        Vector2 newP4;

        if (p1.x < p2.x) //if p1 is to the left of p2
        {
            if (p1.y > p2.y) // if p1 is above p2
            {
                newP1 = p1;
                newP2 = new Vector2(p2.x, p1.y);
                newP3 = new Vector2(p1.x, p2.y);
                newP4 = p2;
            }
            else //if p1 is below p2
            {
                newP1 = new Vector2(p1.x, p2.y);
                newP2 = p2;
                newP3 = p1;
                newP4 = new Vector2(p2.x, p1.y);
            }
        }
        else //if p1 is to the right of p2
        {
            if (p1.y > p2.y) // if p1 is above p2
            {
                newP1 = new Vector2(p2.x, p1.y);
                newP2 = p1;
                newP3 = p2;
                newP4 = new Vector2(p1.x, p2.y);
            }
            else //if p1 is below p2
            {
                newP1 = p2;
                newP2 = new Vector2(p1.x, p2.y);
                newP3 = new Vector2(p2.x, p1.y);
                newP4 = p1;
            }

        }
        Vector2[] corners = { newP1, newP2, newP3, newP4 };
        return corners;
    }

    Mesh GenerateSelectionMesh(Vector3[] corners)
    {
        Vector3[] verts = new Vector3[8];
        int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };
        for (int i = 0; i < 4; i++)
        {
            verts[i] = corners[i];
        }
        for (int i = 4; i < 8; i++)
        {
            verts[i] = corners[i - 4] + Vector3.up * 100.0f;
        }
        Mesh selectionMesh = new Mesh();
        selectionMesh.vertices = verts;
        selectionMesh.triangles = tris;
        return selectionMesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        selectedtable.AddSelected(other.gameObject);
    }
}
