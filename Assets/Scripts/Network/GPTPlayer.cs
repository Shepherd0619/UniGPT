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

    public override void OnStartServer()
    {
        PlayerName = (string)connectionToClient.authenticationData;
    }

    public override void OnStartLocalPlayer()
    {
        ChatWindow.Instance.SetLocalPlayerInfo(this);
    }
}
