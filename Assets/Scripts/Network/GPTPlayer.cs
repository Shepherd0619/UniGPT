using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
public class GPTPlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;
    [SyncVar]
    public Sprite Avatar;
    [SyncVar]
    public GPTNetworkAuthenticator.AuthRequestMessage.Role UserRole;

    public override void OnStartServer()
    {
        PlayerName = ((GPTNetworkAuthenticator.AuthRequestMessage)connectionToClient.authenticationData).Username;
        Avatar = ((GPTNetworkAuthenticator.AuthRequestMessage)connectionToClient.authenticationData).Avatar;
        UserRole = ((GPTNetworkAuthenticator.AuthRequestMessage)connectionToClient.authenticationData).UserRole;
    }

    public override void OnStartLocalPlayer()
    {
        ChatWindow.Instance.SetLocalPlayerInfo(this);
    }
}
