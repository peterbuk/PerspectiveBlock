using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ViewLocation : MonoBehaviour {

    public Text cameraText;
    public GameObject ARCamera;
    public GameObject target;
    public GameObject floaterPointer;
    public GameObject spotlight;

    private LineRenderer line;

    // Use this for initialization
    void Start()
    {
        spotlight = Instantiate(spotlight);
        spotlight.SetActive(false);

        floaterPointer = Instantiate(floaterPointer);
        floaterPointer.SetActive(false);
        line = floaterPointer.GetComponent<LineRenderer>();
    }

    public float x = 0;
    public float y = 0;
    public float z = 0;



    /*
    *   Update the camera subtle cueing indicator
    *   TODO: Transform into spotlight
    */
    public void UpdatePointer(Vector3 position, Quaternion rotation)
    {
        floaterPointer.SetActive(true);
        floaterPointer.transform.position = position;
        floaterPointer.transform.rotation = rotation;
        //floaterPointer.transform.Rotate(0, 90, 270);
        floaterPointer.transform.Rotate(x, y, z);

        spotlight.SetActive(true);
        spotlight.transform.position = position;
        spotlight.transform.rotation = rotation;


        cameraText.text = "Pointer: " + position.ToString() +
                          "\nRotate:" + rotation.ToString() +
                          "\n"+x +"|"+y+"|"+ z;

        cameraText.text += "\nCameraPointer: " + ARCamera.transform.position.ToString() +
                            "\nCameraRotate:" + ARCamera.transform.rotation.ToString();

        /*
        // does something with raycasting that doesn't work yet???
        RaycastHit ray;
        Physics.Raycast(new Ray(cameraPointer.transform.position,
                                        GetForwardVector(cameraPointer.transform.rotation)), 
                                        out ray);

        line.SetPosition(1, ray.point);*/
    }

    Vector3 GetForwardVector(Quaternion q)
    {
        return new Vector3(2 * (q.x * q.z + q.w * q.y),
                           2 * (q.y * q.x - q.w * q.x),
                           1 - 2 * (q.x * q.x + q.y * q.y));
    }

    public void changeX(float inc)
    {
        x += inc;
    }

    public void changeY(float inc)
    {
        y += inc;
    }

    public void changeZ(float inc)
    {
       z += inc;
    }


    // Update is called once per frame
    void Update()
    {
        /*MeshRenderer bgRenderer = GetComponent<MeshRenderer>();
        if (bgRenderer.enabled)
        {
            // switch it off
            bgRenderer.enabled = false;
        }

        debugText.text = "Camera Location: " + ARCamera.transform.position.ToString() + "\n" +
            "Camera Rotation: " + ARCamera.transform.rotation.ToString() +
            "\nDistance from target: " +
            Vector3.Distance(target.transform.position, ARCamera.transform.position).ToString() +
            "\nTarget location: " + target.transform.position.ToString();*/


    }
}
