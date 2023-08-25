using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public RawImage Avatar;
    public string Name;
    public string Message;
    //public ScrollRect messageScroll;
    public RectTransform TextRectTransform;
    public TMP_Text messageText;
    public string orgText;
    private RectTransform rectTransform;
    private RectTransform scrollTransform;
    private Vector2 orgSize;

    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private float requiredHoldTime = 0.5f; // 定义长按所需的时间

    public GameObject menuUI;

    public static GameObject realtimeMenuUI;
    PointerEventData eventData;

    void Start()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        //scrollTransform = messageScroll.GetComponent<RectTransform>();
        orgSize = rectTransform.sizeDelta;
        ScreenSizeDetector.Instance.Listeners.Add(gameObject);
        StartCoroutine(LateStart());

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (isPointerDown)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= requiredHoldTime)
            {
                // 如果时间达到要求，显示右键菜单或执行复制操作
                ShowContextMenu(eventData.position);
            }
        }
#endif
    }

    public void AppendMessage(string name, byte[] avatar, string message)
    {
        Avatar.texture = new Texture2D(1, 1);
        ImageConversion.LoadImage((Texture2D)Avatar.texture, avatar);
        Name = name;
        Message = message;

        orgText = name + "\n" + message;

        messageText.text = "<b>" + name + "</b>\n\n" + message;

        GetComponent<CodeSnippetHighlighter>().FormatAndHighlightCode(messageText);
    }

    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();

        // 在首帧渲染后立即执行的初始化操作
        // ...
        OnScreenSizeChanged();
        // 确保这段代码只会在首帧执行一次
        StopCoroutine(LateStart());
    }

    public void OnScreenSizeChanged()
    {
        //Vector2 ContentSize = messageScroll.content.sizeDelta;
        Vector2 ContentSize = TextRectTransform.sizeDelta;
        rectTransform.sizeDelta = new Vector2(orgSize.x, ContentSize.y + orgSize.y);
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform.parent as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent as RectTransform);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 点击右键时显示右键菜单
            ShowContextMenu(eventData.position);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HideContextMenu();
            isPointerDown = true;
            pointerDownTimer = 0f;
            this.eventData = eventData;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
#if UNITY_IOS || UNITY_ANDROID
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isPointerDown = false;
            if (pointerDownTimer < requiredHoldTime)
            {
                // 如果时间不足，执行默认的点击操作
                ExecuteDefaultClick();
            }
        }
#endif
    }

    private void ShowContextMenu(Vector2 position)
    {
        if (realtimeMenuUI != null)
        {
            Destroy(realtimeMenuUI);
        }

        if(menuUI == null)
        {
            menuUI = UIAssetsManager.Instance.Windows.First((search) => search.name == "ContextMenu");
        }

        realtimeMenuUI = Instantiate(menuUI, transform);
        realtimeMenuUI.transform.position = position;
        // 显示右键菜单
        realtimeMenuUI.SetActive(true);
        realtimeMenuUI.GetComponent<TextContextMenu>().Initialize(this);
    }

    private void HideContextMenu()
    {
        if (realtimeMenuUI != null)
        {
            Destroy(realtimeMenuUI);
        }
    }

    private void ExecuteDefaultClick()
    {
        // 执行默认的点击操作
        Debug.Log("Default click action");
    }
}
