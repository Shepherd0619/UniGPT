using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextContextMenu : MonoBehaviour
{
    public Button CopyBtn;
    public Button PasteBtn;
    public Button ShareBtn;

    private TMP_Text ui_text;
    private bool isReadOnly = false;
    public static TextContextMenu Instance;

    public void Initialize(TMP_Text _Text)
    {
        Instance = this;
        ui_text = _Text;
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
        GUIUtility.systemCopyBuffer = ui_text.text;

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
        share.SetText(ui_text.text + "\n--- Message from UniGPT which was developed by Shepherd0619.\nFor more info, please visit https://shepherd0619.github.io/.");
        share.SetUrl("https://shepherd0619.github.io/");
        share.Share();
#endif

        SelfDestruction();
    }
}