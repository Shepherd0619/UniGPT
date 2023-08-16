//*******************************************************************************
//*																							*
//*							Written by Grady Featherstone								*
//										?Copyright 2011										*
//*******************************************************************************
using Mirror;
using UnityEngine;
public class PauseMenu : MonoBehaviour
{
    public Font pauseMenuFont;
    private bool pauseEnabled = false;

    void Start()
    {
        pauseEnabled = false;
    }

    void Update()
    {
        if (NetworkClient.isConnected)
        {
            //check if pause button (escape key) is pressed
            if (Input.GetKeyDown("escape"))
            {

                //check if game is already paused		
                if (pauseEnabled == true)
                {
                    //unpause the game
                    pauseEnabled = false;
                }

                //else if game isn't paused, then pause it
                else if (pauseEnabled == false)
                {
                    pauseEnabled = true;
                }
            }
        }
        else
        {
            pauseEnabled = false;
        }
    }

    //private bool showGraphicsDropDown = false;

    void OnGUI()
    {

        GUI.skin.box.font = pauseMenuFont;
        GUI.skin.button.font = pauseMenuFont;

        if (pauseEnabled == true)
        {

            //Make a background box
            GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 250, 200), "Pause Menu");

            //Make Main Menu button
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 250, 50), "Disconnect from server"))
            {
                if (NetworkClient.activeHost)
                {
                    MsgBoxManager.Instance.ShowMsgBox("Since you are the host, the server will be stopped immediately once you disconnect from server.\nProceed?", true, (result) =>
                    {
                        if (result)
                        {
                            GPTNetworkManager.singleton.StopHost();
                        }
                    });
                }
                else
                {
                    NetworkClient.Disconnect();
                }
                pauseEnabled = false;
            }

            /*
            //Make Change Graphics Quality button
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2, 250, 50), "Change Graphics Quality"))
            {

                if (showGraphicsDropDown == false)
                {
                    showGraphicsDropDown = true;
                }
                else
                {
                    showGraphicsDropDown = false;
                }
            }

            //Create the Graphics settings buttons, these won't show automatically, they will be called when
            //the user clicks on the "Change Graphics Quality" Button, and then dissapear when they click
            //on it again....
            if (showGraphicsDropDown == true)
            {
                if (GUI.Button(new Rect(Screen.width / 2 + 150, Screen.height / 2, 250, 50), "Fastest"))
                {
                    QualitySettings.currentLevel = QualityLevel.Fastest;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 150, Screen.height / 2 + 50, 250, 50), "Fast"))
                {
                    QualitySettings.currentLevel = QualityLevel.Fast;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 150, Screen.height / 2 + 100, 250, 50), "Simple"))
                {
                    QualitySettings.currentLevel = QualityLevel.Simple;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 150, Screen.height / 2 + 150, 250, 50), "Good"))
                {
                    QualitySettings.currentLevel = QualityLevel.Good;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 150, Screen.height / 2 + 200, 250, 50), "Beautiful"))
                {
                    QualitySettings.currentLevel = QualityLevel.Beautiful;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 150, Screen.height / 2 + 250, 250, 50), "Fantastic"))
                {
                    QualitySettings.currentLevel = QualityLevel.Fantastic;
                }

                if (Input.GetKeyDown("escape"))
                {
                    showGraphicsDropDown = false;
                }
            }
            */

            //Make quit game button
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 250, 50), "Quit App"))
            {
                Application.Quit();
            }
        }
    }
}