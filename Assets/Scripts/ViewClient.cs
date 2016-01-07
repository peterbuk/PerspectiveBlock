using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;
using System;

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

    private ViewLocation location;

    // Use this for initialization
    void Start ()
    {
        client = new NetworkClient();
        client.RegisterHandler(LOCATION_MSG, OnLocationReceive);
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Error, OnError);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnect);

        ipAddress.text = ip;
        location = ARCamera.GetComponent<ViewLocation>();
    }


#region Handlers
    public void OnDisconnect(NetworkMessage netMsg)
    {
        debugText.text = "Disconnected! :(";
        connected = false;
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
#endregion

    /*
    *   Connect to server.
    */
    public void Connect()
    {
        debugText.text = "Connecting to" + ipAddress.text;
        client.Connect(ipAddress.text, PORT);
    }


    /*
    *   Receive location broadcast from server
    */
    public void OnLocationReceive(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        var msg = netMsg.ReadMessage<StringMessage>();
        debugText.text = "[location] " + id + ": " + msg.value + "\n";

        Vector3 position = ParsePosition(msg.value);
        Quaternion rotation = ParseRotation(msg.value);

        // render cameralocation
        location.UpdatePointer(position, rotation);
    }


    // read a Vector3 in the form of x|y|z
    private Vector3 ParsePosition(string msg)
    {
        string[] sub = msg.Split('|');
        return new Vector3(float.Parse(sub[0]),
            float.Parse(sub[1]),
            float.Parse(sub[2]));
    }

    private Quaternion ParseRotation(string msg)
    {
        string[] sub = msg.Split('|');

        return new Quaternion(float.Parse(sub[3]),
            float.Parse(sub[4]),
            float.Parse(sub[5]),
            float.Parse(sub[6]));
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
