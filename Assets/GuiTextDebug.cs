/*
* Creates a debug GUI window on screen
* Source: http://forum.unity3d.com/threads/any-tips-for-debugging-android.70197/#post-448944
*/

using UnityEngine;
using System.Collections;
 
public class GuiTextDebug : MonoBehaviour
{

    private float windowPosition = 120.0f;
    private int positionCheck = 2;
    private static string windowText = "";
    private Vector2 scrollViewVector = Vector2.zero;
    private GUIStyle debugBoxStyle;

    private float leftSide = 0.0f;
    private float debugWidth = 800.0f;

    public bool debugIsOn = false;

    public static void debug(string newString)
    {
        windowText = newString + "\n" + windowText;
        UnityEngine.Debug.Log(newString);
    }

    void Start()
    {
        debugBoxStyle = new GUIStyle();
        debugBoxStyle.alignment = TextAnchor.UpperLeft;
        leftSide = 120; //Screen.width - debugWidth - 3;
    }


    void OnGUI()
    {

        if (debugIsOn)
        {

            GUI.depth = 0;

            GUI.BeginGroup(new Rect(windowPosition, 40.0f, Screen.width, 500.0f));

            scrollViewVector = GUI.BeginScrollView(new Rect(0, 0.0f, debugWidth, 600.0f),
                                                scrollViewVector,
                                                new Rect(0.0f, 0.0f, 400.0f, 500.0f)
                                                , false, false); 
            GUI.Box(new Rect(0, 0.0f, debugWidth, 2000.0f), windowText, debugBoxStyle);
            GUI.EndScrollView();

            GUI.EndGroup();



            if (GUI.Button(new Rect(leftSide, 0.0f, 75.0f, 40.0f), "Debug"))
            {
                if (positionCheck == 1) //off
                {
                    windowPosition = -440.0f;
                    positionCheck = 2;
                }
                else //on
                {
                    windowPosition = leftSide;
                    positionCheck = 1;
                }
            }

            if (GUI.Button(new Rect(leftSide + 80f, 0.0f, 75.0f, 40.0f), "Clear"))
            {
                windowText = "";
            }
        }
    }


}
