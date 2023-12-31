using Mirror;
public class GPTPlayer : NetworkBehaviour
{
    [SyncVar]
    public string Username;
    public readonly SyncList<byte> Avatar = new SyncList<byte>();
    [SyncVar]
    public GPTNetworkAuthenticator.AuthRequestMessage.Role UserRole;

    public override void OnStartServer()
    {
        Username = ((GPTNetworkAuthenticator.AuthRequestMessage)connectionToClient.authenticationData).Username;
        Avatar.Clear();
        foreach (byte data in ((GPTNetworkAuthenticator.AuthRequestMessage)connectionToClient.authenticationData).Avatar)
        {
            Avatar.Add(data);
        }
        UserRole = ((GPTNetworkAuthenticator.AuthRequestMessage)connectionToClient.authenticationData).UserRole;
    }

    public override void OnStartLocalPlayer()
    {
        ChatWindow.Instance.RequestFullChatLog(NetworkClient.localPlayer.GetComponent<GPTPlayer>());
    }
}
