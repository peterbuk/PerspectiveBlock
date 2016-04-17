using UnityEngine;
using System.Collections;

public class TargetSwitcher : MonoBehaviour {

    public GameObject targetA;
    public GameObject targetB;

    public SenderClient sender;

    private int target = 0;

    public void SwitchTo ()
    {
        target = ++target % 2;

        if (target == 0)
        {
            targetA.SetActive(true);
            targetB.SetActive(false);
        }

        else if (target == 1)
        {
            targetB.SetActive(true);
            targetA.SetActive(false);
        }


        sender.SendTextMessage("SWAP");
    }
}
