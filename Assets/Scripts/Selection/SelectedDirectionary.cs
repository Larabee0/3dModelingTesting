using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedDirectionary : MonoBehaviour
{
    public Dictionary<int, GameObject> PosibleSelctionTableObject = new Dictionary<int, GameObject>();//Posible Selction Table of gameObjects
    
    public Dictionary<int, Vector3> PosibleSelctionTablePosition = new Dictionary<int, Vector3>();//positional data of possible to select objects

    public Dictionary<int, GameObject> SelectionTable = new Dictionary<int, GameObject>();//current selection

    private void Start()
    {
        MeshRenderer[] Temp1 = GetComponents<MeshRenderer>();
        for (int i = 0; i < Temp1.Length; i++)
        {
            GameObject gameObject = Temp1[i].gameObject;
            int id = gameObject.GetInstanceID();
            
            PosibleSelctionTableObject.Add(id, gameObject);
            PosibleSelctionTablePosition.Add(id, gameObject.transform.position);
        }
    }
    public void AddSelected(GameObject go)
    {
        int id = go.GetInstanceID();
        if (!PosibleSelctionTableObject.ContainsKey(id))
        {
            PosibleSelctionTableObject.Add(id, go);
            go.AddComponent<SelectionComponent>();
            Debug.Log("Added " + id + " to select dict");
        }
    }

    public void Deselect(int id)
    {
        Destroy(PosibleSelctionTableObject[id].GetComponent<SelectionComponent>());
        PosibleSelctionTableObject.Remove(id);
    }

    public void DeselectAll()
    {
        foreach (KeyValuePair<int,GameObject> pair in PosibleSelctionTableObject)
        {
            if(pair.Value != null)
            {
                Destroy(PosibleSelctionTableObject[pair.Key].GetComponent<SelectionComponent>());
            }
        }
        PosibleSelctionTableObject.Clear();
    }
}
