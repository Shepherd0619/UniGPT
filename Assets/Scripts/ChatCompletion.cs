using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Mirror;
using UnityEngine.Networking;

public class ChatCompletion : MonoBehaviour
{
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    public string OPENAI_API_KEY = "";
    public static ChatCompletion Instance;
    public readonly Dictionary<NetworkConnection, Coroutine> chatRequestUnderProcessing = new Dictionary<NetworkConnection, Coroutine>();
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

        // Create UnityWebRequest object
        UnityWebRequest webRequest = UnityWebRequest.Post(API_URL, jsonRequestData);
        // Add request headers
        webRequest.SetRequestHeader("Authorization", $"Bearer {OPENAI_API_KEY}");
        webRequest.SetRequestHeader("Content-Type", "application/json");

        // Send the request asynchronously
        yield return webRequest.SendWebRequest();

        // Check for errors
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error sending chat request: " + webRequest.error);
            ChatRequestResponseErrorCallback(conn, webRequest.error);
        }
        else
        {
            // Read response data
            string jsonResponseData = webRequest.downloadHandler.text;
            ChatResponse responseData = JsonConvert.DeserializeObject<ChatResponse>(jsonResponseData);

            // Handle the response data
            // ...
            ChatRequestResponseCallback(responseData, conn);
        }
    }
    public void ChatRequestResponseCallback(ChatResponse callback, NetworkConnection conn)
    {
        ChatWindow.Instance.OnReceiveChatGPTMessage(conn, callback.choices[0].message.content);
    }

    public void ChatRequestResponseErrorCallback(NetworkConnection conn, string errorMsg){
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