using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class MainServer : MonoBehaviour {

    const int PORT = 12238;
    const short INTRODUCTION_MSG = 60;
    const short LOCATION_MSG = 61;

    NetworkClient client;
    ArrayList viewClients = new ArrayList();

    public Text debugText;

    // Use this for initialization
    void Start ()
    {

        NetworkServer.Listen(PORT);
        debugText.text = "Listening at " + Network.player.ipAddress + "\n";

        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(INTRODUCTION_MSG, OnJoin);
        NetworkServer.RegisterHandler(LOCATION_MSG, OnLocation);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        debugText.text += "[connect] CLIENT#" + id + " has connected\n";
    }

    public void OnDisconnected(NetworkMessage netMsg)
    {
        int id = netMsg.conn.connectionId;
        debugText.text += "[disconnect] CLIENT#" + id + " has disconnected\n";
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
            debugText.text += "[join] VIEW CLIENT#" + id + "has joined!\n";
        }
        else if (msg.value.Contains("BLOCKCLIENT"))
        {
            debugText.text += "[join] BLOCK CLIENT#" + id + "has joined!\n";
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
        debugText.text += "[location] " + id + ": " + msg.value + "\n";

        // broadcast to all ViewClients
        foreach (int viewClient in viewClients)
        {
            NetworkServer.SendToClient(viewClient, LOCATION_MSG, msg);
        }
    }
	

	// Update is called once per frame
	void Update ()
    {
       
	}
}
