using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using TMPro;
using System;

public class ChatWindow : NetworkBehaviour
{
    public GameObject ChatWindowContainer;
    public ScrollRect ChatContainer;
    public GameObject ChatMessagePrefab;
    public TMP_InputField MessageInputField;
    public RawImage LocalPlayerAvatar;
    public Button SendMessageBtn;
    public static ChatWindow Instance;
    private GPTNetworkAuthenticator.AuthRequestMessage LocalPlayerInfo;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UI_SendMessageToServer();
        }
    }

    public void UI_SendMessageToServer()
    {
        if (!String.IsNullOrWhiteSpace(MessageInputField.text) && MessageInputField.interactable)
        {
            SendMessageToChatGPT(new GPTChatMessage()
            {
                Sender = new GPTChatMessage.Who()
                {
                    Username = LocalPlayerInfo.Username,
                    Avatar = LocalPlayerInfo.Avatar.ToArray()
                },

                content = MessageInputField.text
            });
            MessageInputField.interactable = false;
            SendMessageBtn.interactable = false;
            AppendMessage(LocalPlayerInfo.Username, LocalPlayerInfo.Avatar.ToArray(), MessageInputField.text);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartLocalPlayer();
        LocalPlayerInfo = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).ClientInfo;
        Instance = this;
        SendMessageBtn.onClick.AddListener(UI_SendMessageToServer);
        SendMessageToServer(new GPTChatMessage { content = "Let's give <b>" + LocalPlayerInfo.Username + "</b> a really warm welcome! Hope you can enjoy your stay!" });
        LocalPlayerAvatar.texture = new Texture2D(1, 1);
        ImageConversion.LoadImage((Texture2D)LocalPlayerAvatar.texture, LocalPlayerInfo.Avatar.ToArray());
    }

    public void Init()
    {

    }

    public void AppendMessage(string sender, byte[] avatar, string content)
    {
        GameObject obj = Instantiate(ChatMessagePrefab, ChatContainer.content);
        MessageUI msg = obj.GetComponent<MessageUI>();

        //更新信息框UI
        msg.AppendMessage(sender, avatar, content);
    }

    [ClientRpc]
    ///<summary>
    ///服务器向客户端广播信息
    ///</summary>    
    public void OnReceiveServerMessage(GPTChatMessage msg)
    {
        if (msg.Sender == null)
        {
            msg.Sender = new GPTChatMessage.Who();
            //系统信息
            msg.Sender.Username = "SYSTEM";
            msg.Sender.Avatar = ImageConversion.EncodeToPNG(UIAssetsManager.Instance.GetIcon2Texture("announcement_icon"));
        }

        AppendMessage(msg.Sender.Username, msg.Sender.Avatar, msg.content);

    }

    [TargetRpc]
    ///<summary>
    ///服务器向指定客户端广播信息
    ///</summary>
    public void OnReceiveServerTargetedMessage(NetworkConnection Target, GPTChatMessage msg)
    {
        if (msg.Sender == null)
        {
            msg.Sender = new GPTChatMessage.Who();
            //系统信息
            msg.Sender.Username = "SYSTEM";
            msg.Sender.Avatar = ImageConversion.EncodeToPNG(UIAssetsManager.Instance.GetIcon2Texture("announcement_icon"));
        }

        AppendMessage(msg.Sender.Username, msg.Sender.Avatar, msg.content);
    }

    [TargetRpc]
    public void OnReceiveChatGPTMessage(NetworkConnection conn, string content)
    {
        AppendMessage("ChatGPT", ImageConversion.EncodeToPNG(UIAssetsManager.Instance.GetIcon2Texture("chatgpt_icon")), content);
        MessageInputField.text = null;
        MessageInputField.interactable = true;
        SendMessageBtn.interactable = true;
    }

    [Command(requiresAuthority = false)]
    public void SendMessageToServer(GPTChatMessage msg)
    {
        if (msg.Receiver == null)
        {
            OnReceiveServerMessage(msg);
        }
        else
        {
            GPTNetworkAuthenticator.AuthRequestMessage search = new GPTNetworkAuthenticator.AuthRequestMessage();
            search.Username = msg.Receiver.Username;
            NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == search.Username).Key;
            if (conn != null)
                OnReceiveServerTargetedMessage(conn, msg);
        }
    }

    [Command(requiresAuthority = false)]
    public void SendMessageToChatGPT(GPTChatMessage msg)
    {
        if (msg.Sender != null)
        {
            GPTNetworkAuthenticator.AuthRequestMessage search = new GPTNetworkAuthenticator.AuthRequestMessage();
            search.Username = msg.Sender.Username;
            NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == search.Username).Key;
            if (conn == null)
                return;
            try
            {
                //TODO: 这里应该给出一个将ChatGPT聊天记录保存在本地的选项
                ChatCompletion.ChatRequestLog result = ChatCompletion.Instance.chatRequestLogs.FirstOrDefault(x => (x.sender == msg.Sender.Username));
                ChatCompletion.Instance.chatRequestUnderProcessing.Add(conn, ChatCompletion.Instance.SendChatRequest(result.history, msg.content, conn));
                Debug.Log("ChatRquestLog Found");
            }
            catch
            {
                ChatCompletion.Instance.chatRequestLogs.Add(new ChatCompletion.ChatRequestLog(){ history = new List<ChatMessage>(), sender = msg.Sender.Username });
                ChatCompletion.Instance.chatRequestUnderProcessing.Add(conn, ChatCompletion.Instance.SendChatRequest(null, msg.content, conn));
            }

        }


    }

    [Command(requiresAuthority = false)]
    public void StopChatGPTProcessing(GPTPlayer applicant)
    {
        GPTNetworkAuthenticator.AuthRequestMessage search = new GPTNetworkAuthenticator.AuthRequestMessage();
        search.Username = applicant.Username;
        search.Avatar = applicant.Avatar.ToArray();
        search.UserRole = applicant.UserRole;
        if (((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.ContainsValue(search))
        {
            NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == search.Username).Key;
            Coroutine result = ChatCompletion.Instance.chatRequestUnderProcessing.First(x => x.Key == conn).Value;
            StopCoroutine(result);
        }
    }

}
