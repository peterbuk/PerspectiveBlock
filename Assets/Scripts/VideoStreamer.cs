using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Vuforia;
using Assets.Scripts;

/*
*   Caputres a video frame from the AR Camera and gives to client to send.
*   Code mostly from Kevin Ta
*/

public class VideoStreamer : MonoBehaviour {

    public GameObject ARCamera;
    public GameObject target;
    public GameObject spotlight;

    public Text debugText;
    public Text cameraText;
    public SenderClient sender;

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

    Texture2D tex;

    // Use this for initialization
    void Start ()
    {
        tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        spotlight = Instantiate(spotlight);
        spotlight.SetActive(true);
        VuforiaBehaviour qcarBehaviour = (VuforiaBehaviour)FindObjectOfType(typeof(VuforiaBehaviour));
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
        }
	}


    /*
    *    Stream AR Camera location data
    */
    void Update()
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
        if (sender.Connected && sender.LocationOn)
        {
            // location update
            string msg = "LOC|" +
                          cameraLocation.x + "|" +
                          cameraLocation.y + "|" +
                          cameraLocation.z + "|" +
                          cameraRotation.x + "|" +
                          cameraRotation.y + "|" +
                          cameraRotation.z + "|" +
                          cameraRotation.w;

            sender.SendTextMessage(msg);
        }

        // spotlight
        spotlight.SetActive(true);
        spotlight.transform.position = this.transform.position;
        spotlight.transform.rotation = this.transform.rotation;
    }

    /*
    *   Stream Video data
    */
    public void OnTrackablesUpdated()
    {
        if (!m_RegisteredFormat)
        {
            bool supported = false;
            m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGB565;
            webcamTextureFormat = TextureFormat.RGB24;


            
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

        if (sender.Connected && sender.FrameOn)
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
                    try
                    {

                        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
                        tex.Apply();
                        //TextureScale.Bilinear(tex, Screen.width/2, Screen.height/2);
                        Texture2D newTex = TextureScale.ScaleTexture(tex, Screen.width / 4, Screen.height /4);
                        byte[] bytes = newTex.EncodeToJPG(clientCompressQuality);
                        sender.SendVideoFrame(bytes);
                        debugText.text = "image (" + newTex.width + "," + newTex.height + ")";

                        //Texture2D tex = new Texture2D(image.Width, image.Height, TextureFormat.RGB565, false);
                       // tex.LoadRawTextureData(image.Pixels);
                        //sender.SendVideoFrame(tex.EncodeToJPG(clientCompressQuality));

                        //image.CopyToTexture(planeTexture);
                        //planeTexture.ReadPixels(new Rect(0, 0, image.Width, image.Height), 0, 0);
                        //planeTexture.Apply();
                        //byte[] bytes = planeTexture.EncodeToPNG();
                        //sender.SendVideoFrame(bytes);



                        // workking
                        //planeTexture.LoadRawTextureData(image.Pixels);
                        //sender.SendVideoFrame(planeTexture.EncodeToJPG(clientCompressQuality));


                    }
                    catch (System.Exception e)
                    {
                        debugText.text = e.ToString();
                    }
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




}
