using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ViewLocation : MonoBehaviour {

    public Text cameraText;
    public GameObject ARCamera;
    public GameObject target;
    public GameObject cameraPointer;

    private LineRenderer line;

    // Use this for initialization
    void Start()
    {
        cameraPointer = Instantiate(cameraPointer);
        cameraPointer.SetActive(false);
        line = cameraPointer.GetComponent<LineRenderer>();
    }


    public void UpdatePointer(Vector3 position, Quaternion rotation)
    {
        cameraPointer.SetActive(true);
        cameraPointer.transform.position = position;
        cameraPointer.transform.rotation = rotation;
        cameraPointer.transform.Rotate(-90, 0, 0);

        cameraText.text = "Pointer: " + position.ToString() +
                          "\nRotate:" + rotation.ToString();

        RaycastHit ray;

        Physics.Raycast(new Ray(cameraPointer.transform.position,
                                        GetForwardVector(cameraPointer.transform.rotation)), 
                                        out ray);

        line.SetPosition(1, ray.point);
    }

    Vector3 GetForwardVector(Quaternion q)
    {
        return new Vector3(2 * (q.x * q.z + q.w * q.y),
                           2 * (q.y * q.x - q.w * q.x),
                           1 - 2 * (q.x * q.x + q.y * q.y));
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
