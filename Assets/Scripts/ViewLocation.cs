using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ViewLocation : MonoBehaviour {

    public Text debugText;
    public GameObject ARCamera;
    public GameObject target;

    // Use this for initialization
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        /*MeshRenderer bgRenderer = GetComponent<MeshRenderer>();
        if (bgRenderer.enabled)
        {
            // switch it off
            bgRenderer.enabled = false;
        }*/

        debugText.text = "Camera Location: " + ARCamera.transform.position.ToString() + "\n" +
            "Camera Rotation: " + ARCamera.transform.rotation.ToString() +
            "\nDistance from target: " +
            Vector3.Distance(target.transform.position, ARCamera.transform.position).ToString() +
            "\nTarget location: " + target.transform.position.ToString();
    }
}
