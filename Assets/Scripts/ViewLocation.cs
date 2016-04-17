using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ViewLocation : MonoBehaviour {

    public Text cameraText;
    public GameObject ARCamera;
    public GameObject target;

    public GameObject floaterPointer;
    public GameObject laserLight;
    public GameObject spotLight;
    public GameObject viewCone;

    private GameObject activeObject;

    private int mode;

    readonly int MODE_FLOATER = 0;
    readonly int MODE_SPOT = 1;
    readonly int MODE_VIEWCONE = 2;


    // Use this for initialization
    void Start()
    {
        floaterPointer = Instantiate(floaterPointer);
        floaterPointer.SetActive(false);

        spotLight = Instantiate(spotLight);
        spotLight.SetActive(false);

        laserLight = Instantiate(laserLight);
        laserLight.SetActive(false);

        viewCone = Instantiate(viewCone);
        viewCone.SetActive(false);

        activeObject = laserLight;

    }


    /*
    *   Update the camera subtle cueing indicator
    */
    public void UpdatePointer(Vector3 position, Quaternion rotation)
    {
        if (mode == MODE_FLOATER)
        {
            floaterPointer.SetActive(true);
            floaterPointer.transform.position = position;
            floaterPointer.transform.rotation = rotation;
            floaterPointer.transform.Rotate(-90, 90, -90);
            //floaterPointer.transform.Rotate(x, y, z);
        }
        else
        {
            floaterPointer.SetActive(false);
        }

        activeObject.SetActive(true);
        activeObject.transform.position = position;
        activeObject.transform.rotation = rotation;

        if (mode == MODE_VIEWCONE)
        {
            

            viewCone.SetActive(true);
            viewCone.transform.position = position;
            viewCone.transform.rotation = rotation;
            viewCone.transform.Rotate(90, 0, 0);

            /* future work: scaling viewcone based on distance
            float distance = Vector3.Distance(target.transform.position, position);

            if (distance < 200)
                viewCone.transform.localScale += new Vector3(0, -0.2f, 0);
            else
                viewCone.transform.localScale += new Vector3(0, 0.1f*distance/100, 0);
            //*/
        }
        else
        {
            viewCone.SetActive(false);
        }



        if (cameraText)
        {
            cameraText.text = "Pointer: " + position.ToString() +
                              "\nRotate:" + rotation.ToString()
            +"\n"+x +"|"+y+"|"+ z;

            cameraText.text += "\nCameraPointer: " + ARCamera.transform.position.ToString() +
                                "\nCameraRotate:" + ARCamera.transform.rotation.ToString();

            cameraText.text += "Mode: " + mode;
        }
    }


    public void TogglePointer()
    {
        mode = ++mode % 3;
        activeObject.SetActive(false); // turn off current object

        if (mode == MODE_FLOATER)
        {
            activeObject = laserLight;
        }
        else if (mode == MODE_SPOT)
        {
            activeObject = spotLight;
        }
        else if (mode == MODE_VIEWCONE)
        {
            activeObject = viewCone;
        }
    }


    // debug config functions
    public float x = 0;
    public float y = 0;
    public float z = 0;

    public void changeX(float inc) { x += inc; }
    public void changeY(float inc) { y += inc; }
    public void changeZ(float inc) { z += inc; }

}
