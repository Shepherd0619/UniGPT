using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using TMPro;

public class ChatWindow : NetworkBehaviour
{
    public static ChatWindow Instance;
    public ScrollRect ChatContainer;
    public GameObject ChatMessagePrefab;
    public TMP_InputField MessageInputField;
    public RawImage LocalPlayerAvatar;
    public GPTPlayer LocalPlayer;

    public readonly List<ChatRequestLog> chatRequestLogs = new List<ChatRequestLog>();
    public class ChatRequestLog
    {
        public List<ChatMessage> history;
        public GPTPlayer sender;
    }

    public readonly Dictionary<NetworkConnection, Coroutine> chatRequestUnderProcessing = new Dictionary<NetworkConnection, Coroutine>();

    private void Awake()
    {
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

    public void ShowChatWindow()
    {
        gameObject.SetActive(true);
    }

    public void HideChatWindow()
    {
        gameObject.SetActive(false);
    }

    public void Init()
    {

    }

    public void SetLocalPlayerInfo(GPTPlayer player)
    {
        ImageConversion.LoadImage((Texture2D)LocalPlayerAvatar.texture, player.Avatar.ToArray());
    }

    public void AppendMessage(string sender, byte[] avatar, string content)
    {
        GameObject obj = Instantiate(ChatMessagePrefab, ChatContainer.content);
        MessageUI msg = obj.GetComponent<MessageUI>();

        //更新信息框UI
        msg.AppendMessage(sender, avatar, content);
    }

    [ClientRpc]
    //<summary>
    //服务器向客户端广播信息
    //</summary>    
    public void OnReceiveServerMessage(GPTChatMessage msg)
    {
        if (msg.Sender == null)
        {
            //系统信息
            msg.Sender.Username = "SYSTEM";
            msg.Sender.Avatar.Clear();
            foreach(byte data in ImageConversion.EncodeToPNG(UIAssetsManager.Instance.GetIcon2Texture("announcement_icon"))){
                msg.Sender.Avatar.Add(data);
            }
        }

        AppendMessage(msg.Sender.Username, msg.Sender.Avatar.ToArray(), msg.content);

    }

    [TargetRpc]
    //<summary>
    //服务器向指定客户端广播信息
    //</summary>
    public void OnReceiveServerTargetedMessage(NetworkConnection Target, GPTChatMessage msg)
    {
        if (msg.Sender == null)
        {
            //系统信息
            msg.Sender.Username = "SYSTEM";
            msg.Sender.Avatar.Clear();
            foreach(byte data in ImageConversion.EncodeToPNG(UIAssetsManager.Instance.GetIcon2Texture("announcement_icon"))){
                msg.Sender.Avatar.Add(data);
            }
        }
        
        AppendMessage(msg.Sender.Username, msg.Sender.Avatar.ToArray(), msg.content);
    }

    [TargetRpc]
    public void OnReceiveChatGPTMessage(NetworkConnection conn, string content)
    {
        AppendMessage("ChatGPT", ImageConversion.EncodeToPNG(UIAssetsManager.Instance.GetIcon2Texture("chatgpt_icon")), content);
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
            if (msg.Sender != null)
            {
                GPTNetworkAuthenticator.AuthRequestMessage search = new GPTNetworkAuthenticator.AuthRequestMessage();
                search.Username = msg.Receiver.Username;
                search.Avatar = msg.Receiver.Avatar.ToArray();
                search.UserRole = msg.Receiver.UserRole;
                if (((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.ContainsValue(search))
                {
                    NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == search.Username).Key;

                    OnReceiveServerTargetedMessage(conn, msg);
                }
            }

        }
    }

    [Command(requiresAuthority = false)]
    public void SendMessageToChatGPT(GPTChatMessage msg)
    {
        if (msg.Sender != null)
        {
            GPTNetworkAuthenticator.AuthRequestMessage search = new GPTNetworkAuthenticator.AuthRequestMessage();
            search.Username = msg.Receiver.Username;
            search.Avatar = msg.Receiver.Avatar.ToArray();
            search.UserRole = msg.Receiver.UserRole;
            if (((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.ContainsValue(search))
            {
                NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == search.Username).Key;

                ChatRequestLog result = chatRequestLogs.First(x => (x.sender == msg.Sender));
                if (result != null)
                {
                    chatRequestUnderProcessing.Add(conn, ChatCompletion.Instance.SendChatRequest(result.history, msg.content, conn));
                }
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
            Coroutine result =  chatRequestUnderProcessing.First(x => x.Key == conn).Value;
            StopCoroutine(result);
        }
    }

}
