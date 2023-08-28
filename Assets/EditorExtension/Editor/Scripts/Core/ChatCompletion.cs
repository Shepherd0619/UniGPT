using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UniGPT.Editor
{
    public class ChatCompletion : MonoBehaviour
    {
        public static string API_URL = "https://api.openai.com/v1/chat/completions";
        public static string OPENAI_API_KEY = "";

        public static List<Coroutine> chatRequestUnderProcessing;

        public void SendChatMessage(List<ChatMessage> messages, Action<string> callback)
        {
            StartCoroutine(ChatRequestCoroutine(messages, callback));
        }

        /// <summary>
        /// Send Messages to GPT3.5. If it is a success, will return the reply. (NOT THE FULL CHAT LOG)
        /// </summary>
        /// <param name="messages">Full Chat Log</param>
        /// <returns></returns>
        private IEnumerator ChatRequestCoroutine(List<ChatMessage> messages, Action<string> callback)
        {
            if (messages == null || messages.Count == 0)
            {
                /* 
                messages = new List<ChatMessage>
                {
                    new ChatMessage { role = "system", content = "You are a helpful assistant." },
                    new ChatMessage { role = "user", content = "Hello!" }
                };
                */

                Debug.LogError("[ChatCompletion]Could not process empty chat log. ChatRequestCoroutine Aborted!");
            }

            // Prepare request data
            ChatRequest requestData = new ChatRequest
            {
                model = "gpt-3.5-turbo",

                messages = messages
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
            }
            else
            {
                // Read response data
                string jsonResponseData = webRequest.downloadHandler.text;
                ChatResponse responseData = JsonConvert.DeserializeObject<ChatResponse>(jsonResponseData);

                // Handle the response data
                // ...
                callback.Invoke(responseData.choices[0].message.content);
            }

            webRequest.Dispose();

            yield return null;
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
}