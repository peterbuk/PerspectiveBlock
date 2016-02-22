using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System;
using System.Net;
using UnityEngine.UI;

public class ReceiverServer : MonoBehaviour
{
    // a data class used to hold the incoming message
    private class MessageContainer
    {
        public byte[] buffer = new byte[ReceiverServer.PACKET_SIZE];
        public Socket client = ReceiverServer.server.Client;
    }

    bool debug = true;

    public bool Receiving { get; set; }

    static readonly int PACKET_HEADER_SIZE = 9;
    static readonly int PACKET_SIZE = 1024;  // size of a single packet to send
    static readonly int PAYLOAD_SIZE_FULL = PACKET_SIZE - PACKET_HEADER_SIZE;

    static UdpClient server;
    int port = 12341;
    bool connected = false;

    // multicast
    IPAddress multicastAddress;
    IPEndPoint localEP;

    /*Image drawImage;
    Label countLabel;
    MainWindow gui;*/
    public Text debugText;
    public InputField inputField;
    public GameObject ARCamera;
    //public VideoViewer viewer;

    private ViewLocation location;

    byte[] data = null;         // byte array to store message
    int currentMessage = -1;    // the current msg we are working on
    int receivedBytes = 0;      // total bytes received so far for this msg
    int packetCount = 0; // number of packets received for this msg
    bool messageCompleted = false;
    int dropCount = -1;
    int successCount = 0;


    // Use this for initialization
    void Start ()
    {
        server = new UdpClient(port);

        /* MULTICASTING (NOT WORKING)
        server = new UdpClient();
        localEP = new IPEndPoint(IPAddress.Any, port);

        server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        server.ExclusiveAddressUse = false;

        server.Client.Bind(localEP);
        multicastAddress = IPAddress.Parse("239.0.0.222");
        //server.JoinMulticastGroup(multicastAddress);  not working?
        server.JoinMulticastGroup(multicastAddress, localEP.Address);
        */

        server.Client.ReceiveBufferSize = 81920;
        //location = ARCamera.GetComponent<ViewLocation>();

        // start receiving data
        this.ReceiveData();
    }
	

    public void ReceiveData()
    {
        this.Receiving = true;
        debugText.text = "Ready to receive data.";
        connected = true;

        MessageContainer msgState = new MessageContainer();

        server.Client.BeginReceive(msgState.buffer, 0, PACKET_SIZE, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), msgState);
    }


    private void ReceiveCallback(IAsyncResult ar)
    {
        Debug.Log("packet get");
        
        if (!this.Receiving)
        {
            return;
        }

        // retrieve the object
        MessageContainer msgState = (MessageContainer)ar.AsyncState;
        Socket client = msgState.client;

        int bytesRead = client.EndReceive(ar);

        try {
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

                    if (debug)
                        debugText.text += "NEW MSG#" + messageNum + ":" + packetNum + " RECEIVED. Discarding MSG#" + currentMessage;

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

                if (debug)
                {
                    debugText.text += "Received packet of length " + msgState.buffer.Length;
                    debugText.text += "   [HEADER] #" + messageNum +
                                            " : " + packetNum + " : " + length;
                    debugText.text += "Count: " + packetCount + " received: " + receivedBytes;
                }


                // check if received entire message
                if (receivedBytes == length)
                {
                    //if (debug)
                    debugText.text = "MSG#" + messageNum + " COMPLETE. Length = " + receivedBytes + "\n";

                    messageCompleted = true;
                    successCount++;

                    DrawImage(messageType);

                    msgState = new MessageContainer();
                    client.BeginReceive(msgState.buffer, 0, PACKET_SIZE, SocketFlags.None,
                             new AsyncCallback(ReceiveCallback), msgState);

                }
                else  // continue reading more data
                {
                    if (debug)
                        debugText.text = "NOTDONE #" + currentMessage + " : " + receivedBytes + "/" + length + "\n";

                    // create new container
                    msgState = new MessageContainer();
                    client.BeginReceive(msgState.buffer, 0, PACKET_SIZE, SocketFlags.None,
                             new AsyncCallback(ReceiveCallback), msgState);
                }
            }
        }
        catch (Exception e)
        {
            debugText.text = "WTF IS WRONG WITH THIS" + e.ToString();
        }
        debugText.text = "I should not be here";
    }


    private void DrawImage(byte messageType)
    {
        if (messageType == 1)
        {
            Debug.Log("FUCK");
        }
    }

    public void ReceiveDataBackup()
    {
        byte[] data = null;
        short currentMessage = -1;
        int receivedBytes = 0;
        int packetCount = 0;
        bool messageComplete = true;

        Console.WriteLine("Waiting for data...");

        while (true)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);

            byte[] packet = server.Receive(ref remoteEP);

            // read packet header
            short messageNum = BitConverter.ToInt16(packet, 0);
            short packetNum = BitConverter.ToInt16(packet, 2);
            int length = BitConverter.ToInt32(packet, 4);


            messageNum = System.Net.IPAddress.NetworkToHostOrder(messageNum);
            packetNum = System.Net.IPAddress.NetworkToHostOrder(packetNum);
            length = System.Net.IPAddress.NetworkToHostOrder(length);

            int payloadSize = packet.Length - PACKET_HEADER_SIZE;

            // received a new message, start over
            if (messageNum != currentMessage)
            {
                if (!messageComplete)
                    dropCount++;

                if (debug)
                    Console.WriteLine("NEW MSG#" + messageNum + " RECEIVED. Length = " + length);

                data = new byte[length];
                currentMessage = messageNum;
                receivedBytes = 0;
                packetCount = 0;
                messageComplete = false;
            }


            // write the payload from the packet into the right position in the data
            Array.Copy(packet, PACKET_HEADER_SIZE,
                    data, packetNum * PAYLOAD_SIZE_FULL,
                    payloadSize);

            // count the # of bytes we got
            receivedBytes += payloadSize;
            packetCount++;

            if (debug)
            {
                Console.WriteLine("Received packet of length " + packet.Length);
                Console.WriteLine("   [HEADER] #" + messageNum +
                                        " : " + packetNum + " : " + length);
                Console.WriteLine("Count: " + packetCount + " received: " + receivedBytes);
            }

            // received entire message
            if (receivedBytes == length)
            {
                debugText.text = "MSG#" + messageNum + " COMPLETE. Length = " + receivedBytes;

                messageComplete = true;
                successCount++;

                /*Application.Current.Dispatcher.Invoke(() =>
                {
                    MemoryStream stream = new MemoryStream(data);
                    stream.Seek(0, SeekOrigin.Begin);
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.EndInit();

                    drawImage.Source = image;
                    countLabel.Content = dropCount + " : " + successCount +
                    " (" + (double)dropCount / (double)(successCount + dropCount) * 100 + "% dropped)";

                });*/
            }

        }
    }//end
}
