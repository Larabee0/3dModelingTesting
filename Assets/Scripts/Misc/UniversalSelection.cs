using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UniversalSelection : MonoBehaviour
{
    [Header("Settings")]
    public Material SelectionMat;
    [Header("Selectable Objects")]
    public List<GameObject> SelectableObjects = new List<GameObject>();

    [Header("Debug Info")]
    private Vector3 p1;
    private Vector3 p2;
    [SerializeField] private bool dragSelect;
    public bool SetMatGlobal = true;

    // Update is called once per frame
    public void SelectionMouseDown(Material Mat,bool SetMat)
    {
        p1 = Mouse.current.position.ReadValue();
        SelectionMat = Mat;
        SetMatGlobal = SetMat;
    }

    public void SelectionMouse()
    {
        if (!dragSelect && p1 != Vector3.negativeInfinity)
            if ((p1 - (Vector3)Mouse.current.position.ReadValue()).magnitude > 40)
                dragSelect = true;
    }
    public List<GameObject> SelectionMouseUp(bool leftControlStatus)
    {
        print("Mouse up Triggered");
        List<GameObject> Selection = new List<GameObject>();
        if (SelectionMat != null && p1 != Vector3.negativeInfinity)
        {
            p2 = Mouse.current.position.ReadValue();
            if (dragSelect)
            {
                dragSelect = false;
                if (!leftControlStatus)
                {
                    for (int i = 0; i < SelectableObjects.Count; i++)
                    {
                        if (SelectableObjects[i].GetComponent<SelectionComponent>())
                        {
                            Destroy(SelectableObjects[i].GetComponent<SelectionComponent>());
                        }
                    }
                }

                float width = p2.x - p1.x;
                float height = p2.y - p1.y;

                Vector2 bounds = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
                Vector2 Position = p1 + new Vector3(width / 2, height / 2, 0f);
                Vector2 min = Position - (bounds / 2);
                Vector2 max = Position + (bounds / 2);

                for (int i = 0; i < SelectableObjects.Count; i++)
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(SelectableObjects[i].transform.position);

                    if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
                    {
                        SelectableObjects[i].AddComponent<SelectionComponent>();
                        SelectableObjects[i].GetComponent<SelectionComponent>().Selection = SelectionMat;
                        SelectableObjects[i].GetComponent<SelectionComponent>().SetMat = SetMatGlobal;
                        Selection.Add(SelectableObjects[i]);
                    }
                }
            }
            else
            {
                _ = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hitInfo);
                if (hit)
                {
                    //Debug.Log("Hit " + hitInfo.transform.gameObject.name);
                    if (SelectableObjects.Contains(hitInfo.transform.gameObject))
                    {
                        if (!leftControlStatus)
                        {
                            //Debug.Log("Hit, Mat Clear, no ctrl");
                            for (int i = 0; i < SelectableObjects.Count; i++)
                            {
                                if (SelectableObjects[i].GetComponent<SelectionComponent>())
                                {
                                    Destroy(SelectableObjects[i].GetComponent<SelectionComponent>());
                                }
                            }
                        }
                        Selection.Add(hitInfo.transform.gameObject);
                        Selection[Selection.Count - 1].AddComponent<SelectionComponent>();
                        Selection[Selection.Count - 1].GetComponent<SelectionComponent>().Selection = SelectionMat;
                        Selection[Selection.Count - 1].GetComponent<SelectionComponent>().SetMat = SetMatGlobal;
                    }
                    else
                    {
                        //Debug.Log("Hit, Mat Clear");
                        for (int i = 0; i < SelectableObjects.Count; i++)
                        {
                            if (SelectableObjects[i].GetComponent<SelectionComponent>())
                            {
                                Destroy(SelectableObjects[i].GetComponent<SelectionComponent>());
                            }
                        }
                    }
                }
                else
                {
                    //Debug.Log("No hit, Mat Clear");
                    for (int i = 0; i < SelectableObjects.Count; i++)
                    {
                        if (SelectableObjects[i].GetComponent<SelectionComponent>())
                        {
                            Destroy(SelectableObjects[i].GetComponent<SelectionComponent>());
                        }
                    }
                }
            }
        }
        p1 = Vector3.negativeInfinity;
        return Selection;
    }

    public void RemoveSelectionComponents()
    {
        for (int i = 0; i < SelectableObjects.Count; i++)
        {
            if (SelectableObjects[i].GetComponent<SelectionComponent>())
            {
                Destroy(SelectableObjects[i].GetComponent<SelectionComponent>());
            }
        }
    }

    void OnGUI()
    {
        if (dragSelect)
        {
            var rect = OnDrawUtilities.GetScreenRect(p1, Mouse.current.position.ReadValue());
            OnDrawUtilities.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            OnDrawUtilities.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
}
