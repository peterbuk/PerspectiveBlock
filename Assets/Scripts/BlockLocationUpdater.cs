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
    const short FRAME_MSG = 62;

    string ip = "192.168.1.2";
    float timer = 1.0f;

    NetworkClient client;
    public bool connected = false;

    public Text networkText;
    public Text cameraText;
    public InputField ipAddress;

    public GameObject target;
    public GameObject ARCamera;

    // Use this for initialization
    void Start ()
    {
        ConnectionConfig config = new ConnectionConfig();
        config.AddChannel(QosType.UnreliableFragmented);


        client = new NetworkClient();
        client.Configure(config, 2);
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
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
        {   // send location updates
            // TODO: optimize message to include self-id
            timer -= Time.deltaTime;
            //if (timer < 0)
            
                client.Send(LOCATION_MSG, new StringMessage(
                      cameraLocation.x + "|" +
                      cameraLocation.y + "|" +
                      cameraLocation.z + "|" +
                      cameraRotation.x + "|" +
                      cameraRotation.y + "|" +
                      cameraRotation.z + "|" +
                      cameraRotation.w
                  ));
                timer = 1.0f;
            
        }

    }

    public void Connect()
    {
        networkText.text = "Connecting to" + ipAddress.text;
        client.Connect(ipAddress.text, PORT);
    }

    public void DistributeVideoFrame(byte[] frame)
    {
        networkText.text = "framesize: " + frame.Length;
        client.Send(FRAME_MSG, new FrameMessage(frame));
    }

    /*************************************************************************************
    *************************************************************************************
    *************************************************************************************/

#region Network Handlers
    public void OnConnected(NetworkMessage netMsg)
    {
        networkText.text = "Connected to server";
        client.Send(INTRODUCTION_MSG, new StringMessage("BLOCKCLIENT"));
        connected = true;
    }
    public void OnDisconnected(NetworkMessage netMsg)
    {
        networkText.text = "Disconnected!";
        //???netMsg.ReadMessage<ErrorMessage>();

        connected = false;
    }
    public void OnError(NetworkMessage netMsg)
    {
        networkText.text = "error";
    }

#endregion
}
