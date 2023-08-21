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
    private bool isReadOnly = false;
    public static TextContextMenu Instance;

    public void Initialize(TMP_Text _Text, bool ReadOnly = true)
    {
        Instance = this;
        ui_text = _Text;
        isReadOnly = ReadOnly;
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
        GUIUtility.systemCopyBuffer = Regex.Replace(ui_text.text, @"<.*?>", string.Empty);

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
        share.SetText(Regex.Replace(ui_text.text, @"<.*?>", string.Empty) + "\n--- Message from UniGPT which was developed by Shepherd0619.\nFor more info, please visit https://shepherd0619.github.io/.");
        share.SetUrl("https://shepherd0619.github.io/");
        share.Share();
#endif

        SelfDestruction();
    }
}