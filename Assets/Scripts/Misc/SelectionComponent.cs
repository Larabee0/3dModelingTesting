using UnityEngine;

public class SelectionComponent : MonoBehaviour
{
    public Material Orignal;
    public Material Selection;
    public bool SetMat;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 heading = Camera.main.transform.position - this.transform.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        Ray ray = new Ray(this.transform.position, direction);
        this.gameObject.GetComponent<Collider>().enabled = false;
        if (Physics.Raycast(ray, out RaycastHit hitInfo, distance + 100.0f))
        {
            if (!hitInfo.transform.gameObject.CompareTag("MainCamera"))
            {
                Debug.Log(hitInfo.transform.gameObject.name);
                Destroy(this);
            }
        }
        this.gameObject.GetComponent<Collider>().enabled = true;
        if (SetMat)
        {
            Orignal = GetComponent<MeshRenderer>().material;
            GetComponent<MeshRenderer>().material = Selection;
        }

    }

    private void OnDestroy()
    {
        //Debug.Log(this.gameObject.name + " SelectionComponent Destroyed");
        if (SetMat)
            GetComponent<MeshRenderer>().material = Orignal;
    }
}
