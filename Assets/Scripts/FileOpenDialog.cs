using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;

public class FileOpenDialog : MonoBehaviour
{
#if !UNITY_EDITOR && UNITY_WEBGL
    private Action<string> onFileSelected;
#endif

    public void OpenFileDialog(Action<string> onFileSelected)
    {
#if UNITY_EDITOR
        string filePath = UnityEditor.EditorUtility.OpenFilePanel("Open File", "", "");
        if (!string.IsNullOrEmpty(filePath))
        {
            onFileSelected?.Invoke(filePath);
        }
#elif UNITY_WEBGL
#if !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false; // 禁用键盘输入
#endif
/*
        filePathText.text = ""; // 清空文本框内容
        openButton.interactable = false; // 禁用打开按钮
*/
/*
        onFileSelected += (path) =>
        {
            filePathText.text = path;
            openButton.interactable = true; // 启用打开按钮
        };
*/
        this.onFileSelected = onFileSelected;

        // 通过JavaScript函数来触发文件选择对话框
        Application.ExternalEval(@"
            function onFileSelected(event) {
                var file = event.target.files[0];
                unityInstance.SendMessage('FileOpenDialog', 'OnFileSelected', file.name);
                var reader = new FileReader();
                reader.onload = function(e) {
                    unityInstance.SendMessage('FileOpenDialog', 'OnFileContentLoaded', e.target.result);
                };
                reader.readAsDataURL(file);
            }
            document.getElementById('upload').click();
        ");
#elif UNITY_ANDROID || UNITY_IOS
        // 在移动端通过原生插件来打开文件选择对话框
        NativeGallery.Permission permission = NativeGallery.GetFileFromGallery((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                onFileSelected?.Invoke(path);
            }
        }, "Select File");
#endif
    }

    /*
        OpenFileDialog((filePath) =>
        {
            // 在这里处理用户选择的文件路径
            Debug.Log("Selected File: " + filePath);
        });
    */

    // 在文件选择对话框中选择完成后的回调函数
    public void OnFileSelected(string fileName)
    {
        Debug.Log("Selected File: " + fileName);
    }

    // 在文件内容加载完成后的回调函数
    public void OnFileContentLoaded(string fileContent)
    {
        // 在这里处理文件内容，例如解析文件数据等操作
        Debug.Log("File Content Loaded");
    }
}

/*
WebGL平台下，需要在HTML模板中添加一个隐藏的input元素来接收选择的文件，例如：
```html
<input id="upload" type="file" style="display:none" onchange="onFileSelected(event)">
```

这样，就可以在其他地方直接调用`OpenFileDialog`方法来打开文件选择对话
*/