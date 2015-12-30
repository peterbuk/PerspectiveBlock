using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Vuforia;

[RequireComponent(typeof (NetworkView))]
public class WebcamStreamer : MonoBehaviour {


	[Header("Server Only")]
	public string textureBufferCameraName = "TextureBufferCamera";
	[Tooltip("Frames per second to stream at. Min 1.")]
	public int streamingFramerate = 30;
	[Tooltip("Percentage quality per frame")]
	public int clientCompressQuality = 75;


	[Header("Client Only")]
	[Tooltip("Maximum seconds to clip off old frames (milliseconds)")]
	public int maxDelay = 500;


	[Header("Initialization")]
	[Tooltip("Warning: Do not use a image object that contains this script. This object is optional")]
	public UnityEngine.UI.Image displayImage;
	public bool showWebcamTexture = true;
	[Tooltip("Sets whether this object has the hosting camera, or receiving the camera stream")]
	public bool isServer = false;					//does not imply Network.connectiontype == server

	[Header("Camera Synchronization")]
	[Tooltip("The camera to update the transform of. Assume this camera has no network view. Server will use this as a host object to observe, clients will observe the transform.")]
	public GameObject cameraUpdate;
	public NetworkStateSynchronization stateSynchronization = NetworkStateSynchronization.Unreliable;


	private Camera watchingCamera;


	private Texture2D planeTexture;
	private int texWidth = 640;
	private int texHeight = 480;


	private NetworkView netView;


	private Vuforia.Image.PIXEL_FORMAT m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGB565;
	private TextureFormat webcamTextureFormat = TextureFormat.RGB565;
	private bool m_RegisteredFormat = false;
	private bool m_LogInfo = true;


	private bool initialized = false;


	// Use this for initialization
	void Start () {
		netView = this.gameObject.GetComponent<NetworkView> ();

		initialized = false;


		//generate a network view for the camera to sync
		if (cameraUpdate != null) {
			NetworkView camNetView = cameraUpdate.AddComponent<NetworkView> ();
			camNetView.observed = cameraUpdate.transform;
			camNetView.stateSynchronization = stateSynchronization;
		}


		//register for camera updates
		if (isServer == true){
			//camNetView.viewID = Network.AllocateViewID ();
			VuforiaBehaviour qcarBehaviour = (VuforiaBehaviour) FindObjectOfType(typeof(VuforiaBehaviour));
			if (qcarBehaviour)
			{
				qcarBehaviour.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
			}
		}


		if (cameraUpdate == null) {
			Debug.LogWarning("Warning, inspector field Camera Update not set. Set to main camera and turn off synchronization if nothing else.");
		}

	}

	//https://developer.vuforia.com/library/articles/Solution/How-To-Access-the-Camera-Image-in-Unity
	//the webcam stream without augmentations
	public void OnTrackablesUpdated()
	{

		if (isServer == false) {
			return;
		}

		//find the right format to grab from scene.
		if (!m_RegisteredFormat)
		{

			bool supported = false;
			m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGB565;
			webcamTextureFormat = TextureFormat.RGB565;
			//Try all the modes yo. Not all devices will support all the pixel formats, will try color until greyscale
			while (supported == false){
				
				supported = CameraDevice.Instance.SetFrameFormat(m_PixelFormat, true);
				if (supported == false){
					switch (m_PixelFormat){
					case Vuforia.Image.PIXEL_FORMAT.RGB565:
						m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGB888;
						webcamTextureFormat = TextureFormat.RGB24;					//untested
						break;
					case Vuforia.Image.PIXEL_FORMAT.RGB888:
						m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.RGBA8888;
						webcamTextureFormat = TextureFormat.RGBA32;					//untested
						break;
					case Vuforia.Image.PIXEL_FORMAT.RGBA8888:
						m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.YUV;
						webcamTextureFormat = TextureFormat.YUY2;					//untested
						break;
					case Vuforia.Image.PIXEL_FORMAT.YUV:
						m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.GRAYSCALE;
						webcamTextureFormat = TextureFormat.Alpha8;					//untested
						break;
					case Vuforia.Image.PIXEL_FORMAT.GRAYSCALE:
						Debug.LogWarning("Cannot find a supported frame format.");
						m_PixelFormat = Vuforia.Image.PIXEL_FORMAT.UNKNOWN_FORMAT;
						webcamTextureFormat = TextureFormat.RGBA32;
						supported = true;
						break;
					}
					
				}
			}


			m_RegisteredFormat = true;
			Debug.Log("Image format registered as: " + m_PixelFormat.ToString());
		}

		//need to intialize after we get an image, run once
		if (initialized == false) {

			InitializeTextureCameraCapture ();
			return;
		} 



		//send frames if server started
		if (isServer== true && Network.connections.Length > 0) {
			
			if (Time.time > nextFrame){
				nextFrame = Time.time + (1.0f/ (float) (streamingFramerate < 1 ? 1 : streamingFramerate));
				
				
				CameraDevice cam = CameraDevice.Instance;
				Vuforia.Image image = cam.GetCameraImage(m_PixelFormat);
				if (image == null)
				{
					Debug.Log(m_PixelFormat + " image is not available yet");
				}
				else
				{
					
					//image.CopyToTexture(planeTexture);
					planeTexture.LoadRawTextureData(image.Pixels);
					DistributeVideoFrame(planeTexture.EncodeToJPG(clientCompressQuality));
					//DistributeVideoFrame(image.Pixels);// (clientCompressQuality));


					if (showWebcamTexture == true){
						planeTexture.Apply ();
					}



					/*
					string s = m_PixelFormat + " image: \n";
					s += "  size: " + image.Width + "x" + image.Height + "\n";
					s += "  bufferSize: " + image.BufferWidth + "x" + image.BufferHeight + "\n";
					s += "  stride: " + image.Stride;
					Debug.Log(s);
					m_LogInfo = false;
					*/
				}
				
				
				
			}
			
		}

			

	}

	void InitializeTextureCameraCapture(){

		CameraDevice cam = CameraDevice.Instance;
		Vuforia.Image image = cam.GetCameraImage(m_PixelFormat);
		if (image == null)
		{
			Debug.Log(m_PixelFormat + " image is not available yet");
		}
		else
		{
			

			InitializeWebcamTexture(image.Width, image.Height, webcamTextureFormat);
			initialized = true;
			Debug.Log ("Camera resolution: (" + image.Width + "," + image.Height + ")");

			
			/*
			string s = m_PixelFormat + " image: \n";
			s += "  size: " + image.Width + "x" + image.Height + "\n";
			s += "  bufferSize: " + image.BufferWidth + "x" + image.BufferHeight + "\n";
			s += "  stride: " + image.Stride;
			Debug.Log(s);
			m_LogInfo = false;
			*/
		}
	}
	
	private float nextFrame = 0.0f;
	
	// Update is called once per frame
	void Update () {

		if (isServer == true) {


		} else {
			//is client, await connection
			if (initialized == true){

			}
		}


	}



	//Initial the webcam texture with the given width and height
	void InitializeWebcamTexture(int texWidth, int texHeight, TextureFormat format){

		this.texWidth = texWidth;
		this.texHeight = texHeight;

		planeTexture = new Texture2D (texWidth, texHeight, format, false);
		
		for (int i =0; i < texHeight; i++) {
			for (int j =0; j < texWidth; j++) {
				planeTexture.SetPixel(j, i, Color.green);
			}
		}

		if (displayImage != null) {
			displayImage.sprite = Sprite.Create (planeTexture, new Rect (0, 0, texWidth, texHeight), new Vector2 (0.5f, 0.5f));
		}
		planeTexture.Apply ();

	}

	


	void DistributeVideoFrame(byte[] frame){

		//EditorUtility.CompressTexture(planeTexture, TextureFormat.RGB24, TextureCompressionQuality.Fast);
		netView.RPC ("VideoFrame", RPCMode.Others, frame, (int) m_PixelFormat, System.DateTime.Now.Millisecond);
	}

	//RPCMode.Others expected
	//Assumes bytestream is multiple of 4
	[RPC] 
	void VideoFrame(byte[] stream, int format, int timeStamp){

		//do nothing if we have yet to intialize the webcam texture
		if (initialized == false) {
			return;
		}



		int millis = System.DateTime.Now.Millisecond;

		if (millis - timeStamp >= maxDelay) {
			Debug.LogWarning("Warning: frame expired, skipped");
			return;
		}


		Vuforia.Image.PIXEL_FORMAT imageFormat = (Vuforia.Image.PIXEL_FORMAT)format;
		/*
		switch (imageFormat){
		case Vuforia.Image.PIXEL_FORMAT.RGB565:
		
			break;
		case Vuforia.Image.PIXEL_FORMAT.RGB888:

			break;
		case Vuforia.Image.PIXEL_FORMAT.RGBA8888:
		
			break;
		case Vuforia.Image.PIXEL_FORMAT.YUV:

			break;
		case Vuforia.Image.PIXEL_FORMAT.GRAYSCALE:
			Debug.LogWarning("Client could not find a supported frame format.");

			break;
		}
		*/
		//planeTexture.LoadRawTextureData (stream);


		planeTexture.LoadImage (stream);
	}





	//RPCMode.Others Expected
	[RPC]
	void InitializeClient(int frameWidth, int frameHeight, int textureFormat, NetworkViewID networkID){

		if (initialized == false) {
			//setup camera
			if (cameraUpdate != null){
				NetworkView camNetView = cameraUpdate.GetComponent<NetworkView>();
				camNetView.stateSynchronization = stateSynchronization;
				camNetView.viewID = networkID;
			}
			InitializeWebcamTexture (frameWidth, frameHeight, (TextureFormat) textureFormat);
			initialized = true;
		}

	}


	//update all newly connected players with the texture width and height
	void OnPlayerConnected(NetworkPlayer player){
		
		if (isServer == true) {
			if (initialized == true) {
				netView.RPC ("InitializeClient", RPCMode.Others, texWidth, texHeight, (int) webcamTextureFormat, cameraUpdate.GetComponent<NetworkView> ().viewID);
			} else {
				//eject player with message as we are not yet ready
				Debug.LogWarning ("Server not yet ready, ejecting client");
				Network.CloseConnection (player, true);
			}
		}
	}
	
	
	//if we are the camera server, send intialization data
	void OnConnectedToServer(){
		
		if (isServer == true) {
			if (initialized == true) {
				netView.RPC ("InitializeClient", RPCMode.Others, texWidth, texHeight, (int) webcamTextureFormat, cameraUpdate.GetComponent<NetworkView> ().viewID);
			} else {
				//diconnect as we are not ready
				Debug.LogWarning ("Server not yet ready, ejecting client");
				Network.Disconnect ();
			}
		}
	}
}
