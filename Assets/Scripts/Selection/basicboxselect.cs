using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basicboxselect : MonoBehaviour
{
    public List<GameObject> Objects = new List<GameObject>();
    
    private Vector3 p1;
    private Vector3 p2;
    [SerializeField] private bool dragSelect;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            p1 = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            p2 = Input.mousePosition;
            if (dragSelect)
            {
                dragSelect = false;
                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    for (int i = 0; i < Objects.Count; i++)
                    {
                        if (Objects[i].GetComponent<SelectionComponent>())
                        {
                            Destroy(Objects[i].GetComponent<SelectionComponent>());
                        }
                    }
                }
                
                float width = p2.x - p1.x;
                float height = p2.y - p1.y;

                Vector2 bounds = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
                Vector2 Position = p1 + new Vector3(width / 2, height / 2, 0f);
                Vector2 min = Position - (bounds / 2);
                Vector2 max = Position + (bounds / 2);

                for (int i = 0; i < Objects.Count; i++)
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(Objects[i].transform.position);

                    if(screenPos.x > min.x && screenPos.x <max.x&&screenPos.y>min.y && screenPos.y < max.y)
                    {
                        Objects[i].AddComponent<SelectionComponent>();
                    }
                }
            }
            else
            {
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (hit)
                {
                    if (Objects.Contains(hitInfo.transform.gameObject))
                    {
                        if (!Input.GetKey( KeyCode.LeftControl))
                        {
                            for (int i = 0; i < Objects.Count; i++)
                            {
                                if (Objects[i].GetComponent<SelectionComponent>())
                                {
                                    Destroy(Objects[i].GetComponent<SelectionComponent>());
                                }
                            }
                        }
                        hitInfo.transform.gameObject.AddComponent<SelectionComponent>();
                    }
                    else
                    {
                        for (int i = 0; i < Objects.Count; i++)
                        {
                            if (Objects[i].GetComponent<SelectionComponent>())
                            {
                                Destroy(Objects[i].GetComponent<SelectionComponent>());
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Objects.Count; i++)
                    {
                        if (Objects[i].GetComponent<SelectionComponent>())
                        {
                            Destroy(Objects[i].GetComponent<SelectionComponent>());
                        }
                    }
                }
            }



            
        }
        if (Input.GetMouseButton(0))
        {
            if(!dragSelect)
                if ((p1 - Input.mousePosition).magnitude > 40)
                    dragSelect = true;
        }
    }

    void OnGUI()
    {
        if (dragSelect)
        {

            var rect = Utils.GetScreenRect(p1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
}
