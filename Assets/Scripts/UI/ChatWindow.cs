using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
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
            msg.Sender.PlayerName = "SYSTEM";
            msg.Sender.Avatar = UIAssetsManager.Instance.GetIcon("announcement_icon");
        }

        AppendMessage(msg.Sender.PlayerName,msg.Sender.Avatar,msg.content);

    }

    [TargetRpc]
    //<summary>
    //服务器向指定客户端广播信息
    //</summary>
    public void OnReceiveServerTargetedMessage(GPTChatMessage msg){
        if(msg.Receiver == null || msg.Receiver != LocalPlayer){
            //没有收件人
            Debug.Log("[ChatWindow]"+msg.Sender.PlayerName+" sent a invalid message since there is no receiver info or receiver is not us.");
            return;
        }

    }

    [Command(requiresAuthority = false)]
    public void SendMessageToServer(GPTChatMessage msg){

    }
}
