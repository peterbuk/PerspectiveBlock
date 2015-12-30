using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MainServer : MonoBehaviour {

    int myReliableChannelId;
    int socketId;
    int socketPort = 12345;
    int connectionId;

    NetworkClient client;

    // Use this for initialization
    void Start ()
    {/*
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        myReliableChannelId = config.AddChannel(QosType.Reliable);
        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        socketId = NetworkTransport.AddHost(topology, socketPort);
        Debug.Log("Socket open. SocketId is:" + socketId);*/


        NetworkServer.Listen(socketPort);
        Debug.Log("Listening");

        //client = new NetworkClient();
        client = ClientScene.ConnectLocalServer();
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(55, DootDoot);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("FINALLY CONNECTED");
    }

    public void DootDoot(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<StringMessage>();
        Debug.Log("msg:" + msg.value);

    }
	
	// Update is called once per frame
	void Update ()
    {
        
	}

    public void Doot()
    {
        NetworkServer.SendToAll(55, new StringMessage("Doot"));
    }

    public void Connect()
    {
        byte error;
        connectionId = NetworkTransport.Connect(socketId, "127.0.0.1", socketPort, 0, out error);

        if (!error.Equals(NetworkError.Ok))
        {
            Debug.Log("failed to connect :(");
        }

    }
}
