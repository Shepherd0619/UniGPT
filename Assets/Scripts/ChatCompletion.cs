using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Mirror;
public class ChatCompletion : MonoBehaviour
{
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    public string OPENAI_API_KEY = "";
    public static ChatCompletion Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Coroutine SendChatRequest(List<ChatMessage> history, string msg, NetworkConnection conn)
    {
        if (history != null || history.Count == 0)
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
        // Prepare request data
        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-3.5-turbo",
            messages = msgs
        };

        // Convert request data to JSON
        string jsonRequestData = JsonConvert.SerializeObject(requestData);

        // Create HTTP client and request headers
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {OPENAI_API_KEY}");
        client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        // Send POST request
        var response = client.PostAsync(API_URL, new StringContent(jsonRequestData, Encoding.UTF8, "application/json")).Result;

        // Read response data
        string jsonResponseData = response.Content.ReadAsStringAsync().Result;
        ChatResponse responseData = JsonConvert.DeserializeObject<ChatResponse>(jsonResponseData);

        // Handle the response data
        // ...
        ChatRequestResponseCallback(responseData, conn);
        yield return null;
    }

    public void ChatRequestResponseCallback(ChatResponse callback, NetworkConnection conn)
    {
        ChatWindow.Instance.OnReceiveChatGPTMessage(conn, callback.choices[0].message.content);
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