using UnityEngine;
using System.Collections;

public class PictureInPicture : MonoBehaviour {

    public GameObject Left;
    public GameObject Right;

    private int FULL_MODE = 1;
    private int PIP_MODE = 2;
    private int mode;

    private float scale_amount = 0.4F;
    private Vector3 scale;

    void Start()
    {
        mode = FULL_MODE;
        scale = new Vector3(scale_amount, scale_amount, scale_amount);
    }

	public void SwitchMode()
    {
        if (mode == FULL_MODE)
        {
            // switch to PIP mode
            mode = PIP_MODE;
            Left.transform.localScale -= scale;
            Right.transform.localScale -= scale;
            Left.transform.Translate(150, 100, 0);
            Right.transform.Translate(150, 100, 0);
        }
        else
        {
            // switch to FULL mode
            mode = FULL_MODE;
            Left.transform.localScale += scale;
            Right.transform.localScale += scale;
            Left.transform.Translate(-150, -100, 0);
            Right.transform.Translate(-150, -100, 0);
        }
    }
}
