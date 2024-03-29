﻿using UnityEngine;
using System.Collections;

public class VideoViewer : MonoBehaviour {

    public GameObject imagePanel; // used elsewhere
    public UnityEngine.UI.Image displayLeft;
    public UnityEngine.UI.Image displayRight;

    private Texture2D planeTexture;
    private int texWidth = 480;
    private int texHeight = 270;

    bool initialized = false;

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (!initialized)
        {
            InitializeTexture();
        }
    }

    public void LoadFrame(byte[] frame)
    {

        //Vuforia.Image.PIXEL_FORMAT imageFormat = Vuforia.Image.PIXEL_FORMAT.RGB565;

        //planeTexture.LoadRawTextureData(frame);
        planeTexture.LoadImage(frame);
    }

    void InitializeTexture()
    {
        planeTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, false);

        for (int i = 0; i < texHeight; i++)
        {
            for (int j = 0; j < texWidth; j++)
            {
                planeTexture.SetPixel(j, i, Color.green);
            }
        }

        if (displayLeft != null)
        {
            displayLeft.sprite = Sprite.Create(planeTexture, new Rect(0, 0, texWidth, texHeight), new Vector2(0.5f, 0.5f));
        }
        if (displayRight != null)
        {
            displayRight.sprite = Sprite.Create(planeTexture, new Rect(0, 0, texWidth, texHeight), new Vector2(0.5f, 0.5f));
        }

        planeTexture.Apply();

        initialized = true;
    }

}
