using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
public class ChatWindow : NetworkBehaviour
{
    public static ChatWindow Instance;
    public ScrollRect ChatContainer;
    public GameObject ChatMessagePrefab;
    public Image LocalPlayerAvatar;
    public GPTPlayer LocalPlayer;
    
    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowChatWindow(){
        gameObject.SetActive(true);
    }

    public void HideChatWindow(){
        gameObject.SetActive(false);
    }

    public void Init(){

    }

    public void SetLocalPlayerInfo(GPTPlayer player){
        LocalPlayerAvatar.sprite = player.Avatar;
    }

    public void AppendMessage(string sender, Sprite avatar, string content){
        GameObject obj = Instantiate(ChatMessagePrefab,ChatContainer.content);
        MessageUI msg = obj.GetComponent<MessageUI>();

        //更新信息框UI
        msg.AppendMessage(sender,avatar,content);
    }

    [ClientRpc]
    //<summary>
    //服务器向客户端广播信息
    //</summary>
    public void OnReceiveServerMessage(GPTChatMessage msg){
        if(msg.Sender == null){
            //系统信息
            msg.Sender.Username = "SYSTEM";
            msg.Sender.Avatar = UIAssetsManager.Instance.GetIcon("announcement_icon");
        }

        AppendMessage(msg.Sender.Username,msg.Sender.Avatar,msg.content);

    }

    [TargetRpc]
    //<summary>
    //服务器向指定客户端广播信息
    //</summary>
    public void OnReceiveServerTargetedMessage(NetworkConnection Target, GPTChatMessage msg){
        AppendMessage(msg.Sender.Username,msg.Sender.Avatar,msg.content);
    }

    [Command(requiresAuthority = false)]
    public void SendMessageToServer(GPTChatMessage msg){
        if(msg.Receiver == null){
            OnReceiveServerMessage(msg);
        }else{
            if(msg.Sender != null){
                GPTNetworkAuthenticator.AuthRequestMessage search= new GPTNetworkAuthenticator.AuthRequestMessage();
                search.Username = msg.Receiver.Username;
                search.Avatar = msg.Receiver.Avatar;
                search.UserRole = msg.Receiver.UserRole;
                if(((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.ContainsValue(search)){
                    NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == search.Username).Key;
                    //TODO: 这里应该调用OpenAI的接口，获取回复，然后发给客户端。
                    OnReceiveServerTargetedMessage(conn, msg);
                }
            }
            
        }
    }
}
