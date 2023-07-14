using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

public class FileOpenDialog : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ImageUploaderCaptureClick();
    public static FileOpenDialog Instance;

    public Action<string> onFileSelected;


    private void Awake()
    {
        Instance = this;
    }

    public void OpenFileDialog(Action<string> onFileSelected)
    {
#if UNITY_EDITOR
        string filePath = UnityEditor.EditorUtility.OpenFilePanel("Open File", "", "");
        if (!string.IsNullOrEmpty(filePath))
        {
            FileInfo info = new FileInfo();
            info.Path = filePath;
            info.Filename = Path.GetFileName(filePath);
            onFileSelected?.Invoke(JsonConvert.SerializeObject(info));
        }
#elif UNITY_WEBGL
#if !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false; // 禁用键盘输入
#endif
/*
        filePathText.text = ""; // 清空文本框内容
        openButton.interactable = false; // 禁用打开按钮
*/

        onFileSelected += (data) =>
        {
            WebGLInput.captureAllKeyboardInput = true; // 启用键盘输入
        };

        this.onFileSelected = onFileSelected;

        // 通过JavaScript函数来触发文件选择对话框
        Application.ExternalEval(@"
            document.getElementById('upload').click();
        ");
#endif
    /*
        OpenFileDialog((filePath) =>
        {
            // 在这里处理用户选择的文件路径
            Debug.Log("Selected File: " + filePath);
        });
    */
    }
    // 在文件选择对话框中选择完成后的回调函数
    public void OnFileSelected(string info)
    {
        /*
        FileInfo result = JsonConvert.DeserializeObject<FileInfo>(info);
        Debug.Log("Selected File: " + result.Filename + ", path: " + result.Path);
        StartCoroutine(LoadData(result.Path));
        */
        onFileSelected?.Invoke(info);
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