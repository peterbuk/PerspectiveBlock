using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class ViewClient : MonoBehaviour {

    const int PORT = 12238;
    const short LOCATION_MSG = 55;

    string ip = "192.168.139.110";

    NetworkClient client;
    bool connected = false;

    public Text debugText;
    public InputField ipAddress;

    public GameObject target;
    public GameObject ARCamera;

    // Use this for initialization
    void Start ()
    {
        client = new NetworkClient();
        client.RegisterHandler(LOCATION_MSG, LocationReply);
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Error, OnError);

        ipAddress.text = ip;
    }

    public void Connect()
    {
        debugText.text = "Connecting to" + ipAddress.text;
        client.Connect(ipAddress.text, PORT);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        debugText.text = "Connected to server";
        connected = true;
    }

    public void OnError(NetworkMessage netMsg)
    {
        debugText.text = "error";
    }

    public void LocationReply(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<StringMessage>();
        debugText.text = "reply" + msg.value;
    }


    float timer = 1.0f;
    // Update is called once per frame
    void Update()
    {
        if (connected)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                float distance = Vector3.Distance(target.transform.position, ARCamera.transform.position);
                client.Send(LOCATION_MSG, new StringMessage(distance.ToString()));
                timer = 3;
            }

        }
	}

    public void Doot()
    {
        if (connected)
            client.Send(LOCATION_MSG, new StringMessage("Dootdoot"));
    }
}
