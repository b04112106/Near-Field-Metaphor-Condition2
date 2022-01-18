using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IndiePixel.VR;
using VRTK;
public class OriginalBoundingBox : MonoBehaviour
{
    private Vector3 originalLocalScale;
    private float origianlRadius;
    public bool doUnTouch = true;
    private float scaleFactor = 2f, devideFactor = 1.2f;
    // Start is called before the first frame update
    void Start()
    {
        originalLocalScale = transform.localScale;
        origianlRadius = GetComponent<CapsuleCollider>().radius;
    }

    public void OnTouch()
    {
        originalLocalScale = transform.localScale;
        origianlRadius = GetComponent<CapsuleCollider>().radius;
        transform.localScale = new Vector3(originalLocalScale.x * scaleFactor, originalLocalScale.y, originalLocalScale.z * scaleFactor);
        GetComponent<DoManipulation>().objectToHL.transform.localScale = transform.localScale;
        GetComponent<CapsuleCollider>().radius = origianlRadius / devideFactor;
    }
    public void OnUnTouch()
    {
        if(doUnTouch)
        {
            transform.localScale = originalLocalScale;
            GetComponent<DoManipulation>().objectToHL.transform.localScale = transform.localScale;
        }
        else
        {    
            doUnTouch = true;
        }
        GetComponent<CapsuleCollider>().radius = origianlRadius;
    }
    // Update is called once per frame
    void Update()
    {
    }
}
