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
    public GameObject ChatGPTProcessingIndicator;
    public static ChatWindow Instance;
    private GPTNetworkAuthenticator.AuthRequestMessage LocalPlayerInfo;

    private bool isDragging = false;

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
            ChatGPTProcessingIndicator.SetActive(true);
            AppendMessage(LocalPlayerInfo.Username, LocalPlayerInfo.Avatar.ToArray(), MessageInputField.text);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        LocalPlayerInfo = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).ClientInfo;
        Instance = this;
        SendMessageBtn.onClick.AddListener(UI_SendMessageToServer);
        AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "You have joined " + GPTNetworkManager.singleton.networkAddress + ". ");
        SendMessageToServer(new GPTChatMessage { content = "Let's give <b>" + LocalPlayerInfo.Username + "</b> a really warm welcome! Hope you can enjoy your stay!" });
        LocalPlayerAvatar.texture = new Texture2D(1, 1);
        ImageConversion.LoadImage((Texture2D)LocalPlayerAvatar.texture, LocalPlayerInfo.Avatar.ToArray());
        Reset();
        RequestFullChatLog(NetworkClient.localPlayer.GetComponent<GPTPlayer>());
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Reset();
        SendMessageBtn.onClick.RemoveAllListeners();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        Instance = this;
    }

    public void Reset()
    {
        MessageInputField.text = null;
        MessageInputField.interactable = true;
        SendMessageBtn.interactable = true;
        ChatGPTProcessingIndicator.SetActive(false);
    }

    public void AppendMessage(string sender, byte[] avatar, string content)
    {
        GameObject obj = Instantiate(ChatMessagePrefab, ChatContainer.content);
        MessageUI msg = obj.GetComponent<MessageUI>();

        //更新信息框UI
        msg.AppendMessage(sender, avatar, content);

        if (!isDragging)
            StartCoroutine(ScrollToLatestMessage());
    }

    IEnumerator ScrollToLatestMessage()
    {
        yield return new WaitForSeconds(0.1f);
        ChatContainer.normalizedPosition = new Vector2(0, 0);
    }

    public void OnBeginDrag()
    {
        isDragging = true;
    }

    public void OnEndDrag()
    {
        isDragging = false;
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
        ChatGPTProcessingIndicator.SetActive(false);
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

    [Command(requiresAuthority = false)]
    public void RequestFullChatLog(GPTPlayer applicant)
    {
        GPTNetworkAuthenticator.AuthRequestMessage search = new GPTNetworkAuthenticator.AuthRequestMessage();
        search.Username = applicant.Username;
        search.Avatar = applicant.Avatar.ToArray();
        search.UserRole = applicant.UserRole;
        if (((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.ContainsValue(search))
        {
            Debug.Log("[ChatWindow]Requesting " + applicant.Username + "'s full chat log......");
            List<ChatMessage> result = ChatCompletion.Instance.GetFullChatLog(applicant.Username);
            if (result != null && result.Count > 0)
            {
                Debug.Log("[ChatWindow]Sending " + applicant.Username+"'s chat log to himself/herself.....");
                OnReceiveFullChatLog(((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == search.Username).Key,result);
            }
            else
            {
                Debug.Log("[ChatWindow]Looks like there is no chat log of " + applicant.Username + ".");
            }
        }
    }

    [TargetRpc]
    public void OnReceiveFullChatLog(NetworkConnection conn, List<ChatMessage> log)
    {
        Debug.Log("[ChatWindow]Successfully receive full chat log.");
        //清屏
        foreach(Transform obj in ChatContainer.content.GetComponentsInChildren<Transform>())
        {
            Destroy(obj.gameObject);
        }
        //输出
        foreach(ChatMessage msg in log)
        {
            if (msg.role.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                AppendMessage("ChatGPT", UIAssetsManager.Instance.GetIcon2Texture("chatgpt_icon").EncodeToPNG(), msg.content);
            }
            else
            {
                AppendMessage(((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username, ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Avatar,msg.content);
            }
        }
        AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "--- MESSAGE(S) ABOVE FROM CHAT LOG. ---");
    }
}
