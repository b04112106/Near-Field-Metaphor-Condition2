using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using IndiePixel.VR;
using HighlightPlus;

public class RotateHandle : MonoBehaviour
{
    public GameObject BoundingBox;
    public GameObject RotateContainer;
    public AudioSource audioSource;
    public GameObject [] Edge;
    public GameObject [] SolidEdge;
    private bool isTouching;
    private bool isNearTouching;
    private GameObject coolObject;
    private GameObject rotateContainerChild;
    private GameObject rightHand;
    private Color edgeHLColor, oriColor;
    private float minFlashAlpha = 0.05f, maxFlashAlpha = 0.5f, touchAlpha = 0.6f, grabAlpha = 0.9f;
    private bool firstPlay = true;
    #region condition 2
    private GameObject menu;
    private bool isRotating = false, firstSwitch = true;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        rightHand = GameObject.Find("RightControllerScriptAlias");
        GetComponent<VRTK_InteractableObject>().usingState = 0;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        transform.parent.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        isTouching = false;
        isNearTouching = false;
        if(name == "HandleY")
        {
            // create empty object
            if(!GameObject.Find("coolObject"))
                coolObject = new GameObject("coolObject");
            else
                coolObject = GameObject.Find("coolObject");
        }
        else 
        {
            if(!GameObject.Find("coolObject1"))
                coolObject = new GameObject("coolObject1");
            else
                coolObject = GameObject.Find("coolObject1");
        }
        oriColor = Edge[0].GetComponent<MeshRenderer>().material.color;
        edgeHLColor = new Color(0.9716981f, 0.7230207f, 0.3345942f, minFlashAlpha);
        if(name == "HandleY")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateY][Controllable][ArtificialBased][RotatorContainer]");    
        } 
        else if(name == "HandleZ")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateZ][Controllable][ArtificialBased][RotatorContainer]");
        }
        else if(name == "HandleX")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateX][Controllable][ArtificialBased][RotatorContainer]");
        }
        rotateContainerChild.GetComponent<VRTK_InteractableObject>().grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
        #region condition 2
        menu = GameObject.Find("RadialMenu_Canvas");
        #endregion
    }
    public void PressTrigger()
    {
        GetComponent<VRTK_InteractableObject>().usingState = 1;
        rotateContainerChild.transform.parent = coolObject.transform;
        for(int i=0; i<4; i++){
            SolidEdge[i].SetActive(false);
            Edge[i].SetActive(true);
            Edge[i].transform.position = SolidEdge[i].GetComponent<DoManipulation>().objectToHL.transform.position;
            Edge[i].transform.localScale = SolidEdge[i].transform.localScale;
            SolidEdge[i].GetComponent<DoManipulation>().objectToHL.SetActive(false);
            // load edge opposite profile
            Edge[i].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("EdgeTouch"));
            Edge[i].GetComponent<HighlightEffect>().highlighted = false;
        }
        isRotating = true;
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
    }
    
    public void ReleaseTrigger()
    {
        GetComponent<VRTK_InteractableObject>().usingState = 0;
        rotateContainerChild.transform.parent = RotateContainer.transform;
        for(int i=0; i<4; i++){
            Edge[i].SetActive(false);
            SolidEdge[i].SetActive(true);
            SolidEdge[i].GetComponent<DoManipulation>().objectToHL.SetActive(true);
            // turn off highlight
            Edge[i].GetComponent<HighlightEffect>().highlighted = false;
        }
        // set edge local position to original
        for(int i=0; i<4; i++)
        {
            Edge[i].transform.position = SolidEdge[i].GetComponent<DoManipulation>().objectToHL.transform.position;
        }
        isRotating = false;
        // rightHand.GetComponent<VRTK_Pointer>().enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
        GameObject r1,r2,r3;
        r1 = GameObject.Find("[VRTK][AUTOGEN][RotateY][Controllable][ArtificialBased][RotatorContainer]");
        r2 = GameObject.Find("[VRTK][AUTOGEN][RotateX][Controllable][ArtificialBased][RotatorContainer]");
        r3 = GameObject.Find("[VRTK][AUTOGEN][RotateZ][Controllable][ArtificialBased][RotatorContainer]");
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 0)
        {
            if(firstSwitch)
            {
                PressTrigger();
                firstSwitch = false;
            }
            if(rotateContainerChild.GetComponent<VRTK_InteractableObject>().IsTouched() && rotateContainerChild.GetComponent<VRTK_InteractableObject>().IsGrabbed())
            {
                // grab edge
                for(int i=0; i<4; i++)
                {
                    Edge[i].GetComponent<MeshRenderer>().material.color = new Color(edgeHLColor.r, edgeHLColor.g, edgeHLColor.b, grabAlpha);
                    Edge[i].transform.localScale = new Vector3(SolidEdge[i].transform.localScale.x * 2f, SolidEdge[i].transform.localScale.y, SolidEdge[i].transform.localScale.z * 2f);
                    Edge[i].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("EdgeGrab"));
                }
                if(firstPlay)
                {
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Grab"));
                    firstPlay = false;
                }
            }
            else if(rotateContainerChild.GetComponent<VRTK_InteractableObject>().IsTouched())
            {
                // touch edge
                for(int i=0; i<4; i++)
                {
                    Edge[i].GetComponent<MeshRenderer>().material.color = new Color(edgeHLColor.r, edgeHLColor.g, edgeHLColor.b, touchAlpha);
                    Edge[i].transform.localScale = new Vector3(SolidEdge[i].transform.localScale.x * 2f, SolidEdge[i].transform.localScale.y, SolidEdge[i].transform.localScale.z * 2f);
                    Edge[i].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("EdgeTouch"));
                    Edge[i].GetComponent<HighlightEffect>().highlighted = true;
                }
                firstPlay = true;
            }
            else
            {
                // untouch edge
                for(int i=0; i<4; i++)
                {
                    Edge[i].GetComponent<MeshRenderer>().material.color = oriColor;
                    Edge[i].transform.localScale = SolidEdge[i].transform.localScale;
                    Edge[i].GetComponent<HighlightEffect>().highlighted = false;
                }
                firstPlay = true;
            }
            if(rotateContainerChild.transform.rotation != BoundingBox.transform.rotation)
                rotateContainerChild.transform.rotation = BoundingBox.transform.rotation;
        }
        else
        {
            if(isRotating)
            {
                ReleaseTrigger();
                firstSwitch = true;
            }
        }
    }
}
