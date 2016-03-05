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
    
    public bool FrameOn { get; set; }
    public bool LocationOn { get; set; }
    public bool Connected { get; set; }

    public static readonly byte STRING_MSG = 1;
    public static readonly byte FRAME_MSG = 2;

    UdpClient client;
    public string partnerViewerHostname = "192.168.1.3";    // target of stream

    int port = 12341;
    short messageNum;

    public Text networkText;
    public Text cameraText;
    public Text errorText;
    public InputField partnerViewIP;


    public GameObject target;
    public GameObject ARCamera;

    // Use this for initialization
    void Start ()
    {
        this.FrameOn = false;
        this.LocationOn = true;

        client = new UdpClient();
        messageNum = 0;

        partnerViewIP.text = partnerViewerHostname;
    }

    // Connect to Partner Viewer on Connect button press to stream data
    public void Connect()
    {
        networkText.text = "Connecting to" + partnerViewIP.text;
        errorText.text = "no errors :)";
        client.Connect(partnerViewIP.text, port);
        this.Connected = true;
    }


    public void SendTextMessage(string msg)
    {
        byte[] msgData = StringToBytes(msg);
        networkText.text = "msgsize: " + msgData.Length;
        SendData(msgData, STRING_MSG);
    }
    
    public void SendVideoFrame(byte[] frame)
    {
        networkText.text = "framesize: " + frame.Length;
        SendData(frame, FRAME_MSG);
    }
    

    // toggle between streaming video and location
    public void ToggleStream()
    {
        if (this.FrameOn)
        {
            this.FrameOn = false;
            this.LocationOn = true;
        }
        else
        {
            this.FrameOn = true;
            this.LocationOn = false;
        }
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



