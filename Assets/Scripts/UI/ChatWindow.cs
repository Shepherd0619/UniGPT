using Mirror;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ChatWindow : NetworkBehaviour, IPointerDownHandler
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
    AsyncOperationHandle<GameObject> handle;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Addressables.Release(handle);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Return) && Input.GetKey(KeyCode.RightControl))
        {
            UI_SendMessageToServer();
        }
    }

    public void UI_SendMessageToServer()
    {
        if (!String.IsNullOrWhiteSpace(MessageInputField.text) && MessageInputField.interactable)
        {
            string text = MessageInputField.text.Trim();
            //检测用户是否输入的是指令
            if (text.StartsWith("/") && text.Substring(1) != "/")
            {
                Debug.Log("[ChatWindow]Self-help command detected! Command:" + MessageInputField.text);
                string command = text.Substring(1).TrimEnd('\n'); // 去掉首个斜杠字符和末尾的换行符
                string[] parts = command.Split(' ');
                string commandName = parts[0];
                string[] arguments = parts.Skip(1).ToArray();
                SendSelfHelpCommand(NetworkClient.localPlayer.GetComponent<GPTPlayer>(), commandName, arguments);
                MessageInputField.text = null;
                return;
            }

            SendMessageToChatGPT(new GPTChatMessage()
            {
                Sender = new GPTChatMessage.Who()
                {
                    Username = LocalPlayerInfo.Username,
                    Avatar = LocalPlayerInfo.Avatar.ToArray()
                },

                content = MessageInputField.text.Trim().TrimEnd('\n')//去掉末尾的换行符
            });
            MessageInputField.interactable = false;
            SendMessageBtn.interactable = false;
            ChatGPTProcessingIndicator.SetActive(true);
            AppendMessage(LocalPlayerInfo.Username, LocalPlayerInfo.Avatar.ToArray(), MessageInputField.text.Trim());
        }
    }

    [Command(requiresAuthority = false)]
    public void SendSelfHelpCommand(GPTPlayer applicant, string command, string[] param)
    {
        foreach (GPTNetworkManager.SelfHelpCommand obj in GPTNetworkManager.singleton.SelfHelpCommands)
        {
            if (obj.Command == command)
            {
                NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == applicant.Username).Key;
                if (conn != null)
                    obj.Executation.Invoke(param, conn);
                return;
            }
        }

        OnReceiveServerTargetedMessage(((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == applicant.Username).Key,
                    new GPTChatMessage() { content = "Unrecognized or unavailable command. Type /help to see available commands." });
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        LocalPlayerInfo = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).ClientInfo;
        //Instance = this;
        SendMessageBtn.onClick.AddListener(UI_SendMessageToServer);
        AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "You have joined " + GPTNetworkManager.singleton.networkAddress + ". ");
        SendMessageToServer(new GPTChatMessage { content = "Let's give <b>" + LocalPlayerInfo.Username + "</b> a really warm welcome! Hope you can enjoy your stay!" });
        AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "You can type <b>/help</b> to see available commands.");
        LocalPlayerAvatar.texture = new Texture2D(1, 1);
        ImageConversion.LoadImage((Texture2D)LocalPlayerAvatar.texture, LocalPlayerInfo.Avatar.ToArray());
        Reset();
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
        StartCoroutine(AppendMessage_Async(sender, avatar, content));
    }

    IEnumerator AppendMessage_Async(string sender, byte[] avatar, string content)
    {
        if (ChatMessagePrefab == null)
            ChatMessagePrefab = UIAssetsManager.Instance.Windows.First((search) => search.name == "MessageUI");

        GameObject obj = Instantiate(ChatMessagePrefab, ChatContainer.content);
        MessageUI msg = obj.GetComponent<MessageUI>();

        //更新信息框UI
        msg.AppendMessage(sender, avatar, content);
        if (!isDragging)
            StartCoroutine(ScrollToLatestMessage());

        yield return null;
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (TextContextMenu.Instance != null)
            {
                TextContextMenu.Instance.SelfDestruction();
            }
        }
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
                ChatCompletion.Instance.chatRequestLogs.Add(new ChatCompletion.ChatRequestLog() { history = new List<ChatMessage>(), sender = msg.Sender.Username });
                ChatCompletion.Instance.chatRequestUnderProcessing.Add(conn, ChatCompletion.Instance.SendChatRequest(null, msg.content, conn));
            }

        }


    }

    [Command(requiresAuthority = false)]
    public void ResendMessageToChatGPT(GPTPlayer player)
    {
        NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == player.Username).Key;
        if (conn == null)
            return;
        try
        {
            ChatCompletion.ChatRequestLog result = ChatCompletion.Instance.chatRequestLogs.FirstOrDefault(x => (x.sender == player.Username));
            if (result.history == null || result.history.Count <= 0)
            {
                OnReceiveServerTargetedMessage(player.netIdentity.connectionToClient, new GPTChatMessage()
                {
                    content = "Unable to resend because there is no message/chat log here."
                });
                return;
            }
            ChatCompletion.Instance.chatRequestUnderProcessing.Add(conn, ChatCompletion.Instance.SendChatRequest(result.history, null, conn));
            Debug.Log("ChatRquestLog Found");
        }
        catch
        {
            OnReceiveChatGPTMessage(player.netIdentity.connectionToClient, "Unable to resend because there is no message/chat log here.");
        }
    }

    [Command(requiresAuthority = false)]
    public void StopChatGPTProcessing(GPTPlayer applicant)
    {
        NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == applicant.Username).Key;
        if (conn != null)
        {
            Coroutine result = ChatCompletion.Instance.chatRequestUnderProcessing.First(x => x.Key == conn).Value;
            StopCoroutine(result);
        }
    }

    [Command(requiresAuthority = false)]
    public void RequestFullChatLog(GPTPlayer applicant)
    {
        NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == applicant.Username).Key;
        if (conn != null)
        {
            Debug.Log("[ChatWindow]Requesting " + applicant.Username + "'s full chat log......");
            string result = ChatCompletion.Instance.GetFullChatLog(applicant.Username);
            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log("[ChatWindow]Sending " + applicant.Username + "'s chat log to himself/herself.....");
                OnReceiveFullChatLog(((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == applicant.Username).Key, result);
            }
            else
            {
                Debug.Log("[ChatWindow]Looks like there is no chat log of " + applicant.Username + ".");
                OnReceiveServerTargetedMessage(((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == applicant.Username).Key,
                    new GPTChatMessage() { content = "Looks like there is no chat log exist. Please enjoy a brand new chat!" });
            }
        }
    }

    [TargetRpc]
    public void OnReceiveFullChatLog(NetworkConnection conn, string log)
    {
        Debug.Log("[ChatWindow]Successfully receive full chat log.");
        //清屏
        //foreach(MessageUI obj in ChatContainer.content.GetComponentsInChildren<MessageUI>())
        //{
        //    Destroy(obj.gameObject);
        //}
        List<ChatMessage> result = JsonConvert.DeserializeObject<List<ChatMessage>>(log);
        //输出
        //foreach(ChatMessage msg in result)
        //{
        //    if (msg.role.Equals("system", StringComparison.OrdinalIgnoreCase))
        //    {
        //        AppendMessage("ChatGPT", UIAssetsManager.Instance.GetIcon2Texture("chatgpt_icon").EncodeToPNG(), msg.content);
        //    }
        //    else
        //    {
        //        AppendMessage(LocalPlayerInfo.Username, LocalPlayerInfo.Avatar,msg.content);
        //    }

        //改用协程以避免卡顿

        //AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "<b>--- MESSAGE(S) FROM CHAT LOG ---</b>");
        //for (int i = 1; i < result.Count; i++)
        //{
        //    if (result[i].role.Equals("system", StringComparison.OrdinalIgnoreCase))
        //    {
        //        AppendMessage("ChatGPT", UIAssetsManager.Instance.GetIcon2Texture("chatgpt_icon").EncodeToPNG(), result[i].content);
        //    }
        //    else
        //    {
        //        AppendMessage(LocalPlayerInfo.Username, LocalPlayerInfo.Avatar, result[i].content);
        //    }
        //}
        //AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "<b>--- END OF CHAT LOG ---</b>");

        StartCoroutine(OnPrintMultipleMessages(result));
    }

    IEnumerator OnPrintMultipleMessages(List<ChatMessage> result)
    {
        AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "<b>--- MESSAGE(S) FROM CHAT LOG ---</b>");
        for (int i = 1; i < result.Count; i++)
        {
            if (result[i].role.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                AppendMessage("ChatGPT", UIAssetsManager.Instance.GetIcon2Texture("chatgpt_icon").EncodeToPNG(), result[i].content);
            }
            else
            {
                AppendMessage(LocalPlayerInfo.Username, LocalPlayerInfo.Avatar, result[i].content);
            }
        }
        AppendMessage("SYSTEM", UIAssetsManager.Instance.GetIcon2Texture("announcement_icon").EncodeToPNG(), "<b>--- END OF CHAT LOG ---</b>");
        yield return null;
    }

    [Command(requiresAuthority = false)]
    public void ClearChatLog(GPTPlayer applicant)
    {
        NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == applicant.Username).Key;
        if (conn != null)
        {
            ChatCompletion.Instance.ClearChatLog(applicant.Username);
        }
    }
}
