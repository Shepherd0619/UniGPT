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
    // 客户端信息（如用户名、头像等），后续可以定制一下，比如增加用户名密码验证。
    public AuthRequestMessage ClientInfo;

    #region Messages
    public struct AuthRequestMessage : NetworkMessage
    {
        public string Username;
        public Sprite Avatar;
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
        // 在这里验证客户端信息
        if (string.IsNullOrEmpty(msg.Username) && string.IsNullOrWhiteSpace(msg.Username))
        {
            AuthResponseMessage authResponseMessage = new AuthResponseMessage();
            authResponseMessage.requestResponseCode = AuthResponseMessage.Status.Error;
            authResponseMessage.requestResponseMessage = "Invalid Username.";
            conn.Send(authResponseMessage);
            conn.isAuthenticated = false;
            connectionsPendingDisconnect.Add(conn);
            StartCoroutine(DelayedDisconnect(conn, 1f));
        }
        else
        {
            AuthResponseMessage authResponseMessage = new AuthResponseMessage();

            conn.Send(authResponseMessage);

            // Accept the successful authentication
            ServerAccept(conn);
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
        // Authentication has been accepted
        ClientAccept();
    }

    /// <summary>
    /// Called when client stops, used to unregister message handlers if needed.
    /// </summary>
    public override void OnStopClient()
    {
        // Unregister the handler for the authentication response
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    #endregion
}
