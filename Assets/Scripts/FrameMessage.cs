using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FrameMessage : MessageBase
{
    public byte[] frame;

    public FrameMessage()
    {

    }

    public FrameMessage(byte[] frame)
    {
        this.frame = frame;
    }
}
