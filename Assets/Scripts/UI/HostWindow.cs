using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.IO;

public class HostWindow : MonoBehaviour
{
    public static HostWindow Instance;
    public GameObject ConfigWindow;
    public List<UI_ConfigBool> UI_ConfigBools;
    public List<UI_ConfigInt> UI_ConfigInts;
    public List<UI_ConfigText> UI_ConfigTexts;
    public Button OKBtn;
    private Action OKBtn_Action;
    public Button CancelBtn;
    private Action CancelBtn_Action;

    public class Config
    {
        [System.Serializable]
        public class Text
        {
            public string Name;
            public string Value;
            [JsonIgnore]
            public string Default;
        }
        [System.Serializable]
        public class Int
        {
            public string Name;
            public int Value;
            [JsonIgnore]
            public int Default;
        }
        [System.Serializable]
        public class Bool
        {
            public string Name;
            public bool Value;
            [JsonIgnore]
            public bool Default;
        }
    }
    [System.Serializable]
    public class UI_ConfigText
    {
        public Config.Text Config;
        [JsonIgnore]
        public TMP_InputField UI;
    }
    [System.Serializable]
    public class UI_ConfigInt
    {
        public Config.Int Config;
        [JsonIgnore]
        public TMP_InputField UI;
    }
    [System.Serializable]
    public class UI_ConfigBool
    {
        public Config.Bool Config;
        [JsonIgnore]
        public Toggle UI;
    }

    private void Awake()
    {
        ConfigWindow.SetActive(false);
        Instance = this;
        OKBtn.onClick.AddListener(OnClickOKBtn);
        CancelBtn.onClick.AddListener(OnClickCancelBtn);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowConfigWindow(Action OKCallback = null, Action CancelCallback = null, bool hasCancelBtn = true)
    {
        ConfigWindow.SetActive(true);
        //TODO: 应该弄成类似提示框那种东西，这样的话能把ConfigWindow重新利用。
        //如在开服时作为开服前的参数配置，开服后是正常的修改运行时配置。
        LoadLastSavedConfig();

        OKBtn_Action = OKCallback;
        CancelBtn_Action = CancelCallback;

        if (!hasCancelBtn)
        {
            OKBtn.transform.localPosition = new Vector3(0, OKBtn.transform.localPosition.y, OKBtn.transform.localPosition.z);
        }
        else
        {
            OKBtn.transform.localPosition = new Vector3(-100f, OKBtn.transform.localPosition.y, OKBtn.transform.localPosition.z);
        }

        CancelBtn.gameObject.SetActive(hasCancelBtn);
    }

    public void HideConfigWindow()
    {
        ConfigWindow.SetActive(false);
    }

    public void LoadLastSavedConfig()
    {
        // 设置文件路径
        string filePath = Application.persistentDataPath + "/data.json";

        // 如果文件存在，则读取并填充数据
        if (File.Exists(filePath))
        {
            // 从文件中读取JSON字符串
            string jsonData = File.ReadAllText(filePath);

            // 反序列化JSON字符串为对象
            ConfigLists lists = JsonConvert.DeserializeObject<ConfigLists>(jsonData);

            // 填充数据到对应的列表
            UI_ConfigBools = lists.UI_ConfigBools;
            UI_ConfigInts = lists.UI_ConfigInts;
            UI_ConfigTexts = lists.UI_ConfigTexts;

            Debug.Log("Data has been loaded from file");
            
            foreach (UI_ConfigBool cb in UI_ConfigBools)
            {
                cb.UI.isOn = cb.Config.Value;
            }

            foreach (UI_ConfigInt ci in UI_ConfigInts)
            {
                ci.UI.text = ci.Config.Value.ToString();
            }

            foreach (UI_ConfigText ct in UI_ConfigTexts)
            {
                ct.UI.text = ct.Config.Value;
            }

            Debug.Log("[HostWindow]LastSavedConfig Loaded!");
        }
        else
        {
            Debug.Log("File not found! Revert to default.");
            foreach (UI_ConfigBool cb in UI_ConfigBools)
            {
                cb.UI.isOn = cb.Config.Default;
            }

            foreach (UI_ConfigInt ci in UI_ConfigInts)
            {
                ci.UI.text = ci.Config.Default.ToString();
            }

            foreach (UI_ConfigText ct in UI_ConfigTexts)
            {
                ct.UI.text = ct.Config.Default;
            }
        }
        
    }

    public void WriteConfig()
    {
        // 设置文件路径
        string filePath = Application.persistentDataPath + "/hostConfig.json";

        // 创建一个包含三个List的顶级类
        ConfigLists lists = new ConfigLists();
        lists.UI_ConfigBools = UI_ConfigBools;
        lists.UI_ConfigInts = UI_ConfigInts;
        lists.UI_ConfigTexts = UI_ConfigTexts;

        // 序列化数据为JSON字符串
        string jsonData = JsonConvert.SerializeObject(lists, Formatting.Indented);

        // 将JSON字符串写入文件
        File.WriteAllText(filePath, jsonData);
        Debug.Log("[HostWindow]Config Written!");
    }

    [System.Serializable]
    public class ConfigLists
    {
        public List<UI_ConfigBool> UI_ConfigBools;
        public List<UI_ConfigInt> UI_ConfigInts;
        public List<UI_ConfigText> UI_ConfigTexts;
    }

    public void OnClickOKBtn()
    {
        WriteConfig();
        OKBtn_Action?.Invoke();
        HideConfigWindow();
    }

    public void OnClickCancelBtn()
    {
        CancelBtn_Action?.Invoke();
        HideConfigWindow();
    }
}
