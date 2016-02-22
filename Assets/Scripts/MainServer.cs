﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class MainServer : MonoBehaviour {

    const int PORT = 12238;
    const short INTRODUCTION_MSG = 60;
    const short LOCATION_MSG = 61;
    const short FRAME_MSG = 62;

    NetworkClient client;
    ArrayList viewClients = new ArrayList();

    public Text debugText;
    bool verboseDebug = true;

    // Use this for initialization
    void Start ()
    {
        ConnectionConfig config = new ConnectionConfig();
        config.AddChannel(QosType.UnreliableFragmented);
        NetworkServer.Configure(config, 5);
        NetworkServer.Listen(PORT);
        GuiTextDebug.debug("Listening at " + Network.player.ipAddress);

        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(INTRODUCTION_MSG, OnJoin);
        NetworkServer.RegisterHandler(LOCATION_MSG, OnLocation);
        NetworkServer.RegisterHandler(FRAME_MSG, OnFrame);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        GuiTextDebug.debug("[connect] CLIENT#" + id + " has connected");
    }

    public void OnDisconnected(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        GuiTextDebug.debug("[disconnect] CLIENT#" + id + " has disconnected");
    }

    /*
    *   Custom join message sent by new clients to identify their type
    */
    public void OnJoin(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        var msg = netMsg.ReadMessage<StringMessage>();

        // add view clients only to broadcast list
        if (msg.value.Contains("VIEWCLIENT"))
        {
            viewClients.Add(id);
            GuiTextDebug.debug("[join] VIEW CLIENT#" + id + "has joined!");
        }
        else if (msg.value.Contains("BLOCKCLIENT"))
        {
            GuiTextDebug.debug("[join] BLOCK CLIENT#" + id + "has joined!");
        }
    }

    /*
    *   Message sent by BlockClients with their location
    *   Broadcast info to all connected ViewClients
    */
    public void OnLocation(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        StringMessage msg = netMsg.ReadMessage<StringMessage>();

        if (verboseDebug)
            GuiTextDebug.debug("[location] " + id + ": " + msg.value);

        // broadcast to all ViewClients
        foreach (int viewClient in viewClients)
        {
            NetworkServer.SendToClient(viewClient, LOCATION_MSG, msg);
        }
    }

    public void OnFrame(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        FrameMessage msg = netMsg.ReadMessage<FrameMessage>();

        if (verboseDebug)
            GuiTextDebug.debug("[frame] " + id + ": " + msg.frame.Length);

        // broadcast to all ViewClients
        foreach (int viewClient in viewClients)
        {
            NetworkServer.SendToClient(viewClient, FRAME_MSG, msg);
        }
    }

	// Update is called once per frame
	void Update ()
    {
	}

    public void ToggleVerbose()
    {
        if (verboseDebug)
            verboseDebug = false;
        else
            verboseDebug = true;
    }
}
