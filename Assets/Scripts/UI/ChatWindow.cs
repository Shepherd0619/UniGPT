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

    public void AppendMessage(GPTPlayer sender, string content){
        GameObject obj = Instantiate(ChatMessagePrefab,ChatContainer.content);
        MessageUI msg = obj.GetComponent<MessageUI>();

        //更新信息框UI
        msg.AppendMessage(sender.PlayerName,sender.Avatar,content);
    }

    [ClientRpc]
    //<summary>
    //服务器向客户端广播信息
    //</summary>
    public void OnReceiveServerMessage(){

    }
}
