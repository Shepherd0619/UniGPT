using UnityEngine;
using TMPro;

public class TextContextMenu : MonoBehaviour
{
    private TMP_Text ui_text;
    private bool isReadOnly = false;

    public virtual void CopyText()
    {
        GUIUtility.systemCopyBuffer = ui_text.text;
    }

    public virtual void PasteText()
    {
        if (!isReadOnly)
            ui_text.text = GUIUtility.systemCopyBuffer;
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
    }
}