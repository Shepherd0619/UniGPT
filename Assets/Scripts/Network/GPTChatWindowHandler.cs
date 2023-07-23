using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GPTChatWindowHandler : NetworkBehaviour
{
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

        ChatWindow.Instance.AppendMessage(msg.Sender.Username, msg.Sender.Avatar, msg.content);

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

        ChatWindow.Instance.AppendMessage(msg.Sender.Username, msg.Sender.Avatar, msg.content);
    }

    [TargetRpc]
    public void OnReceiveChatGPTMessage(NetworkConnection conn, string content)
    {
        ChatWindow.Instance.AppendMessage("ChatGPT", ImageConversion.EncodeToPNG(UIAssetsManager.Instance.GetIcon2Texture("chatgpt_icon")), content);
        ChatWindow.Instance.MessageInputField.text = null;
        ChatWindow.Instance.MessageInputField.interactable = true;
        ChatWindow.Instance.SendMessageBtn.interactable = true;
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
                ChatCompletion.Instance.chatRequestLogs.Add(new ChatCompletion.ChatRequestLog() { history = new List<ChatMessage>(), sender = msg.Sender.Username });
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
