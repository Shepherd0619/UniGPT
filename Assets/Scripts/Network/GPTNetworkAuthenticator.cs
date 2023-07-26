using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-authenticators
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class GPTNetworkAuthenticator : NetworkAuthenticator
{
    readonly HashSet<NetworkConnection> connectionsPendingDisconnect = new HashSet<NetworkConnection>();
    public readonly Dictionary<NetworkConnection, AuthRequestMessage> UsersList = new Dictionary<NetworkConnection, AuthRequestMessage>();
    // 客户端信息（如用户名、头像等），后续可以定制一下，比如增加用户名密码验证。
    public AuthRequestMessage ClientInfo;

    #region Messages
    public struct AuthRequestMessage : NetworkMessage
    {
        public string Username;
        public byte[] Avatar;
        public Role UserRole;
        //TODO: 以后要加一个Guest身份，只能看信息，不能发送。
        public enum Role
        {
            User,
            Moderator,
            Admin
        }
    }

    public struct AuthResponseMessage : NetworkMessage
    {
        public enum Status
        {
            Success,
            Error
        }

        public Status requestResponseCode;
        public string requestResponseMessage;
    }

    public struct KickPlayerMessage : NetworkMessage{
        public ReasonID Reason;
        public enum ReasonID{
            None,
            Violation,
            ServerError,
            ClientError,
            Ping,
            BanInProgress,
            Other
        }
        public string Message;
    }

    #endregion

    #region Server

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer()
    {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Called on server from OnServerConnectInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnectionToClient conn) { }

    /// <summary>
    /// Called on server when the client's AuthRequestMessage arrives
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        if (connectionsPendingDisconnect.Contains(conn)) return;
        AuthResponseMessage authResponseMessage = new AuthResponseMessage();
        // 在这里验证客户端信息
        if (string.IsNullOrWhiteSpace(msg.Username))
        {
            authResponseMessage.requestResponseCode = AuthResponseMessage.Status.Error;
            authResponseMessage.requestResponseMessage = "Invalid Username.";
            conn.Send(authResponseMessage);
            conn.isAuthenticated = false;
            connectionsPendingDisconnect.Add(conn);
            StartCoroutine(DelayedDisconnect(conn, 1f));
            return;
        }
        else
        {

            if (msg.UserRole == AuthRequestMessage.Role.Admin)
            {
                if (conn.address == GPTNetworkManager.singleton.networkAddress)
                {
                    //目前Admin只能为本机用户，Moderator可以后设置。
                    Debug.Log("[GPTNetworkAuthenticator]Admin has connected to the server! Username: "+msg.Username);
                    authResponseMessage.requestResponseCode = AuthResponseMessage.Status.Success;
                    conn.authenticationData = msg;
                    conn.Send(authResponseMessage);

                    // Accept the successful authentication
                    ServerAccept(conn);

                    UsersList.Add(conn, msg);
                    Debug.Log("[GPTNetworkAuthenticator]Statistics of online users:" + UsersList.Count);

                    //ChatWindow.Instance.OnReceiveServerMessage(new GPTChatMessage{ content = "A wild server admin just jumped right in! "+msg.Username });
                }
                else
                {
                    authResponseMessage.requestResponseCode = AuthResponseMessage.Status.Error;
                    authResponseMessage.requestResponseMessage = "Illegal Admin";
                    conn.Send(authResponseMessage);
                    conn.isAuthenticated = false;
                    connectionsPendingDisconnect.Add(conn);
                    StartCoroutine(DelayedDisconnect(conn, 1f));
                    return;
                }
            }
            else
            {
                foreach(AuthRequestMessage search in UsersList.Values)
                {
                    if(search.Username == msg.Username)
                    {
                        authResponseMessage.requestResponseCode = AuthResponseMessage.Status.Error;
                        authResponseMessage.requestResponseMessage = "Change the username";
                        conn.Send(authResponseMessage);
                        conn.isAuthenticated = false;
                        connectionsPendingDisconnect.Add(conn);
                        StartCoroutine(DelayedDisconnect(conn, 1f));
                        return;
                    }
                }

                authResponseMessage.requestResponseCode = AuthResponseMessage.Status.Success;
                conn.authenticationData = msg;
                conn.Send(authResponseMessage);

                // Accept the successful authentication
                ServerAccept(conn);

                UsersList.Add(conn, msg);
                Debug.Log("{GPTNetworkAuthenticator]"+msg.Username+" ("+conn.address+") has joined the server.");
                Debug.Log("[GPTNetworkAuthenticator]Statistics of online users:" + UsersList.Count);

                //ChatWindow.Instance.OnReceiveServerMessage(new GPTChatMessage{ content = "Let's give "+msg.Username+" a really warm welcome! Hope you can enjoy your stay!" });
            }


        }


    }

    IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Reject the unsuccessful authentication
        ServerReject(conn);

        yield return null;

        connectionsPendingDisconnect.Remove(conn);
    }

    ///<summary>
    ///服务器踢出用户
    ///</summary>
    public void KickPlayer(NetworkConnectionToClient conn, string msg, KickPlayerMessage.ReasonID reason = KickPlayerMessage.ReasonID.None){
        conn.Send(new KickPlayerMessage(){ Reason = reason, Message =  msg });
        StartCoroutine(DelayedDisconnect(conn, 1f));
    }

    /// <summary>
    /// Called when server stops, used to unregister message handlers if needed.
    /// </summary>
    public override void OnStopServer()
    {
        // Unregister the handler for the authentication request
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient()
    {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
        NetworkClient.RegisterHandler<KickPlayerMessage>(OnKickPlayerMessage, false);
    }

    /// <summary>
    /// Called on client from OnClientConnectInternal when a client needs to authenticate
    /// </summary>
    public override void OnClientAuthenticate()
    {
        //AuthRequestMessage authRequestMessage = new AuthRequestMessage();
        NetworkClient.Send(ClientInfo);
    }

    /// <summary>
    /// Called on client when the server's AuthResponseMessage arrives
    /// </summary>
    /// <param name="msg">The message payload</param>
    public void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        if (msg.requestResponseCode == AuthResponseMessage.Status.Success)
        {
            // Authentication has been accepted
            ClientAccept();
            LoginWindow.Instance.HideLoginScreen();
            //ChatWindow.Instance.ShowChatWindow();
            Debug.Log($"Authentication Response: {msg.requestResponseCode} {msg.requestResponseMessage}");
            //ChatWindow.Instance.SendMessageToServer(new GPTChatMessage{ content = "Let's give <b>"+ClientInfo.Username+"</b> a really warm welcome! Hope you can enjoy your stay!" });
        }
        else
        {

            // Authentication has been rejected
            // StopHost works for both host client and remote clients
            NetworkManager.singleton.StopHost();
            LoginWindow.Instance.ShowLoginScreen();

            // Do this AFTER StopHost so it doesn't get cleared / hidden by OnClientDisconnect
            MsgBoxManager.Instance.ShowMsgBox("Server has rejected your connection with a response.\n\n" + msg.requestResponseMessage, false);
        }
    }

    ///<summary>
    ///客户端接收到踢出信息时要做出的反应
    ///不建议在这里头用客户端那头的断开连接函数，可能会有意料之外的情况。
    ///</summary>
    public void OnKickPlayerMessage(KickPlayerMessage msg){
        string MsgBoxText = "You have been removed from the server due to\n";
        switch (msg.Reason)
        {
            case KickPlayerMessage.ReasonID.BanInProgress:
                MsgBoxText += "<b>Account Ban In Progress.</b>";
                break;
            case KickPlayerMessage.ReasonID.Violation:
                MsgBoxText += "<b>Violation Of Server Rules</b>";
                break;
            case KickPlayerMessage.ReasonID.ServerError:
                MsgBoxText += "<b>Server Internal Error</b>";
                break;
            case KickPlayerMessage.ReasonID.ClientError:
                MsgBoxText += "<b>Client Internal Error</b>";
                break;
            case KickPlayerMessage.ReasonID.Ping:
                MsgBoxText += "<b>Unstable Network</b>";
                break;
            case KickPlayerMessage.ReasonID.None:
                MsgBoxText = "You have been removed from the server.";
                break;
        }

        if(!String.IsNullOrEmpty(msg.Message) && !String.IsNullOrWhiteSpace(msg.Message))
            MsgBoxText += "\nPlease refer to the following detailed message: " + msg.Message;

        ChatWindow.Instance.AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), MsgBoxText);
        MsgBoxManager.Instance.ShowMsgBox(MsgBoxText, true);
    }

    /// <summary>
    /// Called when client stops, used to unregister message handlers if needed.
    /// </summary>
    public override void OnStopClient()
    {
        // Unregister the handler for the authentication response
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
        NetworkClient.UnregisterHandler<KickPlayerMessage>();
    }

    #endregion
}
