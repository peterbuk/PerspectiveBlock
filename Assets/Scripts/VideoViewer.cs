﻿using UnityEngine;
using System.Collections;

public class VideoViewer : MonoBehaviour {

    public UnityEngine.UI.Image displayImage;

    private Texture2D planeTexture;
    private int texWidth = 240;
    private int texHeight = 120;

    bool initialized = false;

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	}

    public void LoadFrame(byte[] frame)
    {
        if (!initialized)
            InitializeTexture();

        //Vuforia.Image.PIXEL_FORMAT imageFormat = Vuforia.Image.PIXEL_FORMAT.RGB565;

        planeTexture.LoadImage(frame);
    }

    void InitializeTexture()
    {

        planeTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGB565, false);

        for (int i = 0; i < texHeight; i++)
        {
            for (int j = 0; j < texWidth; j++)
            {
                planeTexture.SetPixel(j, i, Color.green);
            }
        }

        if (displayImage != null)
        {
            displayImage.sprite = Sprite.Create(planeTexture, new Rect(0, 0, texWidth, texHeight), new Vector2(0.5f, 0.5f));
        }
        planeTexture.Apply();

        initialized = true;
    }
}