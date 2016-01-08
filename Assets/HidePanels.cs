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

    public void HideDebug()
    {
        if (active)
        {
            cameraPanel.SetActive(false);
            networkPanel.SetActive(false);
            active = false;
        }
        else
        {
            cameraPanel.SetActive(true);
            networkPanel.SetActive(true);
            active = true;
        }

    }
}
