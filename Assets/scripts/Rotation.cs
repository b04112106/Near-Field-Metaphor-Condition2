using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IndiePixel.VR;
public class Rotation : MonoBehaviour
{
    private GameObject r1,r2,r3;
    private GameObject menu;
    // Start is called before the first frame update
    void Start()
    {
        r1 = GameObject.Find("[VRTK][AUTOGEN][RotateY][Controllable][ArtificialBased][RotatorContainer]");
        r2 = GameObject.Find("[VRTK][AUTOGEN][RotateX][Controllable][ArtificialBased][RotatorContainer]");
        r3 = GameObject.Find("[VRTK][AUTOGEN][RotateZ][Controllable][ArtificialBased][RotatorContainer]");
        menu = GameObject.Find("RadialMenu_Canvas");
    }
    // Update is called once per frame
    void Update()
    {
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 0)
        {
            if(r1.transform.rotation != transform.rotation)
            {    
                transform.rotation = r1.transform.rotation;
                r2.transform.rotation = transform.rotation;
                r3.transform.rotation = transform.rotation;
            }
            else if(r2.transform.rotation != transform.rotation)
            {    
                transform.rotation = r2.transform.rotation;
                r1.transform.rotation = transform.rotation;
                r3.transform.rotation = transform.rotation;
            }
            else if(r3.transform.rotation != transform.rotation)
            {    
                transform.rotation = r3.transform.rotation;
                r1.transform.rotation = transform.rotation;
                r2.transform.rotation = transform.rotation;
            }
            // if(r1.transform.rotation != transform.rotation && r1.transform.rotation != Quaternion.identity)
            // {    
            //     transform.rotation = r1.transform.rotation;
            //     r2.transform.rotation = transform.rotation;
            //     r3.transform.rotation = transform.rotation;
            // }
            // else if(r2.transform.rotation != transform.rotation && r2.transform.rotation != Quaternion.identity)
            // {    
            //     transform.rotation = r2.transform.rotation;
            //     r1.transform.rotation = transform.rotation;
            //     r3.transform.rotation = transform.rotation;
            // }
            // else if(r3.transform.rotation != transform.rotation && r3.transform.rotation != Quaternion.identity)
            // {    
            //     transform.rotation = r3.transform.rotation;
            //     r1.transform.rotation = transform.rotation;
            //     r2.transform.rotation = transform.rotation;
            // }
        }
    }
}
