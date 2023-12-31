using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class GPTNetworkManager : NetworkManager
{
    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.
    public static new GPTNetworkManager singleton { get; private set; }
    public bool isReconnecting = false;
    public int MaxReconnectAttempt = 5;
    private int CurrentReconnectAttemptCounter = 0;
    //自助指令
    public List<SelfHelpCommand> SelfHelpCommands = new List<SelfHelpCommand>();
    public class SelfHelpCommand
    {
        public string Command;
        public Action<string[], NetworkConnection> Executation;
        public string Summary;
    }

    private int msgboxId;
    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        singleton = this;
    }

    #region 自助指令
    public void RegisterSelfHelpCommands()
    {
        SelfHelpCommands.Clear();
        //注册系统内置指令
        SelfHelpCommands.Add(new SelfHelpCommand()
        {
            Command = "help",
            Executation = (args, conn) =>
            {
                string list = String.Empty;
                foreach (SelfHelpCommand obj in SelfHelpCommands)
                {
                    list += "<b>/" + obj.Command + "</b> - " + obj.Summary + "\n";
                }
                //ChatWindow.Instance.AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "Available commands:\n" + list);
                ChatWindow.Instance.OnReceiveServerTargetedMessage(conn, new GPTChatMessage() { content = "<b>Available commands:</b>\n\n" + list });
            },
            Summary = "Get the list of available commands."
        });

        SelfHelpCommands.Add(new SelfHelpCommand()
        {
            Command = "GetChatLog",
            Executation = (args, conn) =>
            {
                ChatWindow.Instance.RequestFullChatLog(conn.identity.GetComponent<GPTPlayer>());
            },
            Summary = "Get current user's chat log",
        });

        SelfHelpCommands.Add(new SelfHelpCommand()
        {
            Command = "ClearChatLog",
            Executation = (args, conn) =>
            {
                ChatWindow.Instance.ClearChatLog(conn.identity.GetComponent<GPTPlayer>());
            },
            Summary = "Clear current user's chat log. (WARNING: Cannot be undone.) "
        });

        SelfHelpCommands.Add(new SelfHelpCommand()
        {
            Command = "Resend",
            Executation = (args, conn) =>
            {
                ChatWindow.Instance.ResendMessageToChatGPT(conn.identity.GetComponent<GPTPlayer>());
                ChatWindow.Instance.MessageInputField.interactable = false;
                ChatWindow.Instance.SendMessageBtn.interactable = false;
                ChatWindow.Instance.ChatGPTProcessingIndicator.SetActive(true);
            },
            Summary = "Resend messages to ChatGPT."
        });
    }
    #endregion

    #region Unity Callbacks

    public override void OnValidate()
    {
        base.OnValidate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Start()
    {
        base.Start();
#if UNITY_SERVER
        HostWindow.Instance.Server_LoadAndApplySettings();
        StartServer();
        Debug.Log("[GPTNetworkManager]Server mode started!");
#endif
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion

    #region Start & Stop

    /// <summary>
    /// Set the frame rate for a headless server.
    /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
    /// </summary>
    public override void ConfigureHeadlessFrameRate()
    {
        base.ConfigureHeadlessFrameRate();
    }

    /// <summary>
    /// called when quitting the application by closing the window / pressing stop in the editor
    /// </summary>
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

    /// <summary>
    /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    public override void OnServerChangeScene(string newSceneName) { }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName) { }

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnectionToClient conn) { }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        // remove player name from the HashSet
        if (conn.authenticationData != null)
        {
            ((GPTNetworkAuthenticator)authenticator).UsersList.Remove(conn);
            Debug.Log("[GPTNetworkManager]" + ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username + "(" + conn.address + ") has disconnected from server.");
        }
        Debug.Log("[GPTNetworkManager]" + conn.address + " has disconnected from server.");
    }

    /// <summary>
    /// Called on server when transport raises an error.
    /// <para>NetworkConnection may be null.</para>
    /// </summary>
    /// <param name="conn">Connection of the client...may be null</param>
    /// <param name="transportError">TransportError enum</param>
    /// <param name="message">String message of the error.</param>
    public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message)
    {
        if (conn != null)
            Debug.LogError("[GPTNetworkManager]" + ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username + "(" + conn.address + ")'s transport has raised an error.\n" + transportError.ToString() + "\n" + message);
        else
            Debug.LogError("[GPTNetworkManager]There was a critical error in server.\n" + transportError.ToString() + "\n" + message);
    }

    #endregion

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        if (isReconnecting)
        {
            isReconnecting = false;
        }
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    public override void OnClientDisconnect()
    {
        LoginWindow.Instance.ShowLoginScreen();
        if (ChatWindow.Instance)
        {
            ChatWindow.Instance.AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "You have been disconnected from server!");
            ChatWindow.Instance.Reset();
        }

        MsgBoxManager.Instance.ShowMsgBox("You have been disconnected from server.", false);

    }

    /// <summary>
    /// Called on clients when a servers tells the client it is no longer ready.
    /// <para>This is commonly used when switching scenes.</para>
    /// </summary>
    public override void OnClientNotReady() { }

    /// <summary>
    /// Called on client when transport raises an error.</summary>
    /// </summary>
    /// <param name="transportError">TransportError enum.</param>
    /// <param name="message">String message of the error.</param>
    public override void OnClientError(TransportError transportError, string message)
    {
        if (transportError == TransportError.Timeout)
        {
            Debug.LogError("[GPTNetworkAuthenticator]Client Error!" + "Reason: ping timeout or dead link");
            if (!isReconnecting)
            {
                CurrentReconnectAttemptCounter = 0;
                NetworkClient.Connect(networkAddress);
                CurrentReconnectAttemptCounter++;
                Debug.Log("[GPTNetworkAuthenticatior]Client is now reconnecting. (" + CurrentReconnectAttemptCounter + " of " + MaxReconnectAttempt + ")");
                msgboxId = MsgBoxManager.Instance.ShowMsgBoxNonInteractable("There is a timeout or dead link in this connection.\nReconnecting to " + GPTNetworkManager.singleton.networkAddress + "......", true, (result) =>
                {
                    if (!result)
                    {
                        GPTNetworkManager.singleton.StopClient();
                    }
                });
            }
            else
            {
                if (CurrentReconnectAttemptCounter < MaxReconnectAttempt)
                {
                    NetworkClient.Connect(networkAddress);
                    CurrentReconnectAttemptCounter++;
                    Debug.Log("[GPTNetworkAuthenticatior]Client is now reconnecting. (" + CurrentReconnectAttemptCounter + " of " + MaxReconnectAttempt + ")");
                }
                else
                {
                    isReconnecting = false;
                    MsgBoxManager.Instance.RemoveNonInteractableMsgBox(msgboxId, true);
                    Debug.Log("[GPTNetworkAuthenticator]MaxReconnectAttempt reached! Reconnect failed!");
                    CurrentReconnectAttemptCounter = 0;
                    MsgBoxManager.Instance.ShowMsgBox("We are sorry to inform you that we have lost the connection to the server due to <b>ping timeout or dead link</b>.\nWe will send ya back to the login screen.", false, (result) =>
                    {
                        LoginWindow.Instance.ShowLoginScreen();
                    });
                }
            }
        }
        else
        {
            switch (transportError)
            {
                case TransportError.DnsResolve:
                    {
                        MsgBoxManager.Instance.ShowMsgBox("Could not connect to the server due to DNS Resolve Failure.\nPlease check the DNS server and server address.", false, (result) =>
                        {
                            LoginWindow.Instance.ShowLoginScreen();
                        });
                        isReconnecting = false;
                        CurrentReconnectAttemptCounter = 0;
                        break;
                    }

                case TransportError.Refused:
                    {
                        MsgBoxManager.Instance.ShowMsgBox("The server refused your connection.", false, (result) =>
                        {
                            LoginWindow.Instance.ShowLoginScreen();
                        });
                        isReconnecting = false;
                        CurrentReconnectAttemptCounter = 0;
                        break;
                    }

                case TransportError.ConnectionClosed:
                    {
                        MsgBoxManager.Instance.ShowMsgBox("The connection was closed.", false, (result) =>
                        {
                            LoginWindow.Instance.ShowLoginScreen();
                        });
                        isReconnecting = false;
                        CurrentReconnectAttemptCounter = 0;
                        break;
                    }

                default:
                    {
                        MsgBoxManager.Instance.ShowMsgBox("We are sorry to inform you that there is an unexpected error occured.\nIf this is the first time you encounter, please restart the app.\nIf it is not, please contact developer.", false, (result) =>
                        {
                            LoginWindow.Instance.ShowLoginScreen();
                        });
                        isReconnecting = false;
                        CurrentReconnectAttemptCounter = 0;
                        break;
                    }
            }
        }


    }

    #endregion

    #region Start & Stop Callbacks

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.

    /// <summary>
    /// This is invoked when a host is started.
    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartHost() { }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer()
    {
        RegisterSelfHelpCommands();
    }

    /// <summary>
    /// This is invoked when the client is started.
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is called when a host is stopped.
    /// </summary>
    public override void OnStopHost() { }

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient() { }

    #endregion

    #region GUI
    public void OnGUI()
    {
        if (isReconnecting)
        {
            GUI.Label(new Rect(10, 10, 300, 30), "<color=red>WARNING: CONNECTION PROBLEM</color>");
        }
    }
    #endregion
}
