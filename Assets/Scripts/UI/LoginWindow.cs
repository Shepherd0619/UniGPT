using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class LoginWindow : MonoBehaviour
{
    public static LoginWindow Instance;
    public GameObject Splashscreen;
    public GameObject LoginScreen;
    public TMP_InputField ServerAddress;
    public TMP_InputField Port;
    public TMP_InputField Username;
    public Image Avatar;

    private void Awake()
    {
        Instance = this;
        LoginScreen.SetActive(false);
        Splashscreen.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowLoginScreen(bool needSplash)
    {
        if (needSplash)
        {
            Splashscreen.SetActive(true);
            LoginScreen.SetActive(false);
        }
        else
        {
            Splashscreen.SetActive(false);
            LoginScreen.SetActive(true);
        }
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
        int width = texture.width;
        int height = texture.height;

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero);
        ((GPTNetworkAuthenticator)GPTNetworkManager.singleton.authenticator).ClientInfo.Avatar = sprite;
        Avatar.sprite = sprite;
        Debug.Log("Avatar updated!");
    }
}
