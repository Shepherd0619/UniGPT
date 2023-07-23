using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using SFB;
using System.Windows.Forms;

public class FileOpenDialog : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ImageUploaderCaptureClick();
    public static FileOpenDialog Instance;

    private Action<string> m_onFileSelected;


    private void Awake()
    {
        Instance = this;
    }

    public void OpenFileDialog(Action<string> onFileSelected)
    {
        //TODO: 理论上后续应该加个文件后缀的参数，文件选择对话框操作应该是协程
#if UNITY_EDITOR || UNITY_STANDALONE
        ExtensionFilter[] filters = { new ExtensionFilter("Image", "jpg", "png") };
        string[] fileOpened = StandaloneFileBrowser.OpenFilePanel("Open Image", "", filters, false);

        if (fileOpened.Length != 0 && !string.IsNullOrEmpty(fileOpened[0]))
        {
            FileInfo info = new FileInfo();
            info.Path = fileOpened[0];
            info.Filename = Path.GetFileName(fileOpened[0]);
            onFileSelected?.Invoke(JsonConvert.SerializeObject(info));
        }

#elif UNITY_ANDROID || UNITY_IOS
        m_onFileSelected = onFileSelected;
        NativeGallery.RequestPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        NativeGallery.GetImageFromGallery(OnMobileFileSelected, "Open Image");
        
#endif
        /*
            OpenFileDialog((filePath) =>
            {
                // 在这里处理用户选择的文件路径
                Debug.Log("Selected File: " + filePath);
            });
        */
    }

    public void OnMobileFileSelected(string path)
    {
        Debug.Log("[NativeGallery]Path is " + path);
        FileInfo info = new FileInfo();
        info.Path = path;
        info.Filename = Path.GetFileName(path);
        m_onFileSelected?.Invoke(JsonConvert.SerializeObject(info));
    }

    IEnumerator LoadData(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
        request.downloadHandler = handler;
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            yield break;
        }
        Debug.Log("File loaded! " + url);
        byte[] data = handler.data;

        // 根据需要进行处理 data 的操作

    }

    [Serializable]
    public class FileInfo
    {
        public string Path;
        public string Filename;
    }
}
/*
WebGL平台下，需要在HTML模板中添加一个隐藏的input元素来接收选择的文件，例如：
```html
<input id="upload" type="file" style="display:none" onchange="onFileSelected(event)">
```

这样，就可以在其他地方直接调用`OpenFileDialog`方法来打开文件选择对话
*/