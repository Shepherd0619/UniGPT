using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class TextContextMenu : MonoBehaviour
{
    public Button CopyBtn;
    public Button PasteBtn;
    public Button ShareBtn;

    private TMP_Text ui_text;
    private MessageUI ui_message;
    private bool isReadOnly = false;
    private bool isPureText = true;
    public static TextContextMenu Instance;

    /// <summary>
    /// 正常文本标签用
    /// </summary>
    /// <param name="_Text"></param>
    /// <param name="ReadOnly"></param>
    public void Initialize(TMP_Text _Text, bool ReadOnly = true)
    {
        Instance = this;
        ui_text = _Text;
        isReadOnly = ReadOnly;
        isPureText = true;
#if UNITY_ANDROID || UNITY_IOS
        ShareBtn.gameObject.SetActive(true);
#else
        ShareBtn.gameObject.SetActive(false);
#endif

        PasteBtn.gameObject.SetActive(!isReadOnly);
        CopyBtn.gameObject.SetActive(true);

        CopyBtn.onClick.RemoveAllListeners();
        CopyBtn.onClick.AddListener(CopyText);

        PasteBtn.onClick.RemoveAllListeners();
        if (!isReadOnly)
            PasteBtn.onClick.AddListener(PasteText);

        ShareBtn.onClick.AddListener(ShareText);
    }

    /// <summary>
    /// MessageUI专用 
    /// </summary>
    /// <param name="_Text"></param>
    /// <param name="ReadOnly"></param>
    public void Initialize(MessageUI _Text, bool ReadOnly = true)
    {
        Instance = this;
        ui_message = _Text;
        isReadOnly = ReadOnly;
        isPureText = false;
#if UNITY_ANDROID || UNITY_IOS
        ShareBtn.gameObject.SetActive(true);
#else
        ShareBtn.gameObject.SetActive(false);
#endif
        PasteBtn.gameObject.SetActive(!isReadOnly);
        CopyBtn.gameObject.SetActive(true);

        CopyBtn.onClick.RemoveAllListeners();
        CopyBtn.onClick.AddListener(CopyText);

        PasteBtn.onClick.RemoveAllListeners();
        if (!isReadOnly)
            PasteBtn.onClick.AddListener(PasteText);

        ShareBtn.onClick.AddListener(ShareText);
    }

    public void SelfDestruction()
    {
        Instance = null;
        Destroy(gameObject);
    }

    public virtual void CopyText()
    {
        if(isPureText)
            GUIUtility.systemCopyBuffer = ui_text.text;
        else
        {
            GUIUtility.systemCopyBuffer = ui_message.orgText;
        }

        SelfDestruction();
    }

    public virtual void PasteText()
    {
        if (!isReadOnly)
            ui_text.text = GUIUtility.systemCopyBuffer;

        SelfDestruction();
    }

    public virtual void ShareText()
    {
#if UNITY_ANDROID || UNITY_IOS
        NativeShare share = new NativeShare();
        share.Clear();

        if(isPureText)
            share.SetText(ui_text.text + "\n--- Message from UniGPT which was developed by Shepherd0619.\nFor more info, please visit https://shepherd0619.github.io/.");
        else
            share.SetText(ui_message.orgText + "\n--- Message from UniGPT which was developed by Shepherd0619.\nFor more info, please visit https://shepherd0619.github.io/.");

        share.SetUrl("https://shepherd0619.github.io/");
        share.Share();
#endif

        SelfDestruction();
    }
}