using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ViewClient : MonoBehaviour {

    int myReliableChannelId;
    int socketId;
    int socketPort = 12345;
    int connectionId;

    NetworkClient client;

    // Use this for initialization
    void Start ()
    {
        client = new NetworkClient();
        client.RegisterHandler(55, DootDoot);
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.Connect("127.0.0.1", socketPort);

    }

    public void DootDoot(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<StringMessage>();
        Debug.Log("msg" + msg.value);
        
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("FINALLY CONNECTED!!!");
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Connect()
    {
    }
}
