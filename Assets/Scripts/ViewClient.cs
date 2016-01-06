using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class ViewClient : MonoBehaviour {

    const int PORT = 12238;
    const short INTRODUCTION_MSG = 60;
    const short LOCATION_MSG = 61;

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
        client.RegisterHandler(LOCATION_MSG, OnLocationReceive);
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
        client.Send(INTRODUCTION_MSG, new StringMessage("VIEWCLIENT"));
        connected = true;
    }

    public void OnError(NetworkMessage netMsg)
    {
        debugText.text = "error";
    }

    /*
    *   Receive location broadcast from server
    */
    public void OnLocationReceive(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        var msg = netMsg.ReadMessage<StringMessage>();
        debugText.text = "[location] " + id + ": " + msg.value + "\n";
    }


    // Update is called once per frame
    void Update()
    {

	}

    public void Doot()
    {
        if (connected)
            client.Send(LOCATION_MSG, new StringMessage("Dootdoot"));
    }
}
