using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
public class EdgeHighlight : MonoBehaviour
{
    public GameObject edge5;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void UnFUCL()
    {

    }
    // Update is called once per frame
    void Update()
    {
        transform.localScale = edge5.transform.localScale;
        transform.GetComponent<MeshRenderer>().material = edge5.transform.GetComponent<MeshRenderer>().material;
    }
}
