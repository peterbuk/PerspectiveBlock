using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Sockets;
using UnityEngine.UI;
using System;


/*
    Network framework on Block Client Sender side.
    Can send text messaged and image messages using UDP
*/

public class SenderClient : MonoBehaviour
{
    bool debug = false;

    /*      PACKET HEADER
        ---------------------------------------------------------------------
       | MESSAGE # | PACKET # |  MSG TYPE |  LENGTH      |   DATA PAYLOAD    |
        ---------------------------------------------------------------------
            2            2         1             4            ...
    */
    static readonly int PACKET_SIZE = 1024;  // size of a single packet to send
    static readonly int PACKET_HEADER_SIZE = 9;
    static readonly int PAYLOAD_SIZE = PACKET_SIZE - PACKET_HEADER_SIZE;

    public static readonly byte STRING_MSG = 1;
    public static readonly byte BITMAP_MSG = 2;

    UdpClient client;
    string hostname = "192.168.1.3";
    int port = 12341;
    short messageNum;

    /* multicast
    IPAddress multicastAddress;
    IPEndPoint remoteEP;
    */

    float timer = 1.0f;
    public bool connected = false;
    public Text networkText;
    public Text cameraText;
    public Text errorText;
    public InputField ipAddress;

    public GameObject target;
    public GameObject ARCamera;

    // Use this for initialization
    void Start ()
    {
        client = new UdpClient();
        messageNum = 0;
        ipAddress.text = hostname;
        //ipAddress.text = "127.0.0.1";
        /* MULTICAST (NOT WORKING)
        multicastAddress = IPAddress.Parse("239.0.0.222");
        client.JoinMulticastGroup(multicastAddress);
        remoteEP = new IPEndPoint(multicastAddress, port);
        */
        Connect();
    }

    // Connect to ViewServer on Connect button press
    public void Connect()
    {
        networkText.text = "Connecting to" + ipAddress.text;
        client.Connect(ipAddress.text, port);
        connected = true;
    }

    
    /*
    Send location updates to server constantly
    TODO: move this to more appropriate class
    */
    void Update ()
    {
        Vector3 cameraLocation = ARCamera.transform.position;
        Vector3 targetLocation = target.transform.position;
        Quaternion cameraRotation = ARCamera.transform.rotation;
        float distance = Vector3.Distance(targetLocation, cameraLocation);

        cameraText.text = "Camera Location: " + cameraLocation.ToString() + "\n" +
            "Camera Rotation: " + cameraRotation.ToString() +
            "\nDistance from target: " + distance +
            "\nTarget location: " + targetLocation.ToString();
        
        
        // send location updates
        if (connected)
        {
            timer -= Time.deltaTime;
            //if (timer < 0)

            string msg =  cameraLocation.x + "|" +
                          cameraLocation.y + "|" +
                          cameraLocation.z + "|" +
                          cameraRotation.x + "|" +
                          cameraRotation.y + "|" +
                          cameraRotation.z + "|" +
                          cameraRotation.w;

            //SendData(StringToBytes(msg), STRING_MSG);
            //networkText.text = "sent: " + msg.Length;

            timer = 1.0f;
        }
    }

    public void DistributeVideoFrame(byte[] frame)
    {
        networkText.text = "framesize: " + frame.Length;
        SendData(frame, BITMAP_MSG);
    }


    //****************************************************************************
    //****************************************************************************
    #region NetworkSender

    /*
    *   Send a given byte array to the connected server using async threadpools.
    *
    */
    public void SendData(byte[] data, byte type)
    {
        int length = data.Length;

        // number of packets to divide data into
        int numPackets = (int)Math.Ceiling((double)length / PAYLOAD_SIZE);

        if (debug)
            Console.WriteLine("file length:" + data.Length + ", splitting into " + numPackets + " packets");

        int bytesLeft = length;
        byte[] packet = null;

        for (Int16 i = 0; i < numPackets; i++)
        {
            if (debug)
                Console.WriteLine("### BYTESLEFT: " + bytesLeft);

            if (bytesLeft >= PAYLOAD_SIZE)
                packet = new byte[PACKET_SIZE];
            else
                packet = new byte[bytesLeft + PACKET_HEADER_SIZE];

            // message num 
            packet[0] = (byte)(messageNum >> 8);
            packet[1] = (byte)(messageNum & 255);

            // packet num
            packet[2] = (byte)(i >> 8);
            packet[3] = (byte)(i & 255);

            // packet type
            packet[4] = type;

            // payload length
            packet[5] = (byte)(length >> 24);
            packet[6] = (byte)(length >> 16);
            packet[7] = (byte)(length >> 8);
            packet[8] = (byte)length;

            // copy fragment of data into payload
            int offsetIndex = i * PAYLOAD_SIZE;
            if ((length - offsetIndex) >= PAYLOAD_SIZE)
            {
                Array.Copy(data, offsetIndex, packet, PACKET_HEADER_SIZE, PAYLOAD_SIZE);
            }
            else // remaining data is less than payload size
            {
                Array.Copy(data, offsetIndex, packet, PACKET_HEADER_SIZE, length - offsetIndex);
            }

            try
            {
                client.Client.BeginSend(packet, 0, packet.Length, SocketFlags.None,
                               new AsyncCallback(SendCallback), client.Client);

                /* MULTICAST (NOT WORKING)
                client.Client.BeginSendTo(packet, 0, packet.Length, SocketFlags.Broadcast, remoteEP,
                                     new AsyncCallback(SendCallback), client.Client);

                //non-async
                //client.Send(packet, packet.Length, remoteEP);
                */

                bytesLeft -= packet.Length - PACKET_HEADER_SIZE;

                if (debug)
                    Console.WriteLine("Packet #" + i + "sent of payload length " + (packet.Length - PACKET_HEADER_SIZE));

            }
            catch (SocketException e)
            {
                errorText.text = e.ToString();
            }
        }
        if (debug)
            Console.WriteLine("Sent " + data.Length + " bytes");

        messageNum = (short)(++messageNum % 1024);
    }

    /*
    * Callback for async send
    */
    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;

            int bytesSent = client.EndSend(ar);

            if (debug)
                Console.WriteLine("Sent {0} bytes.", bytesSent);

        }
        catch (Exception e)
        {
            errorText.text = "ERROR= " + e.ToString();
        }
    }


    /* 
    *    Convert a string into byte array
    *    Source: http://stackoverflow.com/a/10380166
    */
    public byte[] StringToBytes(string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public byte[] ReadFile(string filename)
    {
        return File.ReadAllBytes(filename);

    }
    #endregion
}



