using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class BlockLocationUpdater : MonoBehaviour
{

    const int PORT = 12238;
    const short INTRODUCTION_MSG = 60;
    const short LOCATION_MSG = 61;

    string ip = "192.168.139.110";

    NetworkClient client;
    bool connected = false;

    public Text networkText;
    public Text cameraText;
    public InputField ipAddress;

    public GameObject target;
    public GameObject ARCamera;
    float timer = 1.0f;

    // Use this for initialization
    void Start ()
    {
        client = new NetworkClient();
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Error, OnError);

        ipAddress.text = ip;
    }

	// Update is called once per frame
	void Update ()
    {
        Vector3 cameraLocation = ARCamera.transform.position;
        Vector3 targetLocation = target.transform.position;
        Quaternion cameraRotation = ARCamera.transform.rotation;
        float distance = Vector3.Distance(targetLocation, cameraLocation);


        cameraText.text = "Camera Location: " + cameraLocation.ToString() + "\n" +
            "Camera Rotation: " + cameraRotation.ToString() +
            "\nDistance from target: " + distance +
            "\nTarget location: " + targetLocation.ToString();

        if (connected)
        {
            timer -= Time.deltaTime;
            //if (timer < 0)
           // {
                client.Send(LOCATION_MSG, new StringMessage(
                        cameraLocation.ToString() + " " +
                        distance.ToString()
                    ));
                timer = 0.1f;
            //}
        }

    }

    public void Connect()
    {
        networkText.text = "Connecting to" + ipAddress.text;
        client.Connect(ipAddress.text, PORT);

    }

    /*************************************************************************************
    *************************************************************************************
    *************************************************************************************/

    #region Network Events

    public void OnConnected(NetworkMessage netMsg)
    {
        networkText.text = "Connected to server";
        client.Send(INTRODUCTION_MSG, new StringMessage("BLOCKCLIENT"));
        connected = true;
    }
    public void OnError(NetworkMessage netMsg)
    {
        networkText.text = "error";
    }

    #endregion
}
