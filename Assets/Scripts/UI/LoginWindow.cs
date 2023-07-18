using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using Mirror;

public class LoginWindow : MonoBehaviour
{
    public static LoginWindow Instance;
    public GameObject Splashscreen;
    public GameObject LoginScreen;
    public TMP_InputField ServerAddress;
    public TMP_InputField Port;
    public TMP_InputField Username;
    public Button LoginAsClientBtn;
    public Button LoginAsHostBtn;
    public RawImage Avatar;

    private void Awake()
    {
        Instance = this;
        LoginScreen.SetActive(false);
        Splashscreen.SetActive(false);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            LoginAsHostBtn.interactable = false;
        }

        LoginAsClientBtn.onClick.AddListener(LoginAsClient);
        LoginAsHostBtn.onClick.AddListener(LoginAsHost);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowSplashScreen()
    {
        Splashscreen.SetActive(true);
        LoginScreen.SetActive(false);
    }

    public void ShowLoginScreen()
    {
        Splashscreen.SetActive(false);
        LoginScreen.SetActive(true);
    }

    public void HideLoginScreen()
    {
        Splashscreen.SetActive(false);
        LoginScreen.SetActive(false);
    }

    public void OnChangeAvatarBtnClicked()
    {
        FileOpenDialog.Instance.OpenFileDialog(OnAvatarOpened);
    }

    public void OnAvatarOpened(string info)
    {
        FileOpenDialog.FileInfo result = JsonConvert.DeserializeObject<FileOpenDialog.FileInfo>(info);
        Debug.Log("Selected File: " + result.Filename + ", path: " + result.Path);
        StartCoroutine(LoadData(result.Path));
    }

    IEnumerator LoadData(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            yield break;
        }
        Debug.Log("File loaded! " + url);

        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(request.downloadHandler.data);
        Debug.Log("LoadImage complete!");

        Avatar.texture = texture;
        Debug.Log("Avatar updated!");
    }

    public void LoginAsClient()
    {
        if (String.IsNullOrWhiteSpace(ServerAddress.text))
        {
            MsgBoxManager.Instance.ShowMsgBox("Please fill in the Server Address.", false);
            return;
        }

        if (String.IsNullOrWhiteSpace(Username.text))
        {
            MsgBoxManager.Instance.ShowMsgBox("Please fill in your Username.", false);
            return;
        }
        SetAuthRequestMessage(false);
        GPTNetworkManager.singleton.networkAddress = ServerAddress.text;
        //(String.IsNullOrWhiteSpace(Port.text) ? ":7777" : ":" + Port.text)
        GPTNetworkManager.singleton.StartClient();
    }

    public void LoginAsHost()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            MsgBoxManager.Instance.ShowMsgBox("Host is not available on WebGL version. \nIf you wish to be host or set up dedicated server, please move to standalone version.", false);
            return;
        }
        SetAuthRequestMessage(true);
        GPTNetworkManager.singleton.StartHost();
    }

    public void SetAuthRequestMessage(bool isAdmin)
    {
        ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).ClientInfo = new GPTNetworkAuthenticator.AuthRequestMessage()
        {
            Username = Username.text,
            Avatar = Avatar.texture?ImageConversion.EncodeToPNG((Texture2D)Avatar.texture):ImageConversion.EncodeToPNG(UIAssetsManager.Instance.GetIcon2Texture("default-avatar")),
            UserRole = isAdmin ? GPTNetworkAuthenticator.AuthRequestMessage.Role.Admin : GPTNetworkAuthenticator.AuthRequestMessage.Role.User
        };
    }
}
