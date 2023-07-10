using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MsgBox : MonoBehaviour
{
    public TMP_Text contentText;
    public GameObject confirmButton;
    public GameObject cancelButton;
    public GameObject maskPanel;
    private GameObject msgBoxPanel;

    public Action<bool> callback; // 回调函数，用于返回用户的操作结果

    private void Awake() {
        
    }

    // 显示信息框并获取用户的操作
    public void ShowMsgBox(string content, bool hasCancelButton, Action<bool> result)
    {
        // 设置信息框的内容和标题
        contentText.text = content;

        // 根据是否有确认按钮来显示或隐藏确认按钮
        confirmButton.SetActive(true);

        // 根据是否有取消按钮来显示或隐藏取消按钮
        cancelButton.SetActive(hasCancelButton);

        if(hasCancelButton){
            confirmButton.transform.position = new Vector3(0.0f, confirmButton.transform.position.y,confirmButton.transform.position.z);
        }

        //保存回调函数
        callback = result;

        // 禁用其他UI，只允许与信息框互动
        maskPanel.SetActive(true);

        // 显示信息框
        msgBoxPanel.SetActive(true);
    }

    // 确认按钮点击事件
    public void ConfirmButtonClicked()
    {
        // 执行确认操作
        // ...

        // 关闭信息框
        msgBoxPanel.SetActive(false);

        // 恢复其他UI的交互能力
        maskPanel.SetActive(false);

        // 调用回调函数，返回用户的操作结果
        callback?.Invoke(true);
        MsgBoxManager.Instance.RemoveMsgBox(this);
    }

    // 取消按钮点击事件
    public void CancelButtonClicked()
    {
        // 执行取消操作
        // ...

        // 关闭信息框
        msgBoxPanel.SetActive(false);

        // 恢复其他UI的交互能力
        maskPanel.SetActive(false);

        // 调用回调函数，返回用户的操作结果
        callback?.Invoke(false);
        MsgBoxManager.Instance.RemoveMsgBox(this);
    }

    /*
    这是一个回调演示。
    private void HandleUserAction(bool result)
    {
        if (result)
        {
            Debug.Log("用户点击了确认按钮");
            // 执行确认操作
            // ...
        }
        else
        {
            Debug.Log("用户点击了取消按钮");
            // 执行取消操作
            // ...
        }
    }
    */
}