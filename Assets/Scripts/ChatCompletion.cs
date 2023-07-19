using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Mirror;
using UnityEngine.Networking;
using System.Linq;

public class ChatCompletion : MonoBehaviour
{
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    public string OPENAI_API_KEY = "";
    public static ChatCompletion Instance;
    public readonly Dictionary<NetworkConnection, Coroutine> chatRequestUnderProcessing = new Dictionary<NetworkConnection, Coroutine>();
    public readonly List<ChatRequestLog> chatRequestLogs = new List<ChatRequestLog>();
    public class ChatRequestLog
    {
        public List<ChatMessage> history;
        public string sender;
    }
    private void Awake()
    {
        Instance = this;
    }

    public Coroutine SendChatRequest(List<ChatMessage> history, string msg, NetworkConnection conn)
    {
        if (history != null && history.Count > 0)
            history.Add(new ChatMessage() { role = "user", content = msg });
        else
        {
            history = new List<ChatMessage>
            {
                new ChatMessage { role = "system", content = "You are a helpful assistant." },
                new ChatMessage { role = "user", content = msg }
            };
        }
        return StartCoroutine(ChatRequestCoroutine(history, conn));
    }

    private IEnumerator ChatRequestCoroutine(List<ChatMessage> msgs,
        NetworkConnection conn)
    {
        Debug.Log("Start processing.");
        // Prepare request data
        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-3.5-turbo",
            messages = msgs
        };

        // Convert request data to JSON
        string jsonRequestData = JsonConvert.SerializeObject(requestData);
        Debug.Log(jsonRequestData);
        // Create UnityWebRequest object
        //UnityWebRequest webRequest = UnityWebRequest.Post(API_URL, jsonRequestData);
        UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST");
        // 一定要要把json数据转成byte传送，否则OpenAI会拒绝
        webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonRequestData));
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        // Add request headers
        webRequest.SetRequestHeader("Authorization", $"Bearer {OPENAI_API_KEY}");
        webRequest.SetRequestHeader("Content-Type", "application/json");
        Debug.Log(webRequest.GetRequestHeader("Authorization"));
        Debug.Log(webRequest.GetRequestHeader("Content-Type"));

        // Send the request asynchronously
        yield return webRequest.SendWebRequest();

        // Check for errors
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error sending chat request: " + webRequest.error);
            string errorLog = "";
            foreach (KeyValuePair<string, string> kvp in webRequest.GetResponseHeaders())
            {
                errorLog += kvp.Key + ": " + kvp.Value + "\n";
            }

            Debug.LogError("Here are Response Headers: \n" + errorLog);

            ChatRequestResponseErrorCallback(conn, webRequest.error);
        }
        else
        {
            // Read response data
            string jsonResponseData = webRequest.downloadHandler.text;
            ChatResponse responseData = JsonConvert.DeserializeObject<ChatResponse>(jsonResponseData);

            // Handle the response data
            // ...
            GPTNetworkAuthenticator.AuthRequestMessage result;
            ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.TryGetValue(conn, out result);
            try{
                ChatCompletion.ChatRequestLog x = chatRequestLogs.FirstOrDefault(x => x.sender == result.Username);
                msgs.Add(new ChatMessage(){ role = "system", content = responseData.choices[0].message.content });
                x.history = msgs;
            }catch{
                Debug.LogWarning("[ChatCompletion]Could not find " + ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username + "'s chat log. The user or server may clear it during this request.");
            }
            ChatRequestResponseCallback(responseData, conn);
        }

        chatRequestUnderProcessing.Remove(conn);
        webRequest.Dispose();
    }

    public void ChatRequestResponseCallback(ChatResponse callback, NetworkConnection conn)
    {
        ChatWindow.Instance.OnReceiveChatGPTMessage(conn, callback.choices[0].message.content);
    }

    public void ChatRequestResponseErrorCallback(NetworkConnection conn, string errorMsg)
    {
        ChatWindow.Instance.OnReceiveChatGPTMessage(conn, "Error sending chat request: " + errorMsg);
    }
}

public class ChatRequest
{
    public string model { get; set; }
    public List<ChatMessage> messages { get; set; }
}

public class ChatMessage
{
    public string role { get; set; }
    public string content { get; set; }
}

public class ChatResponse
{
    public string id { get; set; }
    public string @object { get; set; }
    public long created { get; set; }
    public List<ChatChoice> choices { get; set; }
    public Usage usage { get; set; }
}

public class ChatChoice
{
    public int index { get; set; }
    public ChatMessage message { get; set; }
    public string finish_reason { get; set; }
}

public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}