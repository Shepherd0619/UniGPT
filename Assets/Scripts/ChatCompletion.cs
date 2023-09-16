using Mirror;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

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
        //TODO:后续需要改进一下这个函数，让服务器找记录
        if (history != null && history.Count > 0)
        {
            if (!string.IsNullOrWhiteSpace(msg))
                history.Add(new ChatMessage() { role = "user", content = msg });
        }
        else
        {
            history = new List<ChatMessage>
            {
                new ChatMessage { role = "system", content = "You are a helpful assistant." },
                new ChatMessage { role = "user", content = msg }
            };
        }

        try
        {
            ChatCompletion.ChatRequestLog x = chatRequestLogs.FirstOrDefault(x => x.sender == ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username);
            x.history = history;
        }
        catch
        {
            Debug.LogWarning("[ChatCompletion]Could not find " + ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username + "'s chat log. The user or server may clear it during this request.");
            chatRequestLogs.Add(new ChatRequestLog(){
                history = history,
                sender = ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username
            });
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

        // 设置文件路径
        string filePath = Application.persistentDataPath + "/GPTChatLog_" + ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username + ".json";
        // 序列化数据为JSON字符串
        string jsonData = JsonConvert.SerializeObject(msgs, Formatting.Indented);

        // 将JSON字符串写入文件
        File.WriteAllText(filePath, jsonData);

        // Send the request asynchronously
        yield return webRequest.SendWebRequest();

        // Check for errors
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error sending chat request: " + webRequest.error);
            string errorLog = "";
            Dictionary<string, string> responseHeaders = webRequest.GetResponseHeaders();
            if (responseHeaders != null && responseHeaders.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in responseHeaders)
                {
                    errorLog += kvp.Key + ": " + kvp.Value + "\n";
                }
            }
            else
            {
                errorLog = "Internal Error.";
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
            try
            {
                ChatCompletion.ChatRequestLog x = chatRequestLogs.FirstOrDefault(x => x.sender == result.Username);
                msgs.Add(new ChatMessage() { role = "system", content = responseData.choices[0].message.content });
                x.history = msgs;
            }
            catch
            {
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
        // 设置文件路径
        string filePath = Application.persistentDataPath + "/GPTChatLog_" + ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username + ".json";
        // 如果文件存在，则读取并填充数据
        if (File.Exists(filePath))
        {
            // 从文件中读取JSON字符串
            string jsonData = File.ReadAllText(filePath);

            // 反序列化JSON字符串为对象
            List<ChatMessage> list = JsonConvert.DeserializeObject<List<ChatMessage>>(jsonData);
            ChatMessage response = new ChatMessage() { role = "system", content = callback.choices[0].message.content };
            list.Add(response);
            jsonData = JsonConvert.SerializeObject(list, Formatting.Indented);

            // 将JSON字符串写入文件
            File.WriteAllText(filePath, jsonData);
        }
        else
        {
            Debug.LogWarning("[ChatCompletion]" + ((GPTNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).Username + "'s chat log is missing!");
        }
    }

    public void ChatRequestResponseErrorCallback(NetworkConnection conn, string errorMsg)
    {
        ChatWindow.Instance.OnReceiveChatGPTMessage(conn, "Error sending chat request: " + errorMsg + "\nYou can type <b>/Resend</b> to try again.");
    }

    public string GetFullChatLog(string username)
    {
        //foreach (ChatRequestLog log in chatRequestLogs)
        //{
        //    if(log.sender == username)
        //    {
        //        return log.history;
        //    }
        //}

        //Debug.LogWarning("[ChatCompletion]Could not get " + username + "'s full chat log in server's realtime chat log list. User may be newbie or want to retrieve the local copy?");

        string filePath = Application.persistentDataPath + "/GPTChatLog_" + username + ".json";

        if (File.Exists(filePath))
        {
            // 从文件中读取JSON字符串
            string jsonData = File.ReadAllText(filePath);
            return jsonData;
        }

        return null;
    }

    public void ClearChatLog(string username)
    {

        foreach (ChatRequestLog log in chatRequestLogs)
        {
            if (log.sender == username)
            {
                log.history.Clear();
                Debug.Log("[ChatCompletion]Successfully cleared " + username + "'s realtime chat log.");
                break;
            }
        }

        string filePath = Application.persistentDataPath + "/GPTChatLog_" + username + ".json";

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("[ChatCompletion]Successfully cleared " + username + "'s chat log.");
            NetworkConnection conn = ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).UsersList.First(x => x.Value.Username == username).Key;
            if (conn != null)
                ChatWindow.Instance.OnReceiveServerTargetedMessage(conn, new GPTChatMessage() { content = "Your chat log has been cleared!" });
        }
    }

    public void LoadChatLogFromDisk(string username)
    {
        Debug.Log("[ChatCompletion]Start loading " + username + "'s chat log from disk.");
        string directoryPath = Application.persistentDataPath;

        string searchPattern = $"GPTChatLog_{username}.json";
        string[] files = Directory.GetFiles(directoryPath, searchPattern);

        if (files.Count() > 0)
        {
            Debug.Log("[ChatCompletion]Mounting " + username + "'s chat log to realtime, stand by...");
            List<ChatMessage> result = JsonConvert.DeserializeObject<List<ChatMessage>>(File.ReadAllText(files[0]));
            foreach (ChatRequestLog log in chatRequestLogs)
            {
                if (log.sender == username)
                {
                    log.history = result;
                    return;
                }
            }

            chatRequestLogs.Add(new ChatRequestLog() { history = result, sender = username });
            Debug.Log("[ChatCompletion]Successfully loaded " + username + "'s chat log.");
        }
        else
        {
            Debug.Log("[ChatCompletion]Unable to find " + username + "'s chat log from disk.");
        }
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