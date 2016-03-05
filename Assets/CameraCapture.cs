using UnityEngine;
using System.Collections;
using Vuforia;

/*
    My current custom attempt (not working) at sending camera data
    Maybe its just not regular jpg and needs encoding
*/

public class CameraCapture : MonoBehaviour
{
    public SenderClient client;
    public UnityEngine.UI.Text streamText;
    public UnityEngine.UI.Text errorText;

    private Image.PIXEL_FORMAT m_PixelFormat = Image.PIXEL_FORMAT.GRAYSCALE;
    private bool m_RegisteredFormat = false;


    // Use this for initialization
    void Start ()
    {
        VuforiaBehaviour qcarBehaviour = (VuforiaBehaviour)FindObjectOfType(typeof(VuforiaBehaviour));
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
            streamText.text = "ready to go";
        }
        /*
        CameraDevice cam = CameraDevice.Instance;
        cam.Stop();
        cam.Deinit();
        cam.Init(CameraDevice.CameraDirection.CAMERA_BACK);
        CameraDevice.Instance.SelectVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_SPEED);
        cam.Start();
        */
    }


    void Update()
    {

    }


    float timer = 1.0f;

    public void OnTrackablesUpdated()
    {
        if (!m_RegisteredFormat)
        {
            CameraDevice.Instance.SetFrameFormat(m_PixelFormat, true);
            m_RegisteredFormat = true;
        }


        if (client.Connected /*&& timer < 0*/)
        {
            CameraDevice cam = CameraDevice.Instance;
            Image image = cam.GetCameraImage(m_PixelFormat);
            if (image == null)
            {
                errorText.text = m_PixelFormat + " image is not available yet; client =" + client.Connected;
            }
            else
            {
                string s = m_PixelFormat + " image: \n";
                s += "  size: " + image.Width + "x" + image.Height + "\n";
                s += "  bufferSize: " + image.BufferWidth + "x" + image.BufferHeight + "\n";
                s += "  stride: " + image.Stride;
                s += "  size: " + image.Pixels.Length;

                byte[] data = image.Pixels;

                client.SendVideoFrame(image.Pixels);
                streamText.text = s;
            }
            timer = 0.33f;
        }

        timer -= Time.deltaTime;
        
    }
}
