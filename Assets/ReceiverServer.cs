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

    bool debug = false;

    public bool Receiving { get; set; }

    static readonly int PACKET_HEADER_SIZE = 9;
    static readonly int PACKET_SIZE = 1024;  // size of a single packet to send
    static readonly int PAYLOAD_SIZE_FULL = PACKET_SIZE - PACKET_HEADER_SIZE;

    static UdpClient server;
    int port = 12341;
    bool connected = false;

    public Text debugText;
    public GameObject ARCamera;
    public GameObject targetA;
    public GameObject targetB;

    private VideoViewer viewer;
    private ViewLocation floater;

    private int mode;
    static readonly int MODE_AUGMENTED = 0;
    static readonly int MODE_VIDEO = 1;


    byte[] data = null;         // byte array to store message
    int currentMessage = -1;    // the current msg we are working on
    int receivedBytes = 0;      // total bytes received so far for this msg
    int packetCount = 0; // number of packets received for this msg
    bool messageCompleted = false;
    int dropCount = -1;
    int successCount = 0;

    // Use this for initialization
    void Start()
    {
        viewer = (VideoViewer)this.gameObject.GetComponent<VideoViewer>();
        floater = (ViewLocation)ARCamera.GetComponent<ViewLocation>();
        server = new UdpClient(port);

        server.Client.ReceiveBufferSize = 81920;
        mode = MODE_AUGMENTED;

        // start receiving data
        this.ReceiveData();
    }


    public void ReceiveData()
    {
        this.Receiving = true;

        if (debugText)
            debugText.text = "Ready to receive data.";
        connected = true;

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

                    string err = "dropped msg";
                    MainThread.Call(PrintDebug, err);
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
                    if (debug)
                    {
                        debugText.text += "MSG#" + messageNum + " COMPLETE. Length = " + receivedBytes + "\n";
                        Debug.Log("finished #" + messageNum + " Length=" + receivedBytes);
                    }

                    messageCompleted = true;
                    successCount++;

                    HandleMessage(messageType);

                    //msgState = new MessageContainer();
                    client.BeginReceive(msgState.buffer, 0, PACKET_SIZE, SocketFlags.None,
                             new AsyncCallback(ReceiveCallback), msgState);

                }
                else  // continue reading more data
                {
                    if (debug)
                        debugText.text = "NOTDONE #" + currentMessage + " : " + receivedBytes + "/" + length + "\n";

                    // create new container
                    // msgState = new MessageContainer();
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


    // decide what to do with message
    private void HandleMessage(byte messageType)
    {
        if (messageType == 1)
        {
            MainThread.Call(PrintMsgLength);
            string msg = ByteToString(data);

            if (msg.Contains("LOC"))
                MainThread.Call(LocationUpdate, msg);

            if (msg.Contains("SWAP"))
                MainThread.Call(SwapModel, msg);

        }
        else if (messageType == 2)
        {
            MainThread.Call(PrintMsgLength);
            MainThread.Call(LoadImageFrame);
        }
    }

    /*
    *   Toggle between Augmented Mode and Video Mode
    */
    private void ChangeMode(int newMode)
    {
        try
        {

            if (newMode == MODE_VIDEO)
            {
                viewer.imagePanel.SetActive(true);
                floater.floaterPointer.SetActive(false);
                mode = MODE_VIDEO;
            }
            else if (newMode == MODE_AUGMENTED)
            {
                viewer.imagePanel.SetActive(false);
                floater.floaterPointer.SetActive(true);
                mode = MODE_AUGMENTED;
            }
        }
        catch (Exception e)
        {
            MainThread.Call(PrintDebug, e);
        }
    }

    #region MAINTHREAD FUNCTIONS
    //DEBUG: print out length of message received
    //MAIN THREAD FUNCTION, ONLY CALL FROM MAIN THREAD
    void PrintMsgLength()
    {
        if (debugText)
            debugText.text = "Received " + data.Length + "bytes\n";
    }

    //MAIN THREAD FUNCTION, ONLY CALL FROM MAIN THREAD
    void PrintDebug(System.Object msg)
    {
        if (debugText)
            debugText.text += msg.ToString() + "\n";
    }

    // parse through a location update for floater
    //MAIN THREAD FUNCTION, ONLY CALL FROM MAIN THREAD
    void LocationUpdate(System.Object msg)
    {
        if (mode == MODE_VIDEO)
            ChangeMode(MODE_AUGMENTED);

        string[] values = msg.ToString().Split('|');
        Vector3 position = new Vector3(float.Parse(values[1]),
                                        float.Parse(values[2]),
                                        float.Parse(values[3]));

        Quaternion rotation = new Quaternion(float.Parse(values[4]),
                                            float.Parse(values[5]),
                                            float.Parse(values[6]),
                                            float.Parse(values[7]));

        floater.UpdatePointer(position, rotation);
    }


    // load a video frame data
    //MAIN THREAD FUNCTION, ONLY CALL FROM MAIN THREAD
    void LoadImageFrame()
    {
        if (mode == MODE_AUGMENTED)
            ChangeMode(MODE_VIDEO);

        if (debugText)
            debugText.text = data.Length + "\n";
        viewer.LoadFrame(data);
    }


    private bool aON = true;
    // swap the two house models
    //MAIN THREAD FUNCTION, ONLY CALL FROM MAIN THREAD
    void SwapModel(System.Object msg)
    {
        if (aON)
        {
            targetB.SetActive(true);
            targetA.SetActive(false);
            aON = false;
        }
        else
        {
            targetA.SetActive(true);
            targetB.SetActive(false);
            aON = true;
        }
        /*
        // target letter is found in message[1]
        string[] message = msg.ToString().Split(' ');

        if (message[1].Equals("A"))
        {
            targetA.SetActive(true);
            targetB.SetActive(false);
        }

        else if (message[1].Equals("B"))
        {
            targetB.SetActive(true);
            targetA.SetActive(false);
        }
        //*/
    }
    #endregion

        // helper function
    public string ByteToString(byte[] bytes)
    {
        char[] chars = new char[bytes.Length / sizeof(char)];
        System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

}
