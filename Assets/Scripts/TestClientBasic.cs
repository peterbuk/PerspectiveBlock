using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;
using System;

public class TestClientBasic : MonoBehaviour {


    const int PORT = 12238;
    const short INTRODUCTION_MSG = 60;
    const short LOCATION_MSG = 61;
    const short FRAME_MSG = 62;

    string ip = "192.168.1.2";

    NetworkClient client;
    bool connected = false;

    public Text debugText;
    public InputField ipAddress;

    public VideoViewer viewer;



    // Use this for initialization
    void Start()
    {
        ConnectionConfig config = new ConnectionConfig();
        config.AddChannel(QosType.UnreliableFragmented);

        client = new NetworkClient();
        client.Configure(config, 2);
        client.RegisterHandler(LOCATION_MSG, OnLocationReceive);
        client.RegisterHandler(FRAME_MSG, OnFrame);

        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Error, OnError);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnect);

        ipAddress.text = ip;
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

    }


    public void OnFrame(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        var msg = netMsg.ReadMessage<FrameMessage>();
        debugText.text = "[frame] " + id + ": " + msg.frame.Length;
        viewer.LoadFrame(msg.frame);
    }


    // Update is called once per frame
    void Update()
    {

    }

}
