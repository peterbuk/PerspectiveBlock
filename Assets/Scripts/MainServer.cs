using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class MainServer : MonoBehaviour {

    const int PORT = 12238;
    const short LOCATION_MSG = 55;

    NetworkClient client;

    public Text debugText;
    int messageCount = 0;

    // Use this for initialization
    void Start ()
    {

        NetworkServer.Listen(PORT);
        debugText.text = "Listening at " + Network.player.ipAddress;

        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(LOCATION_MSG, OnLocation);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        NetworkConnection conn = netMsg.conn;
        int id = conn.connectionId;

        debugText.text += "CLIENT#" + id + " has connected";
    }

    public void OnDisconnected(NetworkMessage netMsg)
    {
        NetworkConnection conn = netMsg.conn;
        int id = conn.connectionId;

        debugText.text += "CLIENT#" + id + " has disconnected";
    }


    public void OnLocation(NetworkMessage netMsg)
    {
        NetworkConnection conn = netMsg.conn;
        int id = conn.connectionId;

        var msg = netMsg.ReadMessage<StringMessage>();
        debugText.text += "id#" + id + " msg: " + msg.value + "\n";

        messageCount++;
        if (messageCount > 20)
            debugText.text = "";
    }
	
	// Update is called once per frame
	void Update ()
    {
        
	}
}
