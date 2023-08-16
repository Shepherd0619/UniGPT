using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextContextMenu : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject menuUI;
    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private float requiredHoldTime = 0.5f; // 定义长按所需的时间

    private void Update()
    {
        if (isPointerDown)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= requiredHoldTime)
            {
                // 如果时间达到要求，显示右键菜单或执行复制操作
                ShowContextMenu();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 点击右键时显示右键菜单
            ShowContextMenu();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isPointerDown = true;
            pointerDownTimer = 0f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isPointerDown = false;
            if (pointerDownTimer < requiredHoldTime)
            {
                // 如果时间不足，执行默认的点击操作
                ExecuteDefaultClick();
            }
        }
    }

    private void ShowContextMenu()
    {
        // 显示右键菜单
        menuUI.SetActive(true);
    }

    private void ExecuteDefaultClick()
    {
        // 执行默认的点击操作
        Debug.Log("Default click action");
    }
}