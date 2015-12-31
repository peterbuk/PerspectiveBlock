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

    // Use this for initialization
    void Start ()
    {

        NetworkServer.Listen(PORT);
        Debug.Log("Listening");
        debugText.text += "Listening";

        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(LOCATION_MSG, OnLocation);

        debugText.text = Network.player.ipAddress;
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Client has connected");
        debugText.text += "Client has connected";
    }

    public void OnDisconnected(NetworkMessage netMsg)
    {
        Debug.Log("Client has disconnected");
        debugText.text += "Client has disconnected";
    }


    public void OnLocation(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<StringMessage>();
        Debug.Log("msg: " + msg.value);
        debugText.text += msg.value + "\n";
    }
	
	// Update is called once per frame
	void Update ()
    {
        
	}
}
