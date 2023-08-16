using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextContextMenu : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject menuUI;
    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private float requiredHoldTime = 0.5f; // ���峤�������ʱ��

    private void Update()
    {
        if (isPointerDown)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= requiredHoldTime)
            {
                // ���ʱ��ﵽҪ����ʾ�Ҽ��˵���ִ�и��Ʋ���
                ShowContextMenu();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // ����Ҽ�ʱ��ʾ�Ҽ��˵�
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
                // ���ʱ�䲻�㣬ִ��Ĭ�ϵĵ������
                ExecuteDefaultClick();
            }
        }
    }

    private void ShowContextMenu()
    {
        // ��ʾ�Ҽ��˵�
        menuUI.SetActive(true);
    }

    private void ExecuteDefaultClick()
    {
        // ִ��Ĭ�ϵĵ������
        Debug.Log("Default click action");
    }
}