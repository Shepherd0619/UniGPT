using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MessageUI : MonoBehaviour
{
    public RawImage Avatar;
    public string Name;
    public string Message;
    public ScrollRect messageScroll;
    public TMP_Text messageText;
    private RectTransform rectTransform;
    private RectTransform scrollTransform;
    private Vector2 orgSize;

    void Start()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        scrollTransform = messageScroll.GetComponent<RectTransform>();
        orgSize = rectTransform.sizeDelta;
        ScreenSizeDetector.Instance.Listeners.Add(gameObject);
        StartCoroutine(LateStart());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AppendMessage(string name, byte[] avatar, string message)
    {
        ImageConversion.LoadImage((Texture2D)Avatar.texture,avatar);
        Name = name;
        Message = message;
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
        Vector2 ContentSize = messageScroll.content.sizeDelta;
        rectTransform.sizeDelta = new Vector2(orgSize.x, ContentSize.y + orgSize.y);
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform.parent as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent as RectTransform);
    }
}
