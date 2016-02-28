using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Vuforia;

/*
*   Caputres a video frame from the AR Camera and gives to client to send.
*   Code mostly from Kevin Ta
*/

public class VideoStreamer : MonoBehaviour {

    public Text debugText;
    public SenderClient blockClient;

    [Header("Server Only")]
    public string textureBufferCameraName = "TextureBufferCamera";
    [Tooltip("Frames per second to stream at. Min 1.")]
    public int streamingFramerate = 30;
    [Tooltip("Percentage quality per frame")]
    public int clientCompressQuality = 25;

    private Vuforia.Image.PIXEL_FORMAT m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGB565;
    private TextureFormat webcamTextureFormat = TextureFormat.RGB565;
    private bool m_RegisteredFormat = false;
    private bool m_LogInfo = true;
    private bool initialized = false;

    private Texture2D planeTexture;
    private int texWidth = 640;
    private int texHeight = 480;
    private float nextFrame = 0.0f;

    // Use this for initialization
    void Start () {

        VuforiaBehaviour qcarBehaviour = (VuforiaBehaviour)FindObjectOfType(typeof(VuforiaBehaviour));
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
        }
	
	}

    public void OnTrackablesUpdated()
    {
        if (!m_RegisteredFormat)
        {
            bool supported = false;
            m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGB565;
            webcamTextureFormat = TextureFormat.RGB565;
            //Try all the modes yo. Not all devices will support all the pixel formats, will try color until greyscale
            while (supported == false)
            {
                supported = CameraDevice.Instance.SetFrameFormat(m_PixelFormat, true);
                if (supported == false)
                {
                    switch (m_PixelFormat)
                    {
                        case Vuforia.Image.PIXEL_FORMAT.RGB565:
                            m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGB888;
                            webcamTextureFormat = TextureFormat.RGB24;                  //untested
                            break;
                        case Vuforia.Image.PIXEL_FORMAT.RGB888:
                            m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGBA8888;
                            webcamTextureFormat = TextureFormat.RGBA32;                 //untested
                            break;
                        case Vuforia.Image.PIXEL_FORMAT.RGBA8888:
                            m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.YUV;
                            webcamTextureFormat = TextureFormat.YUY2;                   //untested
                            break;
                        case Vuforia.Image.PIXEL_FORMAT.YUV:
                            m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.GRAYSCALE;
                            webcamTextureFormat = TextureFormat.Alpha8;                 //untested
                            break;
                        case Vuforia.Image.PIXEL_FORMAT.GRAYSCALE:
                            debugText.text += "Cannot find a supported frame format.";
                            m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.UNKNOWN_FORMAT;
                            webcamTextureFormat = TextureFormat.RGBA32;
                            supported = true;
                            break;
                    }
                }
            }
            m_RegisteredFormat = true;
            debugText.text += "Image format registered as: " + m_PixelFormat.ToString();
        }

        //need to intialize after we get an image, run once
        if (initialized == false)
        {

            InitializeTextureCameraCapture();
            return;
        }

        if (blockClient.connected)
        {
            if (Time.time > nextFrame)
            {
                nextFrame = Time.time + (1.0f / (float)(streamingFramerate < 1 ? 1 : streamingFramerate));

                CameraDevice cam = CameraDevice.Instance;
                Vuforia.Image image = cam.GetCameraImage(m_PixelFormat);
                if (image == null)
                {
                    debugText.text += m_PixelFormat + " image is not available yet";
                }
                else
                {
                    
                    planeTexture.LoadRawTextureData(image.Pixels);
                    blockClient.DistributeVideoFrame(planeTexture.EncodeToJPG(clientCompressQuality));
                    
                    //blockClient.DistributeVideoFrame(image.Pixels);
                }
            }
        }
    }


    void InitializeTextureCameraCapture()
    {

        CameraDevice cam = CameraDevice.Instance;
        Vuforia.Image image = cam.GetCameraImage(m_PixelFormat);
        if (image == null)
        {
            Debug.Log(m_PixelFormat + " image is not available yet");
        }
        else
        {
            int width = image.Width;
            int height = image.Height;

            //width = 240;
            //height = 120;

            InitializeWebcamTexture(width, height, webcamTextureFormat);
            initialized = true;
            debugText.text += "Camera resolution: (" + width + "," + height + ")";
        }
    }

    //Initial the webcam texture with the given width and height
    void InitializeWebcamTexture(int texWidth, int texHeight, TextureFormat format)
    {
        this.texWidth = texWidth;
        this.texHeight = texHeight;


        planeTexture = new Texture2D(texWidth, texHeight, format, false);

        for (int i = 0; i < texHeight; i++)
        {
            for (int j = 0; j < texWidth; j++)
            {
                planeTexture.SetPixel(j, i, Color.green);
            }
        }
    }



    // Update is called once per frame
    void Update () {
	
	}
}
