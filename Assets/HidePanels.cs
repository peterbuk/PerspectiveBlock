using UnityEngine;
using System.Collections;

public class HidePanels : MonoBehaviour {

    public GameObject cameraPanel;
    public GameObject networkPanel;

    bool active = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void HideNetwork()
    {
        if (active)
        {
            networkPanel.SetActive(false);
            active = false;
        }
        else
        {
            networkPanel.SetActive(true);
            active = true;
        }
    }

    public void HideDebug()
    {
        if (active)
        {
            if (cameraPanel != null)
                cameraPanel.SetActive(false);
            if (networkPanel != null)
                networkPanel.SetActive(false);
            active = false;
        }
        else
        {
            if (cameraPanel != null)
                cameraPanel.SetActive(true);
            if (networkPanel != null)
                networkPanel.SetActive(true);
            active = true;
        }

    }
}
