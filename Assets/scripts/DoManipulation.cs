using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Controllables;
using IndiePixel.VR;
using HighlightPlus;

public class DoManipulation : MonoBehaviour
{
    public GameObject BoundingBox;
    public GameObject oppsiteObject, objectToHL;
    public Material objectM;
    public GameObject [] resizeFace;
    public GameObject [] resizeEdge;
    public GameObject [] resizeCorner;
    private Color oriColor, hlColor;
    private GameObject RotateHandler, SelectedObject;
    private bool isTranslation = false, isRotation = false, isScaling = false;
    private Quaternion originalRotation;
    private Vector3 originalPosition, originalLocalPosition, originalLocalScale;
    private float ScaleCoefficient = 0.0f;
    private float oriBoundMinSize = 0.0f, oriCollSize = 0.0f;
    private Vector3 oriLocalScale;
    private int flag = 0;
    private int manipulationMode = 0; // 0:default, 1:TX, 2:TY, 3:TZ, 4:RX, 5:RY, 6:RZ, 7:SX, 8:SY, 9:SZ
                                      //            10:TXY, 11:TYZ, 12:TXZ, 13:SXY, 14:SYZ, 15:SXZ
                                      //            16:TXYZ, 17:SXYZ
    //private int dimensionMode = 0; // 0:default, 1:1-dim, 2:2-dim, 3:3-dim
    private int mostRight = 0; // 0:default, 1:face 1(3), 2:face2(4), 3:face0(5)
    private GameObject empty, empty1;
    private GameObject originalParent;
    private char reCh = ' ';
    private float reF = 0.0f;
    private float minFlashAlpha = 0.0f, maxFlashAlpha = 0.4f, touchAlpha = 0.5f, grabAlpha = 0.8f;
    private HighlightEffect effect;
    private GameObject leftHand, rightHand;

    #region condition 2
    private GameObject menu;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        originalLocalPosition = this.gameObject.transform.localPosition;
        originalLocalScale = this.gameObject.transform.localScale;
        SelectedObject = GameObject.FindWithTag("SelectedObject");
        ScaleCoefficient = SelectedObject.GetComponent<PersonalSpace>().ScaleCoefficient;
        RotateHandler = GameObject.Find("RotateHandler");
        leftHand = GameObject.Find("LeftControllerScriptAlias");
        rightHand = GameObject.Find("RightControllerScriptAlias");
        #region condition 2
        menu = GameObject.Find("RadialMenu_Canvas");
        #endregion
        if(!GameObject.Find("empty"))
            empty = new GameObject("empty");
        else    
            empty = GameObject.Find("empty");
        if(!GameObject.Find("empty1"))
            empty1 = new GameObject("empty1");
        else    
            empty1 = GameObject.Find("empty1");
        originalParent = SelectedObject.transform.parent.gameObject;
        // set ignored collider as cooy of object
        if(tag == "Corner" || tag == "Face")
            GetComponent<VRTK_InteractableObject>().ignoredColliders[0] = GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<Collider>();
        oriColor = objectM.color;
        hlColor = oriColor;
        effect = objectToHL.GetComponent<HighlightEffect>();
        oriBoundMinSize = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
        if(tag == "Face")
            oriCollSize = Mathf.Max(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, GetComponent<BoxCollider>().size.z);
        else if(tag == "Corner")
            oriCollSize = GetComponent<SphereCollider>().radius;
        else if(tag == "Edge")
            oriCollSize = GetComponent<CapsuleCollider>().radius;
        oriLocalScale = transform.localScale;
    }
    public void Touch()
    {
        // set material color & HL effect
        if(tag == "Face")
        {
            objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, touchAlpha);
            effect.ProfileLoad(Resources.Load<HighlightProfile>("FaceTouch"));
        }
        else if(tag == "Edge")
        {
            objectToHL.GetComponent<MeshRenderer>().material.color = new Color(0.9716981f, 0.7230207f, 0.3345942f, touchAlpha);
            effect.ProfileLoad(Resources.Load<HighlightProfile>("EdgeTouch"));
        }
        else if(tag == "Corner")
        {
            objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, touchAlpha);
            effect.ProfileLoad(Resources.Load<HighlightProfile>("CornerTouch"));
        }
        effect.highlighted = true;
        // set which controller can interact with target
        if(GetComponent<VRTK_InteractableObject>().GetTouchingObjects()[0].name == "RightControllerScriptAlias")
        {
            GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
            GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
        }
        else if(GetComponent<VRTK_InteractableObject>().GetTouchingObjects()[0].name == "LeftControllerScriptAlias")
        {
            GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.LeftOnly;
            GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.LeftOnly;
        }
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
    }
    public void UnTouch()
    {
        GetComponent<VRTK_InteractableObject>().ResetIgnoredColliders();
        objectToHL.GetComponent<MeshRenderer>().material.color = oriColor;
        effect.highlighted = false;
    }
    public void PressGrip()
    {
        originalPosition = this.gameObject.transform.position;
        originalRotation = this.gameObject.transform.rotation;
        originalLocalPosition = this.gameObject.transform.localPosition;
        #region condition 2
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            isTranslation = true;
            if(this.gameObject.tag == "Face") // 1-dim
            {
                objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                effect.ProfileLoad(Resources.Load<HighlightProfile>("FaceGrab"));
                int index = 5; // number index
                if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3')
                { // X-axis
                    manipulationMode = 1;
                }
                else if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '5')
                { // Y-axis
                    manipulationMode = 2;
                }
                else if(this.gameObject.name[index] == '2' || this.gameObject.name[index] == '4')
                { // Z-axis
                    manipulationMode = 3;
                }
            }
            else if(this.gameObject.tag == "Edge") // 2-dim
            {
                objectToHL.GetComponent<MeshRenderer>().material.color = new Color(0.9716981f, 0.7230207f, 0.3345942f, grabAlpha);
                effect.ProfileLoad(Resources.Load<HighlightProfile>("EdgeGrab"));
                int index = 10; // number index
                if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '2' || this.gameObject.name[index] == '8' || this.gameObject.name.Substring(index) == "10")
                { // X-Y plane
                    manipulationMode = 10;
                }
                else if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3' || this.gameObject.name[index] == '9' || this.gameObject.name.Substring(index) == "11")
                { // Y-Z plane
                    manipulationMode = 11;
                }
                else if(this.gameObject.name[index] == '4' || this.gameObject.name[index] == '5' || this.gameObject.name[index] == '6' || this.gameObject.name[index] == '7')
                { // X-Z plane
                    manipulationMode = 12;
                }
            }
            else if(this.gameObject.tag == "CopyOfObject") // 3-dim
            {
                manipulationMode = 16;
                
            }
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 0) // de rotation
        {

        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7 || menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6) // do uniform/anchored scaling
        { 
            isScaling = true;
            if(this.gameObject.tag == "Face") // 1-dim
            {
                objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                effect.ProfileLoad(Resources.Load<HighlightProfile>("FaceGrab"));
                int index = 5; // number index
                if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3')
                { // X-axis
                    manipulationMode = 7;
                }
                else if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '5')
                { // Y-axis
                    manipulationMode = 8;
                }
                else if(this.gameObject.name[index] == '2' || this.gameObject.name[index] == '4')
                { // Z-axis
                    manipulationMode = 9;
                }
            }
            else if(this.gameObject.tag == "Edge")
            {
                objectToHL.GetComponent<MeshRenderer>().material.color = new Color(0.9716981f, 0.7230207f, 0.3345942f, grabAlpha);
                effect.ProfileLoad(Resources.Load<HighlightProfile>("EdgeGrab"));
                int index = 10; // number index
                if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '2' || this.gameObject.name[index] == '8' || this.gameObject.name.Substring(index) == "10")
                { // X-Y plane
                    manipulationMode = 13;
                }
                else if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3' || this.gameObject.name[index] == '9' || this.gameObject.name.Substring(index) == "11")
                { // Y-Z plane
                    manipulationMode = 14;
                }
                else if(this.gameObject.name[index] == '4' || this.gameObject.name[index] == '5' || this.gameObject.name[index] == '6' || this.gameObject.name[index] == '7')
                { // X-Z plane
                    manipulationMode = 15;
                }
            }
            else if(this.gameObject.tag == "Corner")
            {
                objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                effect.ProfileLoad(Resources.Load<HighlightProfile>("CornerGrab"));
                manipulationMode = 17;
            }
            if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
            {
                empty.transform.position = oppsiteObject.transform.position;
                empty.transform.localScale = new Vector3(1,1,1);
                empty.transform.rotation = BoundingBox.transform.rotation;
                BoundingBox.transform.parent = empty.transform;
                empty1.transform.position = SelectedObject.transform.position + (oppsiteObject.transform.position - BoundingBox.transform.position) / ScaleCoefficient;
                empty1.transform.localScale = new Vector3(1,1,1);
                empty1.transform.rotation = SelectedObject.transform.rotation;
                SelectedObject.transform.parent = empty1.transform;
            }
        }
        #endregion
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
    }
    public void RleaseGrip()
    {
        isTranslation = false;
        isRotation = false;
        isScaling = false;
        manipulationMode = 0;
        // clean to original color
        objectToHL.GetComponent<MeshRenderer>().material.color = oriColor;
        //adjust local position
        transform.localPosition = originalLocalPosition;
        transform.rotation = originalRotation;
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode != 6 && menu.GetComponent<IP_VR_RadialMenu>().menuMode != 7)
        {
            this.gameObject.transform.localScale = originalLocalScale;
        }
        else
        {
            if(reCh == 'X')
                transform.localScale = new Vector3(transform.localScale.x/reF, transform.localScale.y, transform.localScale.z);
            else if(reCh == 'Y')
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y/reF, transform.localScale.z);
            else if(reCh == 'Z')
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z/reF);
            else if(reCh == 'I')
            {
                transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                GetComponent<OriginalBoundingBox>().doUnTouch = false;
            }
            else if(reCh == 'J')
            {
                transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                GetComponent<OriginalBoundingBox>().doUnTouch = false;
            }
            else if(reCh == 'K')
            {
                transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                GetComponent<OriginalBoundingBox>().doUnTouch = false;
            }
            else if(reCh == 'L')
                transform.localScale = new Vector3(transform.localScale.x/reF, transform.localScale.y/reF, transform.localScale.z/reF);
            originalLocalScale = transform.localScale;
            if(tag == "Edge")
                objectToHL.transform.localScale = transform.localScale;
        }
        BoundingBox.transform.parent = null;
        SelectedObject.transform.parent = originalParent.transform;
    }
    private void DoTranslation()
    {
        transform.rotation = originalRotation;
        Vector3 v = transform.position - originalPosition; // controller movement
        Vector3 u = Vector3.zero; // projection vector

        if(manipulationMode == 1) // TX
        {
            u = Vector3.Project(v, BoundingBox.transform.right);
        }
        else if(manipulationMode == 2) // TY
        {
            u = Vector3.Project(v, BoundingBox.transform.up);
        }
        else if(manipulationMode == 3) // TZ
        {
            u = Vector3.Project(v, BoundingBox.transform.forward);
        }
        else if(manipulationMode == 10) // TXY
        {
            u = Vector3.ProjectOnPlane(v, BoundingBox.transform.forward);
        }
        else if(manipulationMode == 11) // TYZ
        {
            u = Vector3.ProjectOnPlane(v, BoundingBox.transform.right);
        }
        else if(manipulationMode == 12) // TXZ
        {
            u = Vector3.ProjectOnPlane(v, BoundingBox.transform.up);
        }
        BoundingBox.transform.Translate(u, Space.World);
        SelectedObject.transform.Translate(u / ScaleCoefficient, Space.World);
        transform.position = originalPosition;
        transform.Translate(u, Space.World);
        originalPosition = transform.position;
    }
    private void DoScaling()
    {
        this.gameObject.transform.rotation = originalRotation;
        Vector3 v = this.gameObject.transform.position - originalPosition; // controller translation vector
        Vector3 u = Vector3.zero; // v project on dir axis
        Vector3 factor = Vector3.zero; // sclaing factor
        Vector3 t = Vector3.zero; // the distance vector
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
            t = originalPosition - empty.transform.position;
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7)
            t = originalPosition - BoundingBox.transform.position;
        float len = t.magnitude;
        reCh = ' ';
        reF = 0.0f;

        if(manipulationMode == 7) // SX
        {
            u = Vector3.Project(v, transform.right);
            if(name == "Face 1")
            {
                if(Vector3.Dot(v, transform.right) > 0 )
                    factor = new Vector3(u.magnitude/len + 1, 1, 1);
                else if(Vector3.Dot(v, transform.right) < 0 )
                    factor = new Vector3(-u.magnitude/len + 1, 1, 1);
            }
            else if(name == "Face 3")
            {                
                if(Vector3.Dot(v, transform.right) > 0 )
                    factor = new Vector3(-u.magnitude/len + 1, 1, 1);
                else if(Vector3.Dot(v, transform.right) < 0 )
                    factor = new Vector3(u.magnitude/len + 1, 1, 1);
            }
            t *= factor.x;
            reCh = 'X';
            reF = factor.x;
        }
        else if(manipulationMode == 8) // SY
        {
            u = Vector3.Project(v, transform.up);
            if(name == "Face 0")
            {
                if(Vector3.Dot(v, transform.up) > 0 )
                    factor = new Vector3(1, u.magnitude/len + 1, 1);
                else if(Vector3.Dot(v, transform.up) < 0 )
                    factor = new Vector3(1, -u.magnitude/len + 1, 1);
            }
            else if(name == "Face 5")
            {                
                if(Vector3.Dot(v, transform.up) > 0 )
                    factor = new Vector3(1, -u.magnitude/len + 1, 1);
                else if(Vector3.Dot(v, transform.up) < 0 )
                    factor = new Vector3(1, u.magnitude/len + 1, 1);
            }
            t *= factor.y;
            reCh = 'Y';
            reF = factor.y;
        }
        else if(manipulationMode == 9) // SZ
        {
            u = Vector3.Project(v, transform.forward);
            if(name == "Face 4")
            {
                if(Vector3.Dot(v, transform.forward) > 0 )
                    factor = new Vector3(1, 1, u.magnitude/len + 1);
                else if(Vector3.Dot(v, transform.forward) < 0 )
                    factor = new Vector3(1, 1, -u.magnitude/len + 1);
            }
            else if(name == "Face 2")
            {                
                if(Vector3.Dot(v, transform.forward) > 0 )
                    factor = new Vector3(1, 1, -u.magnitude/len + 1);
                else if(Vector3.Dot(v, transform.forward) < 0 )
                    factor = new Vector3(1, 1, u.magnitude/len + 1);
            }
            t *= factor.z;
            reCh = 'Z';
            reF = factor.z;
        }
        else if(manipulationMode == 13) // SXY
        {
            Vector3 tmpv = Vector3.zero;
            if(name == "SolidEdge 0")
                tmpv = transform.right - transform.forward;
            else if(name == "SolidEdge 2")
                tmpv = -transform.right - transform.forward;
            else if(name == "SolidEdge 8")
                tmpv = transform.right + transform.forward;
            else if(name == "SolidEdge 10")
                tmpv = -transform.right + transform.forward;
            u = Vector3.Project(v, tmpv);
            if(Vector3.Dot(v, tmpv) > 0)
                factor = new Vector3(u.magnitude/len + 1, u.magnitude/len + 1, 1);
            else if(Vector3.Dot(v, tmpv) < 0)
                factor = new Vector3(-u.magnitude/len + 1, -u.magnitude/len + 1, 1);
            t *= factor.x;
            reCh = 'I';
            reF = factor.x;
        }
        else if(manipulationMode == 14) // SYZ
        {
            Vector3 tmpv = Vector3.zero;
            if(name == "SolidEdge 1")
                tmpv = transform.right - transform.forward;
            else if(name == "SolidEdge 3")
                tmpv = -transform.right - transform.forward;
            else if(name == "SolidEdge 9")
                tmpv = transform.right + transform.forward;
            else if(name == "SolidEdge 11")
                tmpv = -transform.right + transform.forward;
            u = Vector3.Project(v, tmpv);
            if(Vector3.Dot(v, tmpv) > 0)
                factor = new Vector3(1, u.magnitude/len + 1, u.magnitude/len + 1);
            else if(Vector3.Dot(v, tmpv) < 0)
                factor = new Vector3(1, -u.magnitude/len + 1, -u.magnitude/len + 1);
            t *= factor.y;
            reCh = 'J';
            reF = factor.y;
        }
        else if(manipulationMode == 15) // SXZ
        {
            Vector3 tmpv = Vector3.zero;
            if(name == "SolidEdge 4")
                tmpv = transform.right - transform.forward;
            else if(name == "SolidEdge 5")
                tmpv = -transform.right - transform.forward;
            else if(name == "SolidEdge 6")
                tmpv = -transform.right + transform.forward;
            else if(name == "SolidEdge 7")
                tmpv = transform.right + transform.forward;
            u = Vector3.Project(v, tmpv);
            if(Vector3.Dot(v, tmpv) > 0)
                factor = new Vector3(u.magnitude/len + 1, 1, u.magnitude/len + 1);
            else if(Vector3.Dot(v, tmpv) < 0)
                factor = new Vector3(-u.magnitude/len + 1, 1, -u.magnitude/len + 1);
            t *= factor.x;
            reCh = 'K';
            reF = factor.x;
        }
        else if(manipulationMode == 17) // SXYZ
        {
            Vector3 tmpv = Vector3.zero;
            tmpv = transform.right + transform.forward + transform.up;
            u = Vector3.Project(v, tmpv);
            if(Vector3.Dot(v, tmpv) > 0)
                factor = new Vector3(u.magnitude/len + 1, u.magnitude/len + 1, u.magnitude/len + 1);
            else if(Vector3.Dot(v, tmpv) < 0)
                factor = new Vector3(-u.magnitude/len + 1, -u.magnitude/len + 1, -u.magnitude/len + 1);
            t *= factor.x;
            reCh = 'L';
            reF = factor.x;
        }

        // change bounding box & selected object size, and move the target
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7) // do uniform scaling
        {
            if(factor.x != Mathf.Infinity && factor.x != -Mathf.Infinity && factor.x != 0 && factor.y != Mathf.Infinity && factor.y != -Mathf.Infinity && factor.y != 0 && factor.z != Mathf.Infinity && factor.z != -Mathf.Infinity && factor.z != 0)
            {
                BoundingBox.transform.localScale = Vector3.Scale(BoundingBox.transform.localScale, factor);
                SelectedObject.transform.localScale = Vector3.Scale(SelectedObject.transform.localScale, factor);
                if(BoundingBox.transform.localScale.x <= 0 || BoundingBox.transform.localScale.y <= 0 || BoundingBox.transform.localScale.z <= 0)
                {
                    BoundingBox.transform.localScale = new Vector3(BoundingBox.transform.localScale.x / factor.x, BoundingBox.transform.localScale.y / factor.y, BoundingBox.transform.localScale.z / factor.z);
                    SelectedObject.transform.localScale = new Vector3(SelectedObject.transform.localScale.x / factor.x, SelectedObject.transform.localScale.y / factor.y, SelectedObject.transform.localScale.z / factor.z);
                    return;
                }
                transform.position = originalPosition;
                transform.position = BoundingBox.transform.position + t;
                originalPosition = transform.position;
                resize(reCh, reF);
            }
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6) // do anchored scaling
        {
            if(factor.x != Mathf.Infinity && factor.x != -Mathf.Infinity && factor.x != 0 && factor.y != Mathf.Infinity && factor.y != -Mathf.Infinity && factor.y != 0 && factor.z != Mathf.Infinity && factor.z != -Mathf.Infinity && factor.z != 0)
            {
                empty.transform.localScale = Vector3.Scale(empty.transform.localScale, factor);
                empty1.transform.localScale = Vector3.Scale(empty1.transform.localScale, factor);
                if(empty.transform.localScale.x < 0 || empty.transform.localScale.y < 0 || empty.transform.localScale.z < 0)
                {
                    empty.transform.localScale = new Vector3(empty.transform.localScale.x / factor.x, empty.transform.localScale.y / factor.y, empty.transform.localScale.z / factor.z);
                    empty1.transform.localScale = new Vector3(empty1.transform.localScale.x / factor.x, empty1.transform.localScale.y / factor.y, empty1.transform.localScale.z / factor.z);
                    return;
                }
                transform.position = originalPosition;
                transform.position = empty.transform.position + t;
                originalPosition = transform.position;
                resize(reCh, reF);
            }
        }
    }
    private void DoRotation()
    {
        // use VRTK_ArtificialRotator to do rotation
        // rotate handle-bar event control in RotateHandle.cs
    }
    private void resize(char ch, float f)
    {
        if(ch == 'X')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x/f, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z/f);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
        }
        else if(ch == 'Y')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y/f, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
        }
        else if(ch == 'Z')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z/f);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z/f);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
        }
        else if(ch == 'I')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(resizeFace[i].name == "Face 1" || resizeFace[i].name == "Face 3")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x/f, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z);
                else if(resizeFace[i].name == "Face 0" || resizeFace[i].name == "Face 5")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y/f, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(resizeEdge[i].name == "SolidEdge 0" || resizeEdge[i].name == "SolidEdge 2" || resizeEdge[i].name == "SolidEdge 8" || resizeEdge[i].name == "SolidEdge 10")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z/f);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
        }
        else if(ch == 'J')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(resizeFace[i].name == "Face 2" || resizeFace[i].name == "Face 4")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z/f);
                else if(resizeFace[i].name == "Face 0" || resizeFace[i].name == "Face 5")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y/f, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(resizeEdge[i].name == "SolidEdge 0" || resizeEdge[i].name == "SolidEdge 2" || resizeEdge[i].name == "SolidEdge 8" || resizeEdge[i].name == "SolidEdge 10")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z/f);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
        }
        else if(ch == 'K')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(resizeFace[i].name == "Face 2" || resizeFace[i].name == "Face 4")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z/f);
                else if(resizeFace[i].name == "Face 1" || resizeFace[i].name == "Face 3")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x/f, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(resizeEdge[i].name == "SolidEdge 0" || resizeEdge[i].name == "SolidEdge 2" || resizeEdge[i].name == "SolidEdge 8" || resizeEdge[i].name == "SolidEdge 10")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                else if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z/f);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z/f);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
        }
        else if(ch == 'L')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(resizeFace[i].name == "Face 2" || resizeFace[i].name == "Face 4")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z/f);
                else if(resizeFace[i].name == "Face 1" || resizeFace[i].name == "Face 3")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x/f, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z);
                else if(resizeFace[i].name == "Face 0" || resizeFace[i].name == "Face 5")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y/f, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(resizeEdge[i].name == "SolidEdge 0" || resizeEdge[i].name == "SolidEdge 2" || resizeEdge[i].name == "SolidEdge 8" || resizeEdge[i].name == "SolidEdge 10")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z/f);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z/f);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
        }
    }
    private float setValue(char ch)
    {
        float max = 0.0f;
        //maxX = 0.0f; maxY = 0.0f; maxZ = 0.0f;
        int dir = maxDir(ch);
        if(dir == 1)
            max = this.gameObject.transform.position.x - originalPosition.x;
        else if(dir == 2)
            max = this.gameObject.transform.position.y - originalPosition.y;
        else if(dir == 3)
            max = this.gameObject.transform.position.z - originalPosition.z;
        max *= flag;
        return max;
    }
    private int maxDir(char ch)
    {
        float max = -Mathf.Infinity;
        int maxID = 0;
        flag = 1;
        for(int i=0; i<3; i++)
        {
            if(ch == 'X')
            {
                if( Mathf.Abs(this.gameObject.transform.right[i]) > max)
                {
                    max = Mathf.Abs(this.gameObject.transform.right[i]);
                    maxID = i + 1;
                    if(this.gameObject.transform.right[i] < 0)
                        flag = -1;
                    else 
                        flag = 1;
                }
            }
            else if(ch == 'Y')
            {
                if( Mathf.Abs(this.gameObject.transform.up[i]) > max)
                {
                    max = Mathf.Abs(this.gameObject.transform.up[i]);
                    maxID = i + 1;
                    if(this.gameObject.transform.up[i] < 0)
                        flag = -1;
                    else 
                        flag = 1;
                }
            }
            else if(ch == 'Z')
            {
                if( Mathf.Abs(this.gameObject.transform.forward[i]) > max)
                {
                    max = Mathf.Abs(this.gameObject.transform.forward[i]);
                    maxID = i + 1;
                    if(this.gameObject.transform.forward[i] < 0)
                        flag = -1;
                    else 
                        flag = 1;
                }
            }
        }
        return maxID;
    }
    // Update is called once per frame
    void Update()
    {
        if(isTranslation)
        {
            DoTranslation();
        }
        else if(isRotation)
        {
            DoRotation();
        }
        else if(isScaling)
        {
            DoScaling();
            // resize FaceHL, SolidEdgeHL
            if(tag == "Face")
                objectToHL.transform.localScale = oppsiteObject.GetComponent<DoManipulation>().objectToHL.transform.localScale;
            else if(tag == "Edge")
                objectToHL.transform.localScale = new Vector3(oppsiteObject.transform.localScale.x*2f, oppsiteObject.transform.localScale.y, oppsiteObject.transform.localScale.z*2f);
            else if(tag == "Corner")
                objectToHL.transform.localScale = resizeCorner[0].GetComponent<DoManipulation>().objectToHL.transform.localScale;
        }
        #region condition 2
        // set primitive's collider and mesh renderer enable
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == -1) // default mode
        {
            GetComponent<Collider>().enabled = false;
            if(tag == "Face" || tag == "Corner")
            {
                GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = false;
            }
            else
                GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = true;
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 0) // rotation mode
        {
            GetComponent<Collider>().enabled = false;
            GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = false;
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1) // translation mode
        {
            if(tag == "Face" || tag == "Edge")
            {
                GetComponent<Collider>().enabled = true;
                GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                GetComponent<Collider>().enabled = false;
                GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else // uniform or anchored scaling mode
        {
            GetComponent<Collider>().enabled = true;
            GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = true;
        }
        #endregion
        // adjust primitive collider size
        if(tag == "Face")
        {
            var min = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
            if(name == "Face 0" || name == "Face 5")
                GetComponent<BoxCollider>().size = new Vector3(0.8f, oriCollSize * min/oriBoundMinSize, 0.8f);
            else if(name == "Face 1" || name == "Face 3")
                GetComponent<BoxCollider>().size = new Vector3(oriCollSize * min/oriBoundMinSize, 0.8f, 0.8f);
            else if(name == "Face 2" || name == "Face 4")
                GetComponent<BoxCollider>().size = new Vector3(0.8f, 0.8f, oriCollSize * min/oriBoundMinSize);
        }
        else if(tag == "Corner")
        {
            var min = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
            GetComponent<SphereCollider>().radius = oriCollSize * (min / oriBoundMinSize);
        }
        else if(tag == "Edge")
        {
            var min = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
            GetComponent<CapsuleCollider>().radius = oriCollSize * (min / oriBoundMinSize);
        }
    }
}
