using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileOpenDialogTest : MonoBehaviour
{
    public Button btn;
    public TMP_Text txt;
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(OpenFileDialog);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OpenFileDialog()
    {
        FileOpenDialog.Instance.OpenFileDialog(OnFileSelected);
    }

    void OnFileSelected(string data)
    {
        txt.text = JsonConvert.DeserializeObject<FileOpenDialog.FileInfo>(data).Filename;
    }
}
