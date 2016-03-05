using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System;
using System.Net;
using UnityEngine.UI;

public class BlockServer : MonoBehaviour
{
    // a data class used to hold the incoming message
    private class MessageContainer
    {
        public byte[] buffer = new byte[BlockServer.PACKET_SIZE];
        public Socket client = BlockServer.server.Client;
    }

    bool debug = false;

    public bool Receiving { get; set; }

    static readonly int PACKET_HEADER_SIZE = 9;
    static readonly int PACKET_SIZE = 1024;  // size of a single packet to send
    static readonly int PAYLOAD_SIZE_FULL = PACKET_SIZE - PACKET_HEADER_SIZE;

    static UdpClient server;
    int port = 12341;

    public Text debugText;

    public SenderClient sender;

    byte[] data = null;         // byte array to store message
    int currentMessage = -1;    // the current msg we are working on
    int receivedBytes = 0;      // total bytes received so far for this msg
    int packetCount = 0; // number of packets received for this msg
    bool messageCompleted = false;
    int dropCount = -1;
    int successCount = 0;

    string update;

    // Use this for initialization
    void Start()
    {
        server = new UdpClient(port);

        server.Client.ReceiveBufferSize = 81920;

        // start receiving data
        this.ReceiveData();
    }



    public void ReceiveData()
    {
        this.Receiving = true;

        MessageContainer msgState = new MessageContainer();

        server.Client.BeginReceive(msgState.buffer, 0, PACKET_SIZE, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), msgState);
    }


    private void ReceiveCallback(IAsyncResult ar)
    {
        if (!this.Receiving)
        {
            return;
        }

        // retrieve the object
        MessageContainer msgState = (MessageContainer)ar.AsyncState;
        Socket client = msgState.client;

        int bytesRead = client.EndReceive(ar);

        try
        {
            if (bytesRead > 0)
            {
                // read packet header
                short messageNum = BitConverter.ToInt16(msgState.buffer, 0);
                short packetNum = BitConverter.ToInt16(msgState.buffer, 2);
                byte messageType = msgState.buffer[4];
                int length = BitConverter.ToInt32(msgState.buffer, 5);

                // convert endianness
                messageNum = System.Net.IPAddress.NetworkToHostOrder(messageNum);
                packetNum = System.Net.IPAddress.NetworkToHostOrder(packetNum);
                length = System.Net.IPAddress.NetworkToHostOrder(length);

                int payloadSize = bytesRead - PACKET_HEADER_SIZE;

                // received a new message, start new data
                if (messageNum != currentMessage)
                {
                    if (!messageCompleted)
                        dropCount++;

                    data = new byte[length];
                    currentMessage = messageNum;
                    receivedBytes = 0;
                    packetCount = 0;
                    messageCompleted = false;
                }

                // first packet insurance, SHOULD NOT RUN
                if (data == null)
                    data = new byte[length];

                // write the payload from the packet into the right position in the data
                Array.Copy(msgState.buffer, PACKET_HEADER_SIZE,
                            data, packetNum * PAYLOAD_SIZE_FULL,
                            payloadSize);

                // count the # of bytes we got
                receivedBytes += payloadSize;
                packetCount++;

                // check if received entire message
                if (receivedBytes == length)
                {
                    messageCompleted = true;
                    successCount++;

                    HandleMessage(messageType);

                    client.BeginReceive(msgState.buffer, 0, PACKET_SIZE, SocketFlags.None,
                             new AsyncCallback(ReceiveCallback), msgState);
                }
                else  // continue reading more data
                {
                    client.BeginReceive(msgState.buffer, 0, PACKET_SIZE, SocketFlags.None,
                             new AsyncCallback(ReceiveCallback), msgState);
                }
            }
        }
        catch (Exception e)
        {
            MainThread.Call(PrintDebug, e);
            Debug.Log("something went wrong " + e);
        }
    }


    private void HandleMessage(byte messageType)
    {
        if (messageType == 1)
        {
            MainThread.Call(ReceivedToggle);
            string msg = ByteToString(data);

            if (msg.Contains("TOGGLE"))
                sender.ToggleStream();
        }
    }

    //DEBUG: print out length of message received
    //MAIN THREAD FUNCTION
    void ReceivedToggle()
    {
        debugText.text = "Received FROM OTHER BLOC!!!";
    }

    //MAIN THREAD FUNCTION
    void PrintDebug(System.Object msg)
    {
        debugText.text += msg.ToString() + "\n";
    }


    public string ByteToString(byte[] bytes)
    {
        char[] chars = new char[bytes.Length / sizeof(char)];
        System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

}
